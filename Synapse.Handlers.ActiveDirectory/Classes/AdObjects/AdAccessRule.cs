using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices;
using System.Security.AccessControl;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdAccessRule
    {
        [XmlElement]
        public string Identity { get; set; }
        [XmlElement]
        public AccessControlType Type { get; set; }
        [XmlArrayItem(ElementName = "Right")]
        public List<ActiveDirectoryRights> Rights { get; set; }
    }
}
