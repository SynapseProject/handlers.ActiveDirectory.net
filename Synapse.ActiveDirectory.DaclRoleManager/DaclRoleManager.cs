using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.RoleManager
{
    public class DaclRoleManager : IRoleManager
    {

        public DaclRoleManager()
        {

        }

        public void AddRole(string principal, string role, string adObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetRoles()
        {
            throw new NotImplementedException();
        }

        public bool HasRole(string principal, string role, string adObject)
        {
            throw new NotImplementedException();
        }

        public void RemoveRole(string principal, string role, string adObject)
        {
            throw new NotImplementedException();
        }
    }
}
