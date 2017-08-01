using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdOrganizationalUnit : AdObject
    {
        public override AdObjectType GetADType()
        {
            return AdObjectType.OrganizationalUnit;
        }

    }
}
