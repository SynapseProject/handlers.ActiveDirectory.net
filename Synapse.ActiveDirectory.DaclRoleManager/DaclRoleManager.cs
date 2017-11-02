using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.RoleManager
{
    public class DaclRoleManager : IRoleManager
    {
        Dictionary<string, DaclRole> Roles = new Dictionary<string, DaclRole>();
        Dictionary<ActionType, List<string>> Actions = new Dictionary<ActionType, List<string>>();

        public DaclRoleManager()
        {
            DaclRoleManagerConfig config = DaclRoleManagerConfig.Load( @"C:\Source\handlers.ActiveDirectory.net\Synapse.ActiveDirectory.DaclRoleManager\bin\Debug\RoleManagerConfig.yaml" );
            foreach (DaclRole role in config.Roles)
            {
                Roles.Add( role.Name, role );
                foreach ( ActionType action in role.AllowedActions )
                {
                    if ( Actions.ContainsKey( action ) )
                    {
                        Actions[action].Add( role.Name );
                    }
                    else
                    {
                        List<string> newList = new List<string>();
                        newList.Add( role.Name );
                        Actions.Add( action, newList );
                    }
                }
            }
        }

        #region Role Execution

        public bool CanPerformAction(string principal, ActionType action, string adObject)
        {
            throw new NotImplementedException();
        }

        public void CanPerformActionOrException(string principal, ActionType action, string adObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Role Administration

        public void AddRole(string principal, string role, string adObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetRoles()
        {
            return Roles.Keys.ToList<string>();
        }

        public bool HasRole(string principal, string role, string adObject)
        {
            ActiveDirectoryRights rights = GetAdAccessRights( principal, adObject );

            Console.WriteLine( $">> Effective Rights : {rights}" );

            return true;
        }

        public void RemoveRole(string principal, string role, string adObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        private ActiveDirectoryRights GetAdAccessRights(string principal, string adObject)
        {
            ActiveDirectoryRights myRights = 0;
            Principal p = DirectoryServices.GetPrincipal( principal );
            List<DirectoryEntry> groups = DirectoryServices.GetMembership( p, true );

            DirectoryEntry de = DirectoryServices.GetDirectoryEntry( adObject );
            List<AccessRuleObject> rules = DirectoryServices.GetAccessRules( de );

            Dictionary<string, ActiveDirectoryRights> rights = new Dictionary<string, ActiveDirectoryRights>();

            foreach (AccessRuleObject rule in rules)
            {
                if ( rights.Keys.Contains( rule.IdentityReference ) )
                {
                    rights[rule.IdentityReference] |= rule.Rights;
                }
                else
                    rights.Add( rule.IdentityReference, rule.Rights );
            }

            Console.WriteLine( "======================= Cumulative Rights ======================= " );

            foreach ( KeyValuePair<string, ActiveDirectoryRights> right in rights )
                Console.WriteLine( $">> {right.Key} : {right.Value}" );

            foreach (DirectoryEntry entry in groups)
            {
                string sid = DirectoryServices.ConvertByteToStringSid( (byte[])entry.Properties["objectSid"].Value );
                if ( rights.ContainsKey( sid ) )
                    myRights |= rights[sid];
            }

            return myRights;

        }
    }
}
