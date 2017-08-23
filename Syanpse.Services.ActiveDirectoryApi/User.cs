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
    [Route( "user/{name}" )]
    public ActiveDirectoryHandlerResults GetUser(string name)
    {
        string planName = config.Plans.User.Query;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public ActiveDirectoryHandlerResults DeleteUser(string name)
    {
        string planName = config.Plans.User.Delete;
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public ActiveDirectoryHandlerResults CreateUser(string identity, AdUser user)
    {
        string planName = config.Plans.User.Create;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, user );
        return CallPlan( planName, pe );
    }

    [HttpPut]
    [Route( "user/{name}" )]
    public ActiveDirectoryHandlerResults ModifyUser(string identity, AdUser user)
    {
        string planName = config.Plans.User.Modify;
        StartPlanEnvelope pe = GetPlanEnvelope( identity, user );
        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public ActiveDirectoryHandlerResults AddUserToGroup(string name, string group)
    {
        string planName = config.Plans.User.AddToGroup;
        StartPlanEnvelope pe = GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public ActiveDirectoryHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = config.Plans.User.RemoveFromGroup;
        StartPlanEnvelope pe =  GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }
}