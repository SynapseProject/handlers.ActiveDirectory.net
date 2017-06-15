using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapGroup : LdapObject
    {
        [XmlElement]
        public GroupScope Scope { get; set; }
        [XmlElement]
        public bool IsSecurityGroup { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<String> Groups { get; set; }

        public override ObjectClass GetLdapType()
        {
            return ObjectClass.Group;
        }
    }
}
