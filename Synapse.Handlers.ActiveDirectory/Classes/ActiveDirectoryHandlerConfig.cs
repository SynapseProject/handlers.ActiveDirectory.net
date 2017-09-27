using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryHandlerConfig
    {
        [XmlElement]
        public ActionType Action { get; set; }
        [XmlElement]
        public bool RunSequential { get; set; } = false;
        [XmlElement]
        public bool QueryGroupMembership { get; set; } = true;
        [XmlElement]
        public bool ReturnAccessRules { get; set; } = true;
        [XmlElement]
        public bool ReturnObjects { get; set; } = true;
        [XmlElement]
        public bool SuppressOutput { get; set; } = false;
        [XmlElement]
        public bool UseUpsert { get; set; } = true;
        [XmlElement]
        public SerializationFormat OutputType { get; set; } = SerializationFormat.Json;
        [XmlElement]
        public bool PrettyPrint { get; set; } = true;
    }
}
