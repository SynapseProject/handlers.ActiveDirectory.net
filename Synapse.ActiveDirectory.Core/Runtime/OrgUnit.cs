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
        public static void CreateOrganizationUnit(string identity, string description, List<PropertyType> properties, bool isDryRun = false )
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
        public static void CreateOrganizationUnit(string newOrgUnitName, string parentOrgUnitPath, string description, List<PropertyType> properties, bool isDryRun = false)
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
                                if ( !string.IsNullOrWhiteSpace( description ) )
                                {
                                    newOrgUnit.Properties["Description"].Value = description;
                                }

                                newOrgUnit.CommitChanges();
                                SetProperties( newOrgUnit, properties );
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

        public static void ModifyOrganizationUnit(string identity, string description, List<PropertyType> properties, bool isDryRun = false)
        {
            DirectoryEntry orgUnit = GetDirectoryEntry( identity );
            if ( orgUnit != null )
            {
                if ( description != null )
                {
                    orgUnit.Properties["description"].Clear();
                    orgUnit.Properties["description"].Add( description );
                }

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

            DirectoryEntry orgUnitForDelete = GetDirectoryEntry( identity );

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

        public static bool IsExistingDirectoryEntry(string identity, string objectClass = "organizationalUnit")
        {
            return GetDirectoryEntry( identity, objectClass ) != null;
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

        public static void MoveUserToOrganizationUnit(string username, string orgUnitDistName, bool isDryRun = false)
        {
            if ( string.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "User is not specified.", AdStatusType.MissingInput );
            }

            if ( string.IsNullOrWhiteSpace( orgUnitDistName ) )
            {
                throw new AdException( "Organization unit is not specified.", AdStatusType.MissingInput );
            }

            if ( !IsExistingUser( username ) )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( !IsExistingOrganizationUnit( orgUnitDistName ) )
            {
                throw new AdException( "Organization unit cannot be found.", AdStatusType.DoesNotExist );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( username );
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
            if ( string.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group is not specified.", AdStatusType.MissingInput );
            }

            if ( string.IsNullOrWhiteSpace( orgUnitDistName ) )
            {
                throw new AdException( "Organization unit is not specified.", AdStatusType.MissingInput );
            }

            if ( !IsExistingGroup( groupName ) )
            {
                throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( !IsExistingOrganizationUnit( orgUnitDistName ) )
            {
                throw new AdException( "Organization unit cannot be found.", AdStatusType.DoesNotExist );
            }

            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );
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

        public static OrganizationalUnitObject GetOrganizationalUnit(string name, string path)
        {
            String distinguishedName = String.IsNullOrWhiteSpace( path ) ? 
                $"ou={name},{GetDomainDistinguishedName().Replace( "LDAP://", "" )}" : 
                $"ou={name},{path.Replace( "LDAP://", "" )}";
            return GetOrganizationalUnit( distinguishedName );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string distinguishedName)
        {
            DirectoryEntry ou = GetDirectoryEntry( distinguishedName );

            if ( ou == null )
                throw new AdException( $"Organizational Unit [{distinguishedName}] Not Found.", AdStatusType.DoesNotExist );
            else
                return new OrganizationalUnitObject( ou );
        }
    }
}