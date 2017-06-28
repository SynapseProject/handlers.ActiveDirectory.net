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
        [XmlArrayItem( ElementName = "User" )]
        public List<UserPrincipalObjectWithStatus> Users { get; set; }
        [XmlArrayItem( ElementName = "Group" )]
        public List<GroupPrincipalObjectWithStatus> Groups { get; set; }
        [XmlArrayItem( ElementName = "OrganizationalUnit" )]
        public List<OrganizationalUnitObjectWithStatus> OrganizationalUnits { get; set; }

        public void Add(LdapStatus status, UserPrincipalObject user)
        {
            if ( Users == null )
                Users = new List<UserPrincipalObjectWithStatus>();

            UserPrincipalObjectWithStatus obj = new UserPrincipalObjectWithStatus( status )
            {
                User = user
            };

            lock ( Users )
            {
                Users.Add( obj );
            }
        }

        public void Add(LdapStatus status, GroupPrincipalObject group)
        {
            if ( Groups == null )
                Groups = new List<GroupPrincipalObjectWithStatus>();

            GroupPrincipalObjectWithStatus obj = new GroupPrincipalObjectWithStatus( status )
            {
                Group = group
            };

            lock ( Groups )
            {
                Groups.Add( obj );
            }
        }

        public void Add(LdapStatus status, OrganizationalUnitObject orgUnit)
        {
            if ( OrganizationalUnits == null )
                OrganizationalUnits = new List<OrganizationalUnitObjectWithStatus>();

            OrganizationalUnitObjectWithStatus obj = new OrganizationalUnitObjectWithStatus( status )
            {
                OrganizationalUnit = orgUnit
            };

            lock ( OrganizationalUnits )
            {
                OrganizationalUnits.Add( obj );
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

    public class LdapStatus
    {
        [XmlElement]
        public LdapStatusType Status { get; set; } = LdapStatusType.Success;
        [XmlElement]
        public String Message { get; set; } = "Success";
        [XmlElement]
        public ActionType Action { get; set; }
        [XmlElement]
        public String Name { get; set; }
        [XmlElement]
        public String Path { get; set; }

        public LdapStatus() { }

        public LdapStatus(LdapStatus status)
        {
            Status = status.Status;
            Message = status.Message;
            Action = status.Action;
            Name = status.Name;
            Path = status.Path;
        }

    }

    public class UserPrincipalObjectWithStatus : LdapStatus
    {
        [XmlElement]
        public UserPrincipalObject User { get; set; }

        public UserPrincipalObjectWithStatus() { }
        public UserPrincipalObjectWithStatus(LdapStatus status) : base( status ) { }
    }

    public class GroupPrincipalObjectWithStatus : LdapStatus
    {
        [XmlElement]
        public GroupPrincipalObject Group { get; set; }

        public GroupPrincipalObjectWithStatus() { }
        public GroupPrincipalObjectWithStatus(LdapStatus status) : base( status ) { }
    }

    public class OrganizationalUnitObjectWithStatus : LdapStatus
    {
        [XmlElement]
        public OrganizationalUnitObject OrganizationalUnit { get; set; }

        public OrganizationalUnitObjectWithStatus() { }
        public OrganizationalUnitObjectWithStatus(LdapStatus status) : base( status ) { }
    }
}
