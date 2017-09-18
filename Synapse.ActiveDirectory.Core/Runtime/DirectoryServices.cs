using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
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

        public static SerializableDictionary<string, List<string>> GetProperties(DirectoryEntry de)
        {
            SerializableDictionary<string, List<string>> properties = null;
            if ( de.Properties != null )
            {
                properties = new SerializableDictionary<string, List<string>>();
                IDictionaryEnumerator ide = de.Properties.GetEnumerator();
                while ( ide.MoveNext() )
                {
                    List<string> propValues = GetPropertyValues( ide.Key.ToString(), ide.Value );
                    properties.Add( ide.Key.ToString(), propValues );
                }
            }

            return properties;
        }

        public static List<string> GetPropertyValues(string name, object values)
        {
            List<string> propValues = new List<string>();

            PropertyValueCollection pvc = (PropertyValueCollection)values;
            IEnumerator pvcValues = pvc.GetEnumerator();
            while ( pvcValues.MoveNext() )
            {
                Type type = pvcValues.Current.GetType();
                if ( type.FullName == @"System.Byte[]" )
                {
                    byte[] bytes = (byte[])pvcValues.Current;
                    if ( name == "objectGUID" )
                    {
                        Guid guid = new Guid( bytes );
                        propValues.Add( guid.ToString() );
                    }
                    else if ( name == "objectSid" )
                    {
                        String sid = ConvertByteToStringSid( bytes );
                        propValues.Add( sid );
                    }
                    else
                    {
                        string str = System.Text.Encoding.UTF8.GetString( bytes );
                        propValues.Add( str );
                    }
                }
                else if ( type.FullName == @"System.__ComObject" )
                {
                    // TODO : Do something with ComObjects.  For now, just ignore
                    continue;
                }
                else
                {
                    propValues.Add( pvcValues.Current.ToString() );
                }
            }

            return propValues;
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
    }
}