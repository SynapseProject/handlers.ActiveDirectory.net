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
    public class GroupTests
    {
        DirectoryEntry workspace = null;

        [Test]
        public void Handler_GroupTests()
        {
            // Setup Tests
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();

            UserPrincipal managedBy = Utility.CreateUser( workspaceName );

            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create User
            Console.WriteLine( $"Creating Group : [{groupDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );

            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "scope", "Universal" );
            parameters.Add( "description", $"Test Group {groupName} Description" );
            parameters.Add( "securitygroup", "true" );
            parameters.Add( "samaccountname", groupName.Substring( 0, 19 ) );
            parameters.Add( "managedby", managedBy.Name );

            parameters.Add( "displayName", $"[ \"Test Group {groupName}\" ]" );
            parameters.Add( "mail", $"[ \"{groupName}@company.com\" ]" );
            parameters.Add( "info", $"[ \"Random Notes Here.\" ]" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.DistinguishedName, Is.EqualTo( groupDistinguishedName ) );
            Assert.That( result.Results[0].Group.Groups, Is.Not.Null );
            Assert.That( result.Results[0].Group.AccessRules, Is.Not.Null );

            string samAccountName = result.Results[0].Group.SamAccountName;
            string sid = result.Results[0].Group.Sid;
            string guid = result.Results[0].Group.Guid.ToString();

            // Get Group By Name
            Console.WriteLine( $"Getting Group By Name : [{groupName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "false" );
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "false" );

            parameters.Add( "identity", groupName );
            result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group, Is.Null );

            // Get Group By DistinguishedName
            Console.WriteLine( $"Getting Group By DistinguishedName : [{groupDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "false" );
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "true" );

            parameters.Add( "identity", groupDistinguishedName );
            result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.Groups, Is.Null );
            Assert.That( result.Results[0].Group.AccessRules, Is.Null );
            Assert.That( result.Results[0].Group.Properties, Is.Null );

            // Get Group By SamAccountName
            Console.WriteLine( $"Getting Group By SamAccountName : [{samAccountName}]" );
            parameters.Clear();
            parameters.Add( "identity", samAccountName );
            result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group, Is.Not.Null );
            Assert.That( result.Results[0].Group.SamAccountName, Is.EqualTo( samAccountName ) );

            // Get Group By Sid
            Console.WriteLine( $"Getting Group By SecurityId : [{sid}]" );
            parameters.Clear();
            parameters.Add( "identity", sid );
            result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group, Is.Not.Null );
            Assert.That( result.Results[0].Group.Sid, Is.EqualTo( sid ) );

            // Get Group By Guid
            Console.WriteLine( $"Getting Group By Guid : [{guid}]" );
            parameters.Clear();
            parameters.Add( "identity", guid );
            result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group, Is.Not.Null );
            Assert.That( result.Results[0].Group.Guid.ToString(), Is.EqualTo( guid ) );

            // Modify Group
            Console.WriteLine( $"Modifying Group" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "managedby", $"~null~" );
            parameters.Add( "scope", "Global" );
            parameters.Add( "info", $"[ \"Hello World\" ]" );
            result = Utility.CallPlan( "ModifyGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.Properties.ContainsKey( "managedBy" ), Is.False );
            Assert.That( result.Results[0].Group.GroupScope.ToString(), Is.EqualTo("Global") );
            Assert.That( result.Results[0].Group.Properties["info"][0], Is.EqualTo( "Hello World" ) );

            // AccessRules
            int initialRuleCount = result.Results[0].Group.AccessRules.Count;

            // Add Access Rule
            Console.WriteLine( $"Add Access Rule To Group [{groupDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "AddAccessRuleToGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Remove Access Rule
            Console.WriteLine( $"Remove Access Rule From Group [{groupDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "RemoveAccessRuleFromGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Set Access Rule
            Console.WriteLine( $"Set Access Rule On Group [{groupDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "SetAccessRuleOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Purge Access Rule
            Console.WriteLine( $"Purge Access Rules On Group [{groupDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "PurgeAccessRulesOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Delete User
            Console.WriteLine( $"Deleting Group" );
            parameters.Clear();
            parameters.Add( "identity", groupDistinguishedName );
            result = Utility.CallPlan( "DeleteGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );

            // Cleanup Workspace
            Utility.DeleteUser( managedBy.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );

        }
    }
}
