using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class ActiveDirectoryStatus
    {
        [XmlElement]
        public AdStatusType Status { get; set; } = AdStatusType.Success;
        [XmlElement]
        public string Message { get; set; } = "Success";
        [XmlElement]
        public ActionType Action { get; set; }

        public ActiveDirectoryStatus() { }


        public ActiveDirectoryStatus(ActiveDirectoryStatus status)
        {
            Init( status );
        }

        private void Init(ActiveDirectoryStatus status)
        {
            Status = status.Status;
            Message = status.Message;
            Action = status.Action;
        }

    }
}
