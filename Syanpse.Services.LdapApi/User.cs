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
        string planName = @"QueryUser";
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public LdapHandlerResults DeleteUser(string name)
    {
        string planName = @"DeleteUser";
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public LdapHandlerResults CreateUser(string name, LdapUser user)
    {
        string planName = @"CreateUser";
        StartPlanEnvelope pe = GetPlanEnvelope( name, user );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "user/{name}" )]
    public LdapHandlerResults ModifyUser(string name, LdapUser user)
    {
        string planName = @"ModifyUser";
        StartPlanEnvelope pe = GetPlanEnvelope( name, user );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults AddUserToGroup(string name, string group)
    {
        string planName = @"AddUserToGroup";
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = @"RemoveUserFromGroup";
        StartPlanEnvelope pe =  GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }
}