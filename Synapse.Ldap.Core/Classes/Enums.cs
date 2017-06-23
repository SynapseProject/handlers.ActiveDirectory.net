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
        Xml,
        Yaml
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
        None,
        Query,
        Create,
        Modify,
        Delete,
        AddToGroup,
        RemoveFromGroup
    }

    public enum LdapStatusType
    {
        Unknown,
        Success,
        MissingInput,
        AlreadyExists,
        DoesNotExist,
        PasswordPolicyNotMet,
        InvalidPath,
        NotSupported,
        NotAllowed,
        InvalidAttribute,
        ConnectionError,
        InvalidName,
        InvalidContainer
    }

}