using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using YamlDotNet.Serialization;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryHandlerResults
    {
        [XmlArrayItem( ElementName = "Result" )]
        public List<ActiveDirectoryObjectResult> Results { get; set; }

        public void Add(ActiveDirectoryObjectResult result)
        {
            if ( Results == null )
                Results = new List<ActiveDirectoryObjectResult>();

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
            return YamlHelpers.Serialize( this, false, false, true );
        }

        public string ToJson(bool prettyPrint)
        {
            return YamlHelpers.Serialize( this, true, prettyPrint, true );
        }

        public string ToXml(bool prettyPrint)
        {
            return XmlHelpers.Serialize<ActiveDirectoryHandlerResults>( this, true, true, prettyPrint );
        }
    }
}
