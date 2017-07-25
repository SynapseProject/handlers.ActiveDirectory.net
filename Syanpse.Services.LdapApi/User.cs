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
        return RunUserPlan( planName, name );
    }

    [HttpDelete]
    [Route( "user/{name}" )]
    public LdapHandlerResults DeleteUser(string name)
    {
        string planName = @"DeleteUser";
        return RunUserPlan( planName, name );
    }

    [HttpPost]
    [Route( "user/{name}" )]
    public LdapHandlerResults CreateUser(string name, LdapUser user)
    {
        string planName = @"CreateUser";
        return RunUserPlan( planName, name, user );
    }

    [HttpPut]
    [Route( "user/{name}" )]
    public LdapHandlerResults ModifyUser(string name, LdapUser user)
    {
        string planName = @"ModifyUser";
        return RunUserPlan( planName, name, user );
    }

    [HttpPost]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults AddUserToGroup(string name, string group)
    {
        string planName = @"AddUserToGroup";
        return RunUserPlan( planName, name, null, group );
    }

    [HttpDelete]
    [Route( "user/{name}/{group}" )]
    public LdapHandlerResults RemoveUserFromGroup(string name, string group)
    {
        string planName = @"RemoveUserFromGroup";
        return RunUserPlan( planName, name, null, group );
    }

    private LdapHandlerResults RunUserPlan(string planName, string name, LdapUser user = null, string group = null)
    {
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

        if ( user != null )
        {
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
        }

        if (group != null)
            pe.DynamicParameters.Add( nameof( group ), group );


        return CallPlan( planName, pe );
    }

}