﻿using System;
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

        [Test, Category( "Core" )]
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

        [Test, Category( "Core" )]
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
            Assert.That( user, Is.Null );
        }

        [Test, Category( "Core" )]
        public void Core_CreateUserBadDistName()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"GW={userName},{workspaceName}";

            Console.WriteLine( $"Create User [{userDistinguishedName}] With Bad DistinguishedName" );
            UserPrincipal user = null;
            Assert.Throws<AdException>( () => user = DirectoryServices.CreateUserPrincipal( userDistinguishedName ) );
        }

        [Test, Category( "Core" )]
        public void Core_ModifyUserBadData()
        {
            UserPrincipal user = Utility.CreateUser( workspaceName );

            user.SamAccountName = $"{user.Name}abcdefghij1234567890";       // SamAccountName Is Limited To 20 Characters
            Console.WriteLine( $"Modify User [{user.DistinguishedName}] With Bad SamAccountName [{user.SamAccountName}]" );
            Assert.Throws<AdException>( () => DirectoryServices.SaveUser( user ));

            Utility.DeleteUser( user.DistinguishedName );
        }

        [Test, Category( "Core" )]
        public void Core_DeleteUserDoesNotExist()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Console.WriteLine( $"Deleting User [{userDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.DeleteUser( userName ) ).Message.Contains( "cannot be found" );
        }
    }
}
