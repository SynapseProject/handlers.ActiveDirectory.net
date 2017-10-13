using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Security.AccessControl;

namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
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
            if ( String.IsNullOrWhiteSpace( domainName ) )
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

        public static Principal GetPrincipal(string identity, string domainName = null)
        {
            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            PrincipalContext principalContext = GetPrincipalContext( "", domainName );

            Principal principal = Principal.FindByIdentity( principalContext, identity );
            return principal;
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

        public static void SetProperty(DirectoryEntry de, String name, String value, bool commitChanges = false, bool replaceExisting = true)
        {
            List<String> values = new List<string>();
            values.Add( value );
            SetProperty( de, name, values, commitChanges, replaceExisting );
        }

        public static void SetProperty(DirectoryEntry de, String name, List<String> values, bool commitChanges = false, bool replaceExisting = true)
        {
            try
            {
                if ( values != null )
                {
                    List<String> addValues = new List<string>();
                    bool clearValue = false;

                    // Ensure at least one value in the list is non-null
                    foreach ( String v in values )
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
                        if ( de.Properties[name].Value != null && replaceExisting)
                            de.Properties[name].Clear();

                        foreach ( String value in addValues )
                            if ( value != null )
                                de.Properties[name].Add( value );
                    }
                }


                if ( commitChanges )
                    de.CommitChanges();
            }
            catch ( Exception e )
            {
                throw new AdException( $"Property [{name}] Failed To Update With Error [{e.Message?.Trim()}].", e, AdStatusType.InvalidAttribute );
            }
        }

        public static void AddProperty(DirectoryEntry de, String name, String value, bool commitChanges = false)
        {
            SetProperty( de, name, value, commitChanges, false );
        }

        public static void AddProperties(DirectoryEntry de, String name, List<String> values, bool commitChanges = false)
        {
            SetProperty( de, name, values, commitChanges, false );
        }

        public static void DeleteProperty(DirectoryEntry de, String name, String value, bool commitChanges = false, bool replaceExisting = true)
        {
            List<String> values = new List<string>();
            values.Add( value );
            DeleteProperty( de, name, values, commitChanges, replaceExisting );
        }

        public static void DeleteProperty(DirectoryEntry de, String name, List<String> values, bool commitChanges = false, bool replaceExisting = true)
        {
            if ( values != null )
            {
                foreach ( String value in values )
                    de.Properties[name].Remove( value );
            }

            if ( commitChanges )
                de.CommitChanges();
        }

        public static void ClearProperty(DirectoryEntry de, String name, bool commitChanges = false)
        {
            de.Properties[name].Clear();
            if ( commitChanges )
                de.CommitChanges();
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
                String valueStr = GetPropertyValueString( pvcValues.Current );
                if (valueStr != null)
                    propValues.Add( valueStr );
            }

            return propValues;
        }

        private static string GetPropertyValueString(object value)
        {
            String propString = null;

            Type type = value.GetType();
            if ( type.FullName == @"System.Byte[]" )
            {
                byte[] bytes = (byte[])value;
                // Try To Convert To Guid
                if ( propString == null )
                    try
                    { propString = new Guid( bytes ).ToString(); }
                    catch { }

                // Try To Convert To Sid
                if ( propString == null )
                    try
                    { propString = ConvertByteToStringSid( bytes ); }
                    catch { }

                // Default To Byte Array String
                if ( propString == null )
                    try
                    { propString = System.Text.Encoding.UTF8.GetString( bytes ); }
                    catch { }                
            }
            else if ( type.FullName == @"System.__ComObject" )
            {
                // TODO : Do something with ComObjects.  For now, just ignore
            }
            else
                propString = value.ToString();

            return propString;
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
            Principal principal = DirectoryServices.GetPrincipal( identity );
            return principal.DistinguishedName;
        }

        public static List<AccessRuleObject> GetAccessRules(Principal principal)
        {
            if ( principal.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                return GetAccessRules( (DirectoryEntry)principal.GetUnderlyingObject() );
            else
                throw new AdException( $"GetAccessRules Not Available For Object Type [{principal.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
        }

        public static List<AccessRuleObject> GetAccessRules(DirectoryEntry de)
        {
            List<AccessRuleObject> accessRules = new List<AccessRuleObject>();
            Dictionary<string, Principal> principals = new Dictionary<string, Principal>();

            AuthorizationRuleCollection rules = de.ObjectSecurity?.GetAccessRules( true, true, typeof( System.Security.Principal.SecurityIdentifier ) );
            if ( rules != null )
            {
                foreach ( AuthorizationRule rule in rules )
                {
                    ActiveDirectoryAccessRule accessRule = (ActiveDirectoryAccessRule)rule;
                    AccessRuleObject aro = new AccessRuleObject()
                    {
                        ControlType = accessRule.AccessControlType,
                        Rights = accessRule.ActiveDirectoryRights,
                        IdentityReference = accessRule.IdentityReference.Value,
                        InheritanceFlags = accessRule.InheritanceFlags,
                        IsInherited = accessRule.IsInherited,
                    };

                    Principal principal = null;
                    if ( principals.ContainsKey( aro.IdentityReference ) )
                        principal = principals[aro.IdentityReference];
                    else
                    {
                        principal = DirectoryServices.GetPrincipal( aro.IdentityReference );
                        principals.Add( aro.IdentityReference, principal );
                    }

                    aro.IdentityName = principal.Name;
                    accessRules.Add( aro );

                }
            }

            return accessRules;
        }

        public static List<DirectoryEntryObject> Search(string filter, bool getAccessRules = false, bool getObjectProperties = true)
        {
            List<DirectoryEntryObject> searchResults = new List<DirectoryEntryObject>();

            SearchResultCollection results = DoSearch( filter, null );
            foreach ( SearchResult result in results )
            {
                DirectoryEntry de = result.GetDirectoryEntry();
                DirectoryEntryObject deo = new DirectoryEntryObject( de, false, false, true );
                searchResults.Add( deo );
            }

            return searchResults;
        }

        public static SearchResults Search(string filter, string[] returnProperties)
        {
            SearchResults searchResults = new SearchResults();

            string rootName = GetDomainDistinguishedName();
            if ( !rootName.StartsWith( "LDAP://" ) )
                rootName = "LDAP://" + rootName;

            SearchResultCollection results = DoSearch( filter, returnProperties );
            searchResults.Results = new List<SearchResultRow>();

            foreach ( SearchResult result in results )
            {
                SearchResultRow row = new SearchResultRow();
                row.Path = result.Path;

                if ( returnProperties != null )
                {
                    row.Properties = new SerializableDictionary<string, List<string>>();
                    foreach ( string key in returnProperties )
                    {
                        List<string> values = new List<string>();
                        if ( result.Properties.Contains( key ) )
                        {
                            foreach ( object value in result.Properties[key] )
                            {
                                string valueStr = GetPropertyValueString( value );
                                values.Add( valueStr );
                            }
                            row.Properties.Add( key, values );
                        }
                        else
                            row.Properties.Add( key, null );
                    }
                }

                searchResults.Results.Add( row );
            }

            return searchResults;
        }

        private static SearchResultCollection DoSearch(string filter, string[] returnProperties = null)
        {
            string rootName = GetDomainDistinguishedName();
            if ( !rootName.StartsWith( "LDAP://" ) )
                rootName = "LDAP://" + rootName;

            using ( DirectoryEntry root = new DirectoryEntry( rootName ) )
            using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = filter;
                searcher.SearchScope = SearchScope.Subtree;
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
    }
}