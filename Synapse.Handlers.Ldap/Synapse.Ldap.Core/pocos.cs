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