using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.IO;

using Synapse.ActiveDirectory.Core;
using Synapse.Core.Utilities;

namespace Synapse.ActiveDirectory.RoleManager
{
    public class DaclRoleManagerConfig
    {
        public List<DaclRole> Roles { get; set; } = new List<DaclRole>();

        public static DaclRoleManagerConfig Load(string file)
        {
            string config = File.ReadAllText( file );
            return YamlHelpers.Deserialize<DaclRoleManagerConfig>( config, false );
        }
    }

    public class DaclRole
    {
        public string Name { get; set; }
        public List<string> ExtendsRoles { get; set; } = new List<string>();
        public List<ActionType> AllowedActions { get; set; } = new List<ActionType>();
        public List<ActiveDirectoryRights> AdRights { get; set; } = new List<ActiveDirectoryRights>();
    }
}
