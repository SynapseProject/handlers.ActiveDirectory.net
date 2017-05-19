using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

using Synapse.Core;
using Synapse.Services;
using Syanpse.Services.LdapApi;
using Synapse.Ldap.Core;

[RoutePrefix( "lyra" )]
public class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "hello" )]
    public string Hello() { return "Hello from LyraApi, World!"; }

    [HttpGet]
    [Route( "synapse" )]
    public string SynapseHello() { return GetExecuteControllerInstance().Hello(); }

    [HttpGet]
    [Route( "{username}" )]
    public async Task<object> GetUser(string username)
    {
        IExecuteController ec = GetExecuteControllerInstance();

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( username ), username );

        long id = ec.StartPlan( pe, "getUser" );
        StatusType status = await StatusHelper.GetStatusAsync( ec, "getUser", id );

        return status == StatusType.Success ? ec.GetPlanElements( "getUser", id, "Actions[0]:Result:ExitData" ) : null;
    }

    [HttpGet]
    [Route( "object/{type}/{name}" )]
    public async Task<string> GetObject(ObjectClass type, string name)
    {
        IExecuteController ec = GetExecuteControllerInstance();

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( type ), type.ToString() );

        long id = ec.StartPlan( pe, "getObject" );
        StatusType status = await StatusHelper.GetStatusAsync( ec, "getObject", id );

        return status == StatusType.Success ? (string)ec.GetPlanElements( "getObject", id, "Actions[0]:Result:ExitData" ) : null;
    }

    IExecuteController GetExecuteControllerInstance()
    {
        return ExtensibilityUtility.GetExecuteControllerInstance( Url, User );
    }
}