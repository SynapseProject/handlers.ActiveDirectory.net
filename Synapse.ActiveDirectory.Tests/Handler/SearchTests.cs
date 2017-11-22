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

        [Test]
        public void Handler_SearchTests()
        {
            // Setup Tests
            workspace = Utility.CreateWorkspace();
            String workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
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
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 4 ) );

            // Search For Groups
            Console.WriteLine( $"Searching For All Groups In : [{workspaceName}]" );
            parameters.Clear();
            parameters.Add( "searchbase", workspaceName );
            parameters.Add( "filter", "(objectClass=Group)" );
            parameters.Add( "attributes", @"[ ""objectGUID"", ""objectSid"" ]" );

            result = Utility.CallPlan( "Search", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 2 ) );

            // Check Group Membership (GetAllGroups)
            DirectoryServices.AddGroupToGroup( gp1.DistinguishedName, gp2.DistinguishedName );
            DirectoryServices.AddUserToGroup( up1.DistinguishedName, gp1.DistinguishedName );

            Console.WriteLine( $"Searching For All Groups For User : [{up1.DistinguishedName}]" );
            parameters.Clear();
            parameters.Add( "distinguishedname", up1.DistinguishedName );

            result = Utility.CallPlan( "GetAllGroups", parameters );
            Assert.That( result.Results[0].Statuses[0].Status, Is.EqualTo( AdStatusType.Success ) );
            Assert.That( result.Results[0].SearchResults.Results.Count, Is.EqualTo( 3 ) );




            // Cleanup Workspace
            Utility.DeleteUser( up1.DistinguishedName );
            Utility.DeleteUser( up2.DistinguishedName );
            Utility.DeleteUser( up3.DistinguishedName );
            Utility.DeleteUser( up4.DistinguishedName );
            Utility.DeleteGroup( gp1.DistinguishedName );
            Utility.DeleteGroup( gp2.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

    }
}
