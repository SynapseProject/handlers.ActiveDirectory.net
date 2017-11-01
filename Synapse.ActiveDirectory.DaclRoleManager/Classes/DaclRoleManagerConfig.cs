using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;

using Synapse.Core.Utilities;

namespace Synapse.ActiveDirectory.RoleManager
{
    public class DaclRoleManagerConfig
    {
        List<DaclRole> Roles { get; set; } = new List<DaclRole>();

        public static DaclRoleManagerConfig Load(string file)
        {
            return YamlHelpers.Deserialize<DaclRoleManagerConfig>( file, false );
        }

    }

    public class DaclRole
    {
        public string Name { get; set; }
        public List<ActiveDirectoryRights> Rights { get; set; } = new List<ActiveDirectoryRights>();
    }
}
