using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using System.DirectoryServices.AccountManagement;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static string GetObjectDistinguishedName(LdapObjectType objectClass, string objectName, string ldapRoot)
        {
            string distinguishedName = string.Empty;

            using( DirectoryEntry entry = new DirectoryEntry( ldapRoot ) )
            using( DirectorySearcher searcher = new DirectorySearcher( entry ) )
            {
                switch( objectClass )
                {
                    case LdapObjectType.User:
                    {
                        searcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                        break;
                    }
                    case LdapObjectType.Group:
                    case LdapObjectType.Computer:
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

        private static string GetCommonName(String distinguishedName)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
                return match.Groups[1]?.Value?.Trim();
            else
                return distinguishedName;
        }

        public static PrincipalContext GetPrincipalContext(string ouPath = "")
        {
            PrincipalContext principalContext = !String.IsNullOrWhiteSpace( ouPath ) ? new PrincipalContext( ContextType.Domain, null, ouPath ) : new PrincipalContext( ContextType.Domain );
            return principalContext;
        }

        public static DirectoryEntry GetDirectoryEntry(string distinguishedName)
        {
            string rootName = distinguishedName;
            if ( distinguishedName.StartsWith( "LDAP://" ) )
                distinguishedName = distinguishedName.Replace( "LDAP://", "" );
            else
                rootName = $"LDAP://{rootName}";

            DirectoryEntry de = null;
            if ( DirectoryEntry.Exists( rootName ) )
            {
                using ( DirectoryEntry root = new DirectoryEntry( rootName ) )
                using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
                {
                    searcher.Filter = $"(&(objectClass=organizationalUnit))"; //(name={name})
                    searcher.SearchScope = SearchScope.Base;
                    searcher.PropertiesToLoad.Add( "name" );
                    searcher.PropertiesToLoad.Add( "distinguishedname" );
                    searcher.ReferralChasing = ReferralChasingOption.All;

                    SearchResultCollection results = searcher.FindAll();
                    foreach ( SearchResult result in results )
                        if ( result.Properties["distinguishedname"][0].ToString().Equals( distinguishedName, StringComparison.OrdinalIgnoreCase ) )
                            de = result.GetDirectoryEntry();

                }
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
    }
}