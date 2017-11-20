using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Collections.Generic;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
{
    [TestFixture]
    public class OrgUnitTests
    {
        DirectoryEntry workspace = null;

        [Test]
        public void Core_OrgUnitTest()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            String name = $"testou_{Utility.GenerateToken( 8 )}";
            String distinguishedName = $"OU={name},{workspaceName}";

            Dictionary<string, List<string>> properties = new Dictionary<string, List<string>>();
            DirectoryServices.AddProperty( properties, "description", "Test OU" );

            // Create OrgUnit
            Console.WriteLine( $"Creating OrgUnit : [{distinguishedName}]" );
            DirectoryServices.CreateOrganizationUnit( distinguishedName, properties );

            // Get OrgUnit By DistinguishedName
            Console.WriteLine( $"Getting OrgUnit By DisginguishedName : [{distinguishedName}]" );
            OrganizationalUnitObject ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, true, true );
            Assert.That( ouo, Is.Not.Null );

            String guid = ouo.Guid.ToString();

            // Get OrgUnit By Name
            Console.WriteLine( $"Getting OrgUnit By Name : [{name}]" );
            ouo = DirectoryServices.GetOrganizationalUnit( name, false, false );
            Assert.That( ouo, Is.Not.Null );

            // Get OrgUnit By Name
            Console.WriteLine( $"Getting OrgUnit By Guid : [{guid}]" );
            ouo = DirectoryServices.GetOrganizationalUnit( guid, false, true );
            Assert.That( ouo, Is.Not.Null );
            Assert.That( ouo.Properties.ContainsKey( "description" ), Is.True );

            // Modify OrgUnit
            DirectoryServices.AddProperty( properties, "description", "~null~", true );
            DirectoryServices.ModifyOrganizationUnit( distinguishedName, properties );
            ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, false, true );
            Assert.That( ouo.Properties.ContainsKey("description"), Is.False );

            // Create AccessUser For AccessRule Tests (Below)
            DirectoryEntry orgUnit = DirectoryServices.GetDirectoryEntry( distinguishedName );
            UserPrincipal accessRuleUser = Utility.CreateUser( workspaceName );
            int ruleCount = DirectoryServices.GetAccessRules( orgUnit ).Count;

            // Add Access Rule To OrgUnit
            Console.WriteLine( $"Adding AccessRule For User [{accessRuleUser.Name}] To OrgUnit [{orgUnit.Name}]." );
            DirectoryServices.AddAccessRule( orgUnit, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            int newRuleCount = DirectoryServices.GetAccessRules( orgUnit ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Removing Access Rule From OrgUnit
            Console.WriteLine( $"Removing AccessRule For User [{accessRuleUser.Name}] From OrgUnit [{orgUnit.Name}]." );
            DirectoryServices.DeleteAccessRule( orgUnit, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( orgUnit ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Seting Access Rule From OrgUnit
            Console.WriteLine( $"Setting AccessRule For User [{accessRuleUser.Name}] On OrgUnit [{orgUnit.Name}]." );
            DirectoryServices.SetAccessRule( orgUnit, accessRuleUser, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None );
            newRuleCount = DirectoryServices.GetAccessRules( orgUnit ).Count;
            Assert.That( newRuleCount, Is.GreaterThan( ruleCount ) );

            // Purge Access Rule From OrgUnit
            Console.WriteLine( $"Purging AccessRules For User [{accessRuleUser.Name}] From OrgUnit [{orgUnit.Name}]." );
            DirectoryServices.PurgeAccessRules( orgUnit, accessRuleUser );
            newRuleCount = DirectoryServices.GetAccessRules( orgUnit ).Count;
            Assert.That( newRuleCount, Is.EqualTo( ruleCount ) );

            // Delete AccessRule User 
            Utility.DeleteUser( accessRuleUser.DistinguishedName );


            // Delete OrgUnit
            Console.WriteLine( $"Deleting OrgUnit : [{distinguishedName}]" );
            DirectoryServices.DeleteOrganizationUnit( distinguishedName );
            ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, false, false );
            Assert.That( ouo, Is.Null );

            // Cleanup Workspace
            Utility.DeleteWorkspace( workspaceName );


        }
    }
}
