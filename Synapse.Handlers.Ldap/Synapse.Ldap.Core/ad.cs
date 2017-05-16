using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class ldap
    {
        public static string GetObjectDistinguishedName(ObjectClass objectClass, string objectName, string ldapRoot)
        {
            string distinguishedName = string.Empty;

            using( DirectoryEntry entry = new DirectoryEntry( ldapRoot ) )
            using( DirectorySearcher searcher = new DirectorySearcher( entry ) )
            {
                switch( objectClass )
                {
                    case ObjectClass.User:
                    {
                        searcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                        break;
                    }
                    case ObjectClass.Group:
                    case ObjectClass.Computer:
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
    }
}