using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
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

    public enum AdObjectType
    {
        None,
        User,
        Group,
        Computer,
        OrganizationalUnit,
        GroupPolicy
    }

    public enum ActionType
    {
        None,
        Get,
        Create,
        Modify,
        Delete,
        Rename,
        Move,
        AddToGroup,
        RemoveFromGroup,
        Search,
        AddAccessRule,
        RemoveAccessRule,
        SetAccessRule,
        PurgeAccessRules,
        AddRole,
        RemoveRole
    }

    public enum AdStatusType
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
        InvalidContainer,
        MultipleMatches
    }

}