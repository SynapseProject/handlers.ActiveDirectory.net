using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Core.Utilities;
using Synapse.Handlers.Ldap;

public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "group/{name}" )]
    public LdapHandlerResults GetGroup(string name)
    {
        string planName = config.Plans.Group.Query;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{name}" )]
    public LdapHandlerResults DeleteGroup(string name)
    {
        string planName = config.Plans.Group.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{name}" )]
    public LdapHandlerResults CreateGroup(string name, LdapGroup group)
    {
        string planName = config.Plans.Group.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "group/{name}" )]
    public LdapHandlerResults ModifyGroup(string name, LdapGroup group)
    {
        string planName = config.Plans.Group.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{name}/{group}" )]
    public LdapHandlerResults AddGroupToGroup(string name, string group)
    {
        string planName = config.Plans.Group.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{name}/{group}" )]
    public LdapHandlerResults RemoveGroupFromGroup(string name, string group)
    {
        string planName = config.Plans.Group.RemoveFromGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

}