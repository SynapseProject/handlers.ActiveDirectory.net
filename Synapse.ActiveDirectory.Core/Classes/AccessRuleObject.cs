using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;


namespace Synapse.ActiveDirectory.Core
{
    public class AccessRuleObject
    {
        public AccessControlType ControlType { get; set; }
        public ActiveDirectoryRights Rights { get; set; }
        public string IdentityReference { get; set; }
        public string IdentityName { get; set; }
        public InheritanceFlags InheritanceFlags { get; set; }
        public bool IsInherited { get; set; }

    }
}
