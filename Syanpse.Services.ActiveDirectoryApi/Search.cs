using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.ActiveDirectoryApi;
using Synapse.ActiveDirectory.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;

public partial class ActiveDirectoryApiController : ApiController
{
    [HttpPost]
    [Route( "search" )]
    public ActiveDirectoryHandlerResults Search(AdSearchRequest request)
    {
        return DoSearch(request);
    }

    [HttpPost]
    [Route( "search/{planname}" )]
    public ActiveDirectoryHandlerResults DefinedSearch(string planname, Dictionary<string, string> parameters)
    {
        string planName = planname;
        StartPlanEnvelope pe = GetPlanEnvelope( parameters );
        return CallPlan( planName, pe );
    }

    [HttpGet]
    [Route( "search/{planname}/help" )]
    public object DefinedSearch(string planname)
    {
        //TODO : Implement Me
        return null;
    }
}