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
    [Route( "ou/{identity}" )]
    public ActiveDirectoryHandlerResults GetOrgUnit(string identity)
    {
        string planName = config.Plans.OrganizationalUnit.Get;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{identity}" )]
    public ActiveDirectoryHandlerResults DeleteOrgUnit(string identity)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{identity}" )]
    public ActiveDirectoryHandlerResults CreateOrgUnit(string identity, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "ou/{identity}" )]
    public ActiveDirectoryHandlerResults ModifyOrgUnit(string identity, AdOrganizationalUnit ou)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, ou );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "accessrule/ou/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults AddAccessRuleToOrgUnit(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.OrganizationalUnit.AddAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/ou/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromOrgUnit(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.OrganizationalUnit.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "accessrule/ou/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults SetAccessRuleOnOrgUnit(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.OrganizationalUnit.SetAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/ou/{identity}/{principal}" )]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnOrgUnit(string identity, string principal)
    {
        string planName = config.Plans.OrganizationalUnit.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule( principal, null, null );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }
}