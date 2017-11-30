using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;

using YamlDotNet.Serialization;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryObjectResult
    {
        [XmlArrayItem( ElementName = "Status" )]
        public List<ActiveDirectoryStatus> Statuses { get; set; } = new List<ActiveDirectoryStatus>();

        [XmlElement]
        public AdObjectType Type { get; set; } = AdObjectType.None;
        [XmlElement]
        public string Identity { get; set; }

        [XmlElement]
        public object Object { get; set; }

        [XmlElement]
        private Type ObjectType { get { return Object?.GetType(); }  }

        [YamlIgnore]
        [XmlIgnore]
        public UserPrincipalObject User { get { return GetObjectAs<UserPrincipalObject>(); } }

        [YamlIgnore]
        [XmlIgnore]
        public GroupPrincipalObject Group { get { return GetObjectAs<GroupPrincipalObject>(); } }

        [YamlIgnore]
        [XmlIgnore]
        public OrganizationalUnitObject OrganizationalUnit { get { return GetObjectAs<OrganizationalUnitObject>(); } }

        [YamlIgnore]
        [XmlIgnore]
        public SearchResultsObject SearchResults { get { return GetObjectAs<SearchResultsObject>(); } }

        private T GetObjectAs<T>()
        {
            if ( ObjectType == null )
                return default( T );
            else if ( ObjectType == typeof( Dictionary<Object, Object> ) )
            {
                string str = YamlHelpers.Serialize( this.Object );
                if ( str == null )
                    return default( T );
                else
                    return YamlHelpers.Deserialize<T>( str );
            }
            else
                return (T)this.Object;
        }

    }
}
