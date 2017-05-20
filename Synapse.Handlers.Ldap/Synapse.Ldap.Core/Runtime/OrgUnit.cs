using System;
using System.DirectoryServices;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static OrganizationalUnitObject GetOrganizationalUnit(string name, string ldapRoot)
        {
            using( DirectoryEntry root = new DirectoryEntry( ldapRoot ) )
            using( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = $"(&(objectClass=organizationalUnit))(Name={name})))";
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add( "distinguishedName" );

                SearchResult result = searcher.FindOne();

                if( result == null )
                    return null;
                else
                    return new OrganizationalUnitObject( result.GetDirectoryEntry() );
            }
        }
    }
}