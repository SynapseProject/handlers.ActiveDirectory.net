using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
using Synapse.Ldap.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.Ldap;

public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "ou/{name}" )]
    public LdapHandlerResults GetOrgUnit(string name, bool groups = false, bool runAtNode = true)
    {
        String planName = @"QueryOrgUnit";
        if ( runAtNode )
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            pe.DynamicParameters.Add( nameof( name ), name );

            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            foreach ( KeyValuePair<string, string> kvp in query )
                pe.DynamicParameters.Add( kvp.Key, kvp.Value );

            String reply = (String)ec.StartPlanSync( pe, planName, setContentType: false );
            return YamlHelpers.Deserialize<LdapHandlerResults>( reply );
        }
        else
        {
            return null;
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
}