using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;


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

        public static string CreateUserAccount(string ldapPath, string userName, string userPassword)
        {
            string oGUID = string.Empty;

            if (String.IsNullOrEmpty(ldapPath) || String.IsNullOrWhiteSpace(ldapPath))
            {
                ldapPath = "RootDSE";
            }

            if (String.IsNullOrEmpty(userName) || String.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("No username is specified.");
                return oGUID;
            }


            if (String.IsNullOrEmpty(userPassword) || String.IsNullOrWhiteSpace(userPassword))
            {
                Console.WriteLine("No password is specified.");
                return oGUID;
            }

            try
            {
                string connectionPrefix = "LDAP://" + ldapPath;
                DirectoryEntry dirEntry = new DirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = dirEntry.Children.Add("CN=" + userName, "user");
                newUser.Properties["samAccountName"].Value = userName; // Max length of samAccountName is 20
                newUser.CommitChanges();
                oGUID = newUser.Guid.ToString();

                newUser.Invoke("SetPassword", new object[] { userPassword });
                newUser.CommitChanges();
                dirEntry.Close();
                newUser.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException ex)
            {
                Console.WriteLine($"Encountered exception while trying to add user: {ex.Message}");
            }
            return oGUID;
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

        public static bool UpdateUserInfo(string objectFilter, Property objectName, string objectValue, string ldapDomain = "")
        {
            bool status = false;

            if (String.IsNullOrEmpty(objectFilter) || String.IsNullOrWhiteSpace(objectFilter))
            {
                Console.WriteLine("No object filter is specified.");
                return status;
            }

            if (String.IsNullOrEmpty(objectValue) || String.IsNullOrWhiteSpace(objectValue))
            {
                Console.WriteLine("No object value is specified.");
                return status;
            }

            ldapDomain = ldapDomain.Replace("LDAP://", "");
            string connectionPrefix = "LDAP://" + ldapDomain;
            using (DirectoryEntry entry = new DirectoryEntry(connectionPrefix))
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry) { Filter = "(cn=" + objectFilter + ")" })
                {
                    mySearcher.PropertiesToLoad.Add("" + objectName + "");
                    SearchResult result = mySearcher.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry entryToUpdate = result.GetDirectoryEntry();
                        if (!(String.IsNullOrEmpty(objectValue)))
                        {
                            if (result.Properties.Contains("" + objectName + ""))
                            {
                                entryToUpdate.Properties["" + objectName + ""].Value = objectValue;
                            }
                            else
                            {
                                entryToUpdate.Properties["" + objectName + ""].Add(objectValue);
                            }
                            entryToUpdate.CommitChanges();
                            status = true;
                        }
                    }
                }
            };
            return status;
        }

    }
}