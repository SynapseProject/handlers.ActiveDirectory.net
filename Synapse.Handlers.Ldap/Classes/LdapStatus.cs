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
    public class LdapStatus
    {
        [XmlElement]
        public LdapStatusType Status { get; set; } = LdapStatusType.Success;
        [XmlElement]
        public string Message { get; set; } = "Success";
        [XmlElement]
        public ActionType Action { get; set; }

        public LdapStatus() { }


        public LdapStatus(LdapStatus status)
        {
            Init( status );
        }

        private void Init(LdapStatus status)
        {
            Status = status.Status;
            Message = status.Message;
            Action = status.Action;
        }

    }
}
