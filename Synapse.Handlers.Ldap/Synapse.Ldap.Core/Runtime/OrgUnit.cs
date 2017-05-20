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
                searcher.Filter = $"(&(objectClass=organizationalUnit))"; //(name={name})
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add( "name" );
                searcher.PropertiesToLoad.Add( "distinguishedName" );
                searcher.ReferralChasing = ReferralChasingOption.All;

                DirectoryEntry ou = null;
                SearchResultCollection results = searcher.FindAll();
                foreach( SearchResult result in results )
                    if( result.Properties["name"][0].ToString().Equals( name, StringComparison.OrdinalIgnoreCase ) )
                        ou = result.GetDirectoryEntry();

                if( ou == null )
                    return null;
                else
                    return new OrganizationalUnitObject( ou );
            }
        }
    }
}