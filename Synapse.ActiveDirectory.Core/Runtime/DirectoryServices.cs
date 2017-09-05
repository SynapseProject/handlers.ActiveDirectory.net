using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;


namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
/*        public static string GetObjectDistinguishedName(AdObjectType objectClass, string objectName, string ldapRoot)
        {
            string distinguishedName = string.Empty;

            using( DirectoryEntry entry = new DirectoryEntry( ldapRoot ) )
            using( DirectorySearcher searcher = new DirectorySearcher( entry ) )
            {
                switch( objectClass )
                {
                    case AdObjectType.User:
                    {
                        searcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                        break;
                    }
                    case AdObjectType.Group:
                    case AdObjectType.Computer:
                    {
                        searcher.Filter = $"(&(objectClass={objectClass.ToString().ToLower()})(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                        break;
                    }
                }
                SearchResult result = searcher.FindOne();

                if( result == null )
                    throw new KeyNotFoundException( "unable to locate the distinguishedName for the object " + objectName + " in the " + ldapRoot + " ldapRoot" );

                DirectoryEntry directoryObject = result.GetDirectoryEntry();
                distinguishedName = "LDAP://" + directoryObject.Properties["distinguishedName"].Value;

                entry.Close();
            }

            return distinguishedName;
        }
*/
        private static string GetCommonName(String distinguishedName)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
                return match.Groups[1]?.Value?.Trim();
            else
                return distinguishedName;
        }

        public static PrincipalContext GetPrincipalContext(string ouPath = "", string domainName = null)
        {
            if (String.IsNullOrWhiteSpace(domainName))
            {
                // If null, principal context defaults to a domain controller for the domain of the user principal
                // under which the thread is running.
                domainName = null;
            }

            PrincipalContext principalContext = !String.IsNullOrWhiteSpace( ouPath ) ? new PrincipalContext( ContextType.Domain, domainName, ouPath ) : new PrincipalContext( ContextType.Domain, domainName );
            return principalContext;
        }

        public static UserPrincipal GetUserPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity( principalContext, identity );
            return userPrincipal;
        }

        public static GroupPrincipal GetGroupPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity( principalContext, identity );
            return groupPrincipal;
        }

        public static DirectoryEntry GetDirectoryEntry(string identity, string objectClass = "organizationalUnit")
        {
            string rootName = GetDomainDistinguishedName();
            if ( !rootName.StartsWith( "LDAP://" ) )
                rootName = "LDAP://" + rootName;
            string searchString = null;

            if ( IsDistinguishedName( identity ) )
                searchString = $"(distinguishedName={identity})";
            else if ( IsGuid( identity ) )
                searchString = $"(objectGuid={GetGuidSearchBytes( identity )})";
            else
                searchString = $"(name={identity})";


            DirectoryEntry de = null;
            using ( DirectoryEntry root = new DirectoryEntry( rootName ) )
            using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = $"(&(objectClass={objectClass}){searchString})";
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add( "name" );
                searcher.PropertiesToLoad.Add( "distinguishedname" );
                searcher.PropertiesToLoad.Add( "objectGuid" );
                searcher.ReferralChasing = ReferralChasingOption.All;

                SearchResultCollection results = searcher.FindAll();
                if ( results.Count > 1 )
                    throw new AdException( $"Multiple Objects Found With Identity [{identity}].", AdStatusType.MultipleMatches );
                else if ( results.Count == 1 )
                    de = results[0].GetDirectoryEntry();

            }

            return de;
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
            if ( identity.StartsWith( "LDAP://" ) )
                identity = identity.Replace( "LDAP://", "" );
            return Regex.IsMatch( identity, @"^\s*?(cn\s*=|ou\s*=|dc\s*=)", RegexOptions.IgnoreCase );
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

        public static string GetGuidSearchBytes(string identity)
        {
            Guid guid = Guid.Parse( identity );
            byte[] bytes = guid.ToByteArray();
            String str = BitConverter.ToString( bytes );
            str = str.Replace( '-', '\\' );

            return @"\" + str;
        }

        public static void SetProperties(DirectoryEntry de, Dictionary<String, List<String>> properties, bool commitChanges = false)
        {
            if ( properties != null )
            {
                foreach ( KeyValuePair<string, List<string>> property in properties )
                    SetProperty( de, property.Key, property.Value );
            }
        }

        public static void SetProperty(DirectoryEntry de, String name, String value, bool commitChanges = false)
        {
            List<String> values = new List<string>();
            values.Add( value );
            SetProperty( de, name, values, commitChanges );
        }

        public static void SetProperty(DirectoryEntry de, String name, List<String> values, bool commitChanges = false)
        {
            try
            {
                if (values != null)
                {
                    List<String> addValues = new List<string>();
                    bool clearValue = false;

                    // Ensure at least one value in the list is non-null
                    foreach ( String v in values)
                    {
                        if ( v != null )
                        {
                            // If ANY value in the list is "~null~", then clear the existing value only.
                            if ( v.Equals( "~null~", StringComparison.OrdinalIgnoreCase ) )
                            {
                                clearValue = true;
                                break;
                            }
                            else
                                addValues.Add( v );
                        }
                    }


                    // Clear Out Existing Value
                    if ( clearValue )
                    {
                        if ( de.Properties[name].Value != null )
                            de.Properties[name].Clear();
                    }
                    else if ( addValues.Count > 0 )
                    {
                        // Replace Existing Value(s) 
                        if ( de.Properties[name].Value != null )
                            de.Properties[name].Clear();

                        foreach ( String value in addValues )
                            if ( value != null )
                                de.Properties[name].Add( value );
                    }
                }


                if (commitChanges)
                    de.CommitChanges();
            }
            catch ( Exception e )
            {
                throw new AdException( $"Property [{name}] Failed To Update With Error [{e.Message?.Trim()}].", e, AdStatusType.InvalidAttribute );
            }
        }

    }
}