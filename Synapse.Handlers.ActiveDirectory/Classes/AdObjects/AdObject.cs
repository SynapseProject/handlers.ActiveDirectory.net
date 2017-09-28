using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using YamlDotNet.Serialization;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    // Base Class Containing Fields To Address Individual Objects In Active Directory
    abstract public class AdObject
    {
        protected string NULL = "~null~";

        [XmlElement]
        public string Identity { get; set; }
        [XmlArrayItem(ElementName = "Property")]
        public Dictionary<String, List<String>> Properties { get; set; }
        [XmlArrayItem(ElementName = "AccessRule")]
        public List<AdAccessRule> AccessRules { get; set; }

        [YamlIgnore]
        public AdObjectType Type { get { return GetADType(); } }

        public abstract AdObjectType GetADType();

        public string SetValueOrNull(string value)
        {
            if ( value.Equals( NULL, StringComparison.OrdinalIgnoreCase ) )
                return null;
            else
                return value;
        }
    }
}
