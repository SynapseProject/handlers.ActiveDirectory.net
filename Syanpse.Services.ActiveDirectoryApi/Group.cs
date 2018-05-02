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
    [Route("group/{identity}")]
    [Route("group/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults GetGroup(string identity, string domain = null)
    {
        string planName = config.Plans.Group.Get;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("group/{identity}")]
    [Route("group/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults DeleteGroup(string identity, string domain = null)
    {
        string planName = config.Plans.Group.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("group/{identity}")]
    [Route("group/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults CreateGroup(string identity, AdGroup group, string domain = null)
    {
        string planName = config.Plans.Group.Create;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), group );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("group/{identity}")]
    [Route("group/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults ModifyGroup(string identity, AdGroup group, string domain = null)
    {
        string planName = config.Plans.Group.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), group );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("group/{identity}/ou/{moveto}")]
    [Route("group/{domain}/{identity}/ou/{movetodomain}/{moveto}")]
    [Route("group/{domain}/{identity}/ou/{moveto}")]
    [Route("group/{identity}/ou/{movetodomain}/{moveto}")]
    public ActiveDirectoryHandlerResults MoveGroup(string identity, string moveto, string domain = null, string movetodomain = null)
    {
        string planName = config.Plans.Group.Move;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(moveto), BuildIdentity(movetodomain, moveto));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("group/{identity}/group/{groupidentity}")]
    [Route("group/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("group/{domain}/{identity}/group/{groupidentity}")]
    [Route("group/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults AddGroupToGroup(string identity, string groupIdentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.Group.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupIdentity));
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("group/{identity}/group/{groupidentity}")]
    [Route("group/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("group/{domain}/{identity}/group/{groupidentity}")]
    [Route("group/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults RemoveGroupFromGroup(string identity, string groupIdentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.Group.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupIdentity));
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("group/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults AddAccessRuleToGroup(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Group.AddAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance );
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("group/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromGroup(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Group.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("group/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{domain}/{identity}/rule/{principal}/{type}/{rights}/{inheritance?}")]
    [Route("group/{identity}/rule/{principaldomain}/{principal}/{type}/{rights}/{inheritance?}")]
    public ActiveDirectoryHandlerResults SetAccessRuleOnGroup(string identity, string principal, string type, string rights, string domain = null, string principaldomain = null, string inheritance = null)
    {
        string planName = config.Plans.Group.SetAccessRule;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), type, rights, inheritance);
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("group/{identity}/rules/{principal}")]
    [Route("group/{domain}/{identity}/rules/{principaldomain}/{principal}")]
    [Route("group/{domain}/{identity}/rules/{principal}")]
    [Route("group/{identity}/rules/{principaldomain}/{principal}")]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnGroup(string identity, string principal, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Group.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule(BuildIdentity(principaldomain, principal), null, null, null );
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), rule );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("group/{identity}/role/{principal}/{role}")]
    [Route("group/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("group/{domain}/{identity}/role/{principal}/{role}")]
    [Route("group/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults AddRoleToGroup(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Group.AddRole;

        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add( nameof( principal ), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("group/{identity}/role/{principal}/{role}")]
    [Route("group/{domain}/{identity}/role/{principaldomain}/{principal}/{role}")]
    [Route("group/{domain}/{identity}/role/{principal}/{role}")]
    [Route("group/{identity}/role/{principaldomain}/{principal}/{role}")]
    public ActiveDirectoryHandlerResults RemoveRoleFromGroup(string identity, string principal, string role, string domain = null, string principaldomain = null)
    {
        string planName = config.Plans.Group.RemoveRole;

        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add( nameof( principal ), BuildIdentity(principaldomain, principal));
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }
}