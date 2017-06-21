using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    abstract public class LdapObject
    {
        [XmlElement]
        public String Name { get; set; }
        [XmlElement]
        public String Path { get; set; }
        [XmlElement]
        public ObjectClass Type { get { return GetLdapType(); } }
        [XmlElement]
        public String Description { get; set; }

        public abstract ObjectClass GetLdapType();
    }
}
