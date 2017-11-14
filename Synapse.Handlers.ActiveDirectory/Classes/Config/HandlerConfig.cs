using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Synapse.Core.Utilities;

namespace Synapse.Handlers.ActiveDirectory
{
    public class HandlerConfig
    {
        public RoleManagerConfig RoleManager { get; set; } = new RoleManagerConfig();

        public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( HandlerConfig ).Assembly.Location )}";
        public static readonly string FileName = $"{CurrentPath}\\Synapse.Handlers.ActiveDirectory.config.yaml";

        public void Serialize()
        {
            YamlHelpers.SerializeFile( FileName, this, serializeAsJson: false, emitDefaultValues: true );
        }

        public static HandlerConfig Deserialze()
        {
            if ( !File.Exists( FileName ) )
                throw new FileNotFoundException( $"Could not find {FileName}." );

            return YamlHelpers.DeserializeFile<HandlerConfig>( FileName );
        }

        public static HandlerConfig DeserializeOrNew()
        {
            HandlerConfig config = null;

            if ( !File.Exists( FileName ) )
                config = new HandlerConfig();
            else
                config = Deserialze();

            return config;
        }
    }

    public class RoleManagerConfig
    {
        public string Name { get; set; } = "Synapse.ActiveDirectory.Core:DefaultRoleManager";
        public object Config { get; set; }
    }
}
