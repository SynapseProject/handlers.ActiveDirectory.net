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
    [Route( "user/{name}" )]
    public LdapHandlerResults GetUser(string name)
    {
        string planName = config.Plans.User.Query;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public LdapHandlerResults DeleteUser(string name)
    {
        string planName = config.Plans.User.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public LdapHandlerResults CreateUser(string name, LdapUser user)
    {
        string planName = config.Plans.User.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( name, user );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "user/{name}" )]
    public LdapHandlerResults ModifyUser(string name, LdapUser user)
    {
        string planName = config.Plans.User.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( name, user );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults AddUserToGroup(string name, string group)
    {
        string planName = config.Plans.User.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = config.Plans.User.RemoveFromGroup;
        StartPlanEnvelope pe =  GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }
}