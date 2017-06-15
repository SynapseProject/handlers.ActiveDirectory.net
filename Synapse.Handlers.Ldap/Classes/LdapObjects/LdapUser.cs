using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapUser : LdapObject
    {
        [XmlElement]
        public String Password { get; set; }
        [XmlElement]
        public String GivenName { get; set; }
        [XmlElement]
        public String Surname { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<String> Groups { get; set; }

        public override ObjectClass GetLdapType()
        {
            return ObjectClass.User;
        }

    }
}
