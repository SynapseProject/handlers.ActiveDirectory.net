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
            Principal principal = DirectoryServices.GetPrincipal( identity );
            return principal?.DistinguishedName;
        }

        public static SearchResults Search(string searchBase, string filter, string[] returnProperties)
        {
            SearchResults searchResults = new SearchResults();

            try
            {
                SearchResultCollection results = DoSearch( filter, returnProperties, searchBase );
                searchResults.Results = new List<SearchResultRow>();

                foreach ( SearchResult result in results )
                {
                    SearchResultRow row = new SearchResultRow()
                    {
                        Path = result.Path
                    };

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
    }
}