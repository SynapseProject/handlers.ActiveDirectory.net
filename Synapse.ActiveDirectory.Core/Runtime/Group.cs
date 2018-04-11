using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {

        public static void SaveGroup(GroupPrincipal group, bool dryRun = false)
        {
            try
            {
                if ( !dryRun )
                {
                    group.Save();
                }
            }
            catch ( PrincipalServerDownException ex )
            {
                if ( ex.Message.Contains( "The server is not operational." ) )
                {
                    throw new AdException( "Unable to connect to the domain controller. Check the OU path.", AdStatusType.ConnectionError );
                }
                throw;
            }
            catch ( PrincipalExistsException ex )
            {
                if ( ex.Message.Contains( "The object already exists." ) )
                {
                    throw new AdException( "The group already exists.", AdStatusType.AlreadyExists );
                }
            }
            catch ( PrincipalOperationException ex )
            {
                if ( ex.Message.Contains( "Unknown error (0x80005000)" ) || ex.Message.Contains( "An operations error occurred." ) )
                {
                    throw new AdException( "The OU path is not valid.", AdStatusType.InvalidPath );
                }
                throw;
            }
            catch ( InvalidOperationException ioe )
            {
                throw new AdException( ioe, AdStatusType.InvalidInput );
            }
        }

        public static GroupPrincipal CreateGroupPrincipal(string distinguishedName, string samAccountName = null, bool saveOnCreate = true)
        {
            String name = distinguishedName;
            String domain = DirectoryServices.GetDomain(distinguishedName, out name);
            String path = domain;

            if ( DirectoryServices.IsDistinguishedName( distinguishedName ) )
            {
                Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
                Match match = regex.Match( distinguishedName );
                if ( match.Success )
                {
                    name = match.Groups[1]?.Value?.Trim();
                    path = match.Groups[2]?.Value?.Trim();
                }
            }
            else if ( String.IsNullOrWhiteSpace( distinguishedName ) )
                throw new AdException( "Unable To Create Group Principal From Given Input.", AdStatusType.MissingInput );


            path = path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( path );
            GroupPrincipal group = new GroupPrincipal( context );

            group.Name = name;
            if ( samAccountName != null )
            {
                if ( samAccountName.Length < 20 )
                    group.SamAccountName = samAccountName;
                else
                    throw new AdException( $"SamAccountName [{samAccountName}] Is Longer than 20 Characters.", AdStatusType.InvalidAttribute );
            }
            else if ( name.Length < 20 )
                group.SamAccountName = name;

            if ( saveOnCreate )
                SaveGroup( group );

            return group;

        }

        public static void DeleteGroup(string identity, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
            {
                throw new AdException( "Group identity is not specified.", AdStatusType.MissingInput );
            }

            try
            {
                GroupPrincipal groupPrincipal = GetGroupPrincipal( identity );
                if ( groupPrincipal != null )
                {
                    if ( !dryRun )
                    {
                        groupPrincipal.Delete();
                    }
                }
                else
                {
                    throw new AdException( $"Group [{identity}] cannot be found.", AdStatusType.DoesNotExist );
                }
            }
            catch ( InvalidOperationException e )
            {
                throw e;
            }
        }

        public static void AddUserToGroup(string userIdentity, string groupIdentity, bool isDryRun = false, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( userIdentity ) )
            {
                throw new AdException( "User identity is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupIdentity ) )
            {
                throw new AdException( "Group identity is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( userIdentity, domainName );
            if ( userPrincipal == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupIdentity, domainName );
            if ( groupPrincipal == null )
            {
                throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( !IsUserGroupMember( userPrincipal, groupPrincipal ) )
            {
                if ( !isDryRun )
                {
                    groupPrincipal.Members.Add( userPrincipal );
                    groupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( $"User [{userIdentity}] already exists in the group [{groupIdentity}].", AdStatusType.AlreadyExists );
            }
        }

        public static void AddGroupToGroup(string childGroupIdentity, string parentGroupIdentity, bool isDryRun = false, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( childGroupIdentity ) )
            {
                throw new AdException( "Child group name is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupIdentity ) )
            {
                throw new AdException( "Parent group name is not provided.", AdStatusType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupIdentity, domainName );
            if ( childGroupPrincipal == null )
            {
                throw new AdException( "Child group cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupIdentity, domainName );
            if ( parentGroupPrincipal == null )
            {
                throw new AdException( "Parent group cannot be found.", AdStatusType.DoesNotExist );
            }

            // Verify GroupScope of ParentGroup and ChildGroup is allowed
            // Logic from : https://technet.microsoft.com/en-us/library/cc755692(v=ws.10).aspx
            if ( (parentGroupPrincipal.GroupScope == GroupScope.Universal && childGroupPrincipal.GroupScope == GroupScope.Local) ||
                 (parentGroupPrincipal.GroupScope == GroupScope.Global && childGroupPrincipal.GroupScope != GroupScope.Global) )
            {
                throw new AdException( $"Scope Error - Child Group [{childGroupPrincipal.Name}] with [{childGroupPrincipal.GroupScope}] Scope is not allowed to be a member of Parent Group [{parentGroupPrincipal.Name}] with [{parentGroupPrincipal.GroupScope}] Scope.", AdStatusType.NotAllowed );
            }


            if ( !IsGroupGroupMember( childGroupPrincipal, parentGroupPrincipal ) )
            {
                if ( !isDryRun )
                {
                    parentGroupPrincipal.Members.Add( childGroupPrincipal );
                    parentGroupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( $"Child group [{childGroupIdentity}] already exists in the parent group [{parentGroupIdentity}].", AdStatusType.AlreadyExists );
            }
        }

        public static void RemoveUserFromGroup(string userIdentity, string groupIdentity, bool isDryRun = false, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( userIdentity ) )
            {
                throw new AdException( "User identity is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupIdentity ) )
            {
                throw new AdException( "Group identity is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUserPrincipal( userIdentity, domainName );
            if ( userPrincipal == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupIdentity, domainName );
            if ( groupPrincipal == null )
            {
                throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( IsUserGroupMember( userPrincipal, groupPrincipal ) )
            {
                if ( !isDryRun )
                {
                    groupPrincipal.Members.Remove( userPrincipal );
                    groupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( "User does not exist in the group.", AdStatusType.DoesNotExist );
            }
        }

        public static void RemoveGroupFromGroup(string childGroupIdentity, string parentGroupIdentity, bool isDryRun = false, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( childGroupIdentity ) )
            {
                throw new AdException( "Child group name is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupIdentity ) )
            {
                throw new AdException( "Parent group name is not provided.", AdStatusType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupIdentity, domainName );
            if ( childGroupPrincipal == null )
            {
                throw new AdException( "Child group cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupIdentity, domainName );
            if ( parentGroupPrincipal == null )
            {
                throw new AdException( "Parent group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( IsGroupGroupMember( childGroupPrincipal, parentGroupPrincipal ) )
            {
                if ( !isDryRun )
                {
                    parentGroupPrincipal.Members.Remove( childGroupPrincipal );
                    parentGroupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( "Child group does not exist in the parent group.", AdStatusType.DoesNotExist );
            }
        }

        public static bool IsExistingGroup(string identity, string domainName = null)
        {
            return GetGroupPrincipal( identity, domainName ) != null;
        }

        public static bool IsUserGroupMember(string username, string groupName, string domainName = null)
        {
            UserPrincipal userPrincipal = GetUserPrincipal( username, domainName );
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName, domainName );
            return IsUserGroupMember( userPrincipal, groupPrincipal );
        }

        public static bool IsUserGroupMember(UserPrincipal user, GroupPrincipal group)
        {
            if ( user != null && group != null )
            {
                return group.Members.Contains( user );
            }
            return false;
        }

        public static bool IsGroupGroupMember(string childGroupName, string parentGroupName, string domainName = null)
        {
            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupName, domainName );
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupName, domainName );
            return IsGroupGroupMember( childGroupPrincipal, parentGroupPrincipal );
        }

        public static bool IsGroupGroupMember(GroupPrincipal childGroup, GroupPrincipal parentGroup)
        {
            if ( childGroup != null && parentGroup != null )
            {
                return parentGroup.Members.Contains( childGroup );
            }
            return false;
        }

        public static GroupPrincipalObject GetGroup(string identity, bool getGroups, bool getAccessRules, bool getObjectProperties)
        {
            GroupPrincipalObject g = null;
            try
            {
                String idOnly = null;
                String domain = DirectoryServices.GetDomain(identity, out idOnly);
                GroupPrincipal group = GetGroupPrincipal( idOnly, domain );

                if ( group != null )
                {
                    g = new GroupPrincipalObject( group, getAccessRules, getObjectProperties );
                    if ( getGroups )
                        g.GetGroups();
                }
            }
            catch ( MultipleMatchesException mme )
            {
                throw new AdException( $"Multiple Groups Contain The Identity [{identity}].", mme, AdStatusType.MultipleMatches );
            }

            return g;
        }

        public static GroupPrincipal GetGroupPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity( principalContext, identity );
            return groupPrincipal;
        }

        // Returns all groups a Principal is a member of, either directly, or thru group nesting
        public static List<DirectoryEntry> GetGroupMembership(Principal principal, bool includePrincipal = false)
        {
            string filter = $"(member:1.2.840.113556.1.4.1941:={principal.DistinguishedName})";
            if (includePrincipal)
                filter = $"(|(member:1.2.840.113556.1.4.1941:={principal.DistinguishedName})(distinguishedName={principal.DistinguishedName}))";
            return GetDirectoryEntries( filter );
        }
    }
}