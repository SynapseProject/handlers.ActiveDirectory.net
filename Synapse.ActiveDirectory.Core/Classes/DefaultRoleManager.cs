using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Synapse.ActiveDirectory.Core;

public class DefaultRoleManager : IRoleManager
{
    public void Initialize(object config)
    {
    }

    public void AddRole(string principal, string role, string adObject)
    {
    }

    public bool CanPerformAction(string principal, ActionType action, string adObject)
    {
        return true;
    }

    public void CanPerformActionOrException(string principal, ActionType action, string adObject)
    {
    }

    public IEnumerable<string> GetRoles()
    {
        return null;
    }

    public bool HasRole(string principal, string role, string adObject)
    {
        return true;
    }

    public void RemoveRole(string principal, string role, string adObject)
    {
    }
}
