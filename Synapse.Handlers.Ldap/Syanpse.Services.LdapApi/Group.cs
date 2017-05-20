using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
using Synapse.Ldap.Core;
using Synapse.Core.Utilities;

public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "group/{name}" )]
    public async Task<GroupPrincipalObject> GetGroup(string name, bool groups = false, bool runAtNode = true)
    {
        if( runAtNode )
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            pe.DynamicParameters.Add( nameof( name ), name );
            pe.DynamicParameters.Add( nameof( groups ), groups.ToString() );
            pe.DynamicParameters.Add( "type", ObjectClass.Group.ToString() );

            long id = ec.StartPlan( pe, "GetPrincipal" );
            StatusType status = await SynapseHelper.GetStatusAsync( ec, "GetPrincipal", id );

            if( status == StatusType.Success )
                return YamlHelpers.Deserialize<GroupPrincipalObject>( ec.GetPlanElements( "GetPrincipal", id, "Actions[0]:Result:ExitData" ).ToString() );
            else
                return null;
        }
        else
        {
            return DirectoryServices.GetGroup( name, groups );
        }
    }
}