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
    public class UserTests
    {
        DirectoryEntry workspace = null;

        [Test]
        public void Handler_UserTests()
        {
            // Setup Tests
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();

            UserPrincipal manager = Utility.CreateUser( workspaceName );

            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create User
            Console.WriteLine( $"Creating User : [{userDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );

            parameters.Add( "identity", userDistinguishedName );
            parameters.Add("userprincipalname", $"{userName}1@{DirectoryServices.GetDomain(userDistinguishedName)}");
            parameters.Add( "samaccountname", userName.Substring( 0, 19 ) );
            parameters.Add( "displayName", $"Test User {userName}" );
            parameters.Add( "description", $"Test User {userName} Description" );
            parameters.Add( "password", "bi@02LL49_VWQ{b" );
            parameters.Add( "enabled", "true" );
            parameters.Add( "accountexpirationdate", DateTime.Now.AddDays( 30 ).ToString() );
            parameters.Add( "smartcardlogonrequired", "true" );
            parameters.Add( "delegationpermitted", "true" );
            parameters.Add( "homedirectory", "Temp" );
            parameters.Add( "scriptpath", "D:\\Temp\\Scripts\\startup.bat" );
            parameters.Add( "passwordnotrequired", "false" );
            parameters.Add( "passwordneverexpires", "true" );
            parameters.Add( "usercannotchangepassword", "false" );
            parameters.Add( "allowreversiblepasswordencryption", "true" );
            parameters.Add( "homedrive", "D" );
            parameters.Add( "givenname", "Test" );
            parameters.Add( "middlename", "Bartholomew" );
            parameters.Add( "surname", "User" );
            parameters.Add( "emailaddress", "test.b.user@company.com" );
            parameters.Add( "voicetelephonenumber", "713-555-1212" );
            parameters.Add( "employeeid", "42" );
            // Add Properties
            parameters.Add( "initials", @"[ ""TBU"" ]" );
            parameters.Add( "physicaldeliveryofficename", @"[ ""Company Plaza 2"" ]" );
            parameters.Add( "othertelephone", @"[ ""713-555-1313"", ""7135551414"", ""713-555-1515"" ]" );
            parameters.Add( "wwwhomepage", @"[ ""http://www.google.com"" ]" );
            parameters.Add( "url", @"[ ""http://www.bing.com"", ""http://www.altavista.com"", ""http://www.cnn.com"", ""http://www.fakenews.fox.com"" ]" );
            parameters.Add( "logonworkstation", @"[ ""Workstation001"" ]" );
            parameters.Add( "userworkstations",  @"[ ""Workstation002"" ]" );
            parameters.Add( "c", @"[ ""US"" ]" );
            parameters.Add( "l", @"[ ""Translyvania"" ]" );
            parameters.Add( "st", @"[ ""Louisiana"" ]" );
            parameters.Add( "streetaddress", @"[ ""13119 US-65"" ]" );
            parameters.Add( "postofficebox", @"[ ""666"" ]" );
            parameters.Add( "postalcode", @"[ ""71286"" ]" );
            parameters.Add( "co", @"[ ""United States"" ]" );
            parameters.Add( "countrycode", @"[ ""840"" ]" );
            parameters.Add( "title", @"[ ""Laboratory Assistant"" ]" );
            parameters.Add( "department", @"[ ""Vampire Studies"" ]" );
            parameters.Add( "company", @"[ ""True Blood Inc."" ]" );
            parameters.Add( "manager", $"[ \"{manager.DistinguishedName}\" ]" );
            parameters.Add( "profilepath", @"[ ""D:\\Temp\\ProfilePath"" ]" );
            parameters.Add( "homephone", @"[ ""832-555-1212"" ]" );
            parameters.Add( "otherhomephone", @"[ ""832-555-1313"", ""832-555-1414"" ]" );
            parameters.Add( "pager", @"[ ""281-555-1212"" ]" );
            parameters.Add( "otherpager", @"[ ""281-555-1313"", ""281-555-1414"", ""281-555-1515"" ]" );
            parameters.Add( "mobile", @"[ ""346-555-1212"" ]" );
            parameters.Add( "othermobile", @"[ ""346-555-1313"" ]" );
            parameters.Add( "facsimiletelephonenumber", @"[ ""318-555-1111"" ]" );
            parameters.Add( "otherfacsimiletelephonenumber", @"[ ""318-555-2222"", ""318-555-3333"", ""318-555-4444"" ]" );
            parameters.Add( "ipphone", @"[ ""504-555-1111"" ]" );
            parameters.Add( "otheripphone", @"[ ""504-555-2222"", ""504-555-3333"" ]" );
            parameters.Add( "info", @"[ ""Keep Out Of Direct Sunlight"" ]" );
            parameters.Add( "lockouttime", $"[ \"0\" ]" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.DistinguishedName, Is.EqualTo( userDistinguishedName ) );
            Assert.That( result.Results[0].User.Properties["st"][0], Is.EqualTo( "Louisiana" ) );
            Assert.That( result.Results[0].User.Groups, Is.Not.Null );
            Assert.That( result.Results[0].User.AccessRules, Is.Not.Null );

            string userPrincipalName = result.Results[0].User.UserPrincipalName;
            string samAccountName = result.Results[0].User.SamAccountName;
            string sid = result.Results[0].User.Sid;
            string guid = result.Results[0].User.Guid.ToString();

            // Get User By Name
            Console.WriteLine( $"Getting User By Name : [{userName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "false" );
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "false" );

            parameters.Add( "identity", userName );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Null );

            // Get User By DistinguishedName
            Console.WriteLine( $"Getting User By DistinguishedName : [{userDistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "false" );
            parameters.Add( "returnaccessrules", "false" );
            parameters.Add( "returnobjectproperties", "false" );
            parameters.Add( "returnobjects", "true" );

            parameters.Add( "identity", userDistinguishedName );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.Groups, Is.Null );
            Assert.That( result.Results[0].User.AccessRules, Is.Null );
            Assert.That( result.Results[0].User.Properties, Is.Null );

            // Get User By UserPrincipalName
            Console.WriteLine( $"Getting User By UserPrincipalName : [{userPrincipalName}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "returnobjectproperties", "true" );
            parameters.Add( "returnobjects", "true" );

            parameters.Add( "identity", userPrincipalName );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.Groups, Is.Not.Null );
            Assert.That( result.Results[0].User.AccessRules, Is.Not.Null );
            Assert.That( result.Results[0].User.Properties, Is.Not.Null );

            // Get User By SamAccountName
            Console.WriteLine( $"Getting User By SamAccountName : [{samAccountName}]" );
            parameters.Clear();
            parameters.Add( "identity", samAccountName );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.SamAccountName, Is.EqualTo( samAccountName ) );

            // Get User By Sid
            Console.WriteLine( $"Getting User By SecurityId : [{sid}]" );
            parameters.Clear();
            parameters.Add( "identity", sid );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.Sid, Is.EqualTo( sid ) );

            // Get User By Guid
            Console.WriteLine( $"Getting User By Guid : [{guid}]" );
            parameters.Clear();
            parameters.Add( "identity", guid );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.Guid.ToString(), Is.EqualTo( guid ) );

            // Modify User
            Console.WriteLine( $"Modifying User" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "employeeid", "84" );
            parameters.Add( "manager", $"[ \"~null~\" ]" );
            parameters.Add( "otheripphone", @"[ ""504-555-7777"", ""~null~"", ""504-555-8888"" ]" );
            result = Utility.CallPlan( "ModifyUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.EmployeeId, Is.EqualTo( "84" ) );
            Assert.That( result.Results[0].User.Properties.ContainsKey("manager"), Is.False );
            Assert.That( result.Results[0].User.Properties.ContainsKey("otheripphone"), Is.False );

            // AccessRules
            int initialRuleCount = result.Results[0].User.AccessRules.Count;

            // Add Access Rule
            Console.WriteLine( $"Add Access Rule To User [{userDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "AddAccessRuleToUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Remove Access Rule
            Console.WriteLine( $"Remove Access Rule From User [{userDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "RemoveAccessRuleFromUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Set Access Rule
            Console.WriteLine( $"Set Access Rule On User [{userDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "SetAccessRuleOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.AccessRules.Count, Is.EqualTo( initialRuleCount + 1 ) );

            // Purge Access Rule
            Console.WriteLine( $"Purge Access Rules On User [{userDistinguishedName}]." );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );
            result = Utility.CallPlan( "PurgeAccessRulesOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Delete User
            Console.WriteLine( $"Deleting User" );
            parameters.Clear();
            parameters.Add( "identity", userDistinguishedName );
            result = Utility.CallPlan( "DeleteUser", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );

            // Cleanup Workspace
            Utility.DeleteUser( manager.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );

        }
    }
}
