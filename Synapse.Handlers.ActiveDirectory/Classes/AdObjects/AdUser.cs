using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdUser : AdObject
    {
        [XmlElement]
        public string Password { get; set; }
        [XmlElement]
        public string GivenName { get; set; }
        [XmlElement]
        public string Surname { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.User;
        }

    }
}
