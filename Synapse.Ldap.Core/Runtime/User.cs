using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using System.Runtime.InteropServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static UserPrincipalObject GetUser(string sAMAccountName, bool getGroups)
        {
            UserPrincipalObject u = null;
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sAMAccountName);
                u = new UserPrincipalObject(user);
                if (getGroups)
                    u.GetGroups();
            }
            return u;
        }

        public static bool MoveUserToOrganizationUnit(string userDistName, string orgUnitDistName)
        {
            if (String.IsNullOrEmpty(userDistName) || String.IsNullOrWhiteSpace(userDistName))
            {
                Console.WriteLine("No user is specified to be moved.");
                return false;
            }

            if (String.IsNullOrEmpty(orgUnitDistName) || String.IsNullOrWhiteSpace(orgUnitDistName))
            {
                Console.WriteLine("No destination organization unit is specified.");
                return false;
            }

            userDistName = $"LDAP://{userDistName.Replace("LDAP://", "")}";
            orgUnitDistName = $"LDAP://{orgUnitDistName.Replace("LDAP://", "")}";

            try
            {
                DirectoryEntry userLocation = new DirectoryEntry(userDistName);
                DirectoryEntry ouLocation = new DirectoryEntry(orgUnitDistName);
                userLocation.MoveTo(ouLocation);
                ouLocation.Close();
                userLocation.Close();
            }
            catch (DirectoryServicesCOMException ex)
            {
                Console.WriteLine($"Encountered exception while trying to move user to another organization unit: {ex.Message}");
                return false;
            }

            return true;
        }

        public static void CreateUser(string ouPath, string username, string password, string givenName = "", string surname = "", string description = "", bool isEnabled = true, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(ouPath))
            {
                // Default location where user will be created.
                ouPath = $"cn=Users,{GetDomainDistinguishedName()}";
            }

            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Password is not specified.");
            }

            if (String.IsNullOrWhiteSpace(givenName))
            {
                throw new Exception("Given name is not specified.");
            }

            if (String.IsNullOrWhiteSpace(surname))
            {
                throw new Exception("Surname is not specified.");
            }

            if (!IsExistingUser(username))
            {

                ouPath = ouPath.Replace("LDAP://", "");

                PrincipalContext ouPrincipal = GetPrincipalContext(ouPath);
                try
                {
                    UserPrincipal userPrincipal = new UserPrincipal(ouPrincipal, username, password, isEnabled)
                    {
                        UserPrincipalName = username,
                        GivenName = givenName,
                        Surname = surname,
                        DisplayName = $"{surname}, {givenName}",
                        Description = description
                    };
                    if (!isDryRun)
                    {
                        userPrincipal.Save();
                    }
                }
                catch (PrincipalOperationException ex)
                {
                    if (ex.Message.Contains("There is no such object on the server."))
                    {
                        throw new Exception("OU path specified is not valid.");
                    }
                    throw;
                }
                catch (PasswordException ex)
                {
                    if (ex.Message.Contains("The password does not meet the password policy requirements."))
                    {
                        throw new Exception("The password does not meet the password policy requirements.");
                    }
                    throw;
                }
            }
            else
            {
                throw new Exception("The user already exists.");
            }
        }

        public static void CreateUserEx(string ldapPath, string username, string password, string givenName = "", string surname = "", string description = "")
        {
            if (String.IsNullOrWhiteSpace(ldapPath))
            {
                // Default location where user will be created.
                ldapPath = $"CN=Users,{GetDomainDistinguishedName()}";
            }

            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Cannot create user as username is not specified.");
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Cannot create user as password is not specified.");
            }

            if (String.IsNullOrWhiteSpace(givenName))
            {
                throw new Exception("Cannot create user as given name is not specified.");
            }

            if (String.IsNullOrWhiteSpace(surname))
            {
                throw new Exception("Cannot create user as surname is not specified.");
            }

            try
            {
                string connectionPrefix = "LDAP://" + ldapPath.Replace("LDAP://", "");
                using (DirectoryEntry dirEntry = new DirectoryEntry(connectionPrefix))
                {
                    using (DirectoryEntry newUser = dirEntry.Children.Add("CN=" + username, "user"))
                    {
                        newUser.Properties["samAccountName"].Value = username; // Max length of samAccountName is 20
                        newUser.Properties["givenName"].Value = givenName;
                        newUser.Properties["sn"].Value = surname;
                        newUser.Properties["displayName"].Value = $"{surname}, {givenName}";
                        newUser.Properties["description"].Value = description;
                        newUser.CommitChanges();

                        newUser.Invoke("SetPassword", new object[] { password });
                        //                        newUser.Properties["LockOutTime"].Value = 0; //unlock account
                        newUser.Properties["pwdlastset"].Value = 0; //Force user to change password at next logon
                        newUser.CommitChanges();
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("The password does not meet the password policy requirements."))
                {
                    throw new Exception(ex.InnerException.Message);
                }
                throw;
            }
            catch (DirectoryServicesCOMException ex)
            {
                if (ex.Message.Contains("The object already exists."))
                {
                    throw new Exception($"The user already exists.");
                }
                if (ex.Message.Contains("There is no such object on the server."))
                {
                    throw new Exception("The LDAP path is not valid.");
                }
                throw;
            }
        }

        public static void SetUserPassword(string username, string newPassword, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            if (String.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("New password is not specified.");
            }

            try
            {
                UserPrincipal userPrincipal = GetUser(username);
                if (userPrincipal != null)
                {
                    if (!isDryRun)
                    {
                        userPrincipal.SetPassword(newPassword);
                    }
                }
                else
                {
                    throw new Exception("User cannot be found.");
                }
            }
            catch (PasswordException ex)
            {
                if (ex.Message.Contains("The password does not meet the password policy requirements."))
                {
                    throw new Exception("The password does not meet the password policy requirements.");
                }
                throw;
            }
        }

        public static void ResetPasswordEx(string username, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            if (String.IsNullOrWhiteSpace(newPassword))
            {
                throw new Exception("New password is not specified.");
            }

            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userDn = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

            if (userDn == null)
            {
                throw new Exception("User cannot be found.");
            }
            try
            {
                using (DirectoryEntry user = new DirectoryEntry($"LDAP://{userDn.DistinguishedName}"))
                {
                    user.Invoke("SetPassword", new object[] { newPassword });
                    //                    user.Properties["LockOutTime"].Value = 0; //unlock account
                    user.Properties["pwdlastset"].Value = 0; //Force user to change password at next logon
                    user.CommitChanges();
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("The password does not meet the password policy requirements."))
                {
                    throw new Exception(ex.InnerException.Message);
                }
                throw;
            }
        }

        public static void UnlockUserAccount(string username, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            UserPrincipal userPrincipal = GetUser(username);
            if (userPrincipal != null)
            {
                userPrincipal.UnlockAccount();
                userPrincipal.Save();
            }
            else
            {
                throw new Exception("User cannot be found.");
            }
        }

        public static void UnlockUserEx(string username)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userDn = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

            if (userDn == null)
            {
                throw new Exception("User cannot be found.");
            }

            try
            {
                using (DirectoryEntry uEntry = new DirectoryEntry($"LDAP://{userDn.DistinguishedName}"))
                {
                    uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                    uEntry.CommitChanges(); //may not be needed but adding it anyways
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool IsUserLocked(string username)
        {
            bool isLocked = false;

            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not specified.");
            }

            PrincipalContext context = new PrincipalContext(ContextType.Domain);
            UserPrincipal userDn = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

            if (userDn == null)
            {
                throw new Exception("User cannot be found.");
            }

            try
            {
                using (DirectoryEntry uEntry = new DirectoryEntry($"LDAP://{userDn.DistinguishedName}"))
                {
                    isLocked = Convert.ToBoolean(uEntry.InvokeGet("IsAccountLocked"));
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                throw new Exception(ex.Message);
            }

            return isLocked;
        }

        public static string GenerateToken(Byte length)
        {
            var bytes = new byte[length];
            var rnd = new Random();
            rnd.NextBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("=", "").Replace("+", "").Replace("/", "");
        }

        public static bool DeleteUser(string userName)
        {
            bool status = false;

            if (String.IsNullOrEmpty(userName) || String.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("No username is provided.");
                return status;
            }

            // find the user you want to delete
            try
            {
                // set up domain context
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);
                user?.Delete();
                status = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encountered exception while trying to delete user: {ex.Message}");
            }

            return status;
        }

        public static void EnableUserAccount(string username, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not provided.");
            }

            UserPrincipal userPrincipal = GetUser(username);
            if (userPrincipal != null)
            {
                if (!isDryRun)
                {
                    userPrincipal.Enabled = true;
                    userPrincipal.Save();
                }
            }
            else
            {
                throw new Exception("User cannot be found.");
            }
        }

        public static void ExpireUserPassword(string username, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not provided.");
            }

            UserPrincipal userPrincipal = GetUser(username);
            if (userPrincipal != null)
            {
                if (!isDryRun)
                {
                    userPrincipal.ExpirePasswordNow();
                    userPrincipal.Save();
                }
            }
            else
            {
                throw new Exception("User cannot be found.");
            }
        }

        public static void DisableUserAccount(string username, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is not provided.");
            }


            UserPrincipal userPrincipal = GetUser(username);
            if (userPrincipal != null)
            {
                userPrincipal.Enabled = false;
                userPrincipal.Save();
            }
            else
            {
                throw new Exception("User cannot be found.");
            }
        }


        public static void UpdateUserAttribute(string username, string attribute, string value, string ldapPath = "", bool dryRun = false)
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("No username is specified.");
            }

            if (!IsValidUserAttribute(attribute))
            {
                throw new Exception("The attribute specified is not valid.");
            }

            ldapPath = String.IsNullOrWhiteSpace(ldapPath) ? $"LDAP://{GetDomainDistinguishedName()}" : $"LDAP://{ldapPath.Replace("LDAP://", "")}";

            using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry) { Filter = "(sAMAccountName=" + username + ")" })
                {
                    try
                    {
                        mySearcher.PropertiesToLoad.Add("" + attribute + "");
                        SearchResult result = mySearcher.FindOne();
                        if (result != null)
                        {
                            if (!dryRun)
                            {
                                DirectoryEntry entryToUpdate = result.GetDirectoryEntry();
                                if (result.Properties.Contains("" + attribute + ""))
                                {
                                    if (!String.IsNullOrWhiteSpace(value))
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Value = value;
                                    }
                                    else
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Clear();
                                    }
                                }
                                else
                                {
                                    entryToUpdate.Properties["" + attribute + ""].Add(value);
                                }
                                entryToUpdate.CommitChanges();
                            }
                        }
                        else
                        {
                            throw new Exception("User cannot be found.");
                        }
                    }
                    catch (DirectoryServicesCOMException ex)
                    {
                        if (ex.Message.Contains("The attribute syntax specified to the directory service is invalid."))
                        {
                            throw new Exception("The attribute value is invalid.");
                        }
                        if (ex.Message.Contains("A constraint violation occurred."))
                        {
                            throw new Exception("The attribute value is invalid.");
                        }
                        throw;
                    }
                    catch (COMException ex)
                    {
                        if (ex.Message.Contains("The server is not operational."))
                        {
                            throw new Exception("LDAP path specified is not valid.");
                        }
                        throw;
                    }
                }
            };
        }

        public static bool IsValidUserAttribute(string attribute)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>()
            {
                { "company", "Company" },
                { "department", "Department" },
                { "displayName", "Display Name" },
                { "description", "Description" },
                { "employeeID", "Employee ID" },
                { "givenName", "Given Name" },
                { "initials", "Initials" },
                { "mail", "E-mail" },
                { "mobile", "Mobile" },
                { "postalCode", "Postal Code" },
                { "sn", "Surname" },
                { "streetAddress", "Street Address" },
                { "title", "Title" },
            };

            return attributes.ContainsKey(attribute);
        }

        public static bool IsExistingUser(string username)
        {
            if (GetUser(username) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool? IsUserEnabled(string username)
        {
            UserPrincipal userPrincipal = GetUser(username);
            if (userPrincipal == null)
            {
                throw new Exception("User cannot be found.");
            }
            return userPrincipal.Enabled;
        }
    }
}