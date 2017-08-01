using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public class AdException : Exception
    {
        public AdStatusType Type { get; set; } = AdStatusType.Unknown;

        public AdException(AdStatusType type = AdStatusType.Unknown) : base()
        {
            this.Type = type;
        }

        public AdException(string message, AdStatusType type = AdStatusType.Unknown) 
            : base( message )
        {
            this.Type = type;
        }

        public AdException(string message, Exception innerException, AdStatusType type = AdStatusType.Unknown)
            : base( message, innerException )
        {
            this.Type = type;
        }

        public AdException( SerializationInfo info, StreamingContext context, AdStatusType type = AdStatusType.Unknown)
            : base( info, context )
        {
            this.Type = type;
        }

        public AdException(Exception e, AdStatusType type = AdStatusType.Unknown)
            : base( e.Message, e )
        {
            this.Type = AdStatusType.Unknown;
        }


    }
}
