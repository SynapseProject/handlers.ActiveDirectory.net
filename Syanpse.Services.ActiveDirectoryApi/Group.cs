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
    [Route( "group/{identity}" )]
    public ActiveDirectoryHandlerResults GetGroup(string identity)
    {
        string planName = config.Plans.Group.Get;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{identity}" )]
    public ActiveDirectoryHandlerResults DeleteGroup(string identity)
    {
        string planName = config.Plans.Group.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{identity}" )]
    public ActiveDirectoryHandlerResults CreateGroup(string identity, AdGroup group)
    {
        string planName = config.Plans.Group.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, group );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "group/{identity}" )]
    public ActiveDirectoryHandlerResults ModifyGroup(string identity, AdGroup group)
    {
        string planName = config.Plans.Group.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, group );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{identity}/{groupidentity}" )]
    public ActiveDirectoryHandlerResults AddGroupToGroup(string identity, string groupIdentity)
    {
        string planName = config.Plans.Group.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, groupIdentity );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{identity}/{groupidentity}" )]
    public ActiveDirectoryHandlerResults RemoveGroupFromGroup(string identity, string groupIdentity)
    {
        string planName = config.Plans.Group.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, groupIdentity );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "accessrule/group/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults AddAccessRuleToGroup(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.Group.AddAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/group/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromGroup(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.Group.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "accessrule/group/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults SetAccessRuleOnGroup(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.Group.SetAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/group/{identity}/{principal}" )]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnGroup(string identity, string principal)
    {
        string planName = config.Plans.Group.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule( principal, null, null );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

}