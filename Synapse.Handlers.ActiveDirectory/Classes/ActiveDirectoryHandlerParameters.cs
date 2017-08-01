using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryHandlerParameters
    {
        [XmlArrayItem(ElementName = "User")]
        public List<AdUser> Users { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<AdGroup> Groups { get; set; }
        [XmlArrayItem(ElementName = "OrganizationalUnit")]
        public List<AdOrganizationalUnit> OrganizationalUnits { get; set; }
    }
}
