using System;
using System.Threading.Tasks;

using Synapse.Core;


namespace Synapse.Services.LdapApi
{
    class StatusHelper
    {
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