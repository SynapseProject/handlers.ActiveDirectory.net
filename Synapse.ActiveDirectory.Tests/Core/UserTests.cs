using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests
{
    [TestFixture]
    public class UserTests
    {
        DirectoryEntry workspace = null;
        UserPrincipal user = null;

        [Test]
        public void Core_UserTest()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();

            // Create User
            user = Utility.CreateUser( workspaceName );


            
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



            // Delete User
            Utility.DeleteUser( user.DistinguishedName );

            // Cleanup Workspace
            Utility.DeleteWorkspace( workspaceName );
        }
    }
}
