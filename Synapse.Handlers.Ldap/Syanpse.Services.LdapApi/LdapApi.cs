using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Synapse.Services;

namespace Syanpse.Services.LdapApi
{
    [RoutePrefix( "lyra" )]
    public class LdapApiController : ApiController
    {
        [HttpGet]
        [Route( "hello" )]
        public string Hello()
        {
            return "Hello from LyraApi, World!";
        }

        [HttpGet]
        [Route( "synapse" )]
        public string SynapseHello()
        {
            IExecuteController ec = ExtensibilityUtility.GetExecuteControllerInstance();
            return ec.Hello();
        }
    }
}
