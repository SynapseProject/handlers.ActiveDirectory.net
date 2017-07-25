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
        string planName = @"QueryGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{name}" )]
    public LdapHandlerResults DeleteGroup(string name)
    {
        string planName = @"DeleteGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{name}" )]
    public LdapHandlerResults CreateGroup(string name, LdapGroup group)
    {
        string planName = @"CreateGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "group/{name}" )]
    public LdapHandlerResults ModifyGroup(string name, LdapGroup group)
    {
        string planName = @"ModifyGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{name}/{group}" )]
    public LdapHandlerResults AddGroupToGroup(string name, string group)
    {
        string planName = @"AddGroupToGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{name}/{group}" )]
    public LdapHandlerResults RemoveGroupFromGroup(string name, string group)
    {
        string planName = @"RemoveGroupFromGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

}