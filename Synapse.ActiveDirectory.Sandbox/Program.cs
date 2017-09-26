﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.ActiveDirectory.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( "TestGroup", false );

            Console.WriteLine( YamlHelpers.Serialize( gpo ) );

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }
    }
}
