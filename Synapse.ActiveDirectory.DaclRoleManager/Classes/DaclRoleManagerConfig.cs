using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.IO;

using Synapse.ActiveDirectory.Core;
using Synapse.Core.Utilities;

public class DaclRoles
{
    public List<DaclRole> Roles { get; set; } = new List<DaclRole>();

    public static DaclRoles Load(string file)
    {
        string config = File.ReadAllText( file );
        return YamlHelpers.Deserialize<DaclRoles>( config, false );
    }
}

public class DaclRole
{
    public string Name { get; set; }
    public ActiveDirectoryRights AdRights { get; set; }
    public ActionType AllowedActions { get; set; }
    public List<string> ExtendsRoles { get; set; } = new List<string>();
}

public class DaclRoleManagerConfig
{
    public string FileName { get; set; }
}
