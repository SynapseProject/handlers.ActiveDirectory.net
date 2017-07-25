using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Security.Principal;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
using Synapse.Ldap.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.Ldap;

[RoutePrefix( "ad" )]
public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "hello" )]
    public string Hello() { return "Hello from LdapApiController, World!"; }

    [HttpGet]
    [Route( "synapse" )]
    public string SynapseHello() { return GetExecuteControllerInstance().Hello(); }

    [HttpGet]
    [Route( "whoami" )]
    public string WhoAmI()
    {
        string planName = @"WhoAmI";

        IExecuteController ec = GetExecuteControllerInstance();
        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        return (string)ec.StartPlanSync( pe, planName, setContentType: false );
    }

    [HttpGet]
    [Route( "object/{type}/{name}" )]
    public async Task<string> GetObject(ObjectClass type, string name)
    {
        IExecuteController ec = GetExecuteControllerInstance();

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( type ), type.ToString() );

        long id = ec.StartPlan( pe, "getObject" );
        StatusType status = await SynapseHelper.GetStatusAsync( ec, "getObject", id );

        return status == StatusType.Success ? (string)ec.GetPlanElements( "getObject", id, "Actions[0]:Result:ExitData" ) : null;
    }

    IExecuteController GetExecuteControllerInstance()
    {
        return ExtensibilityUtility.GetExecuteControllerInstance( Url, User, this.Request?.Headers?.Authorization );
    }

    // Create and Modify User
    private StartPlanEnvelope GetPlanEnvelope(string name, LdapUser user)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( name );
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

        return pe;
    }

    // Create and Modify Group
    private StartPlanEnvelope GetPlanEnvelope(string name, LdapGroup group)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( name );

        if (group != null)
        {
            if ( !string.IsNullOrWhiteSpace( group.Path ) )
                pe.DynamicParameters.Add( @"path", group.Path );
            if ( !string.IsNullOrWhiteSpace( group.Description ) )
                pe.DynamicParameters.Add( @"description", group.Description );
            pe.DynamicParameters.Add( @"scope", group.Scope.ToString() );
            pe.DynamicParameters.Add( @"securitygroup", group.IsSecurityGroup.ToString() );
        }

        return pe;
    }

    // Add/Remove User or Group to a Group
    private StartPlanEnvelope GetPlanEnvelope(string name, string group)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( name );
        if ( group != null )
            pe.DynamicParameters.Add( nameof( group ), group );

        return pe;
    }

    // Base Envelope for All Objects Retrieved By Either Name or DistinguishedName (Users and Groups)
    private StartPlanEnvelope GetPlanEnvelope(string name)
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

        return pe;
    }

    // Base Envelope For All Objects Retrieved By DistinguishedName (OrgUnits)
    private StartPlanEnvelope GetPlanEnvelopeByDistinguishedName(string distinguishedname)
    {
        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
        pe.DynamicParameters.Add( "name", String.Empty );
        pe.DynamicParameters.Add( "path", String.Empty );
        return pe;
    }

    // Base Envelope For All Objects Retrieved By Name and Path (OrgUnits)
    private StartPlanEnvelope GetPlanEnvelopeByNameAndPath(string name, string path)
    {
        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( path ), path );
        pe.DynamicParameters.Add( "distinguishedname", String.Empty );
        return pe;
    }

    // Create and Modify Organizational Unit By DistinguishedName
    private StartPlanEnvelope GetPlanEnvelope(string distinguishedname, LdapOrganizationalUnit ou)
    {
        StartPlanEnvelope pe = GetPlanEnvelopeByDistinguishedName( distinguishedname );
        if ( ou != null )
        {
            if ( !string.IsNullOrWhiteSpace( ou.Description ) )
                pe.DynamicParameters.Add( @"description", ou.Description );
        }
        return pe;
    }

    // Create and Modify Organizational Unit By Name and Path
    private StartPlanEnvelope GetPlanEnvelope(string name, string path, LdapOrganizationalUnit ou)
    {
        StartPlanEnvelope pe = GetPlanEnvelopeByNameAndPath( name, path );
        if ( ou != null )
        {
            if ( !string.IsNullOrWhiteSpace( ou.Description ) )
                pe.DynamicParameters.Add( @"description", ou.Description );
        }
        return pe;
    }


    private LdapHandlerResults CallPlan(string planName, StartPlanEnvelope planEnvelope)
    {
        IExecuteController ec = GetExecuteControllerInstance();
        StartPlanEnvelope pe = planEnvelope;

        if (pe == null)
            pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };

        IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
        foreach ( KeyValuePair<string, string> kvp in query )
            pe.DynamicParameters.Add( kvp.Key, kvp.Value );

        string reply = (string)ec.StartPlanSync( pe, planName, setContentType: false );
        return YamlHelpers.Deserialize<LdapHandlerResults>( reply );
    }

    private bool IsDistinguishedName(String name)
    {
        return Regex.IsMatch( name, @"^\s*?(cn\s*=|ou\s*=|dc\s*=)", RegexOptions.IgnoreCase );
    }
}