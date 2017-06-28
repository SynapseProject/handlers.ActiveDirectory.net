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
        String planName = @"QueryUser";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public LdapHandlerResults DeleteUser(string name)
    {
        String planName = @"DeleteUser";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public LdapHandlerResults CreateUser(string name, LdapUser user)
    {
        String planName = @"CreateUser";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        if ( !String.IsNullOrWhiteSpace( user.Path ) )
            pe.DynamicParameters.Add( @"path", user.Path );
        if ( !String.IsNullOrWhiteSpace( user.Description ) )
            pe.DynamicParameters.Add( @"description", user.Description );
        if ( !String.IsNullOrWhiteSpace( user.Password ) )
            pe.DynamicParameters.Add( @"password", user.Password );
        if ( !String.IsNullOrWhiteSpace( user.GivenName ) )
            pe.DynamicParameters.Add( @"givenname", user.GivenName );
        if ( !String.IsNullOrWhiteSpace( user.Surname ) )
            pe.DynamicParameters.Add( @"surname", user.Surname );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults AddUserToGroup(string name, string group)
    {
        String planName = @"AddUserToGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( group ), group );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults RemoveUserFromGroup(string name, string group)
    {
        String planName = @"RemoveUserFromGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( group ), group );

        return CallPlan( planName, pe );
    }


}