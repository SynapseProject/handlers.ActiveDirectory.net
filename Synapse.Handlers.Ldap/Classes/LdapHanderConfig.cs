using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapHanderConfig
    {
        [XmlElement]
        public String LdapRoot { get; set; }
        [XmlElement]
        public ActionType Action { get; set; }
        [XmlElement]
        public bool RunSequential { get; set; }
        [XmlElement]
        public bool QueryGroupMembership { get; set; }
    }
}
