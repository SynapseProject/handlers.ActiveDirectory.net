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
            string type = args[0];
            string identity = args[1];
            if (type.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                UserPrincipalObject upo = DirectoryServices.GetUser(identity, false, false, true);
                Console.WriteLine(">> DN : " + upo?.DistinguishedName);
            }
            else if (type.Equals("group", StringComparison.OrdinalIgnoreCase))
            {
                GroupPrincipalObject gpo = DirectoryServices.GetGroup(identity, false, false, true);
                Console.WriteLine(">> DN : " + gpo?.DistinguishedName);
            }
            else if (type.Equals("ou", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryEntryObject ou = DirectoryServices.GetOrganizationalUnit(identity, false, false, false);
                Console.WriteLine(">> DN : " + ou?.DistinguishedName);
            }
            else if (type.Equals("computer", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryEntryObject computer = DirectoryServices.GetComputer(identity, false, false, true);
                Console.WriteLine(">> DN : " + computer?.DistinguishedName);
            }
            else if (type.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                SearchResultsObject results = DirectoryServices.Search(null, identity, null);
                foreach (SearchResultRow row in results.Results)
                    Console.WriteLine(">> " + row.Path);
            }

            //Console.WriteLine( "Press <ENTER> To Continue..." );
            //Console.ReadLine();
        }
    }
}
