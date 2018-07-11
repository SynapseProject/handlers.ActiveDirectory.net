using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;

using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Synapse.Handlers.ActiveDirectory
{
    [XmlInclude(typeof(SearchResultsObject))]
    [XmlInclude(typeof(UserPrincipalObject))]
    [XmlInclude(typeof(GroupPrincipalObject))]
    [XmlInclude(typeof(DirectoryEntryObject))]
    public class ActiveDirectoryObjectResult
    {
        [XmlArrayItem( ElementName = "Status" )]
        public List<ActiveDirectoryStatus> Statuses { get; set; } = new List<ActiveDirectoryStatus>();

        [XmlIgnore]
        [YamlIgnore]
        [JsonIgnore]
        public AdObjectType TypeId { get; set; } = AdObjectType.None;
        [XmlElement]
        public string Type { get { return TypeId.ToString(); } }
        [XmlElement]
        public string Identity { get; set; }

        [XmlElement]
        public object Object { get; set; }

        [XmlElement]
        private Type ObjectType { get { return Object?.GetType(); }  }

        [XmlIgnore]
        [JsonIgnore]
        [YamlIgnore]
        public  UserPrincipalObject User { get { return GetObjectAs<UserPrincipalObject>(); } }

        [XmlIgnore]
        [JsonIgnore]
        [YamlIgnore]
        public GroupPrincipalObject Group { get { return GetObjectAs<GroupPrincipalObject>(); } }

        [XmlIgnore]
        [JsonIgnore]
        [YamlIgnore]
        public DirectoryEntryObject OrganizationalUnit { get { return GetObjectAs<DirectoryEntryObject>(); } }

        [YamlIgnore]
        [JsonIgnore]
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
