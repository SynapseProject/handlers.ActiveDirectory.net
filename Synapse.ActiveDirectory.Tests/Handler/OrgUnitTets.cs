using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.IO;
using System.Collections.Generic;

using NUnit.Framework;

using Synapse.Core;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;
using Synapse.Handlers.ActiveDirectory;

namespace Synapse.ActiveDirectory.Tests.Handler
{
    [TestFixture]
    public class OrgUnitTests
    {
        DirectoryEntry workspace = null;
        String workspaceName = null;
        GroupPrincipal managedBy = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            managedBy = Utility.CreateGroup( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteGroup( managedBy.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category("Handler"), Category( "OrgUnit" )]
        public void Handler_OrgUnitTestsSuccess()
        {
            String ouName = $"testorgunit_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create OrgUnit
            Console.WriteLine( $"Creating OrgUnit : [{ouDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );

            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "description", $"Test OrgUnit {ouName} Description" );
            parameters.Add( "managedby", managedBy.Name );
            parameters.Add( "c", @"[ ""US"" ]" );
            parameters.Add( "l", @"[ ""Translyvania"" ]" );
            parameters.Add( "st", @"[ ""Louisiana"" ]" );
            parameters.Add( "street", @"[ ""13119 US-65"" ]" );
            parameters.Add( "postalcode", @"[ ""71286"" ]" );
            parameters.Add( "co", @"[ ""United States"" ]" );
            parameters.Add( "countrycode", @"[ ""840"" ]" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.DistinguishedName, Is.EqualTo( ouDistinguishedName ) );
            Assert.That( result.Results[0].OrganizationalUnit.Properties["postalCode"][0], Is.EqualTo( "71286" ) );
            Assert.That( result.Results[0].OrganizationalUnit.Properties["managedBy"][0], Is.EqualTo( managedBy.DistinguishedName ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules, Is.Not.Null );

            string guid = result.Results[0].OrganizationalUnit.Guid.ToString();
            int initialRuleCount = result.Results[0].OrganizationalUnit.AccessRules.Count;

            // Get OrgUnit By Name
            Console.WriteLine( $"Getting OrgUnit By Name : [{ouName}]" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "false" );

            parameters.Add( "identity", ouName );
            result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit, Is.Null );

            // Get OrgUnit By DistinguishedName
            Console.WriteLine( $"Getting OrgUnit By DistinguishedName : [{ouDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "true" );

            parameters.Add( "identity", ouDistinguishedName );
            result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules, Is.Null );
            Assert.That( result.Results[0].OrganizationalUnit.Properties, Is.Null );

            // Get OrgUnit By Guid
            Console.WriteLine( $"Getting OrgUnit By Guid : [{guid}]" );
            parameters.Clear();
            parameters.Add( "identity", guid );
            result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit, Is.Not.Null );
            Assert.That( result.Results[0].OrganizationalUnit.Guid.ToString(), Is.EqualTo( guid ) );

            // Modify OrgUnit
            Console.WriteLine( $"Modifying OrgUnit" );
            parameters.Clear();
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "managedby", $"~null~" );
            parameters.Add( "postalcode", $"[ \"90210\" ]" );
            result = Utility.CallPlan( "ModifyOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.Properties.ContainsKey( "managedBy" ), Is.False );
            Assert.That( result.Results[0].OrganizationalUnit.Properties["postalCode"][0], Is.EqualTo( "90210" ) );

            // Add Access Rule
            Console.WriteLine( $"Add Access Rule To OrgUnit [{ouDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "AddAccessRuleToOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Remove Access Rule
            Console.WriteLine( $"Remove Access Rule From OrgUnit [{ouDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "RemoveAccessRuleFromOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Set Access Rule
            Console.WriteLine( $"Set Access Rule On OrgUnit [{ouDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "SetAccessRuleOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Purge Access Rule
            Console.WriteLine( $"Purge Access Rules On OrgUnit [{ouDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "PurgeAccessRulesOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Delete OrgUnit
            Console.WriteLine( $"Deleting Group" );
            parameters.Clear();
            parameters.Add( "identity", ouDistinguishedName );
            result = Utility.CallPlan( "DeleteOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_CreateOrgUnitBadDistName()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"GW={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Create OrgUnit With Bad Distinguished Name [{ouDistinguishedName}]" );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.MissingInput ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Must Be A Distinguished Name" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_CreateOrgUnitBadProperty()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Creating OrgUnit [{ouDistinguishedName}] With A Bad Property." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "st", "Louisiana" );   // Properties Should Be An Array Of Values

            YamlDotNet.Core.SyntaxErrorException e = Assert.Throws<YamlDotNet.Core.SyntaxErrorException>( () => Utility.CallPlan( "CreateOrgUnit", parameters ) );
            Console.WriteLine( $"Exception Message : {e.Message}" );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_ModifyOrgUnitBadProperty()
        {
            DirectoryServices.CreateOrganizationUnit( $"ou=OuDoesNotExist,{workspaceName}", null );
            DirectoryEntryObject ouo = DirectoryServices.GetOrganizationalUnit( $"ou=OuDoesNotExist,{workspaceName}", false, true, false );
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Modifying OrgUnit [{ouo.DistinguishedName}] With A Bad Property." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouo.DistinguishedName );
            parameters.Add( "st", "Louisiana" );   // Properties Should Be An Array Of Values

            YamlDotNet.Core.SyntaxErrorException e = Assert.Throws<YamlDotNet.Core.SyntaxErrorException>( () => Utility.CallPlan( "ModifyOrgUnit", parameters ) );
            Console.WriteLine( $"Exception Message : {e.Message}" );

            DirectoryServices.DeleteOrganizationUnit( ouo.DistinguishedName );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_GetOrgUnitDoesNotExist()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Getting OrgUnit [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Was Not Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_DeleteUserDoesNotExist()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting OrgUnit [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", ouDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "DeleteOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_AddAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist To Target [{workspaceName}]." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", workspaceName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_AddAccessRuleBadTarget()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For Group [{managedBy.DistinguishedName}] To Target [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_RemoveAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Removing AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist From Target [{workspaceName}]." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", workspaceName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void HandlerRemoveAccessRuleBadTarget()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Removing AccessRule For Group [{managedBy.DistinguishedName}] From Target [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_SetAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Setting AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist On Target [{workspaceName}]." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", workspaceName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_SetAccessRuleBadTarget()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Setting AccessRule For Group [{managedBy.DistinguishedName}] On Target [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_PurgeAccessRulesBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRules For Group [{groupDistinguishedName}] Which Should Not Exist On Target [{workspaceName}]." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", workspaceName );
            parameters.Add( "ruleidentity", groupDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "OrgUnit" )]
        public void Handler_PurgeAccessRulesBadTarget()
        {
            String ouName = $"testou_{Utility.GenerateToken( 8 )}";
            String ouDistinguishedName = $"OU={ouName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRules For Group [{managedBy.DistinguishedName}] On Target [{ouDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }



    }
}
