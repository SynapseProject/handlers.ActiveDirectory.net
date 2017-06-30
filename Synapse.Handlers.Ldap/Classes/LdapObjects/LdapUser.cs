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
        public string Password { get; set; }
        [XmlElement]
        public string GivenName { get; set; }
        [XmlElement]
        public string Surname { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override ObjectClass GetLdapType()
        {
            return ObjectClass.User;
        }

    }
}
