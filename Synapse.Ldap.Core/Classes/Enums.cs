using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public enum SerializationFormat
    {
        Json,
        Xml
    }

    public enum PrincipalType
    {
        User,
        Group,
    }

    public enum ObjectClass
    {
        User,
        Group,
        Computer,
        OrganizationalUnit,
        GroupPolicy
    }

    public enum ActionType
    {
        Query,
        Create,
        Modify,
        Delete
    }
}