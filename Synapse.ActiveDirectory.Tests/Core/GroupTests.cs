using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests
{
    [TestFixture]
    public class GroupTests
    {
        DirectoryEntry workspace = null;
        GroupPrincipal group = null;

        [Test]
        public void Core_GroupTest()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();

            // Create Group
            group = Utility.CreateGroup( workspaceName );

            // Get Group By Distinguished Name
            Console.WriteLine( $"Getting Group By DisginguishedName : [{group.DistinguishedName}]" );
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( group.DistinguishedName, true, true, true );
            Assert.That( gpo.DistinguishedName, Is.EqualTo( group.DistinguishedName ) );

            String groupName = gpo.Name;
            String groupSamAccountName = gpo.SamAccountName;
            Guid? groupGuid = gpo.Guid;
            String groupSid = gpo.Sid;

            // Get Group By Name
            Console.WriteLine( $"Getting Group By Name: [{groupName}]" );
            gpo = DirectoryServices.GetGroup( groupName, true, true, true );
            Assert.That( gpo.Name, Is.EqualTo( groupName ) );

            // Get Group By SamAccountName
            Console.WriteLine( $"Getting Group By SamAccountName : [{groupSamAccountName}]" );
            gpo = DirectoryServices.GetGroup( groupSamAccountName, true, true, true );
            Assert.That( gpo.SamAccountName, Is.EqualTo( groupSamAccountName ) );

            // Get Group By Guid
            Console.WriteLine( $"Getting Group By Guid : [{groupGuid}]" );
            gpo = DirectoryServices.GetGroup( groupGuid.ToString(), true, true, true );
            Assert.That( gpo.Guid, Is.EqualTo( groupGuid ) );

            // Get Group By Sid
            Console.WriteLine( $"Getting Group By Sid : [{groupSid}]" );
            gpo = DirectoryServices.GetGroup( groupSid, true, true, true );
            Assert.That( gpo.Sid, Is.EqualTo( groupSid ) );

            // Modify Group
            Console.WriteLine( $"Modifying Group : [{groupName}]" );
            GroupPrincipal gp = DirectoryServices.GetGroupPrincipal( groupName );
            gp.DisplayName = "Unit Test Group";
            gp.Description = "Unit Test Description";
            DirectoryServices.SaveGroup( gp );

            gpo = DirectoryServices.GetGroup( groupName, false, false, false );
            Assert.That( gpo.DisplayName, Is.EqualTo( "Unit Test Group" ) );
            Assert.That( gpo.Description, Is.EqualTo( "Unit Test Description" ) );


            // Create AccessUser For AccessRule Tests (Below)
            UserPrincipal accessRuleUser = Utility.CreateUser( workspaceName );
            int ruleCount = DirectoryServices.GetAccessRules( group ).Count;

            // Add Access Rule To Group
            Console.WriteLine( $"Adding AccessRule For User [{accessRuleUser.Name}] To Group [{group.Name}]." );
            DirectoryServices.AddAccessRule( group, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            int newRuleCount = DirectoryServices.GetAccessRules( group ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Removing Access Rule From Group
            Console.WriteLine( $"Removing AccessRule For User [{accessRuleUser.Name}] From Group [{group.Name}]." );
            DirectoryServices.DeleteAccessRule( group, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( group ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Seting Access Rule From Group
            Console.WriteLine( $"Setting AccessRule For User [{accessRuleUser.Name}] On Group [{group.Name}]." );
            DirectoryServices.SetAccessRule( group, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( group ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Purge Access Rule From Group
            Console.WriteLine( $"Purging AccessRules For User [{accessRuleUser.Name}] From Group [{group.Name}]." );
            DirectoryServices.PurgeAccessRules( group, accessRuleUser );
            newRuleCount = DirectoryServices.GetAccessRules( group ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Delete AccessRule User 
            Utility.DeleteUser( accessRuleUser.DistinguishedName );





            // Delete Group
            Utility.DeleteGroup( group.DistinguishedName );

            // Cleanup Workspace
            Utility.DeleteWorkspace( workspaceName );

        }
    }
}
