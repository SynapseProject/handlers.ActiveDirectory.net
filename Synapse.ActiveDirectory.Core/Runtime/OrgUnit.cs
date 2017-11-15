using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
        public static void CreateOrganizationUnit(string identity, string description, Dictionary<String, List<String>> properties, bool isDryRun = false )
        {
            Regex regex = new Regex( @"ou=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( identity );
            if ( match.Success )
            {
                string ouName = match.Groups[1]?.Value?.Trim();
                string parentPath = match.Groups[2]?.Value?.Trim();
                CreateOrganizationUnit( ouName, parentPath, description, properties, isDryRun );
            }
            else
                throw new AdException( $"Unable To Locate OrgUnit Name In Distinguished Name [{identity}]." );

        }

        // TODO : Make "private" after removed from all the OrgUnit Tests.
        private static void CreateOrganizationUnit(string newOrgUnitName, string parentOrgUnitPath, string description, Dictionary<String, List<String>> properties, bool isDryRun = false)
        {
            if ( string.IsNullOrWhiteSpace( newOrgUnitName ) )
            {
                throw new AdException( "New organization unit is not specified.", AdStatusType.MissingInput );
            }

            parentOrgUnitPath = string.IsNullOrWhiteSpace( parentOrgUnitPath ) ? GetDomainDistinguishedName() : parentOrgUnitPath.Replace( "LDAP://", "" );
            string newOrgUnitPath = $"OU={newOrgUnitName},{parentOrgUnitPath}";

            if ( IsExistingOrganizationUnit( parentOrgUnitPath ) )
            {
                using ( DirectoryEntry parentOrgUnit = new DirectoryEntry(
                    $"LDAP://{parentOrgUnitPath}",
                    null, // Username
                    null, // Password
                    AuthenticationTypes.Secure ) )
                {
                    if ( !IsExistingOrganizationUnit( newOrgUnitPath ) )
                    {

                        using ( DirectoryEntry newOrgUnit = parentOrgUnit.Children.Add( $"OU={newOrgUnitName}", "OrganizationalUnit" ) )
                        {
                            if ( !isDryRun )
                            {
                                if ( !String.IsNullOrWhiteSpace(description) )
                                    SetProperty( newOrgUnit, "description", description );

                                SetProperties( newOrgUnit, properties );
                                newOrgUnit.CommitChanges();
                            }
                        }
                    }
                    else
                        throw new AdException( "New organization unit already exists.", AdStatusType.AlreadyExists );

                }
            }
            else
            {
                throw new AdException( "Parent organization unit does not exist.", AdStatusType.DoesNotExist );
            }
        }

        public static void ModifyOrganizationUnit(string identity, string description, Dictionary<String, List<String>> properties, bool isDryRun = false)
        {
            DirectoryEntry orgUnit = GetDirectoryEntry( identity, "organizationalUnit" );
            if ( orgUnit != null )
            {
                if ( description != null )
                    SetProperty( orgUnit, "description", description );

                SetProperties( orgUnit, properties );
                orgUnit.CommitChanges();
            }
            else
                throw new AdException( $"Organizational Unit [{identity}] Not Found." );
        }

        public static void DeleteOrganizationUnit(string identity, bool isDryRun = false)
        {
            // Exact distinguished name of the organization unit is expected.
            if ( string.IsNullOrWhiteSpace( identity ) )
            {
                throw new AdException( "Organization unit is not specified.", AdStatusType.MissingInput );
            }

            identity = identity.Replace( "LDAP://", "" );

            DirectoryEntry orgUnitForDelete = GetDirectoryEntry( identity, "organizationalUnit" );

            if ( orgUnitForDelete != null )
            {
                if ( !isDryRun )
                {
                    try
                    {
                        orgUnitForDelete.DeleteTree();
                        orgUnitForDelete.CommitChanges();
                    }
                    catch ( InvalidOperationException )
                    {
                        throw new AdException( "Organization unit specified is not a container.", AdStatusType.InvalidContainer );
                    }
                }
            }
            else
            {
                throw new AdException( "Organization unit cannot be found.", AdStatusType.DoesNotExist );
            }
        }

        public static bool IsExistingDirectoryEntry(string identity)
        {
            return GetDirectoryEntry( identity ) != null;
        }

        public static bool IsExistingOrganizationUnit(string ouPath)
        {
            if ( string.IsNullOrWhiteSpace( ouPath ) )
                return false;

            string rootPath = GetDomainDistinguishedName();
            if ( !ouPath.ToLower().Contains( rootPath.ToLower() ) )
                return false;

            ouPath = $"LDAP://{ouPath.Replace( "LDAP://", "" )}";
            return DirectoryEntry.Exists( ouPath );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string identity, bool getAccessRules, bool getObjectProperties)
        {
            string searchString = null;
            if ( IsDistinguishedName( identity ) )
                searchString = $"(distinguishedName={identity})";
            else if ( IsGuid( identity ) )
                searchString = $"(objectGuid={GetGuidSearchBytes( identity )})";
            else
                searchString = $"(name={identity})";

            string filter = $"(&(objectClass=organizationalUnit){searchString})";

            List<DirectoryEntry> entries = GetDirectoryEntries( filter );

            if ( entries.Count < 1 )
                throw new AdException( $"Organizational Unit [{identity}] Not Found.", AdStatusType.DoesNotExist );
            else if ( entries.Count > 1 )
                throw new AdException( $"Multiple Organizational Unites Contain The Identity [{identity}].", AdStatusType.MultipleMatches );
            else
                return new OrganizationalUnitObject( entries[0], getAccessRules, getObjectProperties );
        }

        public static DirectoryEntry Move(string identity, string destination)
        {
            DirectoryEntry source = GetDirectoryEntry( identity );
            DirectoryEntry target = GetDirectoryEntry( destination );

            source.MoveTo( target );

            return GetDirectoryEntry( identity );
        }
    }
}