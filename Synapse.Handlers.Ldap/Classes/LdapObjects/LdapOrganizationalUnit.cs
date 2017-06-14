using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;


namespace Synapse.Handlers.Ldap
{
    public class LdapOrganizationalUnit
    {
        [XmlElement]
        public String Name { get; set; }
        [XmlElement]
        public String Parent { get; set; }
    }
}
