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
        string planName = config.Plans.OrganizationalUnit.Query;
        StartPlanEnvelope pe = GetPlanEnvelopeByDistinguishedName( distinguishedname );
        return CallPlan( planName, pe );
    }

    [HttpGet]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults GetOrgUnit(string name, string path)
    {
        string planName = config.Plans.OrganizationalUnit.Query;
        StartPlanEnvelope pe = GetPlanEnvelopeByNameAndPath( name, path );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults DeleteOrgUnit(string distinguishedname)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelopeByDistinguishedName( distinguishedname );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults DeleteOrgUnit(string name, string path)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelopeByNameAndPath( name, path );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults CreateOrgUnit(string distinguishedname, LdapOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( distinguishedname, ou );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults CreateOrgUnit(string name, string path, LdapOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( name, path, ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults ModifyOrgUnit(string distinguishedname, LdapOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( distinguishedname, ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults ModifyOrgUnit(string name, string path, LdapOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( name, path, ou );
        return CallPlan( planName, pe );
    }

}