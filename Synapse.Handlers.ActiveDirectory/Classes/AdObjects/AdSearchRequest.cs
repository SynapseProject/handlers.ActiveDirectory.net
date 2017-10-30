using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdSearchRequest
    {
        public string Filter { get; set; }
        public List<RegexParameters> Parameters {get; set;}
        public List<string> ReturnAttributes { get; set; }
    }

    public class RegexParameters
    {
        public string Find { get; set; }
        public string ReplaceWith { get; set; }
    }
}
