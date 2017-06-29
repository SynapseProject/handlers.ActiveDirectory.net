using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static UserPrincipalObject GetUser(string name, bool getGroups)
        {
            String sAMAccountName = GetSamAccountName( name );

            UserPrincipalObject u = null;
            using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                UserPrincipal user = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, sAMAccountName );
                if ( user == null )
                    throw new LdapException( $"User [{sAMAccountName}] Not Found.", LdapStatusType.DoesNotExist );

                u = new UserPrincipalObject( user );
                if ( getGroups )
                    u.GetGroups();
            }
            return u;
        }

        public static void CreateUser(string distinguishedName, string password, string givenName, string surname, string description, bool isEnabled = true, bool isDryRun = false)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                String username = match.Groups[1]?.Value?.Trim();
                String parentPath = match.Groups[2]?.Value?.Trim();
                CreateUser( username, parentPath, password, givenName, surname, description, isEnabled, isDryRun );
            }
        }

        public static void CreateUser(string username, string ouPath, string password, string givenName, string surname, string description, bool isEnabled = true, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                // Default location where user will be created.
                ouPath = $"cn=Users,{GetDomainDistinguishedName()}";
            }

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( password ) )
            {
                throw new LdapException( "Password is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( givenName ) )
            {
                throw new LdapException( "Given name is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( surname ) )
            {
                throw new LdapException( "Surname is not specified.", LdapStatusType.MissingInput );
            }

            if ( !IsExistingUser( username ) )
            {

                ouPath = ouPath.Replace( "LDAP://", "" );

                PrincipalContext ouPrincipal = GetPrincipalContext( ouPath );
                try
                {
                    UserPrincipal userPrincipal = new UserPrincipal( ouPrincipal, username, password, isEnabled )
                    {
                        UserPrincipalName = username,
                        GivenName = givenName,
                        Surname = surname,
                        DisplayName = $"{surname}, {givenName}",
                        Description = description
                    };
                    if ( !isDryRun )
                    {
                        userPrincipal.Save();
                    }
                }
                catch ( PrincipalOperationException ex )
                {
                    if ( ex.Message.Contains( "There is no such object on the server." ) )
                    {
                        throw new LdapException( "OU path specified is not valid.", LdapStatusType.InvalidPath );
                    }
                    throw;
                }
                catch ( PasswordException ex )
                {
                    if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                    {
                        throw new LdapException( "The password does not meet the password policy requirements.", LdapStatusType.PasswordPolicyNotMet );
                    }
                    throw;
                }
            }
            else
            {
                throw new LdapException( "The user already exists.", LdapStatusType.AlreadyExists );
            }
        }

        public static void CreateUserEx(string ldapPath, string username, string password, string givenName = "", string surname = "", string description = "")
        {
            if ( String.IsNullOrWhiteSpace( ldapPath ) )
            {
                // Default location where user will be created.
                ldapPath = $"CN=Users,{GetDomainDistinguishedName()}";
            }

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Cannot create user as username is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( password ) )
            {
                throw new LdapException( "Cannot create user as password is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( givenName ) )
            {
                throw new LdapException( "Cannot create user as given name is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( surname ) )
            {
                throw new LdapException( "Cannot create user as surname is not specified.", LdapStatusType.MissingInput );
            }

            try
            {
                string connectionPrefix = "LDAP://" + ldapPath.Replace( "LDAP://", "" );
                using ( DirectoryEntry dirEntry = new DirectoryEntry( connectionPrefix ) )
                {
                    using ( DirectoryEntry newUser = dirEntry.Children.Add( "CN=" + username, "user" ) )
                    {
                        newUser.Properties["samAccountName"].Value = username; // Max length of samAccountName is 20
                        newUser.Properties["givenName"].Value = givenName;
                        newUser.Properties["sn"].Value = surname;
                        newUser.Properties["displayName"].Value = $"{surname}, {givenName}";
                        newUser.Properties["description"].Value = description;
                        newUser.CommitChanges();

                        newUser.Invoke( "SetPassword", new object[] { password } );
                        //                        newUser.Properties["LockOutTime"].Value = 0; //unlock account
                        newUser.Properties["pwdlastset"].Value = 0; //Force user to change password at next logon
                        newUser.CommitChanges();
                    }
                }
            }
            catch ( TargetInvocationException ex )
            {
                if ( ex.InnerException != null && ex.InnerException.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new LdapException( ex.InnerException.Message, LdapStatusType.PasswordPolicyNotMet );
                }
                throw;
            }
            catch ( DirectoryServicesCOMException ex )
            {
                if ( ex.Message.Contains( "The object already exists." ) )
                {
                    throw new LdapException( $"The user already exists.", LdapStatusType.AlreadyExists );
                }
                if ( ex.Message.Contains( "There is no such object on the server." ) )
                {
                    throw new LdapException( "The LDAP path is not valid.", LdapStatusType.InvalidPath );
                }
                throw;
            }
        }

        public static void SetUserPassword(string username, string newPassword, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( newPassword ) )
            {
                throw new LdapException( "New password is not specified.", LdapStatusType.MissingInput );
            }

            try
            {
                UserPrincipal userPrincipal = GetUser( username );
                if ( userPrincipal != null )
                {
                    if ( !isDryRun )
                    {
                        userPrincipal.SetPassword( newPassword );
                    }
                }
                else
                {
                    throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
                }
            }
            catch ( PasswordException ex )
            {
                if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new LdapException( "The password does not meet the password policy requirements.", LdapStatusType.PasswordPolicyNotMet );
                }
                throw;
            }
        }

        public static void ResetPasswordEx(string username, string newPassword)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( newPassword ) )
            {
                throw new LdapException( "New password is not specified.", LdapStatusType.MissingInput );
            }

            PrincipalContext context = new PrincipalContext( ContextType.Domain );
            UserPrincipal userDn = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username );

            if ( userDn == null )
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
            try
            {
                using ( DirectoryEntry user = new DirectoryEntry( $"LDAP://{userDn.DistinguishedName}" ) )
                {
                    user.Invoke( "SetPassword", new object[] { newPassword } );
                    //                    user.Properties["LockOutTime"].Value = 0; //unlock account
                    user.Properties["pwdlastset"].Value = 0; //Force user to change password at next logon
                    user.CommitChanges();
                }
            }
            catch ( TargetInvocationException ex )
            {
                if ( ex.InnerException != null && ex.InnerException.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new LdapException( ex.InnerException.Message, LdapStatusType.PasswordPolicyNotMet );
                }
                throw;
            }
        }

        public static void UnlockUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal != null )
            {
                userPrincipal.UnlockAccount();
                userPrincipal.Save();
            }
            else
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
        }

        public static void UnlockUserEx(string username)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            PrincipalContext context = new PrincipalContext( ContextType.Domain );
            UserPrincipal userDn = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username );

            if ( userDn == null )
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }

            try
            {
                using ( DirectoryEntry uEntry = new DirectoryEntry( $"LDAP://{userDn.DistinguishedName}" ) )
                {
                    uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                    uEntry.CommitChanges(); //may not be needed but adding it anyways
                }
            }
            catch ( DirectoryServicesCOMException ex )
            {
                throw ex;
            }
        }

        public static bool IsUserLocked(string username)
        {
            bool isLocked = false;

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            PrincipalContext context = new PrincipalContext( ContextType.Domain );
            UserPrincipal userDn = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username );

            if ( userDn == null )
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }

            try
            {
                using ( DirectoryEntry uEntry = new DirectoryEntry( $"LDAP://{userDn.DistinguishedName}" ) )
                {
                    isLocked = Convert.ToBoolean( uEntry.InvokeGet( "IsAccountLocked" ) );
                }
            }
            catch ( DirectoryServicesCOMException ex )
            {
                throw ex;
            }

            return isLocked;
        }

        public static string GenerateToken(Byte length)
        {
            var bytes = new byte[length];
            var rnd = new Random();
            rnd.NextBytes( bytes );
            return Convert.ToBase64String( bytes ).Replace( "=", "" ).Replace( "+", "" ).Replace( "/", "" );
        }


        public static void DeleteUser(string name, bool isDryRun = false)
        {
            String username = GetSamAccountName( name );

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal != null )
            {
                if ( !isDryRun )
                {
                    userPrincipal.Delete();
                }
            }
            else
            {
                throw new LdapException( $"User [{name}]cannot be found.", LdapStatusType.DoesNotExist );
            }
        }

        public static void EnableUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal != null )
            {
                if ( !isDryRun )
                {
                    userPrincipal.Enabled = true;
                    userPrincipal.Save();
                }
            }
            else
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
        }

        public static void ExpireUserPassword(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal != null )
            {
                if ( !isDryRun )
                {
                    userPrincipal.ExpirePasswordNow();
                    userPrincipal.Save();
                }
            }
            else
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
        }

        public static void DisableUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapStatusType.MissingInput );
            }


            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal != null )
            {
                userPrincipal.Enabled = false;
                userPrincipal.Save();
            }
            else
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
        }


        public static void UpdateUserAttribute(string username, string attribute, string value, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not specified.", LdapStatusType.MissingInput );
            }

            if ( !IsValidUserAttribute( attribute ) )
            {
                throw new LdapException( "Attribute is not supported.", LdapStatusType.NotSupported );
            }

            string ldapPath = $"LDAP://{GetDomainDistinguishedName()}";

            using ( DirectoryEntry entry = new DirectoryEntry( ldapPath ) )
            {
                using ( DirectorySearcher mySearcher = new DirectorySearcher( entry )
                {
                    Filter = "(sAMAccountName=" + username + ")"
                } )
                {
                    try
                    {
                        mySearcher.PropertiesToLoad.Add( "" + attribute + "" );
                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            if ( !dryRun )
                            {
                                DirectoryEntry entryToUpdate = result.GetDirectoryEntry();
                                if ( result.Properties.Contains( "" + attribute + "" ) )
                                {
                                    if ( !String.IsNullOrWhiteSpace( value ) )
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
                                    entryToUpdate.Properties["" + attribute + ""].Add( value );
                                }
                                entryToUpdate.CommitChanges();
                            }
                        }
                        else
                        {
                            throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
                        }
                    }
                    catch ( DirectoryServicesCOMException ex )
                    {
                        if ( ex.Message.Contains( "The attribute syntax specified to the directory service is invalid." ) )
                        {
                            throw new LdapException( "The attribute value is invalid.", LdapStatusType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "A constraint violation occurred." ) )
                        {
                            throw new LdapException( "The attribute value is invalid.", LdapStatusType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "The directory service cannot perform the requested operation on the RDN attribute of an object." ) )
                        {
                            throw new LdapException( "Operation is not allowed.", LdapStatusType.NotAllowed );
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
                { "department", "Department" },
                { "displayName", "Display Name" },
                { "description", "Description" },
                { "employeeID", "Employee ID" },
                { "givenName", "Given Name" },
                { "mail", "Email Address" },
                { "middleName", "Middle Name" },
                { "mobile", "Mobile" },
                { "postalCode", "Postal Code" },
                { "sAMAccountName", "Sam Account Name" }, // e.g. johndoe
                { "sn", "Surname" },
                { "streetAddress", "Street Address" },
                { "telephoneNumber", "Voice Telephone Number"}, // e.g. johndoe@xxx.com
                { "userPrincipalName", "User Principal Name"} // e.g. johndoe@xxx.com

            };

            return attributes.ContainsKey( attribute );
        }

        public static bool IsExistingUser(string username)
        {
            return GetUser( username ) != null;
        }

        public static bool? IsUserEnabled(string username)
        {
            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal == null )
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }
            return userPrincipal.Enabled;
        }

        private static string GetSamAccountName(String distinguishedName)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
                return match.Groups[1]?.Value?.Trim();
            else
                return distinguishedName;
        }

            #region To Be Deleted
            public static bool DeleteUserEx(string userName)
        {
            bool status = false;

            if ( String.IsNullOrEmpty( userName ) || String.IsNullOrWhiteSpace( userName ) )
            {
                Console.WriteLine( "No username is provided." );
                return status;
            }

            // find the user you want to delete
            try
            {
                // set up domain context
                PrincipalContext ctx = new PrincipalContext( ContextType.Domain );
                UserPrincipal user = UserPrincipal.FindByIdentity( ctx, userName );
                user?.Delete();
                status = true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Encountered exception while trying to delete user: {ex.Message}" );
            }

            return status;
        }

        #endregion
    }
}