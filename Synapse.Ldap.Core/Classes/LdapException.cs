using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public enum LdapExceptionType
    {
        Unknown,
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

    public class LdapException : Exception
    {
        public LdapExceptionType Type { get; set; } = LdapExceptionType.Unknown;

        public LdapException(LdapExceptionType type = LdapExceptionType.Unknown) : base()
        {
            this.Type = type;
        }

        public LdapException(String message, LdapExceptionType type = LdapExceptionType.Unknown) 
            : base( message )
        {
            this.Type = type;
        }

        public LdapException(String message, Exception innerException, LdapExceptionType type = LdapExceptionType.Unknown)
            : base( message, innerException )
        {
            this.Type = type;
        }

        public LdapException( SerializationInfo info, StreamingContext context, LdapExceptionType type = LdapExceptionType.Unknown)
            : base( info, context )
        {
            this.Type = type;
        }


    }
}
