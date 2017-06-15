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

    [HttpPost]
    [Route("ou/{name}")]
    public OrganizationalUnitObject CreateOrgUnit(string parentOrgUnitDistName, string newOrgUnitName, bool runAtNode = true)
    {
        if (runAtNode)
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            // TODO: Check with Steve on how to do this
            //pe.DynamicParameters.Add(nameof(name), name);
            //pe.DynamicParameters.Add("type", ObjectClass.OrganizationalUnit.ToString());

            return SynapseHelper.ExecuteAsync<OrganizationalUnitObject>( ec, "CreateOrgUnit", pe );
        }
        else
        {
            return DirectoryServices.GetOrganizationalUnit( parentOrgUnitDistName, newOrgUnitName );
        }
    }

    [HttpDelete]
    [Route("ou/{name}")]
    public bool DeleteOrgUnit(string orgUnitDistName, bool runAtNode = true)
    {
        if (runAtNode)
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            // TODO: Check with Steve on relevant codes for node execution
            //pe.DynamicParameters.Add(nameof(name), name);
            //pe.DynamicParameters.Add("type", ObjectClass.OrganizationalUnit.ToString());

            return SynapseHelper.ExecuteAsync<bool>( ec, "DeleteOrgUnit", pe );
        }
        else
        {
            return DirectoryServices.DeleteOrganizationUnit( orgUnitDistName );
        }
    }
}