using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;

using Synapse.Core;
using Synapse.Core.Utilities;

namespace Synapse.ActiveDirectory.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Principal target = DirectoryServices.GetPrincipal( "TestGroup" );

            DirectoryServices.AddAccessRights( target, "wagug0", ActiveDirectoryRights.Self, AccessControlType.Allow );
            DirectoryServices.AddAccessRights( target, "wagug0", ActiveDirectoryRights.GenericRead, AccessControlType.Allow );
            DirectoryServices.AddAccessRights( target, "wagug0", ActiveDirectoryRights.WriteProperty, AccessControlType.Deny );
            DirectoryServices.AddAccessRights( target, "wagug0", ActiveDirectoryRights.Delete | ActiveDirectoryRights.DeleteTree, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.DeleteAccessRights( target, "wagug0", ActiveDirectoryRights.GenericRead, AccessControlType.Allow );
            DirectoryServices.DeleteAccessRights( target, "wagug0", ActiveDirectoryRights.Delete, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.SetAccessRights( target, "wagug0", ActiveDirectoryRights.CreateChild, AccessControlType.Allow );
            DirectoryServices.SetAccessRights( target, "wagug0", ActiveDirectoryRights.DeleteTree, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.PurgeAccessRights( target, "wagug0" );
            WriteAccessRights();

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        public static void WriteAccessRights()
        {
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( "TestGroup", false );

            foreach ( AccessRuleObject rule in gpo.AccessRules )
            {
                if ( rule.Principal.Name == "wagug0" )
                {
                    String type = rule.IsInherited ? "INHERITED" : "EXPLICIT ";
                    Console.WriteLine( $">> {type} {rule.Principal.Name} = [{rule.ControlType} - {rule.Rights}]" );
                }
            }
            Console.WriteLine( "==================================" );
        }

    }
}
