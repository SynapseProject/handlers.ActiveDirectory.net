using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryObjectResult
    {
        [XmlArrayItem( ElementName = "Status" )]
        public List<ActiveDirectoryStatus> Statuses { get; set; } = new List<ActiveDirectoryStatus>();

        [XmlElement]
        public AdObjectType Type { get; set; } = AdObjectType.None;
        [XmlElement]
        public string Identity { get; set; }

        [XmlElement]
        public UserPrincipalObject User { get; set; }
        [XmlElement]
        public GroupPrincipalObject Group { get; set; }
        [XmlElement]
        public OrganizationalUnitObject OrganizationalUnit { get; set; }
        [XmlElement]
        public SearchResults SearchResults { get; set; }
    }
}
