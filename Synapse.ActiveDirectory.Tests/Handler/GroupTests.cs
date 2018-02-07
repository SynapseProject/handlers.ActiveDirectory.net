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

        [Test, Category("Handler"), Category( "Group" )]
        public void Handler_GroupTestsSuccess()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create Group
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

            // Delete Group
            Console.WriteLine( $"Deleting Group" );
            parameters.Clear();
            parameters.Add( "identity", groupDistinguishedName );
            result = Utility.CallPlan( "DeleteGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_CreateGroupBadDistName()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"GW={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Create Group With Bad Distinguished Name [{groupDistinguishedName}]" );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.MissingInput ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Must Be A Distinguished Name" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_CreateGroupBadProperty()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Creating Group [{groupDistinguishedName}] With A Bad Property." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "mail", "badformat@company.com" );   // Properties Should Be An Array Of Values

            YamlDotNet.Core.SyntaxErrorException e = Assert.Throws<YamlDotNet.Core.SyntaxErrorException>( () => Utility.CallPlan( "CreateGroup", parameters ) );
            Console.WriteLine( $"Exception Message : {e.Message}" );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_ModifyGroupBadProperty()
        {
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Modifying Group [{gp.DistinguishedName}] With A Bad Property." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", gp.DistinguishedName );
            parameters.Add( "mail", "badformat@company.com" );   // Properties Should Be An Array Of Values

            YamlDotNet.Core.SyntaxErrorException e = Assert.Throws<YamlDotNet.Core.SyntaxErrorException>( () => Utility.CallPlan( "ModifyGroup", parameters ) );
            Console.WriteLine( $"Exception Message : {e.Message}" );

            Utility.DeleteGroup( gp.DistinguishedName );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_GetGroupDoesNotExist()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Getting Group [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "GetGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Was Not Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_DeleteGroupDoesNotExist()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting Group [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", groupDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "DeleteGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_AddAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist To Target [{managedBy.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_AddAccessRuleBadTarget()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For Group [{managedBy.DistinguishedName}] To Target [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", managedBy.DistinguishedName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_RemoveAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist From Target [{managedBy.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_RemoveAccessRuleBadTarget()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting AccessRule For Group [{managedBy.DistinguishedName}] From Target [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", managedBy.DistinguishedName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_SetAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Setting AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist On Target [{managedBy.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_SetAccessRuleBadTarget()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Setting AccessRule For Group [{managedBy.DistinguishedName}] On Target [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", managedBy.DistinguishedName );
            parameters.Add( "ruleidentity", groupDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_PurgeAccessRuleBadGroup()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRule For Group [{groupDistinguishedName}] Which Should Not Exist On Target [{managedBy.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", groupDistinguishedName );
            parameters.Add( "ruleidentity", managedBy.DistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "Group" )]
        public void Handler_PurgeAccessRuleBadTarget()
        {
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"CN={groupName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRule For Group [{managedBy.DistinguishedName}] On Target [{groupDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", managedBy.DistinguishedName );
            parameters.Add( "ruleidentity", groupDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }




    }
}
