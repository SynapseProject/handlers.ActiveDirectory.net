using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
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

        [Test, Category("Core")]
        public void Core_SearchTest()
        {
            // Create Users
            UserPrincipal up1 = Utility.CreateUser( workspaceName );
            UserPrincipal up2 = Utility.CreateUser( workspaceName );
            UserPrincipal up3 = Utility.CreateUser( workspaceName );

            // Create Groups
            GroupPrincipal gp1 = Utility.CreateGroup( workspaceName );
            GroupPrincipal gp2 = Utility.CreateGroup( workspaceName );

            // Search For Users
            Console.WriteLine( $"Searching For Users In [{workspaceName}]." );
            string[] properties = new string[] { "name", "objectGUID", "objectSid" };
            SearchResults results = DirectoryServices.Search( workspaceName, @"(objectClass=User)", properties );
            Assert.That( results.Results.Count, Is.EqualTo( 3 ) );
            foreach ( SearchResultRow row in results.Results )
            {
                Console.WriteLine( $"  >> [{row.Path}]" );
                Assert.That( row.Properties.ContainsKey( "name" ), Is.True );
                Assert.That( row.Properties["name"], Is.Not.Null );
                Assert.That( row.Properties.ContainsKey( "objectGUID" ), Is.True );
                Assert.That( row.Properties["objectGUID"], Is.Not.Null );
                Assert.That( row.Properties.ContainsKey( "objectSid" ), Is.True );
                Assert.That( row.Properties["objectSid"], Is.Not.Null );
            }

            // Search For Groups
            Console.WriteLine( $"Searching For Groups In [{workspaceName}]." );
            results = DirectoryServices.Search( workspaceName, @"(objectClass=Group)", properties );
            Assert.That( results.Results.Count, Is.EqualTo( 2 ) );
            foreach ( SearchResultRow row in results.Results )
            {
                Console.WriteLine( $"  >> [{row.Path}]" );
                Assert.That( row.Properties.ContainsKey( "name" ), Is.True );
                Assert.That( row.Properties["name"], Is.Not.Null );
                Assert.That( row.Properties.ContainsKey( "objectGUID" ), Is.True );
                Assert.That( row.Properties["objectGUID"], Is.Not.Null );
                Assert.That( row.Properties.ContainsKey( "objectSid" ), Is.True );
                Assert.That( row.Properties["objectSid"], Is.Not.Null );
            }

            // Delete Users
            Utility.DeleteUser( up1.DistinguishedName );
            Utility.DeleteUser( up2.DistinguishedName );
            Utility.DeleteUser( up3.DistinguishedName );

            // Delete Groups
            Utility.DeleteGroup( gp1.DistinguishedName );
            Utility.DeleteGroup( gp2.DistinguishedName );
        }

    }
}
