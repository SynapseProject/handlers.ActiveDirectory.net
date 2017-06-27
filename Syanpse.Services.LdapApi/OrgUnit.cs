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
    public LdapHandlerResults GetOrgUnit(string name)
    {
        String planName = @"QueryOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{name}" )]
    public LdapHandlerResults DeleteOrgUnit(string name)
    {
        String planName = @"DeleteOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

}