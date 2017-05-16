using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Synapse.Core;
using Synapse.Services;

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

    [HttpGet]
    [Route( "{username}" )]
    public string GetUser(string username)
    {
        StartPlanEnvelope pe = new StartPlanEnvelope();
        pe.DynamicParameters = new Dictionary<string, string>();
        pe.DynamicParameters.Add( nameof( username ), username );

        IExecuteController ec = ExtensibilityUtility.GetExecuteControllerInstance();
        long id = ec.StartPlan( pe, "getUser" );
        bool poll = true;
        while( poll )
        {
            System.Threading.Thread.Sleep( 1000 );
            int result = ec.
            poll = false;
        }
        return "foo";
    }
}