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
        Query,
        Create,
        Modify,
        Delete,
        AddToGroup,
        RemoveFromGroup,
        Move,
        Search,
        AddAccessRule,
        RemoveAccessRule,
        SetAccessRule,
        PurgeAccessRules
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