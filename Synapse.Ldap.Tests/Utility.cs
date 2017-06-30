using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Tests
{
    public class Utility
    {

        public static string GenerateToken(byte length)
        {
            var bytes = new byte[length];
            var rnd = new Random();
            rnd.NextBytes( bytes );
            return Convert.ToBase64String( bytes ).Replace( "=", "" ).Replace( "+", "" ).Replace( "/", "" );
        }
    }
}
