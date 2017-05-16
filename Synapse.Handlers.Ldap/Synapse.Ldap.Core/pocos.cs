using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class ConnectionInfo
    {
        public string LdapRoot { get; set; }
    }

    public enum SerializationFormat
    {
        Json,
        Xml
    }

    public class LdapHandlerParameters
    {
        public ActionType Action { get; set; } = ActionType.Query;
        public ObjectType Type { get; set; }
        public SerializationFormat ReturnFormat { get; set; } = SerializationFormat.Json;
        public object Request { get; set; }
    }

    public enum ObjectType
    {
        User,
        Group,
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