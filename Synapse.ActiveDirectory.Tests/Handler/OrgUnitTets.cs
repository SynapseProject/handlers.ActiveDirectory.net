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

        [Test]
        public void Handler_OrgUnitTests()
        {
            // Setup Tests
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();

            GroupPrincipal managedBy = Utility.CreateGroup( workspaceName );

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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit, Is.Null );

            // Get OrgUnit By DistinguishedName
            Console.WriteLine( $"Getting OrgUnit By DistinguishedName : [{ouDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "true" );

            parameters.Add( "identity", ouDistinguishedName );
            result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules, Is.Null );
            Assert.That( result.Results[0].OrganizationalUnit.Properties, Is.Null );

            // Get OrgUnit By Guid
            Console.WriteLine( $"Getting OrgUnit By Guid : [{guid}]" );
            parameters.Clear();
            parameters.Add( "identity", guid );
            result = Utility.CallPlan( "GetOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit, Is.Not.Null );
            Assert.That( result.Results[0].OrganizationalUnit.Guid.ToString(), Is.EqualTo( guid ) );

            // Modify OrgUnit
            Console.WriteLine( $"Modifying OrgUnit" );
            parameters.Clear();
            parameters.Add( "identity", ouDistinguishedName );
            parameters.Add( "managedby", $"~null~" );
            parameters.Add( "postalcode", $"[ \"90210\" ]" );
            result = Utility.CallPlan( "ModifyOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].OrganizationalUnit.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Delete OrgUnit
            Console.WriteLine( $"Deleting Group" );
            parameters.Clear();
            parameters.Add( "identity", ouDistinguishedName );
            result = Utility.CallPlan( "DeleteOrgUnit", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );

            // Cleanup Workspace
            Utility.DeleteGroup( managedBy.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );

        }


    }
}
