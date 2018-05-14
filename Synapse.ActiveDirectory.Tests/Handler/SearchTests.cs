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
    public class SearchTests
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

        [Test, Category("Handler"), Category( "Search" )]
        public void Handler_SearchTestsSuccess()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // Create Objects To Search
            UserPrincipal up1 = Utility.CreateUser( workspaceName );
            UserPrincipal up2 = Utility.CreateUser( workspaceName );
            UserPrincipal up3 = Utility.CreateUser( workspaceName );
            UserPrincipal up4 = Utility.CreateUser( workspaceName );

            GroupPrincipal gp1 = Utility.CreateGroup( workspaceName );
            GroupPrincipal gp2 = Utility.CreateGroup( workspaceName );


            // Search For Users
            Console.WriteLine( $"Searching For All Users In : [{workspaceName}]" );
            parameters.Clear();
            parameters.Add( "searchbase", workspaceName );
            parameters.Add( "filter", "(objectClass=User)" );
            parameters.Add( "attributes", @"[ ""objectGUID"", ""objectSid"" ]" );

            ActiveDirectoryHandlerResults result = Utility.CallPlan( "Search", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 4 ) );

            // Search For Groups
            Console.WriteLine( $"Searching For All Groups In : [{workspaceName}]" );
            parameters.Clear();
            parameters.Add( "searchbase", workspaceName );
            parameters.Add( "filter", "(objectClass=Group)" );
            parameters.Add( "attributes", @"[ ""objectGUID"", ""objectSid"" ]" );

            result = Utility.CallPlan( "Search", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 2 ) );

            // Check Group Membership (GetAllGroups)
            DirectoryServices.AddToGroup(gp2.DistinguishedName, gp1.DistinguishedName, "group");
            DirectoryServices.AddToGroup(gp1.DistinguishedName, up1.DistinguishedName, "user");

            Console.WriteLine( $"Searching For All Groups For User : [{up1.DistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "distinguishedname", up1.DistinguishedName );

            result = Utility.CallPlan( "GetAllGroups", parameters );
            Assert.That( result.Results[0].Statuses[0].StatusId, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 3 ) );

            // Delete Search Objects
            Utility.DeleteUser( up1.DistinguishedName );
            Utility.DeleteUser( up2.DistinguishedName );
            Utility.DeleteUser( up3.DistinguishedName );
            Utility.DeleteUser( up4.DistinguishedName );
            Utility.DeleteGroup( gp1.DistinguishedName );
            Utility.DeleteGroup( gp2.DistinguishedName );
        }

    }
}
