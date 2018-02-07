using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices;
using System.Security.AccessControl;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdRole
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Principal { get; set; }
    }
}
