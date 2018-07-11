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
    public class ActiveDirectoryStatus
    {
        [XmlIgnore]
        [YamlIgnore]
        [JsonIgnore]
        public AdStatusType StatusId { get; set; } = AdStatusType.Success;
        [XmlElement]
        public string Status { get { return StatusId.ToString(); } }
        [XmlElement]
        public string Message { get; set; } = "Success";
        [XmlIgnore]
        [YamlIgnore]
        [JsonIgnore]
        public ActionType ActionId { get; set; }
        [XmlElement]
        public string Action { get { return ActionId.ToString(); } }

        public ActiveDirectoryStatus() { }


        public ActiveDirectoryStatus(ActiveDirectoryStatus status)
        {
            Init( status );
        }

        private void Init(ActiveDirectoryStatus status)
        {
            StatusId = status.StatusId;
            Message = status.Message;
            ActionId = status.ActionId;
        }

    }
}
