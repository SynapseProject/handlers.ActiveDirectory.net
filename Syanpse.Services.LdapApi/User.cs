using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
using Synapse.Ldap.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.Ldap;


public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "user/{name}" )]
    public LdapHandlerResults GetUser(string name, bool groups = false, bool runAtNode = true)
    {
        String planName = @"QueryUser";
        if( runAtNode )
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            pe.DynamicParameters.Add( nameof( name ), name );

            String reply = (String)ec.StartPlanSync( pe, planName, setContentType: false );
            return YamlHelpers.Deserialize<LdapHandlerResults>( reply );
        }
        else
        {
//            return DirectoryServices.GetUser( name, false );
            return null;
        }
    }
}