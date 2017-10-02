using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;


namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
        public static UserPrincipalObject GetUser(string identity, bool getGroups, bool getAccessRules, bool getObjectProperties)
        {
            UserPrincipalObject u = null;
            using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                try
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity( context, identity );
                    if ( user == null )
                        throw new AdException( $"User [{identity}] Not Found.", AdStatusType.DoesNotExist );

                    u = new UserPrincipalObject( user, getAccessRules, getObjectProperties );
                    if ( getGroups )
                        u.GetGroups();
                }
                catch (MultipleMatchesException mme)
                {
                    throw new AdException( $"Multiple Users Contain The Identity [{identity}].", mme, AdStatusType.MultipleMatches );
                }
            }
            return u;
        }

        public static void SaveUser( UserPrincipal user, bool isDryRun = false )
        {
            try
            {
                user.Save();
            }
            catch ( PrincipalOperationException ex )
            {
                if ( ex.Message.Contains( "There is no such object on the server." ) )
                {
                    throw new AdException( "OU path specified is not valid.", AdStatusType.InvalidPath );
                }
                throw;
            }
            catch ( PasswordException ex )
            {
                if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                }
                throw;
            }

        }

        // TODO : Delete Me.  Only Used In Tests
        public static void CreateUser(string username, string ouPath, string password, string givenName, string surname, string description, bool isEnabled = true, bool isDryRun = false )
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                // Default location where user will be created.
                ouPath = $"cn=Users,{GetDomainDistinguishedName()}";
            }

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( password ) )
            {
                throw new AdException( "Password is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( givenName ) )
            {
                throw new AdException( "Given name is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( surname ) )
            {
                throw new AdException( "Surname is not specified.", AdStatusType.MissingInput );
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
                        throw new AdException( "OU path specified is not valid.", AdStatusType.InvalidPath );
                    }
                    throw;
                }
                catch ( PasswordException ex )
                {
                    if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                    {
                        throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                    }
                    throw;
                }
            }
            else
            {
                throw new AdException( "The user already exists.", AdStatusType.AlreadyExists );
            }
        }

/*
        public static void ModifyUser( UserPrincipal user, bool isDryRun = false )
        {
            try
            {
                if ( user == null )
                    throw new AdException( $"User [{user.Name}] Not Found.", AdStatusType.DoesNotExist );
                if ( !isDryRun )
                {
                    user.Save();
                }
            }
            catch ( PrincipalOperationException ex )
            {
                if ( ex.Message.Contains( "There is no such object on the server." ) )
                {
                    throw new AdException( "OU path specified is not valid.", AdStatusType.InvalidPath );
                }
                throw;
            }
            catch ( PasswordException ex )
            {
                if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                }
                throw;
            }

        }
*/
        // TODO : Delete Me.  Only used in Create Method marked for deletion.
        public static void ModifyUser(string username, string ouPath, string password, string givenName, string surname, string description, bool isEnabled = true, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( password ) )
            {
                throw new AdException( "Password is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( givenName ) )
            {
                throw new AdException( "Given name is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( surname ) )
            {
                throw new AdException( "Surname is not specified.", AdStatusType.MissingInput );
            }

            if ( IsExistingUser( username ) )
            {
                try
                {
                    String commonName = GetCommonName( username );

                    using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                    {
                        UserPrincipal user = UserPrincipal.FindByIdentity( context, commonName );
                        if ( user == null )
                            throw new AdException( $"User [{commonName}] Not Found.", AdStatusType.DoesNotExist );

                        user.GivenName = givenName;
                        user.Surname = surname;
                        user.Description = description;
                        user.SetPassword( password );

                        if ( !isDryRun )
                        {
                            user.Save();
                        }
                    }
                }
                catch ( PrincipalOperationException ex )
                {
                    if ( ex.Message.Contains( "There is no such object on the server." ) )
                    {
                        throw new AdException( "OU path specified is not valid.", AdStatusType.InvalidPath );
                    }
                    throw;
                }
                catch ( PasswordException ex )
                {
                    if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                    {
                        throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                    }
                    throw;
                }
            }
            else
            {
                throw new AdException( "The user does not exist.", AdStatusType.DoesNotExist );
            }
        }

        public static void SetUserPassword(string username, string newPassword, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( newPassword ) )
            {
                throw new AdException( "New password is not specified.", AdStatusType.MissingInput );
            }

            try
            {
                UserPrincipal userPrincipal = GetUserPrincipal( username );
                if ( userPrincipal != null )
                {
                    if ( !isDryRun )
                    {
                        userPrincipal.SetPassword( newPassword );
                    }
                }
                else
                {
                    throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
                }
            }
            catch ( PasswordException ex )
            {
                if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                }
                throw;
            }
        }

        public static void UnlockUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( username );
            if ( userPrincipal != null )
            {
                userPrincipal.UnlockAccount();
                userPrincipal.Save();
            }
            else
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
        }

/*
        public static bool IsUserLocked(string username)
        {
            bool isLocked = false;

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            PrincipalContext context = new PrincipalContext( ContextType.Domain );
            UserPrincipal userDn = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username );

            if ( userDn == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
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
*/
        public static void DeleteUser(string identity, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
            {
                throw new AdException( "Identity is not specified.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( identity );
            if ( userPrincipal != null )
            {
                if ( !isDryRun )
                {
                    userPrincipal.Delete();
                }
            }
            else
            {
                throw new AdException( $"User [{identity}]cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static void EnableUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( username );
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
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static void ExpireUserPassword(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( username );
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
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static void DisableUserAccount(string username, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not provided.", AdStatusType.MissingInput );
            }


            UserPrincipal userPrincipal = GetUserPrincipal( username );
            if ( userPrincipal != null )
            {
                userPrincipal.Enabled = false;
                userPrincipal.Save();
            }
            else
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static void UpdateUserAttribute(string username, string attribute, string value, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not specified.", AdStatusType.MissingInput );
            }

            if ( !IsValidUserAttribute( attribute ) )
            {
                throw new AdException( "Attribute is not supported.", AdStatusType.NotSupported );
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
                            throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
                        }
                    }
                    catch ( DirectoryServicesCOMException ex )
                    {
                        if ( ex.Message.Contains( "The attribute syntax specified to the directory service is invalid." ) )
                        {
                            throw new AdException( "The attribute value is invalid.", AdStatusType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "A constraint violation occurred." ) )
                        {
                            throw new AdException( "The attribute value is invalid.", AdStatusType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "The directory service cannot perform the requested operation on the RDN attribute of an object." ) )
                        {
                            throw new AdException( "Operation is not allowed.", AdStatusType.NotAllowed );
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

        public static bool IsExistingUser(string identity)
        {
            return GetUserPrincipal( identity ) != null;
        }

        public static bool? IsUserEnabled(string username)
        {
            UserPrincipal userPrincipal = GetUserPrincipal( username );
            if ( userPrincipal == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
            return userPrincipal.Enabled;
        }
    }
}