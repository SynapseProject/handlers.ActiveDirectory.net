using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;


namespace Synapse.Handlers.Ldap
{
    public class LdapUser
    {
        [XmlElement]
        public String UserName { get; set; }
        [XmlElement]
        public String Password { get; set; }
        [XmlElement]
        public String GivenName { get; set; }
        [XmlElement]
        public String Surname { get; set; }
        [XmlElement]
        public String Description { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<String> Groups { get; set; }
    }
}
