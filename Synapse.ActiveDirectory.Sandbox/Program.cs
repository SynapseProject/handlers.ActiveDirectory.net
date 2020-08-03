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
using Synapse.Handlers.ActiveDirectory;

namespace Synapse.ActiveDirectory.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            string type = (args.Length > 0) ? args[0] : null;
            string identity = (args.Length > 1) ? args[1] : null;
            string arg2 = (args.Length > 2) ? args[2] : null;

            ActiveDirectoryApiController api = new ActiveDirectoryApiController();
            if (type.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                ActiveDirectoryHandlerResults results = api.GetUser(identity);
                string resultStr = YamlHelpers.Serialize(results, true);
                Console.WriteLine(resultStr);
            }
            else if (type.Equals("group", StringComparison.OrdinalIgnoreCase))
            {
                ActiveDirectoryHandlerResults results = api.GetGroup(identity);
                string resultStr = YamlHelpers.Serialize(results, true);
                Console.WriteLine(resultStr);
            }
            else if (type.Equals("ou", StringComparison.OrdinalIgnoreCase))
            {
                ActiveDirectoryHandlerResults results = api.GetOrgUnit(identity);
                string resultStr = YamlHelpers.Serialize(results, true);
                Console.WriteLine(resultStr);
            }
            else if (type.Equals("computer", StringComparison.OrdinalIgnoreCase))
            {
                ActiveDirectoryHandlerResults results = api.GetComputer(identity);
                string resultStr = YamlHelpers.Serialize(results, true);
                Console.WriteLine(resultStr);
            }
            else if (type.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                AdSearchRequest request = new AdSearchRequest();
                request.Filter = identity;
                request.SearchBase = arg2;
                request.ReturnAttributes = new List<string>();
                request.ReturnAttributes.Add("cn");
                request.ReturnAttributes.Add("distinguishedName");

                ActiveDirectoryHandlerResults results = api.DoSearch(request);
                string resultStr = YamlHelpers.Serialize(results, true);
                Console.WriteLine(resultStr);
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
