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
        public static void CreateOrganizationUnit(string distinguishedName, string description, bool isDryRun = false, bool upsert = true )
        {
            Regex regex = new Regex( @"ou=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                string ouName = match.Groups[1]?.Value?.Trim();
                string parentPath = match.Groups[2]?.Value?.Trim();
                CreateOrganizationUnit( ouName, parentPath, description, isDryRun );
            }
            else
                throw new LdapException( $"Unable To Locate OrgUnit Name In Distinguished Name [{distinguishedName}]." );

        }

        public static void CreateOrganizationUnit(string newOrgUnitName, string parentOrgUnitPath, string description, bool isDryRun = false, bool upsert = true)
        {
            if ( string.IsNullOrWhiteSpace( newOrgUnitName ) )
            {
                throw new LdapException( "New organization unit is not specified.", LdapStatusType.MissingInput );
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
                            }
                        }
                    }
                    else if ( upsert )
                    {
                        ModifyOrganizationUnit( newOrgUnitName, parentOrgUnitPath, description, isDryRun, false );
                    }
                    else
                        throw new LdapException( "New organization unit already exists.", LdapStatusType.AlreadyExists );

                }
            }
            else
            {
                throw new LdapException( "Parent organization unit does not exist.", LdapStatusType.DoesNotExist );
            }
        }

        public static void ModifyOrganizationUnit(string distinguishedName, string description, bool isDryRun = false, bool upsert = true)
        {
            Regex regex = new Regex( @"ou=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                string ouName = match.Groups[1]?.Value?.Trim();
                string parentPath = match.Groups[2]?.Value?.Trim();
                ModifyOrganizationUnit( ouName, parentPath, description, isDryRun );
            }
            else
                throw new LdapException( $"Unable To Locate OrgUnit Name In Distinguished Name [{distinguishedName}]." );

        }

        public static void ModifyOrganizationUnit(string orgUnitName, string parentOrgUnitPath, string description, bool isDryRun = false, bool upsert = true)
        {
            if ( string.IsNullOrWhiteSpace( orgUnitName ) )
            {
                throw new LdapException( "New organization unit is not specified.", LdapStatusType.MissingInput );
            }

            parentOrgUnitPath = string.IsNullOrWhiteSpace( parentOrgUnitPath ) ? GetDomainDistinguishedName() : parentOrgUnitPath.Replace( "LDAP://", "" );
            string orgUnitPath = $"OU={orgUnitName},{parentOrgUnitPath}";

            DirectoryEntry ou = GetDirectoryEntry( orgUnitPath );
            if (ou != null)
            {
                if ( description != null )
                {
                    ou.Properties["description"].Clear();
                    ou.Properties["description"].Add( description );
                }
                ou.CommitChanges();
            }
            else if ( upsert )
            {
                CreateOrganizationUnit( orgUnitName, parentOrgUnitPath, description, isDryRun, false );
            }
            else
            {
                throw new LdapException( "Organization Unit does not exist.", LdapStatusType.DoesNotExist );
            }

        }

        public static void DeleteOrganizationUnit(string name, string path, bool isDryRun = false)
        {
            string distinguishedName = $"ou={name},{path.Replace( "LDAP://", "" )}";
            DeleteOrganizationUnit( distinguishedName, isDryRun );
        }

        public static void DeleteOrganizationUnit(string distinguishedName, bool isDryRun = false)
        {
            // Exact distinguished name of the organization unit is expected.
            if ( string.IsNullOrWhiteSpace( distinguishedName ) )
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
                throw new LdapException( "User is not specified.", LdapStatusType.MissingInput );
            }

            if ( string.IsNullOrWhiteSpace( orgUnitDistName ) )
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
            if ( string.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group is not specified.", LdapStatusType.MissingInput );
            }

            if ( string.IsNullOrWhiteSpace( orgUnitDistName ) )
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
            string distinguishedName = $"ou={name},{path.Replace( "LDAP://", "" )}";
            return GetOrganizationalUnit( distinguishedName );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string distinguishedName)
        {
            DirectoryEntry ou = GetDirectoryEntry( distinguishedName );

            if ( ou == null )
                throw new LdapException( $"Organizational Unit [{distinguishedName}] Not Found.", LdapStatusType.DoesNotExist );
            else
                return new OrganizationalUnitObject( ou );
        }
    }
}