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
    [Route( "user/{identity}" )]
    public ActiveDirectoryHandlerResults GetUser(string identity)
    {
        string planName = config.Plans.User.Query;
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
    [Route( "user/{name}/{group}" )]
    public ActiveDirectoryHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = config.Plans.User.RemoveFromGroup;
        StartPlanEnvelope pe =  GetPlanEnvelope( name, group );
        return CallPlan( planName, pe );
    }
}