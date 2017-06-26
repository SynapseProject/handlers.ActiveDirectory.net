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
    public LdapHandlerResults GetUser(string name, bool groups = false, bool runAtNode = true)
    {
        String planName = @"QueryUser";

        if( runAtNode )
        {
            IExecuteController ec = GetExecuteControllerInstance();

            StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
            pe.DynamicParameters.Add( nameof( name ), name );

            IEnumerable<KeyValuePair<string, string>> query = this.Request.GetQueryNameValuePairs();
            foreach ( KeyValuePair<string, string> kvp in query )
                pe.DynamicParameters.Add( kvp.Key, kvp.Value );

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