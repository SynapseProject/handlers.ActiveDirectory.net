using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Xml.Serialization;


namespace Synapse.ActiveDirectory.Core
{
    public class PropertyType
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlArrayItem( ElementName = "Value" )]
        public List<string> Values { get; set; } = new List<string>();
    }
}
