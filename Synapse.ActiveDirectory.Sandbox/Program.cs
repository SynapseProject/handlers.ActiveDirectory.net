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

            DirectoryServices.AddAccessRule( target, "wagug0", ActiveDirectoryRights.Self, AccessControlType.Allow );
            DirectoryServices.AddAccessRule( target, "wagug0", ActiveDirectoryRights.GenericRead, AccessControlType.Allow );
            DirectoryServices.AddAccessRule( target, "wagug0", ActiveDirectoryRights.WriteProperty, AccessControlType.Deny );
            DirectoryServices.AddAccessRule( target, "wagug0", ActiveDirectoryRights.Delete | ActiveDirectoryRights.DeleteTree, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.DeleteAccessRule( target, "wagug0", ActiveDirectoryRights.GenericRead, AccessControlType.Allow );
            DirectoryServices.DeleteAccessRule( target, "wagug0", ActiveDirectoryRights.Delete, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.SetAccessRule( target, "wagug0", ActiveDirectoryRights.CreateChild, AccessControlType.Allow );
            DirectoryServices.SetAccessRule( target, "wagug0", ActiveDirectoryRights.DeleteTree, AccessControlType.Deny );
            WriteAccessRights();

            DirectoryServices.PurgeAccessRules( target, "wagug0" );
            WriteAccessRights();

            Console.WriteLine( "Press <ENTER> To Continue..." );
            Console.ReadLine();
        }

        public static void WriteAccessRights()
        {
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( "TestGroup", false, false, false );

            foreach ( AccessRuleObject rule in gpo.AccessRules )
            {
                if ( rule.IdentityName == "wagug0" )
                {
                    String type = rule.IsInherited ? "INHERITED" : "EXPLICIT ";
                    Console.WriteLine( $">> {type} {rule.IdentityName} = [{rule.ControlType} - {rule.Rights}]" );
                }
            }
            Console.WriteLine( "==================================" );
        }

    }
}
