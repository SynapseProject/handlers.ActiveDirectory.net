using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.ActiveDirectoryApi;
using Synapse.ActiveDirectory.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;

public partial class ActiveDirectoryApiController : ApiController
{
    [HttpGet]
    [Route( "ou/{distinguishedname}" )]
    public ActiveDirectoryHandlerResults GetOrgUnit(string distinguishedname)
    {
        string planName = config.Plans.OrganizationalUnit.Query;
        StartPlanEnvelope pe = GetPlanEnvelopeByDistinguishedName( distinguishedname );
        return CallPlan( planName, pe );
    }

    [HttpGet]
    [Route( "ou/{name}/{path}" )]
    public ActiveDirectoryHandlerResults GetOrgUnit(string name, string path)
    {
        string planName = config.Plans.OrganizationalUnit.Query;
        StartPlanEnvelope pe = GetPlanEnvelopeByNameAndPath( name, path );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{distinguishedname}" )]
    public ActiveDirectoryHandlerResults DeleteOrgUnit(string distinguishedname)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelopeByDistinguishedName( distinguishedname );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{name}/{path}" )]
    public ActiveDirectoryHandlerResults DeleteOrgUnit(string name, string path)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelopeByNameAndPath( name, path );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{distinguishedname}" )]
    public ActiveDirectoryHandlerResults CreateOrgUnit(string distinguishedname, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( distinguishedname, ou );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{name}/{path}" )]
    public ActiveDirectoryHandlerResults CreateOrgUnit(string name, string path, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( name, path, ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "ou/{distinguishedname}" )]
    public ActiveDirectoryHandlerResults ModifyOrgUnit(string distinguishedname, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( distinguishedname, ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "ou/{name}/{path}" )]
    public ActiveDirectoryHandlerResults ModifyOrgUnit(string name, string path, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( name, path, ou );
        return CallPlan( planName, pe );
    }

}