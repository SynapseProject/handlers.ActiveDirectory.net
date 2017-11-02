using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Core
{
    public interface IRoleManager
    {
        // Role Execution
        bool CanPerformAction(string principal, ActionType action, string adObject);
        void CanPerformActionOrException(string principal, ActionType action, string adObject);

        // Rold Administration
        IEnumerable<string> GetRoles();
        bool HasRole(string principal, string role, string adObject);
        void AddRole(string principal, string role, string adObject);
        void RemoveRole(string principal, string role, string adObject);

    }
}
