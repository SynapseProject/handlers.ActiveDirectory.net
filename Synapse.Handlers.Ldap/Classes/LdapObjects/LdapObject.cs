using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using YamlDotNet.Serialization;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    abstract public class LdapObject
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string DistinguishedName { get; set; }
        [XmlElement]
        public string Path { get; set; }
        [XmlIgnore]
        [YamlIgnore]
        public ObjectClass Type { get { return GetLdapType(); } }
        [XmlElement]
        public string Description { get; set; }

        public abstract ObjectClass GetLdapType();
    }
}
