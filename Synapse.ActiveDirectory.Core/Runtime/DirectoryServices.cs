using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
        private static PrincipalContext GetPrincipalContext(string ouPath = "", string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( domainName ) )
            {
                // If null, principal context defaults to a domain controller for the domain of the user principal
                // under which the thread is running.
                domainName = null;
            }

            PrincipalContext principalContext = !String.IsNullOrWhiteSpace( ouPath ) ? new PrincipalContext( ContextType.Domain, domainName, ouPath ) : new PrincipalContext( ContextType.Domain, domainName );
            return principalContext;
        }

        public static Principal GetPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            Principal principal = Principal.FindByIdentity( principalContext, identity );
            return principal;
        }

        public static string GetDomain(string identity, out string identityOnly)
        {
            String domain = null;
            String idOnly = identity;

            if (String.IsNullOrWhiteSpace(identity))
            {
                domain = GetDomainDistinguishedName();
            }
            else if (IsDistinguishedName(identity))
            {
                domain = GetDomain(identity);
            }
            else if (IsUserPrincipalName(identity))
            {
                domain = identity.Substring(identity.LastIndexOf('@') + 1);
            }
            else if (identity.Contains(@"\"))
            {
                domain = identity.Substring(0, identity.IndexOf('\\'));
                idOnly = identity.Substring(identity.IndexOf('\\') + 1);
            }
            else if (identity.Contains(@"/"))
            {
                domain = identity.Substring(0, identity.IndexOf('/'));
                idOnly = identity.Substring(identity.IndexOf('/') + 1);
            }

            identityOnly = idOnly;
            return domain;
        }

        public static string GetDomainDistinguishedName()
        {
            // connect to "RootDSE" to find default naming context.
            // "RootDSE" is not a container.
            DirectoryEntry rootDSE = new DirectoryEntry( "LDAP://RootDSE" );

            // Return the distinguished name for the domain of which this directory server is a member.
            return rootDSE.Properties["defaultNamingContext"][0].ToString();
        }

        public static bool IsDistinguishedName(String identity)
        {
            if (String.IsNullOrWhiteSpace(identity))
                return false;

            if ( identity.StartsWith( "LDAP://" ) )
                identity = identity.Replace( "LDAP://", "" );
            return Regex.IsMatch( identity, @"^\s*?(cn\s*=|ou\s*=|dc\s*=)", RegexOptions.IgnoreCase );
        }

        public static bool IsUserPrincipalName(String identity)
        {
            if (String.IsNullOrWhiteSpace(identity))
                return false;

            return identity.Contains("@");
        }

        public static bool IsGuid(String identity)
        {
            bool rc = false;
            try
            {
                Guid.Parse( identity );
                rc = true;
            }
            catch ( Exception ) { }

            return rc;
        }

        public static bool IsSid(String identity)
        {
            bool rc = false;
            try
            {
                SecurityIdentifier sid = new SecurityIdentifier( identity );
                rc = true;
            }
            catch ( Exception ) { }

            return rc;
        }

        public static string GetGuidSearchBytes(string identity)
        {
            Guid guid = Guid.Parse( identity );
            byte[] bytes = guid.ToByteArray();
            String str = BitConverter.ToString( bytes );
            str = str.Replace( '-', '\\' );

            return @"\" + str;
        }

        // Copied From https://www.codeproject.com/Articles/3688/How-to-get-user-SID-using-DirectoryServices-classe
        public static string ConvertByteToStringSid(Byte[] bytes)
        {
            StringBuilder strSid = new StringBuilder();
            strSid.Append( "S-" );
            try
            {
                // Add SID revision.
                strSid.Append( bytes[0].ToString() );
                // Next six bytes are SID authority value.
                if ( bytes[6] != 0 || bytes[5] != 0 )
                {
                    string strAuth = String.Format
                        ( "0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}",
                        (Int16)bytes[1],
                        (Int16)bytes[2],
                        (Int16)bytes[3],
                        (Int16)bytes[4],
                        (Int16)bytes[5],
                        (Int16)bytes[6] );
                    strSid.Append( "-" );
                    strSid.Append( strAuth );
                }
                else
                {
                    Int64 iVal = (Int32)(bytes[1]) +
                        (Int32)(bytes[2] << 8) +
                        (Int32)(bytes[3] << 16) +
                        (Int32)(bytes[4] << 24);
                    strSid.Append( "-" );
                    strSid.Append( iVal.ToString() );
                }

                // Get sub authority count...
                int iSubCount = Convert.ToInt32( bytes[7] );
                int idxAuth = 0;
                for ( int i = 0; i < iSubCount; i++ )
                {
                    idxAuth = 8 + i * 4;
                    UInt32 iSubAuth = BitConverter.ToUInt32( bytes, idxAuth );
                    strSid.Append( "-" );
                    strSid.Append( iSubAuth.ToString() );
                }
            }
            catch ( Exception )
            {
                return System.Text.Encoding.UTF8.GetString( bytes );
            }
            return strSid.ToString();
        }

        public static String GetDomain(String distinguishedName)
        {
            String domain = Regex.Replace( distinguishedName, @"(.*?)DC\s*=\s*(.*)", "$2", RegexOptions.IgnoreCase );
            domain = Regex.Replace( domain, @"\s*dc\s*=\s*", "", RegexOptions.IgnoreCase );
            domain = Regex.Replace( domain, @"\s*,\s*", "." );

            return domain;
        }

        public static string GetDistinguishedName(string identity)
        {
            String idOnly = null;
            String domain = GetDomain(identity, out idOnly);
            Principal principal = DirectoryServices.GetPrincipal( idOnly, domain );
            return principal?.DistinguishedName;
        }

        public static SearchResultsObject Search(string searchBase, string filter, string[] returnProperties)
        {
            SearchResultsObject searchResults = new SearchResultsObject();

            try
            {
                using (SearchResultCollection results = DoSearch(filter, returnProperties, searchBase))
                {
                    searchResults.Results = new List<SearchResultRow>();

                    foreach (SearchResult result in results)
                    {
                        SearchResultRow row = new SearchResultRow()
                        {
                            Path = result.Path
                        };

                        if (returnProperties != null)
                        {
                            row.Properties = new SerializableDictionary<string, List<string>>();
                            foreach (string key in returnProperties)
                            {
                                List<string> values = new List<string>();
                                if (result.Properties.Contains(key))
                                {
                                    foreach (object value in result.Properties[key])
                                    {
                                        string valueStr = GetPropertyValueString(value);
                                        values.Add(valueStr);
                                    }
                                    row.Properties.Add(key, values);
                                }
                                else
                                    row.Properties.Add(key, null);
                            }
                        }

                        searchResults.Results.Add(row);
                    }
                }
            }
            catch (ArgumentException argEx)
            {
                throw new AdException( argEx, AdStatusType.InvalidInput );
            }
            catch ( DirectoryServicesCOMException comEx )
            {
                throw new AdException( comEx, AdStatusType.DoesNotExist );
            }

            return searchResults;
        }

        private static SearchResultCollection DoSearch(string filter, string[] returnProperties = null, string searchBase = null)
        {
            if ( String.IsNullOrWhiteSpace( searchBase ) )
                searchBase = GetDomainDistinguishedName();
            if ( !searchBase.StartsWith( "LDAP://" ) )
                searchBase = "LDAP://" + searchBase;

            using ( DirectoryEntry root = new DirectoryEntry( searchBase ) )
            using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = filter;
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PageSize = 1000;
                if ( returnProperties != null )
                {
                    foreach ( string property in returnProperties )
                        searcher.PropertiesToLoad.Add( property );
                }
                searcher.ReferralChasing = ReferralChasingOption.All;

                SearchResultCollection results = searcher.FindAll();
                return results;
            }
        }

        public static string GetParentPath(string distinguishedName)
        {
            Regex regex = new Regex( @",(.*)" );
            Match match = regex.Match( distinguishedName );

            return match.Groups[1].Value;
        }

        public static void AddToGroup(String groupIdentity, String identity, String objectType, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(identity))
            {
                throw new AdException("Child group name is not provided.", AdStatusType.MissingInput);
            }

            if (String.IsNullOrWhiteSpace(groupIdentity))
            {
                throw new AdException("Parent group name is not provided.", AdStatusType.MissingInput);
            }

            DirectoryEntry groupDe = GetDirectoryEntry(groupIdentity, "group");
            if (groupDe == null)
            {
                throw new AdException($"Parent group [{groupIdentity}] cannot be found.", AdStatusType.DoesNotExist);
            }

            DirectoryEntry childDe = GetDirectoryEntry(identity, objectType);
            if (childDe == null)
            {
                throw new AdException($"{objectType} [{groupIdentity}] cannot be found.", AdStatusType.DoesNotExist);
            }

            // Verify GroupScope of ParentGroup and ChildGroup is allowed
            // Logic from : https://technet.microsoft.com/en-us/library/cc755692(v=ws.10).aspx
            if (childDe.SchemaClassName == "group")
            {
                GroupScope? childGroupScope = GetGroupScope(childDe);
                GroupScope? parentGroupScope = GetGroupScope(groupDe);
                if ((parentGroupScope == GroupScope.Universal && childGroupScope == GroupScope.Local) ||
                     (parentGroupScope == GroupScope.Global && childGroupScope != GroupScope.Global))
                {
                    throw new AdException($"Scope Error - Child Group [{childDe.Name}] with [{childGroupScope}] Scope is not allowed to be a member of Parent Group [{groupDe.Name}] with [{parentGroupScope}] Scope.", AdStatusType.NotAllowed);
                }
            }


            String childDistName = childDe.Properties["distinguishedName"].Value.ToString();

            if (groupDe.Properties["member"].Contains(childDistName))
                throw new AdException($"{objectType} [{identity}] already exists in the group [{groupIdentity}].", AdStatusType.AlreadyExists);

            groupDe.Properties["member"].Add(childDistName);
            if (!isDryRun)
                groupDe.CommitChanges();
        }

        public static void RemoveFromGroup(String groupIdentity, String identity, String objectType, bool isDryRun = false)
        {
            if (String.IsNullOrWhiteSpace(identity))
            {
                throw new AdException("Child group name is not provided.", AdStatusType.MissingInput);
            }

            if (String.IsNullOrWhiteSpace(groupIdentity))
            {
                throw new AdException("Parent group name is not provided.", AdStatusType.MissingInput);
            }

            DirectoryEntry groupDe = GetDirectoryEntry(groupIdentity, "group");
            if (groupDe == null)
            {
                throw new AdException($"Parent group [{groupIdentity}] cannot be found.", AdStatusType.DoesNotExist);
            }

            DirectoryEntry childDe = GetDirectoryEntry(identity, objectType);
            if (childDe == null)
            {
                throw new AdException($"{objectType} [{groupIdentity}] cannot be found.", AdStatusType.DoesNotExist);
            }

            String childDistName = childDe.Properties["distinguishedName"].Value.ToString();

            if (!groupDe.Properties["member"].Contains(childDistName))
                throw new AdException($"{objectType} [{identity}] does not exist in the group [{groupIdentity}].", AdStatusType.DoesNotExist);

            groupDe.Properties["member"].Remove(childDistName);
            if (!isDryRun)
                groupDe.CommitChanges();
        }

        public static GroupScope? GetGroupScope(DirectoryEntry de)
        {
            int GLOBAL = 2;
            int LOCAL = 4;
            int UNIVERSAL = 8;             

            GroupScope? scope = null;
            if (de.Properties["groupType"] != null)
            {
                int groupType = (int)de.Properties["groupType"].Value;

                if ((groupType & GLOBAL) == GLOBAL)
                    scope = GroupScope.Global;
                else if ((groupType & LOCAL) == LOCAL)
                    scope = GroupScope.Local;
                else if ((groupType & UNIVERSAL) == UNIVERSAL)
                    scope = GroupScope.Universal;
            }

            return scope;
        }

    }
}