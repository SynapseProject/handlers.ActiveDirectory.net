﻿using System;
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
        String workspaceName = null;
        UserPrincipal manager = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            manager = Utility.CreateUser( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteUser( manager.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category("Handler"), Category("User")]
        public void Handler_UserTestsSuccess()
        {
            String userName = $"testuser_{Utility.GenerateToken(8)}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create User
            Console.WriteLine($"Creating User : [{userDistinguishedName}]");
            parameters.Clear();
            parameters.Add("returngroupmembership", "true");
            parameters.Add("returnaccessrules", "true");

            parameters.Add("identity", userDistinguishedName);
            parameters.Add("userprincipalname", $"{userName}1@{DirectoryServices.GetDomain(userDistinguishedName)}");
            parameters.Add("samaccountname", userName.Substring(0, 19));
            parameters.Add("displayName", $"Test User {userName}");
            parameters.Add("description", $"Test User {userName} Description");
            parameters.Add("password", "bi@02LL49_VWQ{b");
            parameters.Add("enabled", "true");
            parameters.Add("accountexpirationdate", DateTime.Now.AddDays(30).ToString());
            parameters.Add("smartcardlogonrequired", "true");
            parameters.Add("delegationpermitted", "true");
            parameters.Add("homedirectory", "Temp");
            parameters.Add("scriptpath", "D:\\Temp\\Scripts\\startup.bat");
            parameters.Add("passwordnotrequired", "false");
            parameters.Add("passwordneverexpires", "true");
            parameters.Add("usercannotchangepassword", "false");
            parameters.Add("allowreversiblepasswordencryption", "true");
            parameters.Add("homedrive", "D");
            parameters.Add("givenname", "Test");
            parameters.Add("middlename", "Bartholomew");
            parameters.Add("surname", "User");
            parameters.Add("emailaddress", "test.b.user@company.com");
            parameters.Add("voicetelephonenumber", "713-555-1212");
            parameters.Add("employeeid", "42");

            // Add Properties
            Dictionary<string, string[]> properties = new Dictionary<string, string[]>();
            properties.Add("initials", new string[] { "TBU" });
            properties.Add("physicaldeliveryofficename", new string[] { "Company Plaza 2" });
            properties.Add("othertelephone", new string[] { "713-555-1313", "713-555-1414", "713-555-1515" });
            properties.Add("wwwhomepage", new string[] { "http://www.google.com" });
            properties.Add("url", new string[] { "http://www.bing.com", "http://www.altavista.com", "http://www.cnn.com", "http://www.fakenews.fox.com" });
            properties.Add("logonworkstation", new string[] { "Workstation001" });
            properties.Add("userworkstations", new string[] { "Workstation002" });
            properties.Add("c", new string[] { "US" });
            properties.Add("l", new string[] { "Translyvania" });
            properties.Add("st", new string[] { "Louisiana" });
            properties.Add("streetaddress", new string[] { "13119 US-65" });
            properties.Add("postofficebox", new string[] { "666" });
            properties.Add("postalcode", new string[] { "71286" });
            properties.Add("co", new string[] { "United States" });
            properties.Add("countrycode", new string[] { "840" });
            properties.Add("title", new string[] { "Laboratory Assistant" });
            properties.Add("department", new string[] { "Vampire Studies" });
            properties.Add("company", new string[] { "True Blood Inc." });
            properties.Add("manager", new string[] { $"{manager.DistinguishedName}" });
            properties.Add("profilepath", new string[] { "D:\\Temp\\ProfilePath" });
            properties.Add("homephone", new string[] { "832-555-1212" });
            properties.Add("otherhomephone", new string[] { "832-555-1313", "832-555-1414" });
            properties.Add("pager", new string[] { "281-555-1212" });
            properties.Add("otherpager", new string[] { "281-555-1313", "281-555-1414", "281-555-1515" });
            properties.Add("mobile", new string[] { "346-555-1212" });
            properties.Add("othermobile", new string[] { "346-555-1313" });
            properties.Add("facsimiletelephonenumber", new string[] { "318-555-1111" });
            properties.Add("otherfacsimiletelephonenumber", new string[] { "318-555-2222", "318-555-3333", "318-555-4444" });
            properties.Add("ipphone", new string[] { "504-555-1111" });
            properties.Add("otheripphone", new string[] { "504-555-2222", "504-555-3333" } );
            properties.Add( "info", new string[] { "Keep Out Of Direct Sunlight" } );
            properties.Add( "lockouttime", new string[] { "0" } );
            parameters.Add("properties", YamlHelpers.Serialize(properties, true, false));

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.Groups, Is.Not.Null );
            Assert.That( result.Results[0].User.AccessRules, Is.Not.Null );
            Assert.That( result.Results[0].User.Properties, Is.Not.Null );

            // Get User By SamAccountName
            Console.WriteLine( $"Getting User By SamAccountName : [{samAccountName}]" );
            parameters.Clear();
            parameters.Add( "identity", samAccountName );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.SamAccountName, Is.EqualTo( samAccountName ) );

            // Get User By Sid
            Console.WriteLine( $"Getting User By SecurityId : [{sid}]" );
            parameters.Clear();
            parameters.Add( "identity", sid );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.Sid, Is.EqualTo( sid ) );

            // Get User By Guid
            Console.WriteLine( $"Getting User By Guid : [{guid}]" );
            parameters.Clear();
            parameters.Add( "identity", guid );
            result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User, Is.Not.Null );
            Assert.That( result.Results[0].User.Guid.ToString(), Is.EqualTo( guid ) );

            // Modify User
            Console.WriteLine( $"Modifying User" );
            parameters.Clear();
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "employeeid", "84" );

            properties.Clear();
            properties.Add( "manager", new string[] { "~null~" } );
            properties.Add( "otheripphone", new string[] { "504-555-7777", "~null~", "504-555-8888" } );
            parameters.Add("properties", YamlHelpers.Serialize(properties, true, false));

            result = Utility.CallPlan( "ModifyUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
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
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.AccessRules.Count, Is.EqualTo( initialRuleCount ) );

            // Delete User
            Console.WriteLine( $"Deleting User" );
            parameters.Clear();
            parameters.Add( "identity", userDistinguishedName );
            result = Utility.CallPlan( "DeleteUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
        }


        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_CreateUserBadDistName()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"GW={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Create User With Bad Distinguished Name [{userDistinguishedName}]" );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.MissingInput ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Must Be A Distinguished Name" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_CreateUserSimplePassword()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Creating User [{userDistinguishedName}] With A Simple Password." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "password", "password" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "CreateUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Unknown ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "The password does not meet the password policy requirements" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_GetUserDoesNotExist()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Getting User [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "GetUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Was Not Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_DeleteUserDoesNotExist()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting User [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "returnaccessrules", "true" );
            parameters.Add( "identity", userDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "DeleteUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "cannot be found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_AddAccessRuleBadUser()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For User [{userDistinguishedName}] Which Should Not Exist To Target [{manager.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_AddAccessRuleBadTarget()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Adding AccessRule For User [{manager.DistinguishedName}] To Target [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", manager.DistinguishedName );
            parameters.Add( "ruleidentity", userDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddAccessRuleToUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_RemoveAccessRuleBadUser()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting AccessRule For User [{userDistinguishedName}] Which Should Not Exist From Target [{manager.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_RemoveAccessRuleBadTarget()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Deleting AccessRule For User [{manager.DistinguishedName}] From Target [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", manager.DistinguishedName );
            parameters.Add( "ruleidentity", userDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "RemoveAccessRuleFromUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_SetAccessRuleBadUser()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Settomg AccessRule For User [{userDistinguishedName}] Which Should Not Exist On Target [{manager.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_SetAccessRuleBadTarget()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Setting AccessRule For User [{manager.DistinguishedName}] On Target [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", manager.DistinguishedName );
            parameters.Add( "ruleidentity", userDistinguishedName );
            parameters.Add( "ruletype", "Allow" );
            parameters.Add( "rulerights", "GenericAll" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "SetAccessRuleOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_PurgeAccessRuleBadUser()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRule For User [{userDistinguishedName}] Which Should Not Exist On Target [{manager.DistinguishedName}]" );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", userDistinguishedName );
            parameters.Add( "ruleidentity", manager.DistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

        [Test, Category( "Handler" ), Category( "User" )]
        public void Handler_PurgeAccessRuleBadTarget()
        {
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"CN={userName},{workspaceName}";
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            Console.WriteLine( $"Purging AccessRule For User [{manager.DistinguishedName}] On Target [{userDistinguishedName}] Which Should Not Exist." );
            parameters.Add( "returnobjects", "true" );
            parameters.Add( "identity", manager.DistinguishedName );
            parameters.Add( "ruleidentity", userDistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "PurgeAccessRulesOnUser", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.DoesNotExist ) );
            Assert.That( result.Results[0].Statuses[0].Message, Contains.Substring( "Can Not Be Found" ) );
        }

    }
}
