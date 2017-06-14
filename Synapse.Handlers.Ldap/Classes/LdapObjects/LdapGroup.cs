using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;


namespace Synapse.Handlers.Ldap
{
    public class LdapGroup
    {
        [XmlElement]
        public String GroupName { get; set; }
        [XmlElement]
        public String OUPath { get; set; }
        [XmlElement]
        public GroupScope Scope { get; set; }
        [XmlElement]
        public bool IsSecurityGroup { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<String> Groups { get; set; }
    }
}
