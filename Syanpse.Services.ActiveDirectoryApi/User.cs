using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Security.AccessControl;
using System.DirectoryServices;

using Synapse.Core;
using Synapse.Services;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;


public partial class ActiveDirectoryApiController : ApiController
{
    [HttpGet]
    [Route( "user/{identity}" )]
    public ActiveDirectoryHandlerResults GetUser(string identity)
    {
        string planName = config.Plans.User.Get;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{identity}" )]
    public ActiveDirectoryHandlerResults DeleteUser(string identity)
    {
        string planName = config.Plans.User.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{identity}" )]
    public ActiveDirectoryHandlerResults CreateUser(string identity, AdUser user)
    {
        string planName = config.Plans.User.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, user );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "user/{identity}" )]
    public ActiveDirectoryHandlerResults ModifyUser(string identity, AdUser user)
    {
        string planName = config.Plans.User.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, user );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{identity}/{groupidentity}" )]
    public ActiveDirectoryHandlerResults AddUserToGroup(string identity, string groupidentity)
    {
        string planName = config.Plans.User.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, groupidentity );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{identity}/{groupidentity}" )]
    public ActiveDirectoryHandlerResults RemoveUserFromGroup(string identity, string groupIdentity)
    {
        string planName = config.Plans.User.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, groupIdentity );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "accessrule/user/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults AddAccessRuleToUser(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.User.AddAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/user/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults RemoveAccessRuleFromUser(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.User.RemoveAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "accessrule/user/{identity}/{principal}/{type}/{rights}" )]
    public ActiveDirectoryHandlerResults SetAccessRuleOnUser(string identity, string principal, string type, string rights)
    {
        string planName = config.Plans.User.SetAccessRule;

        AdAccessRule rule = CreateAccessRule( principal, type, rights );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "accessrule/user/{identity}/{principal}" )]
    public ActiveDirectoryHandlerResults PurgeAccessRulesOnUser(string identity, string principal)
    {
        string planName = config.Plans.User.PurgeAccessRules;

        AdAccessRule rule = CreateAccessRule( principal, null, null );
        StartPlanEnvelope pe = GetPlanEnvelope( identity, rule );
        return CallPlan( planName, pe );
    }


}