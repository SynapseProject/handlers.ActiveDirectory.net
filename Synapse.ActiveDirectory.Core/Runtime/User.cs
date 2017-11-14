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

        public static bool IsExistingUser(string identity)
        {
            return GetUserPrincipal( identity ) != null;
        }
    }
}