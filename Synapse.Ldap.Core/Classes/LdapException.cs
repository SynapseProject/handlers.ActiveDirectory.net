using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class LdapException : Exception
    {
        public LdapStatusType Type { get; set; } = LdapStatusType.Unknown;

        public LdapException(LdapStatusType type = LdapStatusType.Unknown) : base()
        {
            this.Type = type;
        }

        public LdapException(string message, LdapStatusType type = LdapStatusType.Unknown) 
            : base( message )
        {
            this.Type = type;
        }

        public LdapException(string message, Exception innerException, LdapStatusType type = LdapStatusType.Unknown)
            : base( message, innerException )
        {
            this.Type = type;
        }

        public LdapException( SerializationInfo info, StreamingContext context, LdapStatusType type = LdapStatusType.Unknown)
            : base( info, context )
        {
            this.Type = type;
        }

        public LdapException(Exception e, LdapStatusType type = LdapStatusType.Unknown)
            : base( e.Message, e )
        {
            this.Type = LdapStatusType.Unknown;
        }


    }
}
