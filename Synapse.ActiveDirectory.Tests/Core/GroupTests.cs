using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
{
    [TestFixture]
    public class GroupTests
    {
        DirectoryEntry workspace = null;
        String workspaceName = null;
        GroupPrincipal group = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            group = Utility.CreateGroup( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteGroup( group.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_GroupTest()
        {
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
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_GroupNotFound()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Getting Group [{groupName}] Which Should Not Exist." );
            GroupPrincipalObject group = DirectoryServices.GetGroup( groupName, true, true, true );
            Assert.That( group, Is.Null );

            Console.WriteLine( $"Getting Group Principal [{groupName}] Which Should Not Exist." );
            GroupPrincipal gp = DirectoryServices.GetGroupPrincipal( groupDistinguishedName );
            Assert.That( gp, Is.Null );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_CreateGroupBadDistName()
        {
            // Get User That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"GW={groupName},{workspaceName}";

            Console.WriteLine( $"Create Group [{groupDistinguishedName}] With Bad DistinguishedName" );
            GroupPrincipal group = null;
            Assert.Throws<AdException>( () => group = DirectoryServices.CreateGroupPrincipal( groupDistinguishedName ) );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_ModifyGroupBadData()
        {
            GroupPrincipal badGroup = Utility.CreateGroup( workspaceName );

            badGroup.GroupScope = 0;       // Set To Invalid Group Scope
            Console.WriteLine( $"Modify Group [{badGroup.DistinguishedName}] With Bad GroupScope [{badGroup.GroupScope}]" );
            Assert.Throws<AdException>( () => DirectoryServices.SaveGroup( badGroup ) );

            Utility.DeleteGroup( badGroup.Name );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_DeleteGroupDoesNotExist()
        {
            // Get User That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Deleting Group [{groupDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.DeleteGroup( groupName ) ).Message.Contains( "cannot be found" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_AddRuleBadTarget()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Adding AccessRule For Group [{group.Name}] To Group [{groupName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( groupName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_AddRuleBadUser()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Adding AccessRule For Group [{groupName}] Which Should Not Exist To Group [{group.Name}]." );
            Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( group.Name, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_DeleteRuleBadTarget()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Deleting AccessRule For Group [{group.Name}] From Group [{groupName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( groupName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_DeleteRuleBadUser()
        {
            // Get User That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Deleting AccessRule For Group [{groupName}] Which Should Not Exist From Group [{group.Name}]." );
            Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( group.Name, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_SetRuleBadTarget()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For Group [{group.Name}] On Group [{groupName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( groupName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_SetRuleBadUser()
        {
            // Get User That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For Group [{groupName}] Which Should Not Exist On Group [{group.Name}]." );
            Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( group.Name, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_PurgeRuleBadTarget()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Purging AccessRule For Group [{group.Name}] From Group [{groupName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( groupName, group.Name ) ).Message.Contains( "Can Not Be NULL" );
        }

        [Test, Category( "Core" ), Category( "Group" )]
        public void Core_PurgeRuleBadUser()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For Group [{groupName}] Which Should Not Exist On Group [{group.Name}]." );
            Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( group.Name, groupName ) ).Message.Contains( "Can Not Be NULL" );
        }



    }
}
