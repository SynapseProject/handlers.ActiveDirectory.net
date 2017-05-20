using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
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
    [Route( "{type}/{name}" )]
    public async Task<object> GetPrincipal(PrincipalType type, string name, bool groups = false)
    {
        IExecuteController ec = GetExecuteControllerInstance();

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( groups ), groups.ToString() );
        pe.DynamicParameters.Add( nameof( type ), type.ToString() );

        long id = ec.StartPlan( pe, "GetPrincipal" );
        StatusType status = await StatusHelper.GetStatusAsync( ec, "GetPrincipal", id );

        return status == StatusType.Success ? ec.GetPlanElements( "GetPrincipal", id, "Actions[0]:Result:ExitData" ) : null;
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