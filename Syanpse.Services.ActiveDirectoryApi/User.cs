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
    [Route("user/{identity}")]
    [Route("user/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults GetUser(string identity, string domain = null)
    {
        string planName = config.Plans.User.Get;
        StartPlanEnvelope pe = GetPlanEnvelope( BuildIdentity(domain, identity) );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("user/{identity}")]
    [Route("user/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults DeleteUser(string identity, string domain = null)
    {
        string planName = config.Plans.User.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        return CallPlan(planName, pe);
    }

    [HttpPut]
    [Route("user/{identity}/ou/{moveto}")]
    [Route("user/{domain}/{identity}/ou/{movetodomain}/{moveto}")]
    [Route("user/{domain}/{identity}/ou/{moveto}")]
    [Route("user/{identity}/ou/{movetodomain}/{moveto}")]
    public ActiveDirectoryHandlerResults MoveUser(string identity, string moveto, string domain = null, string movetodomain = null)
    {
        string planName = config.Plans.User.Move;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity));
        pe.DynamicParameters.Add(nameof(moveto), BuildIdentity(movetodomain, moveto));
        return CallPlan(planName, pe);
    }

    [HttpPost]
    [Route("user/{identity}")]
    [Route("user/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults CreateUser(string identity, AdUser user, string domain = null)
    {
        string planName = config.Plans.User.Create;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), user );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route("user/{identity}")]
    [Route("user/{domain}/{identity}")]
    public ActiveDirectoryHandlerResults ModifyUser(string identity, AdUser user, string domain = null)
    {
        string planName = config.Plans.User.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), user );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route("user/{identity}/group/{groupidentity}")]
    [Route("user/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("user/{domain}/{identity}/group/{groupidentity}")]
    [Route("user/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults AddUserToGroup(string identity, string groupidentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.User.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupidentity));
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route("user/{identity}/group/{groupidentity}")]
    [Route("user/{domain}/{identity}/group/{groupdomain}/{groupidentity}")]
    [Route("user/{domain}/{identity}/group/{groupidentity}")]
    [Route("user/{identity}/group/{groupdomain}/{groupidentity}")]
    public ActiveDirectoryHandlerResults RemoveUserFromGroup(string identity, string groupidentity, string domain = null, string groupdomain = null)
    {
        string planName = config.Plans.User.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope(BuildIdentity(domain, identity), BuildIdentity(groupdomain, groupidentity));
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

    [HttpPost]
    [Route( "role/user/{identity}/{principal}/{role}" )]
    public ActiveDirectoryHandlerResults AddRoleToUser(string identity, string principal, string role)
    {
        string planName = config.Plans.User.AddRole;

        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        pe.DynamicParameters.Add( nameof( principal ), principal );
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "role/user/{identity}/{principal}/{role}" )]
    public ActiveDirectoryHandlerResults RemoveRoleFromUser(string identity, string principal, string role)
    {
        string planName = config.Plans.User.RemoveRole;

        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        pe.DynamicParameters.Add( nameof( principal ), principal );
        pe.DynamicParameters.Add( nameof( role ), role );

        return CallPlan( planName, pe );
    }
}