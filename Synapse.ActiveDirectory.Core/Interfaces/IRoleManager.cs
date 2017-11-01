using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public interface IRoleManager
    {
        IEnumerable<string> GetRoles();

        bool HasRole(string principal, string role, string adObject);
        void AddRole(string principal, string role, string adObject);
        void RemoveRole(string principal, string role, string adObject);

    }
}
