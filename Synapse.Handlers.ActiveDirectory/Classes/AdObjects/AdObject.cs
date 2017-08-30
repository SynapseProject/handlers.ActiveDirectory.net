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

        public string Identity { get; set; }
        public List<PropertyType> Properties { get; set; } = new List<PropertyType>();
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
