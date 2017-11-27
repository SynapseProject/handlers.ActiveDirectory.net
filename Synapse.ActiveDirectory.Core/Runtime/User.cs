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
            try
            {
                UserPrincipal user = GetUserPrincipal( identity );

                if ( user != null )
                {
                    u = new UserPrincipalObject( user, getAccessRules, getObjectProperties );
                    if ( getGroups )
                        u.GetGroups();
                }
            }
            catch ( MultipleMatchesException mme )
            {
                throw new AdException( $"Multiple Users Contain The Identity [{identity}].", mme, AdStatusType.MultipleMatches );
            }

            return u;
        }

        public static UserPrincipal GetUserPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity( principalContext, identity );
            return userPrincipal;
        }

        public static UserPrincipal CreateUserPrincipal(string distinguishedName, string userPrincipalName = null, string samAccountName = null, bool saveOnCreate = true)
        {
            String name = distinguishedName;
            String path = DirectoryServices.GetDomainDistinguishedName();
            String domain = DirectoryServices.GetDomain( path );

            if ( DirectoryServices.IsDistinguishedName( distinguishedName ) )
            {
                Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
                Match match = regex.Match( distinguishedName );
                if ( match.Success )
                {
                    name = match.Groups[1]?.Value?.Trim();
                    path = match.Groups[2]?.Value?.Trim();
                }
                domain = DirectoryServices.GetDomain( distinguishedName );
            }
            else if ( String.IsNullOrWhiteSpace( distinguishedName ) )
                throw new AdException( "Unable To Create User Principal From Given Input.", AdStatusType.MissingInput );

            path = path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( path );
            UserPrincipal user = new UserPrincipal( context )
            {
                Name = name,
                UserPrincipalName = userPrincipalName ?? $"{name}@{domain}"
            };

            if ( samAccountName != null )
            {
                if ( samAccountName.Length < 20 )
                    user.SamAccountName = samAccountName;
                else
                    throw new AdException( $"SamAccountName [{samAccountName}] Is Longer than 20 Characters.", AdStatusType.InvalidAttribute );
            }
            else if ( name.Length < 20 )
                user.SamAccountName = name;

            if ( saveOnCreate )
                SaveUser( user );

            return user;
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
                else if (ex.Message.Contains("A device attached to the system is not functioning."))
                {
                    throw new AdException( "Invalid Input.  Check SamAccountName length (max length = 20) as this might be the cause.", ex, AdStatusType.InvalidInput );
                }
                else
                    throw new AdException( ex, AdStatusType.Unknown );
            }
            catch ( PasswordException ex )
            {
                if ( ex.Message.Contains( "The password does not meet the password policy requirements." ) )
                {
                    throw new AdException( "The password does not meet the password policy requirements.", AdStatusType.PasswordPolicyNotMet );
                }
                throw;
            }
            catch ( InvalidOperationException ioe )
            {
                throw new AdException( ioe, AdStatusType.InvalidInput );
            }

        }

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
                throw new AdException( $"User [{identity}] cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static bool IsExistingUser(string identity)
        {
            return GetUserPrincipal( identity ) != null;
        }
    }
}