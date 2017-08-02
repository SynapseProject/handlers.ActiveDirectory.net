using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdGroup : AdObject
    {
        // Settable Principal Fields
        public string UserPrincipalName { get; set; }
        public string SamAccountName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        // Settable GroupPrincipalFields
        public GroupScope Scope { get; set; }
        public bool IsSecurityGroup { get; set; }

        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.Group;
        }
    }
}
