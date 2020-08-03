using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Security.Principal;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.ActiveDirectory.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            string type = (args.Length > 0) ? args[0] : null;
            string identity = (args.Length > 1) ? args[1] : null;
            string arg2 = (args.Length > 2) ? args[2] : null;

            if (type.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                UserPrincipalObject upo = DirectoryServices.GetUser(identity, false, false, true);
                Console.WriteLine(">> DN   : " + upo?.DistinguishedName);
                Console.WriteLine(">> GUID : " + upo?.Guid);
            }
            else if (type.Equals("group", StringComparison.OrdinalIgnoreCase))
            {
                GroupPrincipalObject gpo = DirectoryServices.GetGroup(identity, false, false, true);
                Console.WriteLine(">> DN   : " + gpo?.DistinguishedName);
                Console.WriteLine(">> GUID : " + gpo?.Guid);
            }
            else if (type.Equals("ou", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryEntryObject ou = DirectoryServices.GetOrganizationalUnit(identity, false, false, false);
                Console.WriteLine(">> DN   : " + ou?.DistinguishedName);
                Console.WriteLine(">> GUID : " + ou?.Guid);
            }
            else if (type.Equals("computer", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryEntryObject computer = DirectoryServices.GetComputer(identity, false, false, true);
                Console.WriteLine(">> DN   : " + computer?.DistinguishedName);
                Console.WriteLine(">> GUID : " + computer?.Guid);
            }
            else if (type.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                SearchResultsObject results = DirectoryServices.Search(arg2, identity, null);
                foreach (SearchResultRow row in results.Results)
                    Console.WriteLine(">> " + row.Path);
            }
            else if (type.Equals("encrypt", StringComparison.OrdinalIgnoreCase))
            {
                string pwd = CryptoHelpers.Encrypt(filePath: identity, value: arg2);
                Console.WriteLine(pwd);
            }
            else if (type.Equals("decrypt", StringComparison.OrdinalIgnoreCase))
            {
                string pwd = CryptoHelpers.Decrypt(filePath: identity, value: arg2);
                Console.WriteLine(pwd);
            }

            //Console.WriteLine( "Press <ENTER> To Continue..." );
            //Console.ReadLine();
        }
    }
}
