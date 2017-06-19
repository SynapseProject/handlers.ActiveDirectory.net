using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;

using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapHandlerResults
    {
        [XmlArrayItem( ElementName = "User" )]
        public List<UserPrincipalObject> Users { get; set; }
        [XmlArrayItem( ElementName = "Group" )]
        public List<GroupPrincipalObject> Groups { get; set; }
        [XmlArrayItem( ElementName = "OrganizationalUnit" )]
        public List<OrganizationalUnitObject> OrganizationalUnits { get; set; }

        public void Add(UserPrincipalObject user)
        {
            if ( user == null )
                return;

            if ( Users == null )
                Users = new List<UserPrincipalObject>();

            lock ( Users )
            {
                Users.Add( user );
            }
        }

        public void Add(GroupPrincipalObject group)
        {
            if ( group == null )
                return;

            if ( Groups == null )
                Groups = new List<GroupPrincipalObject>();

            lock ( Groups )
            {
                Groups.Add( group );
            }
        }

        public void Add(OrganizationalUnitObject orgUnit)
        {
            if ( orgUnit == null )
                return;

            if ( OrganizationalUnits == null )
                OrganizationalUnits = new List<OrganizationalUnitObject>();

            lock ( OrganizationalUnits )
            {
                OrganizationalUnits.Add( orgUnit );
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
