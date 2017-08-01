using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using YamlDotNet.Serialization;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    abstract public class AdObject
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string DistinguishedName { get; set; }
        [XmlElement]
        public string Path { get; set; }
        [XmlIgnore]
        [YamlIgnore]
        public AdObjectType Type { get { return GetADType(); } }
        [XmlElement]
        public string Description { get; set; }

        public abstract AdObjectType GetADType();
    }
}
