using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
{
    [TestFixture]
    public class GroupManagementTests
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

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_GroupManagementTestSuccess()
        {
            // Add User To Group
            UserPrincipal user = Utility.CreateUser( workspaceName );
            UserPrincipalObject upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            int beforeCount = upo.Groups.Count;
            Console.WriteLine( $"Adding User [{user.Name}] To Group [{group.Name}]." );
            DirectoryServices.AddToGroup(group.DistinguishedName, user.DistinguishedName, "user");
            upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            int afterCount = upo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount + 1 ) );

            // Remove User From Group
            beforeCount = afterCount;
            Console.WriteLine( $"Removing User [{user.Name}] From Group [{group.Name}]." );
            DirectoryServices.RemoveFromGroup(group.DistinguishedName, user.DistinguishedName, "user");
            upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            afterCount = upo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount - 1 ) );

            // Delete User
            Utility.DeleteUser( user.DistinguishedName );

            // Add Group To Group
            GroupPrincipal newGroup = Utility.CreateGroup( workspaceName );
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            beforeCount = gpo.Groups.Count;
            Console.WriteLine( $"Adding Group [{newGroup.Name}] To Group [{group.Name}]." );
            DirectoryServices.AddToGroup(group.DistinguishedName, newGroup.DistinguishedName, "group");
            gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            afterCount = gpo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount + 1 ) );

            // Remove Group From Group
            beforeCount = afterCount;
            Console.WriteLine( $"Removing Group [{newGroup.Name}] From Group [{group.Name}]." );
            DirectoryServices.RemoveFromGroup(group.DistinguishedName, newGroup.DistinguishedName, "group");
            gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            afterCount = gpo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount - 1 ) );

            // Delete Groups
            Utility.DeleteGroup( newGroup.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddUserToNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            UserPrincipal up = Utility.CreateUser( workspaceName );

            Console.WriteLine( $"Adding User [{up.DistinguishedName}] To Group [{groupDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.AddToGroup(groupDistinguishedName, up.DistinguishedName, "user"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddGroupToNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Adding Group [{gp.DistinguishedName}] To Group [{groupDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.AddToGroup(groupDistinguishedName, gp.DistinguishedName, "group"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

            Utility.DeleteGroup( gp.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddNonExistantUserToGroup()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"OU={userName},{workspaceName}";

            Console.WriteLine( $"Adding User [{userDistinguishedName}] Which Should Not Exist To Group [{group.DistinguishedName}]." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.AddToGroup(group.DistinguishedName, userDistinguishedName, "user"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddNonExistantGroupToGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";

            Console.WriteLine( $"Adding Group [{groupDistinguishedName}] Which Should Not Exist To Group [{group.DistinguishedName}]." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.AddToGroup(group.DistinguishedName, groupDistinguishedName, "group"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveUserFromNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            UserPrincipal up = Utility.CreateUser( workspaceName );

            Console.WriteLine( $"Removing User [{up.DistinguishedName}] From Group [{groupDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(groupDistinguishedName, up.DistinguishedName, "user"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );


            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveGroupFromNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Removing Group [{gp.DistinguishedName}] From Group [{groupDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(groupDistinguishedName, gp.DistinguishedName, "group"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

            Utility.DeleteGroup( gp.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveNonExistantUserFromGroup()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"OU={userName},{workspaceName}";

            Console.WriteLine( $"Removing User [{userDistinguishedName}] Which Should Not Exist From Group [{group.DistinguishedName}]." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(group.DistinguishedName, userDistinguishedName, "user"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveNonExistantGroupFromGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";

            Console.WriteLine( $"Removing Group [{groupDistinguishedName}] Which Should Not Exist From Group [{group.DistinguishedName}]." );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(group.DistinguishedName, groupDistinguishedName, "group"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );

        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveUserFromGroupWhenNotMember()
        {
            UserPrincipal up = Utility.CreateUser( workspaceName );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(group.DistinguishedName, up.DistinguishedName, "user"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "does not exist in the group" ) );
            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveGroupFromGroupWhenNotMember()
        {
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );
            AdException ex = Assert.Throws<AdException>(() => DirectoryServices.RemoveFromGroup(group.DistinguishedName, gp.DistinguishedName, "group"));
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "does not exist in the group" ) );
            Utility.DeleteGroup( gp.DistinguishedName );
        }

    }
}
