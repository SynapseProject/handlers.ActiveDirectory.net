using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Synapse.Ldap.Tests
{
    public class Utility
    {
        public static Random random = new Random();

        public static string GenerateToken(byte length)
        {
            var bytes = new byte[length];
            random.NextBytes( bytes );
            return Convert.ToBase64String( bytes ).Replace( "=", "e" ).Replace( "+", "p" ).Replace( "/", "s" );
        }

        public static string GetGroupOrganizationUnit(string groupName)
        {
            try
            {
                using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                {
                    using ( GroupPrincipal user = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, groupName ) )
                    {
                        if ( user != null )
                        {
                            using ( DirectoryEntry deGroup = user.GetUnderlyingObject() as DirectoryEntry )
                            {
                                if ( deGroup != null )
                                {
                                    using ( DirectoryEntry deGroupContainer = deGroup.Parent )
                                    {
                                        return deGroupContainer.Properties["Name"].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public static string GetUserOrganizationUnit(string username)
        {
            try
            {
                using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                {
                    using ( UserPrincipal user = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username ) )
                    {
                        if ( user != null )
                        {
                            using ( DirectoryEntry deUser = user.GetUnderlyingObject() as DirectoryEntry )
                            {
                                if ( deUser != null )
                                {
                                    using ( DirectoryEntry deUserContainer = deUser.Parent )
                                    {
                                        return deUserContainer.Properties["Name"].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

    }
}
