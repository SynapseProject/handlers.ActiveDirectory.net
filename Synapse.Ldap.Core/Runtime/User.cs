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
            using( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                UserPrincipal user = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, sAMAccountName );
                u = new UserPrincipalObject( user );
                if( getGroups )
                    u.GetGroups();
            }
            return u;
        }
    }
}