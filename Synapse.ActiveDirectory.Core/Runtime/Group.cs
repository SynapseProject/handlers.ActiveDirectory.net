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
        public static GroupPrincipal CreateGroup(string distinguishedName, string description, GroupScope groupScope = GroupScope.Universal, bool isSecurityGroup = true, bool dryRun = false, bool upsert = true)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                String groupName = match.Groups[1]?.Value?.Trim();
                String parentPath = match.Groups[2]?.Value?.Trim();
                return CreateGroup( groupName, parentPath, description, groupScope, isSecurityGroup, dryRun, upsert );
            }
            else
                throw new AdException( $"Unable To Locate Group Name In Distinguished Name [{distinguishedName}]." );
        }

        public static GroupPrincipal CreateGroup(string groupName, string ouPath, string description, GroupScope groupScope = GroupScope.Universal, bool isSecurityGroup = true, bool dryRun = false, bool upsert = true)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                throw new AdException( "OU path is not specified.", AdStatusType.InvalidPath );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group name is not specified.", AdStatusType.MissingInput );
            }

            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );
            if ( groupPrincipal == null )
            {
                try
                {
                    // OU path here cannot have the LDAP prefix.
                    ouPath = ouPath.Replace( "LDAP://", "" );
                    PrincipalContext principalContext = GetPrincipalContext( ouPath );

                    groupPrincipal = new GroupPrincipal( principalContext, groupName )
                    {
                        Description = !String.IsNullOrWhiteSpace( description ) ? description : null, // Description cannot be empty string.
                        GroupScope = groupScope,
                        IsSecurityGroup = isSecurityGroup
                    };
                    if ( !dryRun )
                    {
                        groupPrincipal.Save();
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
            }
            else if (upsert)
            {
                ModifyGroup( groupName, ouPath, description, groupScope, isSecurityGroup, false );
            }
            else
            {
                throw new AdException( "The group already exists.", AdStatusType.AlreadyExists );
            }

            return groupPrincipal;
        }

        public static GroupPrincipal ModifyGroup(string distinguishedName, string description, GroupScope groupScope = GroupScope.Universal, bool isSecurityGroup = true, bool dryRun = false, bool upsert = true )
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                String groupName = match.Groups[1]?.Value?.Trim();
                String parentPath = match.Groups[2]?.Value?.Trim();
                return ModifyGroup( groupName, parentPath, description, groupScope, isSecurityGroup, dryRun, upsert );
            }
            else
                throw new AdException( $"Unable To Locate Group Name In Distinguished Name [{distinguishedName}]." );
        }

        public static GroupPrincipal ModifyGroup(string groupName, string ouPath, string description, GroupScope groupScope = GroupScope.Universal, bool isSecurityGroup = true, bool dryRun = false, bool upsert = true )
        {
            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group name is not specified.", AdStatusType.MissingInput );
            }

            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );
            if ( groupPrincipal != null )
            {
                try
                {
                    groupPrincipal.Description = !String.IsNullOrWhiteSpace( description ) ? description : null;
                    groupPrincipal.GroupScope = groupScope;
                    groupPrincipal.IsSecurityGroup = isSecurityGroup;

                    if ( !dryRun )
                    {
                        groupPrincipal.Save();
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
                catch ( PrincipalOperationException ex )
                {
                    if ( ex.Message.Contains( "Unknown error (0x80005000)" ) || ex.Message.Contains( "An operations error occurred." ) )
                    {
                        throw new AdException( "The OU path is not valid.", AdStatusType.InvalidPath );
                    }
                    throw;
                }
            }
            else if (upsert)
            {
                CreateGroup( groupName, ouPath, description, groupScope, isSecurityGroup, dryRun, upsert );
            }
            else
            {
                throw new AdException( "The group does not exist.", AdStatusType.DoesNotExist );
            }

            return groupPrincipal;
        }

        public static void DeleteGroup(string name, bool dryRun = false)
        {
            string groupName = GetCommonName( name );

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group name is not specified.", AdStatusType.MissingInput );
            }

            try
            {
                PrincipalContext ctx = GetPrincipalContext();
                GroupPrincipal groupPrincipal = new GroupPrincipal( ctx ) { Name = groupName };
                PrincipalSearcher searcher = new PrincipalSearcher( groupPrincipal );

                Principal foundGroup = searcher.FindOne();
                if ( foundGroup != null )
                {
                    if ( !dryRun )
                    {
                        foundGroup.Delete();
                    }
                }
                else
                {
                    throw new AdException( "Group does not exist.", AdStatusType.DoesNotExist );
                }
            }
            catch ( InvalidOperationException e )
            {
                throw e;
            }
        }

        public static void UpdateGroupAttribute(string groupName, string attribute, string value, bool dryRun = false)
        {
            GroupPrincipal gp = GetGroupPrincipal( groupName );
            if ( gp == null )
            {
                throw new AdException( "Group does not exist.", AdStatusType.DoesNotExist );
            }

            if ( !IsValidGroupAttribute( attribute ) )
            {
                throw new AdException( "The attribute is not supported.", AdStatusType.NotSupported );
            }

            string ldapPath = $"LDAP://{GetDomainDistinguishedName()}";

            using ( DirectoryEntry entry = new DirectoryEntry( ldapPath ) )
            {
                using ( DirectorySearcher mySearcher = new DirectorySearcher( entry )
                {
                    Filter = "(sAMAccountName=" + groupName + ")"
                } )
                {
                    try
                    {
                        mySearcher.PropertiesToLoad.Add( "" + attribute + "" );
                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            if ( !dryRun )
                            {
                                DirectoryEntry entryToUpdate = result.GetDirectoryEntry();

                                if ( result.Properties.Contains( "" + attribute + "" ) )
                                {
                                    if ( !(String.IsNullOrEmpty( value )) )
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Value = value;
                                    }
                                    else
                                    {
                                        entryToUpdate.Properties["" + attribute + ""].Clear();
                                    }
                                }
                                else
                                {
                                    entryToUpdate.Properties["" + attribute + ""].Add( value );
                                }
                                entryToUpdate.CommitChanges();
                                entryToUpdate.Close();
                            }
                        }
                        else
                        {
                            throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
                        }
                    }
                    catch ( DirectoryServicesCOMException ex )
                    {
                        if ( ex.Message.Contains( "The attribute syntax specified to the directory service is invalid." ) )
                        {
                            throw new AdException( "The attribute value is invalid.", AdStatusType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "A constraint violation occurred." ) )
                        {
                            throw new AdException( "The attribute value is invalid.", AdStatusType.InvalidAttribute );
                        }
                        throw ex;
                    }
                    catch ( COMException ex )
                    {
                        if ( ex.Message.Contains( "The server is not operational." ) )
                        {
                            throw new AdException( "LDAP path specifieid is not valid.", AdStatusType.ConnectionError );
                        }
                        throw;
                    }
                }
            };
        }

        public static void AddUserToGroup(string name, string group, bool isDryRun = false)
        {
            String username = GetCommonName( name );
            String groupName = GetCommonName( group );

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group name is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );
            if ( groupPrincipal == null )
            {
                throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( !IsUserGroupMember( username, groupName ) )
            {
                if ( !isDryRun )
                {
                    groupPrincipal.Members.Add( userPrincipal );
                    groupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( $"User [{username}] already exists in the group [{groupName}].", AdStatusType.AlreadyExists );
            }
        }

        public static void AddGroupToGroup(string group, string parentGroup, bool isDryRun = false)
        {
            String childGroupName = GetCommonName( group );
            String parentGroupName = GetCommonName( parentGroup );

            if ( String.IsNullOrWhiteSpace( childGroupName ) )
            {
                throw new AdException( "Child group name is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupName ) )
            {
                throw new AdException( "Parent group name is not provided.", AdStatusType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupName );
            if ( childGroupPrincipal == null )
            {
                throw new AdException( "Child group cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupName );
            if ( parentGroupPrincipal == null )
            {
                throw new AdException( "Parent group cannot be found.", AdStatusType.DoesNotExist );
            }

            // Verify GroupScope of ParentGroup and ChildGroup is allowed
            // Logic from : https://technet.microsoft.com/en-us/library/cc755692(v=ws.10).aspx
            if ( ( parentGroupPrincipal.GroupScope == GroupScope.Universal && childGroupPrincipal.GroupScope == GroupScope.Local ) ||
                 ( parentGroupPrincipal.GroupScope == GroupScope.Global && childGroupPrincipal.GroupScope != GroupScope.Global ) )
            {
                throw new AdException( $"Scope Error - Child Group [{childGroupPrincipal.Name}] with [{childGroupPrincipal.GroupScope}] Scope is not allowed to be a member of Parent Group [{parentGroupPrincipal.Name}] with [{parentGroupPrincipal.GroupScope}] Scope.", AdStatusType.NotAllowed );
            }


            if ( !IsGroupGroupMember( childGroupName, parentGroupName ) )
            {
                if ( !isDryRun )
                {
                    parentGroupPrincipal.Members.Add( childGroupPrincipal );
                    parentGroupPrincipal.Save();
                }
            }
            else
            {
                throw new AdException( $"Child group [{childGroupName}] already exists in the parent group [{parentGroupName}].", AdStatusType.AlreadyExists );
            }
        }

        public static void RemoveUserFromGroup(string name, string group, bool isDryRun = false)
        {
            String username = GetCommonName( name );
            String groupName = GetCommonName( group );

            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new AdException( "Username is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new AdException( "Group name is not provided.", AdStatusType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal == null )
            {
                throw new AdException( "User cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );
            if ( groupPrincipal == null )
            {
                throw new AdException( "Group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( IsUserGroupMember( username, groupName ) )
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

        public static void RemoveGroupFromGroup(string group, string parentGroup, bool isDryRun = false)
        {
            String childGroupName = GetCommonName( group );
            String parentGroupName = GetCommonName( parentGroup );

            if ( String.IsNullOrWhiteSpace( childGroupName ) )
            {
                throw new AdException( "Child group name is not provided.", AdStatusType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupName ) )
            {
                throw new AdException( "Parent group name is not provided.", AdStatusType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupName );
            if ( childGroupPrincipal == null )
            {
                throw new AdException( "Child group cannot be found.", AdStatusType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupName );
            if ( parentGroupPrincipal == null )
            {
                throw new AdException( "Parent group cannot be found.", AdStatusType.DoesNotExist );
            }

            if ( IsGroupGroupMember( childGroupName, parentGroupName ) )
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

        public static bool IsExistingGroup(string groupName)
        {
            return GetGroupPrincipal( groupName ) != null;
        }

        public static bool IsUserGroupMember(string username, string groupName)
        {
            UserPrincipal userPrincipal = GetUser( username );
            GroupPrincipal groupPrincipal = GetGroupPrincipal( groupName );

            if ( userPrincipal != null && groupPrincipal != null )
            {
                return groupPrincipal.Members.Contains( userPrincipal );
            }
            return false;
        }

        public static bool IsGroupGroupMember(string childGroupName, string parentGroupName)
        {
            GroupPrincipal childGroupPrincipal = GetGroupPrincipal( childGroupName );
            GroupPrincipal parentGroupPrincipal = GetGroupPrincipal( parentGroupName );

            if ( childGroupPrincipal != null && parentGroupPrincipal != null )
            {
                return parentGroupPrincipal.Members.Contains( childGroupPrincipal );
            }
            return false;
        }

        public static GroupPrincipalObject GetGroup(string name, bool getGroups)
        {
            string groupName = GetCommonName( name );

            GroupPrincipalObject g = null;
            using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, groupName );
                if ( group == null )
                    throw new AdException( $"Group [{groupName}] Not Found.", AdStatusType.DoesNotExist );

                g = new GroupPrincipalObject( group );
                if ( getGroups )
                    g.GetGroups();
            }
            return g;
        }

        #region Helper Methods
        public static List<String> GetUserGroups(string username)
        {
            List<String> myItems = new List<string>();
            UserPrincipal userPrincipal = GetUser( username );

            PrincipalSearchResult<Principal> searchResult = userPrincipal.GetGroups();

            foreach ( Principal result in searchResult )
            {
                myItems.Add( result.Name );
            }
            return myItems;
        }

        public static UserPrincipal GetUser(string username)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext();

            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity( principalContext, username );
            return userPrincipal;
        }

        public static GroupPrincipal GetGroupPrincipal(string groupName)
        {
            if ( String.IsNullOrWhiteSpace( groupName ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext();

            GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity( principalContext, groupName );
            return groupPrincipal;
        }

        public static bool IsValidGroupAttribute(string attribute)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>()
            {
                { "description", "Description" },
                { "displayName", "Display Name" },
                { "mail", "E-mail" },
                { "managedBy", "Managed By" },
                { "sAMAccountName", "Sam Account Name"}
            };

            return attributes.ContainsKey( attribute );
        }

        #endregion

    }
}