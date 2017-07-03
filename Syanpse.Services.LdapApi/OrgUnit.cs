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
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults GetOrgUnit(string distinguishedname)
    {
        string planName = @"QueryOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
        pe.DynamicParameters.Add( "name", String.Empty );
        pe.DynamicParameters.Add( "path", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpGet]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults GetOrgUnit(string name, string path)
    {
        string planName = @"QueryOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( path ), path );
        pe.DynamicParameters.Add( "distinguishedname", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults DeleteOrgUnit(string distinguishedname)
    {
        string planName = @"DeleteOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
        pe.DynamicParameters.Add( "name", String.Empty );
        pe.DynamicParameters.Add( "path", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults DeleteOrgUnit(string name, string path)
    {
        string planName = @"DeleteOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( path ), path );
        pe.DynamicParameters.Add( "distinguishedname", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults CreateOrgUnit(string distinguishedname, LdapOrganizationalUnit ou)
    {
        string planName = @"CreateOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
        pe.DynamicParameters.Add( "name", String.Empty );
        pe.DynamicParameters.Add( "path", String.Empty );

        if ( !string.IsNullOrWhiteSpace( ou.Description ) )
            pe.DynamicParameters.Add( @"description", ou.Description );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults CreateOrgUnit(string name, string path, LdapOrganizationalUnit ou)
    {
        string planName = @"CreateOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( path ), path );
        pe.DynamicParameters.Add( "distinguishedname", String.Empty );

        if ( ou != null )
        {
            if ( !string.IsNullOrWhiteSpace( ou.Description ) )
                pe.DynamicParameters.Add( @"description", ou.Description );
        }

        return CallPlan( planName, pe );
    }


}