using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Synapse.Core;
using Synapse.Services;

[RoutePrefix( "lyra" )]
public class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "hello" )]
    public string Hello() { return "Hello from LyraApi, World!"; }

    [HttpGet]
    [Route( "synapse" )]
    public string SynapseHello() { return ExtensibilityUtility.GetExecuteControllerInstance().Hello(); }

    [HttpGet]
    [Route( "{username}" )]
    public async Task<string> GetUser(string username)
    {
        IExecuteController ec = ExtensibilityUtility.GetExecuteControllerInstance();

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( username ), username );

        long id = ec.StartPlan( pe, "getUser" );
        StatusType status = await StatusHelper.GetStatusAsync( ec, id );

        return status == StatusType.Success ? (string)ec.GetPlanElements( "getUser", id, "Actions[0]:Result:ExitData" ) : null;
    }
}
class StatusHelper
{
    public static StatusType GetStatus(IExecuteController ec, long id)
    {
        bool poll = true;
        int result = 0;
        while( poll )
        {
            System.Threading.Thread.Sleep( 1000 );
            result = (int)ec.GetPlanElements( "getUser", id, "Status" );
            poll = result < (int)StatusType.Success;
        }
        return (StatusType)result;
    }
    public static Task<StatusType> GetStatusAsync(IExecuteController ec, long id)
    {
        return Task.Run( () => GetStatus( ec, id ) );
    }
}