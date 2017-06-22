using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static GroupPrincipal CreateGroup(string ouPath, string groupName, string description = null, GroupScope groupScope = GroupScope.Universal, bool isSecurityGroup = true, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                throw new LdapException( "OU path is not specified.", LdapExceptionType.InvalidPath );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not specified.", LdapExceptionType.MissingInput );
            }

            // OU path here cannot have the LDAP prefix.
            ouPath = ouPath.Replace( "LDAP://", "" );

            GroupPrincipal groupPrincipal = null;
            try
            {
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
                    throw new LdapException( "Unable to connect to the domain controller. Check the OU path.", LdapExceptionType.ConnectionError );
                }
                throw;
            }
            catch ( PrincipalExistsException ex )
            {
                if ( ex.Message.Contains( "The object already exists." ) )
                {
                    throw new LdapException( "The group already exists.", LdapExceptionType.AlreadyExists );
                }
            }
            catch ( PrincipalOperationException ex )
            {
                if ( ex.Message.Contains( "Unknown error (0x80005000)" ) || ex.Message.Contains( "An operations error occurred." ) )
                {
                    throw new LdapException( "The OU path is not valid.", LdapExceptionType.InvalidPath );
                }
                throw;
            }

            return groupPrincipal;
        }

        public static void DeleteGroup(string groupName, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not specified.", LdapExceptionType.MissingInput );
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
                    throw new LdapException( "Group does not exist.", LdapExceptionType.DoesNotExist );
                }
            }
            catch ( InvalidOperationException e )
            {
                throw e;
            }
        }

        public static void UpdateGroupAttribute(string groupName, string attribute, string value, bool dryRun = false)
        {
            GroupPrincipal gp = GetGroup( groupName );
            if ( gp == null )
            {
                throw new LdapException( "Group does not exist.", LdapExceptionType.DoesNotExist );
            }

            if ( !IsValidGroupAttribute( attribute ) )
            {
                throw new LdapException( "The attribute is not supported.", LdapExceptionType.NotSupported );
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
                            throw new LdapException( "Group cannot be found.", LdapExceptionType.DoesNotExist );
                        }
                    }
                    catch ( DirectoryServicesCOMException ex )
                    {
                        if ( ex.Message.Contains( "The attribute syntax specified to the directory service is invalid." ) )
                        {
                            throw new LdapException( "The attribute value is invalid.", LdapExceptionType.InvalidAttribute );
                        }
                        if ( ex.Message.Contains( "A constraint violation occurred." ) )
                        {
                            throw new LdapException( "The attribute value is invalid.", LdapExceptionType.InvalidAttribute );
                        }
                        throw ex;
                    }
                    catch ( COMException ex )
                    {
                        if ( ex.Message.Contains( "The server is not operational." ) )
                        {
                            throw new LdapException( "LDAP path specifieid is not valid.", LdapExceptionType.ConnectionError );
                        }
                        throw;
                    }
                }
            };
        }

        public static void AddUserToGroup(string username, string groupName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not provided.", LdapExceptionType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal == null )
            {
                throw new LdapException( "User cannot be found.", LdapExceptionType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroup( groupName );
            if ( groupPrincipal == null )
            {
                throw new LdapException( "Group cannot be found.", LdapExceptionType.DoesNotExist );
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
                throw new LdapException( "User already exists in the group.", LdapExceptionType.AlreadyExists );
            }
        }

        public static void AddGroupToGroup(string childGroupName, string parentGroupName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( childGroupName ) )
            {
                throw new LdapException( "Child group name is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupName ) )
            {
                throw new LdapException( "Parent group name is not provided.", LdapExceptionType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroup( childGroupName );
            if ( childGroupPrincipal == null )
            {
                throw new LdapException( "Child group cannot be found.", LdapExceptionType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroup( parentGroupName );
            if ( parentGroupPrincipal == null )
            {
                throw new LdapException( "Parent group cannot be found.", LdapExceptionType.DoesNotExist );
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
                throw new LdapException( "Child group already exists in the parent group.", LdapExceptionType.AlreadyExists );
            }
        }

        public static void RemoveUserFromGroup(string username, string groupName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not provided.", LdapExceptionType.MissingInput );
            }

            UserPrincipal userPrincipal = GetUser( username );
            if ( userPrincipal == null )
            {
                throw new LdapException( "User cannot be found.", LdapExceptionType.DoesNotExist );
            }
            GroupPrincipal groupPrincipal = GetGroup( groupName );
            if ( groupPrincipal == null )
            {
                throw new LdapException( "Group cannot be found.", LdapExceptionType.DoesNotExist );
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
                throw new LdapException( "User does not exist in the group.", LdapExceptionType.DoesNotExist );
            }
        }

        public static void RemoveGroupFromGroup(string childGroupName, string parentGroupName, bool isDryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( childGroupName ) )
            {
                throw new LdapException( "Child group name is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( parentGroupName ) )
            {
                throw new LdapException( "Parent group name is not provided.", LdapExceptionType.MissingInput );
            }

            GroupPrincipal childGroupPrincipal = GetGroup( childGroupName );
            if ( childGroupPrincipal == null )
            {
                throw new LdapException( "Child group cannot be found.", LdapExceptionType.DoesNotExist );
            }
            GroupPrincipal parentGroupPrincipal = GetGroup( parentGroupName );
            if ( parentGroupPrincipal == null )
            {
                throw new LdapException( "Parent group cannot be found.", LdapExceptionType.DoesNotExist );
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
                throw new LdapException( "Child group does not exist in the parent group.", LdapExceptionType.DoesNotExist );
            }
        }

        public static bool IsExistingGroup(string groupName)
        {
            return GetGroup( groupName ) != null;
        }

        public static bool IsUserGroupMember(string username, string groupName)
        {
            UserPrincipal userPrincipal = GetUser( username );
            GroupPrincipal groupPrincipal = GetGroup( groupName );

            if ( userPrincipal != null && groupPrincipal != null )
            {
                return groupPrincipal.Members.Contains( userPrincipal );
            }
            return false;
        }

        public static bool IsGroupGroupMember(string childGroupName, string parentGroupName)
        {
            GroupPrincipal childGroupPrincipal = GetGroup( childGroupName );
            GroupPrincipal parentGroupPrincipal = GetGroup( parentGroupName );

            if ( childGroupPrincipal != null && parentGroupPrincipal != null )
            {
                return parentGroupPrincipal.Members.Contains( childGroupPrincipal );
            }
            return false;
        }

        #region Helper Methods
        public static PrincipalContext GetPrincipalContext(string ouPath = "")
        {
            PrincipalContext principalContext = !String.IsNullOrWhiteSpace( ouPath ) ? new PrincipalContext( ContextType.Domain, null, ouPath ) : new PrincipalContext( ContextType.Domain );
            return principalContext;
        }

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

        public static GroupPrincipal GetGroup(string groupName)
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

        #region To Be Removed
        public static void AddUserToGroupEx(string username, string groupName, string ldapPath = "")
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( ldapPath ) )
            {
                ldapPath = $"LDAP://{GetDomainDistinguishedName()}";
            }
            else
            {
                ldapPath = $"LDAP://{ldapPath.Replace( "LDAP://", "" )}";
            }

            using ( DirectoryEntry entry = new DirectoryEntry( ldapPath ) )
            {
                try
                {
                    string userDn = "";
                    using ( DirectorySearcher mySearcher = new DirectorySearcher( entry ) { Filter = "(sAMAccountName=" + username + ")" } )
                    {
                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            userDn = result.Path;
                        }
                        else
                        {
                            throw new LdapException( "Specified user cannot be found.", LdapExceptionType.DoesNotExist );
                        }
                    }

                    using ( DirectorySearcher mySearcher = new DirectorySearcher( entry ) { Filter = "(sAMAccountName=" + groupName + ")" } )
                    {

                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            DirectoryEntry groupEntry = result.GetDirectoryEntry();
                            if ( !groupEntry.Properties["member"].Contains( userDn ) )
                            {
                                groupEntry.Properties["member"].Add( userDn );
                                groupEntry.CommitChanges();
                            }
                            else
                            {
                                throw new LdapException( "User is already a member of the group.", LdapExceptionType.AlreadyExists );
                            }
                        }
                        else
                        {
                            throw new LdapException( "Specified group cannot be found.", LdapExceptionType.DoesNotExist );
                        }
                    }
                }
                catch ( DirectoryServicesCOMException ex )
                {
                    if ( ex.Message.Contains( "The server is unwilling to process the request." ) )
                    {
                        throw new LdapException( "User's distinguished name is invalid.", LdapExceptionType.InvalidName );
                    }
                }
                catch ( COMException ex )
                {
                    if ( ex.Message.Contains( "The server is not operational." ) )
                    {
                        throw new LdapException( "LDAP path specifieid is not valid.", LdapExceptionType.InvalidPath );
                    }
                }
            }
        }

        public static void CreateGroupEx(string ouPath, string groupName, bool dryRun = false)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                throw new LdapException( "OU path is not specified.", LdapExceptionType.MissingInput );
            }


            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not specified.", LdapExceptionType.MissingInput );
            }

            ouPath = ouPath.Contains( "LDAP://" ) ? ouPath : $"LDAP://{ouPath}";
            string groupPath = $"LDAP://CN={groupName},{ouPath.Replace( "LDAP://", "" )}";

            try
            {
                if ( !DirectoryEntry.Exists( groupPath ) )
                {
                    if ( !dryRun )
                    {
                        DirectoryEntry entry = new DirectoryEntry( ouPath );
                        DirectoryEntry group = entry.Children.Add( "CN=" + groupName, "group" );
                        // By default if no GroupType property is set, the group is created as a domain security group.
                        group.Properties["sAmAccountName"].Value = groupName;
                        group.CommitChanges();
                    }
                }
                else
                {
                    throw new LdapException( groupPath + " already exists.", LdapExceptionType.AlreadyExists );
                }
            }
            catch ( COMException e )
            {
                if ( e.Message.Contains( "Unknown error " ) )
                {
                    throw new LdapException( "OU path is not valid.", LdapExceptionType.InvalidPath );
                }
            }
        }

        public static void DeleteGroupEx(string ouPath, string groupPath, bool dryRun)
        {
            if ( String.IsNullOrWhiteSpace( ouPath ) )
            {
                throw new LdapException( "OU path is not specified.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupPath ) )
            {
                throw new LdapException( "Group path is not specified.", LdapExceptionType.MissingInput );
            }

            ouPath = $"LDAP://{ouPath.Replace( "LDAP://", "" )}";
            groupPath = $"LDAP://{groupPath.Replace( "LDAP://", "" )}";

            try
            {
                if ( DirectoryEntry.Exists( "LDAP://" + groupPath ) )
                {
                    try
                    {
                        if ( !dryRun )
                        {
                            DirectoryEntry entry = new DirectoryEntry( ouPath );
                            DirectoryEntry group = new DirectoryEntry( groupPath );
                            entry.Children.Remove( group );
                            group.CommitChanges();
                        }
                    }
                    catch ( Exception e )
                    {
                        if ( e.Message.Contains( "Unknown error " ) )
                        {
                            throw new LdapException( "OU path is not valid.", LdapExceptionType.InvalidPath );
                        }
                    }
                }
                else
                {
                    throw new LdapException( ouPath + " doesn't exist", LdapExceptionType.InvalidPath );
                }
            }
            catch ( COMException e )
            {
                if ( e.Message.Contains( "Unknown error " ) )
                {
                    throw new LdapException( "OU path is not valid.", LdapExceptionType.InvalidPath );
                }
            }
        }

        public static void RemoveUserFromGroupEx(string username, string groupName, string ldapPath = "")
        {
            if ( String.IsNullOrWhiteSpace( username ) )
            {
                throw new LdapException( "Username is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( groupName ) )
            {
                throw new LdapException( "Group name is not provided.", LdapExceptionType.MissingInput );
            }

            if ( String.IsNullOrWhiteSpace( ldapPath ) )
            {
                ldapPath = $"LDAP://{GetDomainDistinguishedName()}";
            }
            else
            {
                ldapPath = $"LDAP://{ldapPath.Replace( "LDAP://", "" )}";
            }

            using ( DirectoryEntry entry = new DirectoryEntry( ldapPath ) )
            {
                try
                {
                    string userDn = "";
                    using ( DirectorySearcher mySearcher = new DirectorySearcher( entry ) { Filter = "(sAMAccountName=" + username + ")" } )
                    {
                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            userDn = result.Path;
                        }
                        else
                        {
                            throw new LdapException( "Specified user cannot be found.", LdapExceptionType.DoesNotExist );
                        }
                    }

                    using ( DirectorySearcher mySearcher = new DirectorySearcher( entry ) { Filter = "(sAMAccountName=" + groupName + ")" } )
                    {

                        SearchResult result = mySearcher.FindOne();
                        if ( result != null )
                        {
                            DirectoryEntry groupEntry = result.GetDirectoryEntry();
                            if ( groupEntry.Properties["member"].Contains( userDn ) )
                            {
                                groupEntry.Properties["member"].Remove( userDn );
                                groupEntry.CommitChanges();
                            }
                            else
                            {
                                throw new LdapException( "User is not a member of the group.", LdapExceptionType.DoesNotExist );
                            }
                        }
                        else
                        {
                            throw new LdapException( "Specified group cannot be found.", LdapExceptionType.DoesNotExist );
                        }
                    }
                }
                catch ( DirectoryServicesCOMException ex )
                {
                    if ( ex.Message.Contains( "The server is unwilling to process the request." ) )
                    {
                        throw new LdapException( "User's distinguished name is invalid.", LdapExceptionType.InvalidName );
                    }
                }
                catch ( COMException ex )
                {
                    if ( ex.Message.Contains( "The server is not operational." ) )
                    {
                        throw new LdapException( "LDAP path specifieid is not valid.", LdapExceptionType.InvalidPath );
                    }
                }
            }
        }

        public static GroupPrincipalObject GetGroup(string sAMAccountName, bool getGroups)
        {
            GroupPrincipalObject g = null;
            using ( PrincipalContext context = new PrincipalContext( ContextType.Domain ) )
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity( context, IdentityType.SamAccountName, sAMAccountName );
                g = new GroupPrincipalObject( group );
                if ( getGroups )
                    g.GetGroups();
            }
            return g;
        }
        #endregion
    }
}