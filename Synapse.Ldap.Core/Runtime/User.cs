using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;


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

        public static void CreateUser(string ldapPath, string username, string password, string givenName = "", string surname = "", string description = "")
        {
            if (String.IsNullOrWhiteSpace(ldapPath))
            {
                // Default location where user will be created.
                ldapPath = $"cn=Users,{GetDomainName()}";
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

                        newUser.Invoke("SetPassword", new object[] {password});
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

        public static void ResetPassword(string username, string newPassword)
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
                    user.Invoke("SetPassword", new object[] {newPassword});
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

        public static void UnlockUser(string username)
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

        public static bool AddUserToGroup(string userDn, string groupDn)
        {
            bool status = false;

            if (String.IsNullOrEmpty(userDn) || String.IsNullOrWhiteSpace(userDn))
            {
                Console.WriteLine("No user distinguished name is provided.");
                return status;
            }

            if (String.IsNullOrEmpty(groupDn) || String.IsNullOrWhiteSpace(groupDn))
            {
                Console.WriteLine("No group distinguished name is specified.");
                return status;
            }

            groupDn = groupDn.Replace("LDAP://", "");

            try
            {
                using (DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn))
                {
                    dirEntry.Properties["member"].Add(userDn);
                    dirEntry.CommitChanges();
                    status = true;
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                Console.WriteLine($"Encountered error while trying to add user to group: {ex.Message}");
            }

            return status;
        }

        public static bool RemoveUserFromGroup(string userDn, string groupDn)
        {
            bool status = false;

            if (String.IsNullOrEmpty(userDn) || String.IsNullOrWhiteSpace(userDn))
            {
                Console.WriteLine("No user distinguished name is provided.");
                return status;
            }

            if (String.IsNullOrEmpty(groupDn) || String.IsNullOrWhiteSpace(groupDn))
            {
                Console.WriteLine("No group distinguished name is specified.");
                return status;
            }

            groupDn = groupDn.Replace("LDAP://", "");
            try
            {
                using (DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn))
                {
                    dirEntry.Properties["member"].Remove(userDn);
                    dirEntry.CommitChanges();
                    status = true;
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                Console.WriteLine($"Encountered error while trying to remove user from group: {ex.Message}");
            }

            return status;
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

        public enum Property
        {
            title, displayName, sn, l, postalCode, physicalDeliveryOfficeName, telephoneNumber,
            mail, givenName, initials, co, department, company,
            streetAddress, employeeID, mobile, userPrincipalName, description
        }

        public static void UpdateUserAttribute(string username, string attribute, string value, string ldapDomain = "")
        {
            if (String.IsNullOrWhiteSpace(username))
            {
                throw new Exception("No username is specified.");
            }

            if (!IsValidUserAttribute(attribute))
            {
                Console.WriteLine("Invalid attribute is specified.");
            }

            if (String.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("No attribute value is specified.");
            }

            ldapDomain = ldapDomain.Replace("LDAP://", "");
            string connectionPrefix = "LDAP://" + ldapDomain;
            using (DirectoryEntry entry = new DirectoryEntry(connectionPrefix))
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry) { Filter = "(cn=" + username + ")" })
                {
                    mySearcher.PropertiesToLoad.Add("" + attribute + "");
                    SearchResult result = mySearcher.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry entryToUpdate = result.GetDirectoryEntry();
                        if (!(String.IsNullOrEmpty(value)))
                        {
                            if (result.Properties.Contains("" + attribute + ""))
                            {
                                entryToUpdate.Properties["" + attribute + ""].Value = value;
                            }
                            else
                            {
                                entryToUpdate.Properties["" + attribute + ""].Add(value);
                            }
                            entryToUpdate.CommitChanges();
                        }
                    }
                }
            };
        }

        public static bool IsValidUserAttribute(string attribute)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>()
            {
                {"title", "Title" },
                {"displayName", "Display Name" },
                {"sn", "Surname" },
                {"postalCode", "Postal Code" },
                {"givenName", "Given Name" },
                {"initials", "Initials" },
                {"company", "Company" },
                {"department", "Department" },
                {"streetAddress", "Street Address" },
                {"mobile", "Mobile" },
                {"userPrincipalName",  "User Principal Name" },
                {"description", "Description" },
                {"employeeID", "Employee ID" }
            };

            return attributes.ContainsKey(attribute);
        }
    }
}