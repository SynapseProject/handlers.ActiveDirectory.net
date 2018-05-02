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
    [Route("ou/{identity}")]
    [Route("ou/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults GetOrgUnit(string identity, string domain = null)
    {
        string planName = config.Plans.OrganizationalUnit.Get;
        StartPlanEnvelope pe = GetPlanEnvelope( BuildIdentity(domain, identity) );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("ou/{identity}")]
    [Route("ou/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults DeleteOrgUnit(string identity, string domain = null)
    {
        string planName = config.Plans.OrganizationalUnit.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("ou/{identity}")]
    [Route("ou/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults CreateOrgUnit(string identity, AdOrganizationalUnit ou, string domain = null)
    {
        string planName = config.Plans.OrganizationalUnit.Create;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("ou/{identity}")]
    [Route("ou/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults ModifyOrgUnit(string identity, AdOrganizationalUnit ou, string domain = null)
    {
        string planName = config.Plans.OrganizationalUnit.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), ou );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("ou/{identity}/ou/{moveto}")]
    [Route("ou/{domain}/{identity}/ou/{movetodomain}/{moveto}")]
    [Route("ou/{domain}/{identity}/ou/{moveto}")]
    [Route("ou/{identity}/ou/{movetodomain}/{moveto}")]
    public ActiveDirectoryHandlerResults MoveOrgUnit(string identity, string moveto, string domain = null, string movetodomain =  null)
    {
        string planName = config.Plans.OrganizationalUnit.Move;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(moveto), BuildIdentity(movetodomain, moveto));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("ou/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults AddAccessRuleToOrgUnit(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.OrganizationalUnit.AddAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance );
        StartPlanEnvelope pe = GetPlanEnvelope( BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("ou/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromOrgUnit(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.OrganizationalUnit.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("ou/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("ou/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults SetAccessRuleOnOrgUnit(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.OrganizationalUnit.SetAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("ou/{identity}/rules/{principal}")]
    [Route("ou/{domain}/{identity}/rules/{principaldomain}/{principal}")]
    [Route("ou/{domain}/{identity}/rules/{principal}")]
    [Route("ou/{identity}/rules/{principaldomain}/{principal}")]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnOrgUnit(string identity, string principal, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.OrganizationalUnit.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), null, null, null );
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("ou/{identity}/role/{principal}/{role}")]
    [Route("ou/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("ou/{domain}/{identity}/role/{principal}/{role}")]
    [Route("ou/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults AddRoleToOrgUnit(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.OrganizationalUnit.AddRole;

        StartPlanEnvelope pe = GetPlanEnvelope( BuildIdentity(domain, identity) );
        pe.DynamicParameters.Add( nameof( principal ), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("ou/{identity}/role/{principal}/{role}")]
    [Route("ou/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("ou/{domain}/{identity}/role/{principal}/{role}")]
    [Route("ou/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults RemoveRoleFromOrgUnit(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.OrganizationalUnit.RemoveRole;

        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add( nameof( principal ), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }
}