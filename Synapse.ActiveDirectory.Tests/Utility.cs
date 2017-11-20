using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Synapse.ActiveDirectory.Core;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Synapse.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.ActiveDirectory;

namespace Synapse.ActiveDirectory.Tests
{
    public class Utility
    {
        public static Random random = new Random();

        public static string GenerateToken(byte length)
        {
            var bytes = new byte[length];
            random.NextBytes( bytes );
            return Convert.ToBase64String( bytes ).Replace( "=", "e" ).Replace( "+", "p" ).Replace( "/", "s" );
        }

        public static DirectoryEntry CreateWorkspace()
        {
            String domainRoot = DirectoryServices.GetDomainDistinguishedName();
            String orgUnitName = $"OU=SynUnitTests_{Utility.GenerateToken( 8 )},{domainRoot}";

            // Setup Test Workspace
            Console.WriteLine( $"Creating Workspace : [{orgUnitName}]" );
            Dictionary<string, List<string>> properties = new Dictionary<string, List<string>>();
            DirectoryServices.AddProperty( properties, "description", "UnitTest Workspace" );
            DirectoryEntry workspace = DirectoryServices.CreateDirectoryEntry( "OrganizationalUnit", orgUnitName, properties );
            Assert.That( workspace, Is.Not.Null );
            return workspace;
        }

        public static void DeleteWorkspace(string workspaceName)
        {
            Console.WriteLine( $"Deleting Workspace : [{workspaceName}]" );
            DirectoryServices.DeleteDirectoryEntry( "OrganizationalUnit", workspaceName );
            DirectoryEntry de = DirectoryServices.GetDirectoryEntry( workspaceName, "OrganizationalUnit" );
            Assert.That( de, Is.Null );
        }

        public static UserPrincipal CreateUser(string workspaceName)
        {
            String name = $"testuser_{Utility.GenerateToken( 8 )}";
            String testUserName = $"CN={name},{workspaceName}";
            Console.WriteLine( $"Creating User : [{testUserName}]" );
            UserPrincipal testUser = DirectoryServices.CreateUserPrincipal( testUserName );
            DirectoryServices.SaveUser( testUser );
            Assert.That( testUser.Name, Is.EqualTo( name ) );

            return testUser;
        }

        public static void DeleteUser(string identity)
        {
            Console.WriteLine( $"Deleting User [{identity}]" );
            DirectoryServices.DeleteUser( identity );

            UserPrincipalObject upo = DirectoryServices.GetUser( identity, false, false, false );
            Assert.That( upo, Is.Null );
        }

        public static GroupPrincipal CreateGroup(string workspaceName)
        {
            String name = $"testgroup_{Utility.GenerateToken( 8 )}";
            String testGroupName = $"CN={name},{workspaceName}";
            Console.WriteLine( $"Creating Group : [{testGroupName}]" );
            GroupPrincipal testGroup = DirectoryServices.CreateGroupPrincipal( testGroupName );
            DirectoryServices.SaveGroup( testGroup );
            Assert.That( testGroup.Name, Is.EqualTo( name ) );

            return testGroup;
        }

        public static void DeleteGroup(string identity)
        {
            Console.WriteLine( $"Deleting Group [{identity}]" );
            DirectoryServices.DeleteGroup( identity );

            GroupPrincipalObject gpo = DirectoryServices.GetGroup( identity, false, false, false );
            Assert.That( gpo, Is.Null );
        }

        public static string GetGroupOrganizationUnit(string groupName)
        {
            try
            {
                using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                {
                    using ( GroupPrincipal user = GroupPrincipal.FindByIdentity( context, groupName ) )
                    {
                        if ( user != null )
                        {
                            using ( DirectoryEntry deGroup = user.GetUnderlyingObject() as DirectoryEntry )
                            {
                                if ( deGroup != null )
                                {
                                    using ( DirectoryEntry deGroupContainer = deGroup.Parent )
                                    {
                                        return deGroupContainer.Properties["Name"].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public static string GetUserOrganizationUnit(string username)
        {
            try
            {
                using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                {
                    using ( UserPrincipal user = UserPrincipal.FindByIdentity( context, username ) )
                    {
                        if ( user != null )
                        {
                            using ( DirectoryEntry deUser = user.GetUnderlyingObject() as DirectoryEntry )
                            {
                                if ( deUser != null )
                                {
                                    using ( DirectoryEntry deUserContainer = deUser.Parent )
                                    {
                                        return deUserContainer.Properties["Name"].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static string FindPlanFile(string planName)
        {
            if ( !planName.EndsWith( ".yaml", StringComparison.OrdinalIgnoreCase ) )
                planName = $"{planName}.yaml";

            string plansDirectory = $"{TestContext.CurrentContext.TestDirectory}\\..\\..\\Handler\\Plans";
            DirectoryInfo plansDir = new DirectoryInfo( plansDirectory );
            plansDirectory = plansDir.FullName;

            String fileName = $"{plansDirectory}\\{planName}";
            // If Not In Folder Under "Tests", Check Under "ActiveDirectoryApi" Folder
            if ( !File.Exists( fileName ) )
            {
                plansDirectory = $"{TestContext.CurrentContext.TestDirectory}\\..\\..\\..\\Syanpse.Services.ActiveDirectoryApi\\Plans";
                plansDir = new DirectoryInfo( plansDirectory );
                plansDirectory = plansDir.FullName;
                fileName = $"{plansDirectory}\\{planName}";
            }

            return fileName;
        }

        private static object CallRawPlan(string planName, Dictionary<string, string> dynamicParameters = null)
        {
            String planFullPath = Utility.FindPlanFile( planName );
            Console.WriteLine( $"Executing Plan : {planFullPath}" );
            if (dynamicParameters != null)
            {
                Console.WriteLine( $"Dynamic Parameters :" );
                foreach ( KeyValuePair<string, string> param in dynamicParameters )
                    Console.WriteLine( $"  {param.Key} : {param.Value}" );
            }
            Console.WriteLine( "" );
            Plan plan = Plan.FromYaml( planFullPath );
            plan.Start( dynamicParameters, false, true );
            object output = (String)(plan.ResultPlan.Actions[0].Result.ExitData);

            return output;
        }

        public static ActiveDirectoryHandlerResults CallPlan(string planName, Dictionary<string, string> dynamicParameters = null)
        {
            object output = CallRawPlan( planName, dynamicParameters );
            ActiveDirectoryHandlerResults result = YamlHelpers.Deserialize<ActiveDirectoryHandlerResults>( output.ToString() );
            Console.WriteLine( YamlHelpers.Serialize( result ) );
            return result;
        }

    }
}
