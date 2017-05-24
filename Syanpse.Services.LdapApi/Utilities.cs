using System;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.Services.LdapApi
{
    class SynapseHelper
    {
        public static T ExecuteAsync<T>(IExecuteController ec, string planName, StartPlanEnvelope pe, string path = "Actions[0]:Result:ExitData")
        {
            long id = ec.StartPlan( pe, planName );
            StatusType status = Task.Run( () => GetStatus( ec, planName, id ) ).Result;
            if( status == StatusType.Success )
                return YamlHelpers.Deserialize<T>( ec.GetPlanElements( planName, id, path ).ToString() );
            else
                return default( T );
        }
        public static Task<StatusType> ExecuteAsync(IExecuteController ec, string planName, StartPlanEnvelope pe, out long id)
        {
            long pid = id = ec.StartPlan( pe, "GetOrgUnit" );
            return Task.Run( () => GetStatus( ec, planName, pid ) );
        }
        public static StatusType GetStatus(IExecuteController ec, string planName, long id)
        {
            int c = 0;
            StatusType status = StatusType.New;
            while( c < 30 )
            {
                System.Threading.Thread.Sleep( 1000 );
                try { Enum.TryParse( ec.GetPlanElements( planName, id, "Result:Status" ).ToString(), out status ); } catch { }
                c = status < StatusType.Success ? c + 1 : int.MaxValue;
            }
            return status;
        }
        public static Task<StatusType> GetStatusAsync(IExecuteController ec, string planName, long id)
        {
            return Task.Run( () => GetStatus( ec, planName, id ) );
        }
    }
}