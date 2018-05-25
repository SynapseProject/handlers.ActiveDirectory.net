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
        GroupPolicy,
        SearchResults
    }

    [Flags]
    // Used as "flags" when assigning to a Role.  Used indivdually when performing an action.
    // "All" value should only be used when assigning to a Role.
    public enum ActionType
    {
        None = 0,
        Get = 1,
        Create = 2,
        Modify = 4,
        Delete = 8,
        Unused = 16,    // Rename was added to "Modify".  Can re-use this action if needed.
        Move = 32,
        AddToGroup = 64,
        RemoveFromGroup = 128,
        Search = 256,
        AddAccessRule = 512,
        RemoveAccessRule = 1024,
        SetAccessRule = 2048,
        PurgeAccessRules = 4096,
        AddRole = 8192,
        RemoveRole = 16384,
        All = 32767
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
        MultipleMatches,
        InvalidInput
    }

}