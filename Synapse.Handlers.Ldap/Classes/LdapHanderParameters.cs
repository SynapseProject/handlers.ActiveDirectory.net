using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace Synapse.Handlers.Ldap
{
    public class LdapHanderParameters
    {
        [XmlArrayItem(ElementName = "User")]
        public List<LdapUser> Users { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<LdapGroup> Groups { get; set; }
        [XmlArrayItem(ElementName = "OrganizationalUnit")]
        public List<LdapOrganizationalUnit> OrganizationalUnits { get; set; }
    }
}
