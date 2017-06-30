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

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        if ( IsDistinguishedName( name ) )
        {
            String distinguishedname = name;
            pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
            pe.DynamicParameters.Add( nameof( name ), String.Empty );
        }
        else
        {
            String distinguishedname = String.Empty;
            pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
            pe.DynamicParameters.Add( nameof( name ), name );
        }

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public LdapHandlerResults DeleteUser(string name)
    {
        string planName = @"DeleteUser";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public LdapHandlerResults CreateUser(string name, LdapUser user)
    {
        string planName = @"CreateUser";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        if ( !string.IsNullOrWhiteSpace( user.Path ) )
            pe.DynamicParameters.Add( @"path", user.Path );
        if ( !string.IsNullOrWhiteSpace( user.Description ) )
            pe.DynamicParameters.Add( @"description", user.Description );
        if ( !string.IsNullOrWhiteSpace( user.Password ) )
            pe.DynamicParameters.Add( @"password", user.Password );
        if ( !string.IsNullOrWhiteSpace( user.GivenName ) )
            pe.DynamicParameters.Add( @"givenname", user.GivenName );
        if ( !string.IsNullOrWhiteSpace( user.Surname ) )
            pe.DynamicParameters.Add( @"surname", user.Surname );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults AddUserToGroup(string name, string group)
    {
        string planName = @"AddUserToGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( group ), group );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = @"RemoveUserFromGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( group ), group );

        return CallPlan( planName, pe );
    }


}