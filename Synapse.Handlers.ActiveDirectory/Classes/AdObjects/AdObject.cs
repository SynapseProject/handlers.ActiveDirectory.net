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
        public string Identity { get; set; } 
//        public string DistinguishedName { get; set; }               
//        public string Path { get; set; }
        [YamlIgnore]
        public AdObjectType Type { get { return GetADType(); } }

        public abstract AdObjectType GetADType();
    }
}
