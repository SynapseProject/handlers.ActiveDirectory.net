using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using YamlDotNet.Serialization;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapHandlerResults
    {
        [XmlArrayItem( ElementName = "Result" )]
        public List<LdapObjectResult> Results { get; set; }

        public void Add(LdapObjectResult result)
        {
            if ( Results == null )
                Results = new List<LdapObjectResult>();

            lock (Results)
            {
                Results.Add( result );
            }
        }

        public string Serialize(SerializationFormat format, bool prettyPrint)
        {
            switch ( format )
            {
                case SerializationFormat.Xml:
                    return ToXml( prettyPrint );
                case SerializationFormat.Json:
                    return ToJson( prettyPrint );
                case SerializationFormat.Yaml:
                    return ToYaml();
                default:
                    throw new Exception( "Unsupported Format Type [" + format + "]" );
            }
        }

        public string ToYaml()
        {
            return YamlHelpers.Serialize( this, false, false, false );
        }

        public string ToJson(bool prettyPrint)
        {
            return YamlHelpers.Serialize( this, true, prettyPrint, false );
        }

        public string ToXml(bool prettyPrint)
        {
            return XmlHelpers.Serialize<LdapHandlerResults>( this, true, true, prettyPrint );
        }
    }
}
