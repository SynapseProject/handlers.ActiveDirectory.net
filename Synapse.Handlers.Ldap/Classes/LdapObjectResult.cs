using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapObjectResult
    {
        [XmlArrayItem( ElementName = "Status" )]
        public List<LdapStatus> Statuses { get; set; } = new List<LdapStatus>();

        [XmlElement]
        public LdapObjectType Type { get; set; } = LdapObjectType.None;
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Path { get; set; }
        [XmlElement]
        public string DistinguishedName { get; set; }

        [XmlElement]
        public UserPrincipalObject User { get; set; }
        [XmlElement]
        public GroupPrincipalObject Group { get; set; }
        [XmlElement]
        public OrganizationalUnitObject OrganizationalUnit { get; set; }
    }
}
