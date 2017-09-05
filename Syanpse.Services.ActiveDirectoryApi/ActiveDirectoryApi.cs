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
using Synapse.Services.ActiveDirectoryApi;
using Synapse.ActiveDirectory.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;

[RoutePrefix( "ad" )]
public partial class ActiveDirectoryApiController : ApiController
{
    ActiveDirectoryApiConfig config = ActiveDirectoryApiConfig.DeserializeOrNew();

    [HttpGet]
    [Route( "hello" )]
    public string Hello() { return "Hello from ActiveDirectoryApiController, World!"; }

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
    public async Task<string> GetObject(AdObjectType type, string name)
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
    private StartPlanEnvelope GetPlanEnvelope(string identity, AdUser user)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        if ( user != null )
        {
            // Set Principal Fields
            if ( !string.IsNullOrWhiteSpace( user.Description ) )
                pe.DynamicParameters.Add( @"description", user.Description );
            if ( !string.IsNullOrWhiteSpace( user.UserPrincipalName ) )
                pe.DynamicParameters.Add( @"userprincipalname", user.UserPrincipalName );
            if ( !string.IsNullOrWhiteSpace( user.SamAccountName ) )
                pe.DynamicParameters.Add( @"samaccountname", user.SamAccountName );
            if ( !string.IsNullOrWhiteSpace( user.DisplayName ) )
                pe.DynamicParameters.Add( @"displayname", user.DisplayName );

            // Set AuthenticationPrincipal Fields
            if ( user.Enabled != null )
                pe.DynamicParameters.Add( @"enabled", user.Enabled.ToString() );
            if (user.PermittedLogonTimes != null)
                pe.DynamicParameters.Add( @"permittedlogontimes", user.PermittedLogonTimes.ToString() );
            if ( user.AccountExpirationDate != null  )
                pe.DynamicParameters.Add( @"accountexpirationdate", user.AccountExpirationDate.ToString() );
            if ( user.SmartcardLogonRequired != null  )
                pe.DynamicParameters.Add( @"smartcardlogonrequired", user.SmartcardLogonRequired.ToString() );
            if ( user.DelegationPermitted != null )
                pe.DynamicParameters.Add( @"delegationpermitted", user.DelegationPermitted.ToString() );
            if ( !string.IsNullOrWhiteSpace( user.HomeDirectory ) )
                pe.DynamicParameters.Add( @"homedirectory", user.HomeDirectory );
            if ( !string.IsNullOrWhiteSpace( user.ScriptPath ) )
                pe.DynamicParameters.Add( @"scriptpath", user.ScriptPath );
            if ( user.PasswordNotRequired != null )
                pe.DynamicParameters.Add( @"passwordnotrequired", user.PasswordNotRequired.ToString() );
            if ( user.PasswordNeverExpires != null )
                pe.DynamicParameters.Add( @"passwordneverexpires", user.PasswordNeverExpires.ToString() );
            if ( user.UserCannotChangePassword != null )
                pe.DynamicParameters.Add( @"usercannotchangepassword", user.UserCannotChangePassword.ToString() );
            if ( user.AllowReversiblePasswordEncryption != null )
                pe.DynamicParameters.Add( @"allowreversiblepasswordencryption", user.AllowReversiblePasswordEncryption.ToString() );
            if ( !string.IsNullOrWhiteSpace( user.HomeDrive ) )
                pe.DynamicParameters.Add( @"homedrive", user.HomeDrive );

            // Set UserPrincipalFields
            if ( !string.IsNullOrWhiteSpace( user.Password ) )
                pe.DynamicParameters.Add( @"password", user.Password );
            if ( !string.IsNullOrWhiteSpace( user.GivenName ) )
                pe.DynamicParameters.Add( @"givenname", user.GivenName );
            if ( !string.IsNullOrWhiteSpace( user.Surname ) )
                pe.DynamicParameters.Add( @"surname", user.Surname );
            if ( !string.IsNullOrWhiteSpace( user.EmailAddress ) )
                pe.DynamicParameters.Add( @"emailaddress", user.EmailAddress );
            if ( !string.IsNullOrWhiteSpace( user.VoiceTelephoneNumber ) )
                pe.DynamicParameters.Add( @"voicetelephonenumber", user.VoiceTelephoneNumber );
            if ( !string.IsNullOrWhiteSpace( user.EmployeeId ) )
                pe.DynamicParameters.Add( @"employeeid", user.EmployeeId );

        }

        return pe;
    }

    // Create and Modify Group
    private StartPlanEnvelope GetPlanEnvelope(string identity, AdGroup group)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( identity );

        if (group != null)
        {
            if ( !string.IsNullOrWhiteSpace( group.Description ) )
                pe.DynamicParameters.Add( @"description", group.Description );
            if ( !string.IsNullOrWhiteSpace( group.SamAccountName ) )
                pe.DynamicParameters.Add( @"samaccountname", group.SamAccountName );
            if ( group.Scope != null)  
                pe.DynamicParameters.Add( @"scope", group.Scope.ToString() );
            if (group.IsSecurityGroup != null)
                pe.DynamicParameters.Add( @"securitygroup", group.IsSecurityGroup.ToString() );
        }

        return pe;
    }

    // Add/Remove User or Group to a Group
    private StartPlanEnvelope GetPlanEnvelope(string identity, string groupidentity)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        if ( groupidentity != null )
            pe.DynamicParameters.Add( nameof( groupidentity ), groupidentity );

        return pe;
    }

    // Base Envelope for All Objects Retrieved By Either Name or DistinguishedName (Users, Groups and OrgUnits)
    private StartPlanEnvelope GetPlanEnvelope(string identity)
    {
        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( identity ), identity );

        return pe;
    }

    // Create and Modify Organizational Unit By DistinguishedName
    private StartPlanEnvelope GetPlanEnvelope(string identity, AdOrganizationalUnit ou)
    {
        StartPlanEnvelope pe = GetPlanEnvelope( identity );
        if ( ou != null )
        {
            if ( !string.IsNullOrWhiteSpace( ou.Description ) )
                pe.DynamicParameters.Add( @"description", ou.Description );

            AddPropertiesToPlan( pe, ou.Properties );
        }
        return pe;
    }

    private ActiveDirectoryHandlerResults CallPlan(string planName, StartPlanEnvelope planEnvelope)
    {
        IExecuteController ec = GetExecuteControllerInstance();
        StartPlanEnvelope pe = planEnvelope;

        if (pe == null)
            pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };

        IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
        foreach ( KeyValuePair<string, string> kvp in query )
            pe.DynamicParameters.Add( kvp.Key, kvp.Value );

        string reply = (string)ec.StartPlanSync( pe, planName, setContentType: false );
        return YamlHelpers.Deserialize<ActiveDirectoryHandlerResults>( reply );
    }

    private void AddPropertiesToPlan(StartPlanEnvelope pe, Dictionary<String, List<String>> properties)
    {
        if ( properties != null )
        {
            foreach ( KeyValuePair<string, List<string>> property in properties )
            {
                if ( property.Value?.Count > 0 && !(String.IsNullOrWhiteSpace(property.Key)) )
                {
                    String pName = property.Key.ToLower();
                    String pValue = property.Value[0];     // TODO : Figure Out How To Pass Multiple Values
                    pe.DynamicParameters.Add( pName, pValue );
                }
            }
        }
    }

    private bool IsDistinguishedName(String name)
    {
        return Regex.IsMatch( name, @"^\s*?(cn\s*=|ou\s*=|dc\s*=)", RegexOptions.IgnoreCase );
    }
}