﻿using System;
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
        String planName = @"QueryGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "group/{name}" )]
    public LdapHandlerResults DeleteGroup(string name)
    {
        String planName = @"DeleteGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "group/{name}" )]
    public LdapHandlerResults CreateGroup(string name, LdapGroup group)
    {
        String planName = @"CreateGroup";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        if ( !String.IsNullOrWhiteSpace( group.Path ) )
            pe.DynamicParameters.Add( @"path", group.Path );
        if ( !String.IsNullOrWhiteSpace( group.Description ) )
            pe.DynamicParameters.Add( @"description", group.Description );

        pe.DynamicParameters.Add( @"scope", group.Scope.ToString() );
        pe.DynamicParameters.Add( @"securitygroup", group.IsSecurityGroup.ToString() );

        return CallPlan( planName, pe );
    }
}