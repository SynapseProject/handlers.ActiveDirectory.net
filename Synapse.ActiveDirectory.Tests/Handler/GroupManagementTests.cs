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
    public class GroupManagementTests
    {
        DirectoryEntry workspace = null;
        String workspaceName = null;
        GroupPrincipal targetGroup = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            targetGroup = Utility.CreateGroup( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteGroup( targetGroup.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category("Handler"), Category( "GroupManagement" )]
        public void Handler_GroupManagementTestsSuccess()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Users
            UserPrincipal user = Utility.CreateUser( workspaceName );
            UserPrincipalObject upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            int initialCount = upo.Groups.Count;

            // Add User To Group
            Console.WriteLine( $"Adding User [{user.Name}] To Group [{targetGroup.Name}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "identity", user.DistinguishedName );
            parameters.Add( "memberof", targetGroup.DistinguishedName );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "AddUserToGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.Groups.Count, Is.EqualTo( initialCount + 1 ) );

            // Remove User From Group
            Console.WriteLine( $"Removing User [{user.Name}] From Group [{targetGroup.Name}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "identity", user.DistinguishedName );
            parameters.Add( "memberof", targetGroup.DistinguishedName );

            result = Utility.CallPlan( "RemoveUserFromGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].User.Groups.Count, Is.EqualTo( initialCount ) );

            Utility.DeleteUser( user.DistinguishedName );

            // Groups
            GroupPrincipal group = Utility.CreateGroup( workspaceName );
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( group.DistinguishedName, true, false, false );
            initialCount = gpo.Groups.Count;

            // Add Group To Group
            Console.WriteLine( $"Adding Group [{group.Name}] To Group [{targetGroup.Name}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "identity", group.DistinguishedName );
            parameters.Add( "memberof", targetGroup.DistinguishedName );

            result = Utility.CallPlan( "AddGroupToGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.Groups.Count, Is.EqualTo( initialCount + 1 ) );

            // Remove Group From Group
            Console.WriteLine( $"Removing Group [{group.Name}] From Group [{targetGroup.Name}]" );
            parameters.Clear();
            parameters.Add( "returngroupmembership", "true" );
            parameters.Add( "identity", group.DistinguishedName );
            parameters.Add( "memberof", targetGroup.DistinguishedName );

            result = Utility.CallPlan( "RemoveGroupFromGroup", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].Group.Groups.Count, Is.EqualTo( initialCount ) );

            Utility.DeleteGroup( group.DistinguishedName );
        }

    }
}
