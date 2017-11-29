using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
{
    [TestFixture]
    public class UserTests
    {
        DirectoryEntry workspace = null;
        String workspaceName = null;
        UserPrincipal user = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            user = Utility.CreateUser( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteUser( user.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );

        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_UserTest()
        {
            // Get User By Distinguished Name
            Console.WriteLine( $"Getting User By DisginguishedName : [{user.DistinguishedName}]" );
            UserPrincipalObject upo = DirectoryServices.GetUser( user.DistinguishedName, true, true, true );
            Assert.That( upo.DistinguishedName, Is.EqualTo( user.DistinguishedName ) );

            String userName = upo.Name;
            String userPrincipalName = upo.UserPrincipalName;
            String userSamAccountName = upo.SamAccountName;
            Guid? userGuid = upo.Guid;
            String userSid = upo.Sid;

            // Get User By Name
            Console.WriteLine( $"Getting User By Name: [{userName}]" );
            upo = DirectoryServices.GetUser( userName, true, true, true );
            Assert.That( upo.Name, Is.EqualTo( userName ) );

            // Get User By UserPrincipalName
            Console.WriteLine( $"Getting User By UserPrincipalName: [{userPrincipalName}]" );
            upo = DirectoryServices.GetUser( userPrincipalName, true, true, true );
            Assert.That( upo.UserPrincipalName, Is.EqualTo( userPrincipalName ) );

            // Get User By SamAccountName
            Console.WriteLine( $"Getting User By SamAccountName : [{userSamAccountName}]" );
            upo = DirectoryServices.GetUser( userSamAccountName, true, true, true );
            Assert.That( upo.SamAccountName, Is.EqualTo( userSamAccountName ) );

            // Get User By Guid
            Console.WriteLine( $"Getting User By Guid : [{userGuid}]" );
            upo = DirectoryServices.GetUser( userGuid.ToString(), true, true, true );
            Assert.That( upo.Guid, Is.EqualTo( userGuid ) );

            // Get User By Sid
            Console.WriteLine( $"Getting User By Sid : [{userSid}]" );
            upo = DirectoryServices.GetUser( userSid, true, true, true );
            Assert.That( upo.Sid, Is.EqualTo( userSid ) );

            // Modify User
            Console.WriteLine( $"Modifying User : [{userName}]" );
            UserPrincipal up = DirectoryServices.GetUserPrincipal( userName );
            up.DisplayName = "Guy Michael Waguespack";
            up.GivenName = "Guy";
            up.MiddleName = "Michael";
            up.Surname = "Waguespack";
            DirectoryServices.SaveUser( up );

            upo = DirectoryServices.GetUser( userName, false, false, false );
            Assert.That( upo.DisplayName, Is.EqualTo( "Guy Michael Waguespack" ) );
            Assert.That( upo.GivenName, Is.EqualTo( "Guy" ) );
            Assert.That( upo.MiddleName, Is.EqualTo( "Michael" ) );
            Assert.That( upo.Surname, Is.EqualTo( "Waguespack" ) );

            // Create AccessUser For AccessRule Tests (Below)
            UserPrincipal accessRuleUser = Utility.CreateUser( workspaceName );
            int ruleCount = DirectoryServices.GetAccessRules( user ).Count;

            // Add Access Rule To User
            Console.WriteLine( $"Adding AccessRule For User [{accessRuleUser.Name}] To User [{user.Name}]." );
            DirectoryServices.AddAccessRule( user, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            int newRuleCount = DirectoryServices.GetAccessRules( user ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Removing Access Rule From User
            Console.WriteLine( $"Removing AccessRule For User [{accessRuleUser.Name}] From User [{user.Name}]." );
            DirectoryServices.DeleteAccessRule( user, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( user ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Seting Access Rule From User
            Console.WriteLine( $"Setting AccessRule For User [{accessRuleUser.Name}] On User [{user.Name}]." );
            DirectoryServices.SetAccessRule( user, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( user ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Purge Access Rule From User
            Console.WriteLine( $"Purging AccessRules For User [{accessRuleUser.Name}] From User [{user.Name}]." );
            DirectoryServices.PurgeAccessRules( user, accessRuleUser );
            newRuleCount = DirectoryServices.GetAccessRules( user ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Delete AccessRule User 
            Utility.DeleteUser( accessRuleUser.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_UserNotFound()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Getting User [{userName}] Which Should Not Exist." );
            UserPrincipalObject user = DirectoryServices.GetUser( userName, true, true, true );
            Assert.That( user, Is.Null );

            Console.WriteLine( $"Getting User Principal [{userName}] Which Should Not Exist." );
            UserPrincipal up = DirectoryServices.GetUserPrincipal( userDistinguishedName );
            Assert.That( up, Is.Null );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_CreateUserBadDistName()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"GW={userName},{workspaceName}";

            Console.WriteLine( $"Create User [{userDistinguishedName}] With Bad DistinguishedName" );
            UserPrincipal user = null;
            AdException ex = Assert.Throws<AdException>( () => user = DirectoryServices.CreateUserPrincipal( userDistinguishedName ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_ModifyUserBadData()
        {
            UserPrincipal badUser = Utility.CreateUser( workspaceName );

            badUser.SamAccountName = $"{badUser.Name}abcdefghij1234567890";       // SamAccountName Is Limited To 20 Characters
            Console.WriteLine( $"Modify User [{badUser.DistinguishedName}] With Bad SamAccountName [{badUser.SamAccountName}]" );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.SaveUser( badUser ));

            Utility.DeleteUser( badUser.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_DeleteUserDoesNotExist()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Deleting User [{userDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteUser( userName ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_AddRuleBadTarget()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Adding AccessRule For User [{user.Name}] To User [{userName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( userName, user.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_AddRuleBadUser()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Adding AccessRule For User [{userName}] Which Should Not Exist To User [{user.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( user.Name, userName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_DeleteRuleBadTarget()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Deleting AccessRule For User [{user.Name}] From User [{userName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( userName, user.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_DeleteRuleBadUser()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Deleting AccessRule For User [{userName}] Which Should Not Exist From User [{user.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( user.Name, userName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_SetRuleBadTarget()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For User [{user.Name}] On User [{userName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( userName, user.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_SetRuleBadUser()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For User [{userName}] Which Should Not Exist On User [{user.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( user.Name, userName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_PurgeRuleBadTarget()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Purging AccessRule For User [{user.Name}] From User [{userName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( userName, user.Name ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "User" )]
        public void Core_PurgeRuleBadUser()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Setting AccessRule For User [{userName}] Which Should Not Exist On User [{user.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( user.Name, userName ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }


    }
}
