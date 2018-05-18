using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;

public partial class ActiveDirectoryApiController : ApiController
{
    [HttpGet]
    [Route("computer/{identity}")]
    [Route("computer/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults GetComputer(string identity, string domain = null)
    {
        string planName = config.Plans.Computer.Get;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan(planName, pe);
    }

    [HttpDelete]
    [Route("computer/{identity}")]
    [Route("computer/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults DeleteComputer(string identity, string domain = null)
    {
        string planName = config.Plans.Computer.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("computer/{identity}")]
    [Route("computer/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults CreateComputer(string identity, AdComputer computer, string domain = null)
    {
        string planName = config.Plans.Computer.Create;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), computer);
        return CallPlan(planName, pe);
    }

    [HttpPut]
    [Route("computer/{identity}")]
    [Route("computer/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults ModifyComputer(string identity, AdComputer computer, string domain = null)
    {
        string planName = config.Plans.Computer.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), computer);
        return CallPlan(planName, pe);
    }

    [HttpPut]
    [Route("computer/{identity}/ou/{moveto}")]
    [Route("computer/{domain}/{identity}/ou/{movetodomain}/{moveto}")]
    [Route("computer/{domain}/{identity}/ou/{moveto}")]
    [Route("computer/{identity}/ou/{movetodomain}/{moveto}")]
    public ActiveDirectoryHandlerResults MoveComputer(string identity, string moveto, string domain = null, string movetodomain = null)
    {
        string planName = config.Plans.Computer.Move;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(moveto), BuildIdentity(movetodomain, moveto));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("computer/{identity}/group/{groupidentity}")]
    [Route("computer/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("computer/{domain}/{identity}/group/{groupidentity}")]
    [Route("computer/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults AddComputerToGroup(string identity, string groupIdentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.Computer.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupIdentity));
        return CallPlan(planName, pe);
    }

    [HttpDelete]
    [Route("computer/{identity}/group/{groupidentity}")]
    [Route("computer/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("computer/{domain}/{identity}/group/{groupidentity}")]
    [Route("computer/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults RemoveComputerFromGroup(string identity, string groupIdentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.Computer.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupIdentity));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("computer/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults AddAccessRuleToComputer(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Computer.AddAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule);
        return CallPlan(planName, pe);
    }

    [HttpDelete]
    [Route("computer/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromComputer(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Computer.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule);
        return CallPlan(planName, pe);
    }

    [HttpPut]
    [Route("computer/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("computer/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults SetAccessRuleOnComputer(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Computer.SetAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule);
        return CallPlan(planName, pe);
    }

    [HttpDelete]
    [Route("computer/{identity}/rules/{principal}")]
    [Route("computer/{domain}/{identity}/rules/{principaldomain}/{principal}")]
    [Route("computer/{domain}/{identity}/rules/{principal}")]
    [Route("computer/{identity}/rules/{principaldomain}/{principal}")]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnComputer(string identity, string principal, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Computer.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), null, null, null);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule);
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("computer/{identity}/role/{principal}/{role}")]
    [Route("computer/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("computer/{domain}/{identity}/role/{principal}/{role}")]
    [Route("computer/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults AddRoleToComputer(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Computer.AddRole;

        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(principal), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add(nameof(role), role);

        return CallPlan(planName, pe);
    }

    [HttpDelete]
    [Route("computer/{identity}/role/{principal}/{role}")]
    [Route("computer/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("computer/{domain}/{identity}/role/{principal}/{role}")]
    [Route("computer/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults RemoveRoleFromComputer(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Computer.RemoveRole;

        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(principal), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add(nameof(role), role);

        return CallPlan(planName, pe);
    }
}