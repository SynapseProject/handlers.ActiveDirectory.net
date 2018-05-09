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
        String workspaceName = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_OrgUnitTestSuccess()
        {
            // Setup Test
            String name = $"testou_{Utility.GenerateToken( 8 )}";
            String distinguishedName = $"OU={name},{workspaceName}";
            Dictionary<string, List<string>> properties = new Dictionary<string, List<string>>();
            DirectoryServices.AddProperty( properties, "description", "Test OU" );

            // Create OrgUnit
            Console.WriteLine( $"Creating OrgUnit : [{distinguishedName}]" );
            DirectoryServices.CreateOrganizationUnit( distinguishedName, properties );

            // Get OrgUnit By DistinguishedName
            Console.WriteLine( $"Getting OrgUnit By DisginguishedName : [{distinguishedName}]" );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, true, true, false );
            Assert.That( ouo, Is.Not.Null );

            String guid = ouo.Guid.ToString();

            // Get OrgUnit By Name
            Console.WriteLine( $"Getting OrgUnit By Name : [{name}]" );
            ouo = DirectoryServices.GetOrganizationalUnit( name, false, false, false );
            Assert.That( ouo, Is.Not.Null );

            // Get OrgUnit By Name
            Console.WriteLine( $"Getting OrgUnit By Guid : [{guid}]" );
            ouo = DirectoryServices.GetOrganizationalUnit( guid, false, true, false );
            Assert.That( ouo, Is.Not.Null );
            Assert.That( ouo.Properties.ContainsKey( "description" ), Is.True );

            // Modify OrgUnit
            DirectoryServices.AddProperty( properties, "description", "~null~", true );
            DirectoryServices.ModifyOrganizationUnit( distinguishedName, properties );
            ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, false, true, false );
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
            ouo = DirectoryServices.GetOrganizationalUnit( distinguishedName, false, false, false );
            Assert.That( ouo, Is.Null );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_OrgUnitNotFound()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";

            Console.WriteLine( $"Getting OrgUnit [{ouName}] Which Should Not Exist." );
            DirectoryEntryObject badOrgUnit = DirectoryServices.GetOrganizationalUnit( ouName, true, true, false );
            Assert.That( badOrgUnit, Is.Null );

            Console.WriteLine( $"Getting OrgUnit Principal [{ouName}] Which Should Not Exist." );
            DirectoryEntry de = DirectoryServices.GetDirectoryEntry( ouDistinguishedName );
            Assert.That( de, Is.Null );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_CreateOrgUnitBadDistName()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";

            Console.WriteLine( $"Create OrgUnit [{ouDistinguishedName}] With Bad DistinguishedName" );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.CreateOrganizationUnit( ouDistinguishedName, null ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_ModifyOrgUnitBadData()
        {
            String badOuName = $"ou=BadOrgUnit,{workspaceName}";
            DirectoryServices.CreateOrganizationUnit( badOuName, null );
            DirectoryEntryObject badOrgUnit = DirectoryServices.GetOrganizationalUnit( badOuName, false, true, false );
            Dictionary <string, List<string>> properties = new Dictionary<string, List<string>>();

            DirectoryServices.AddProperty( properties, "managedBy", "BadManager" );
            Console.WriteLine( $"Modify OrgUnit [{badOrgUnit.Name}] With Bad Property [ManagedBy]" );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.ModifyOrganizationUnit( badOrgUnit.Properties["distinguishedName"][0].ToString(), properties ) );

            DirectoryServices.DeleteOrganizationUnit( badOrgUnit.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_DeleteOrgUnitDoesNotExist()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";

            Console.WriteLine( $"Deleting OrgUnuit [{ouDistinguishedName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteOrganizationUnit( ouDistinguishedName ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_AddRuleBadTarget()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";
            GroupPrincipal group = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Adding AccessRule For Group [{group.Name}] To OrgUnit [{ouName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( ouName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_AddRuleBadUser()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            
            String testOuName = $"ou=TestOrgUnit001,{workspaceName}";
            DirectoryServices.CreateOrganizationUnit( testOuName, null );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( testOuName, false, true, false );

            Console.WriteLine( $"Adding AccessRule For Group [{groupName}] Which Should Not Exist To OrgUnit [{ouo.DistinguishedName}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.AddAccessRule( ouo.DistinguishedName, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );

            DirectoryServices.DeleteOrganizationUnit( ouo.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_DeleteRuleBadTarget()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";
            GroupPrincipal group = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Deleting AccessRule For Group [{group.Name}] From OrgUnit [{ouName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( ouName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_DeleteRuleBadUser()
        {
            // Get Group That Does Not Exist
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            String testOuName = $"ou=TestOrgUnit001,{workspaceName}";
            DirectoryServices.CreateOrganizationUnit( testOuName, null );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( testOuName, false, true, false );

            Console.WriteLine( $"Deleting AccessRule For Group [{groupName}] Which Should Not Exist From OrgUnit [{ouo.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.DeleteAccessRule( ouo.DistinguishedName, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );

            DirectoryServices.DeleteOrganizationUnit( ouo.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_SetRuleBadTarget()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";
            GroupPrincipal group = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Setting AccessRule For Group [{group.Name}] On OrgUnit [{ouName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( ouName, group.Name, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_SetRuleBadUser()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            String testOuName = $"ou=TestOrgUnit001,{workspaceName}";
            DirectoryServices.CreateOrganizationUnit( testOuName, null );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( testOuName, false, true, false );

            Console.WriteLine( $"Setting AccessRule For Group [{groupName}] Which Should Not Exist On OrgUnit [{ouo.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.SetAccessRule( ouo.DistinguishedName, groupName, ActiveDirectoryRights.GenericRead, System.Security.AccessControl.AccessControlType.Allow, ActiveDirectorySecurityInheritance.None ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );

            DirectoryServices.DeleteOrganizationUnit( ouo.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_PurgeRuleBadTarget()
        {
            // Get OrgUnit That Does Not Exist
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";
            GroupPrincipal group = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Purging AccessRule For Group [{group.Name}] From OrgUnit [{ouName}] Which Should Not Exist." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( ouName, group.Name ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Core" ), Category( "OrgUnit" )]
        public void Core_PurgeRuleBadUser()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            String testOuName = $"ou=TestOrgUnit001,{workspaceName}";
            DirectoryServices.CreateOrganizationUnit( testOuName, null );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( testOuName, false, true, false );

            Console.WriteLine( $"Purging AccessRule For Group [{groupName}] Which Should Not Exist From OrgUnit [{ouo.Name}]." );
            AdException ex = Assert.Throws<AdException>( () => DirectoryServices.PurgeAccessRules( ouo.DistinguishedName, groupName ) );
            Console.WriteLine( $"Exception Message : {ex.Message}" );
            Assert.That( ex.Message, Contains.Substring( "Can Not Be Found" ) );

            DirectoryServices.DeleteOrganizationUnit( ouo.DistinguishedName );
        }





    }
}
