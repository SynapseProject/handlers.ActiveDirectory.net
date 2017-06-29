using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static void CreateOrganizationUnit(string distinguishedPath, string description, bool isDryRun = false)
        {
            Regex regex = new Regex( @"ou=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedPath );
            if ( match.Success )
            {
                String ouName = match.Groups[1]?.Value?.Trim();
                String parentPath = match.Groups[2]?.Value?.Trim();
                CreateOrganizationUnit( ouName, parentPath, description, isDryRun );
            }
        }

        public static void CreateOrganizationUnit(string newOrgUnitName, string parentOrgUnitPath, string description, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( newOrgUnitName ) )
            {
                throw new LdapException( "New organization unit is not specified.", LdapStatusType.MissingInput );
            }

            parentOrgUnitPath = String.IsNullOrWhiteSpace( parentOrgUnitPath ) ? GetDomainDistinguishedName() : parentOrgUnitPath.Replace( "LDAP://", "" );
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitPath}";

            if ( IsExistingOrganizationUnit( parentOrgUnitPath ) )
            {
                using ( DirectoryEntry parentOrgUnit = new DirectoryEntry(
                    $"LDAP://{parentOrgUnitPath}",
                    null, // Username
                    null, // Password
                    AuthenticationTypes.Secure ) )
                {
                    if ( IsExistingOrganizationUnit( newOrgUnitPath ) )
                    {
                        throw new LdapException( "New organization unit already exists.", LdapStatusType.AlreadyExists );
                    }

                    using ( DirectoryEntry newOrgUnit = parentOrgUnit.Children.Add( $"OU={newOrgUnitName}", "OrganizationalUnit" ) )
                    {
                        if ( !isDryRun )
                        {
                            if ( !String.IsNullOrWhiteSpace( description ) )
                            {
                                newOrgUnit.Properties["Description"].Value = description;
                            }

                            newOrgUnit.CommitChanges();
                        }
                    }
                }
            }
            else
            {
                throw new LdapException( "Parent organization unit does not exist.", LdapStatusType.DoesNotExist );
            }
        }

        public static void DeleteOrganizationUnit(string name, string path, bool isDryRun = false)
        {
            String distinguishedName = $"ou={name},{path.Replace( "LDAP://", "" )}";
            DeleteOrganizationUnit( distinguishedName, isDryRun );
        }

        public static void DeleteOrganizationUnit(string distinguishedName, bool isDryRun = false)
        {
            // Exact distinguished name of the organization unit is expected.
            if ( String.IsNullOrWhiteSpace( distinguishedName ) )
            {
                throw new LdapException( "Organization unit is not specified.", LdapStatusType.MissingInput );
            }

            distinguishedName = distinguishedName.Replace( "LDAP://", "" );

            if ( IsExistingOrganizationUnit( distinguishedName ) )
            {
                using ( DirectoryEntry orgUnitForDeletion = new DirectoryEntry(
                    $"LDAP://{distinguishedName}",
                    null, // Username
                    null, // Password
                    AuthenticationTypes.Secure ) )
                {
                    if ( !isDryRun )
                    {
                        try
                        {
                            orgUnitForDeletion.DeleteTree();
                            orgUnitForDeletion.CommitChanges();
                        }
                        catch ( InvalidOperationException )
                        {
                            throw new LdapException( "Organization unit specified is not a container.", LdapStatusType.InvalidContainer );
                        }
                    }
                }
            }
            else
            {
                throw new LdapException( "Organization unit cannot be found.", LdapStatusType.DoesNotExist );
            }
        }

        public static string GetDomainDistinguishedName()
        {
            // connect to "RootDSE" to find default naming context.
            // "RootDSE" is not a container.
            DirectoryEntry rootDSE = new DirectoryEntry( "LDAP://RootDSE" );

            // Return the distinguished name for the domain of which this directory server is a member.
            return rootDSE.Properties["defaultNamingContext"][0].ToString();
        }

        public static string FriendlyDomainToLdapDomain(string friendlyDomainName)
        {
            string ldapPath = null;
            try
            {
                DirectoryContext objContext = new DirectoryContext(
                    DirectoryContextType.Domain, friendlyDomainName );
                Domain objDomain = Domain.GetDomain( objContext );
                ldapPath = objDomain.Name;
            }
            catch ( DirectoryServicesCOMException e )
            {
                ldapPath = e.Message;
            }
            return ldapPath;
        }

        public static void DirectoryEntryConfigurationSettings(string domainADsPath)
        {
            // Result may look like below:
            // Server: XXXXXX.XXX.XXX
            // Page Size: 99
            // Password Encoding: PasswordEncodingSsl
            // Password Port: 636
            // Referral: External
            // Security Masks: Owner, Group, Dacl
            // Is Mutually Authenticated: True
            // Bind to current domain
            DirectoryEntry entry = new DirectoryEntry( domainADsPath );
            DirectoryEntryConfiguration entryConfiguration = entry.Options;

            Console.WriteLine( "Server: " + entryConfiguration.GetCurrentServerName() );
            Console.WriteLine( "Page Size: " + entryConfiguration.PageSize.ToString() );
            Console.WriteLine( "Password Encoding: " +
                entryConfiguration.PasswordEncoding.ToString() );
            Console.WriteLine( "Password Port: " +
                entryConfiguration.PasswordPort.ToString() );
            Console.WriteLine( "Referral: " + entryConfiguration.Referral.ToString() );
            Console.WriteLine( "Security Masks: " +
                entryConfiguration.SecurityMasks.ToString() );
            Console.WriteLine( "Is Mutually Authenticated: " +
                entryConfiguration.IsMutuallyAuthenticated().ToString() );
            Console.WriteLine();
            Console.Read();
        }

        public static List<string> EnumerateDomainControllers()
        {
            List<string> alDcs = new List<string>();
            Domain domain = Domain.GetCurrentDomain();
            foreach ( DomainController dc in domain.DomainControllers )
            {
                alDcs.Add( dc.Name );
            }
            return alDcs;
        }

        public static List<string> EnumerateOUMembers(string OrgUnitDistName)
        {
            // The parameter OrgUnitDistName is the Organizational Unit distinguishedName
            // such as OU=Users,dc=myDomain,dc=com
            List<string> alObjects = new List<string>();
            try
            {
                DirectoryEntry directoryObject = new DirectoryEntry( "LDAP://" + OrgUnitDistName );
                foreach ( DirectoryEntry child in directoryObject.Children )
                {
                    string childPath = child.Path.ToString();
                    alObjects.Add( childPath.Remove( 0, 7 ) );
                    //remove the LDAP prefix from the path

                    child.Close();
                    child.Dispose();
                }
                directoryObject.Close();
                directoryObject.Dispose();
            }
            catch ( DirectoryServicesCOMException e )
            {
                //TODO : Should This Error Be Ignored?  - Guy
                Console.WriteLine( "An Error Occurred: " + e.Message.ToString() );
            }
            return alObjects;
        }

        public static bool IsExistingOrganizationUnit(string ouPath)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
                return false;

            string rootPath = GetDomainDistinguishedName();
            if ( !ouPath.ToLower().Contains( rootPath.ToLower() ) )
                return false;

            ouPath = $"LDAP://{ouPath.Replace( "LDAP://", "" )}";
            return DirectoryEntry.Exists( ouPath );
        }

        public static void MoveUserToOrganizationUnit(string username, string orgUnitDistName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "User is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( orgUnitDistName ) )
            {
                throw new LdapException( "Organization unit is not specified.", LdapStatusType.MissingInput );
            }

            if ( !IsExistingUser( username ) )
            {
                throw new LdapException( "User cannot be found.", LdapStatusType.DoesNotExist );
            }

            if ( !IsExistingOrganizationUnit( orgUnitDistName ) )
            {
                throw new LdapException( "Organization unit cannot be found.", LdapStatusType.DoesNotExist );
            }

            UserPrincipal userPrincipal = GetUser( username );
            userPrincipal.GetUnderlyingObject();
            orgUnitDistName = $"LDAP://{orgUnitDistName.Replace( "LDAP://", "" )}";

            try
            {
                using ( DirectoryEntry userLocation = (DirectoryEntry)userPrincipal.GetUnderlyingObject() )
                {
                    using ( DirectoryEntry ouLocation = new DirectoryEntry( orgUnitDistName ) )
                    {
                        if ( !isDryRun )
                        {
                            userLocation.MoveTo( ouLocation );
                        }
                    }
                }
            }
            catch ( DirectoryServicesCOMException ex )
            {
                throw ex;
                //               throw new Exception($"Encountered exception while trying to move user to another organization unit: {ex.Message}");
            }
        }

        public static void MoveGroupToOrganizationUnit(string groupName, string orgUnitDistName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group is not specified.", LdapStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( orgUnitDistName ) )
            {
                throw new LdapException( "Organization unit is not specified.", LdapStatusType.MissingInput );
            }

            if ( !IsExistingGroup( groupName ) )
            {
                throw new LdapException( "Group cannot be found.", LdapStatusType.DoesNotExist );
            }

            if ( !IsExistingOrganizationUnit( orgUnitDistName ) )
            {
                throw new LdapException( "Organization unit cannot be found.", LdapStatusType.DoesNotExist );
            }

            GroupPrincipal groupPrincipal = GetGroup( groupName );
            groupPrincipal.GetUnderlyingObject();
            orgUnitDistName = $"LDAP://{orgUnitDistName.Replace( "LDAP://", "" )}";

            try
            {
                using ( DirectoryEntry groupLocation = (DirectoryEntry)groupPrincipal.GetUnderlyingObject() )
                {
                    using ( DirectoryEntry ouLocation = new DirectoryEntry( orgUnitDistName ) )
                    {
                        if ( !isDryRun )
                        {
                            groupLocation.MoveTo( ouLocation );
                        }
                    }
                }
            }
            catch ( DirectoryServicesCOMException ex )
            {
                throw ex;
                //                throw new Exception($"Encountered exception while trying to move group to another organization unit: {ex.Message}");
            }
        }

        public static string GetGroupOrganizationUnit(string groupName)
        {
            try
            {
                using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
                {
                    using ( GroupPrincipal user = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, groupName ) )
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
                    using ( UserPrincipal user = UserPrincipal.FindByIdentity( context, IdentityType.SamAccountName, username ) )
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

        public static OrganizationalUnitObject GetOrganizationalUnit(string name, string path)
        {
            String distinguishedName = $"ou={name},{path.Replace( "LDAP://", "" )}";
            return GetOrganizationalUnit( distinguishedName );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string distinguishedName)
        {
            String rootName = distinguishedName;
            if (distinguishedName.StartsWith("LDAP://"))
                distinguishedName = distinguishedName.Replace( "LDAP://", "" );
            else
                rootName = $"LDAP://{rootName}";

            using ( DirectoryEntry root = new DirectoryEntry( rootName ) )
            using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = $"(&(objectClass=organizationalUnit))"; //(name={name})
                searcher.SearchScope = SearchScope.Base;
                searcher.PropertiesToLoad.Add( "name" );
                searcher.PropertiesToLoad.Add( "distinguishedname" );
                searcher.ReferralChasing = ReferralChasingOption.All;

                DirectoryEntry ou = null;
                SearchResultCollection results = searcher.FindAll();
                foreach ( SearchResult result in results )
                    if ( result.Properties["distinguishedname"][0].ToString().Equals( distinguishedName, StringComparison.OrdinalIgnoreCase ) )
                        ou = result.GetDirectoryEntry();

                if ( ou == null )
                    throw new LdapException( $"Organizational Unit [{distinguishedName}] Not Found.", LdapStatusType.DoesNotExist );
                else
                    return new OrganizationalUnitObject( ou );
            }
        }

        #region To Be Deleted

        #endregion
    }
}