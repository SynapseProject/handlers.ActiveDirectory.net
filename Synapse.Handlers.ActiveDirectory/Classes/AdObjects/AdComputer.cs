using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdComputer : AdObject
    {
        public string Description { get; set; }
        public string ManagedBy { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.Computer;
        }

    }
}
