using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapOrganizationalUnit : LdapObject
    {
        public override ObjectClass GetLdapType()
        {
            return ObjectClass.OrganizationalUnit;
        }

    }
}
