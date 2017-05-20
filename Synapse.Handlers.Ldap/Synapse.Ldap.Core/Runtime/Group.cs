using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static GroupPrincipalObject GetGroup(string sAMAccountName, bool getGroups)
        {
            GroupPrincipalObject g = null;
            using( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, sAMAccountName );
                g = new GroupPrincipalObject( group );
                if( getGroups )
                    g.GetGroups();
            }
            return g;
        }
    }
}