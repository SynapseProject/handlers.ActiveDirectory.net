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
    [Route( "ou/{name}" )]
    public OrganizationalUnitObject GetOrgUnit(string name, bool runAtNode = true)
    {
        if( runAtNode )
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            pe.DynamicParameters.Add( nameof( name ), name );
            pe.DynamicParameters.Add( "type", ObjectClass.OrganizationalUnit.ToString() );

            return SynapseHelper.ExecuteAsync<OrganizationalUnitObject>( ec, "GetOrgUnit", pe );
        }
        else
        {
            return DirectoryServices.GetOrganizationalUnit( name, null );
        }
    }
}