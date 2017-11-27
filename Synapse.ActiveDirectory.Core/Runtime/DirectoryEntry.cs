using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Synapse.ActiveDirectory.Core
{
    public partial class DirectoryServices
    {
        public static DirectoryEntry CreateDirectoryEntry(string schemaClassName, string distinguishedName, Dictionary<String, List<String>> properties = null, bool saveOnCreate = true)
        {
            DirectoryEntry entry = null;

            Regex regex = new Regex( @"(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                string name = match.Groups[1]?.Value?.Trim();
                string parentPath = match.Groups[2]?.Value?.Trim();
                entry = CreateDirectoryEntry( schemaClassName, name, parentPath, properties, saveOnCreate );
            }
            else
                throw new AdException( $"Unable To Locate {schemaClassName} Name In Distinguished Name [{distinguishedName}]." );

            return entry;
        }

        private static DirectoryEntry CreateDirectoryEntry(string schemaClassName, string name, string parentPath, Dictionary<String, List<String>> properties = null, bool saveOnCreate = true)
        {
            DirectoryEntry child = null;

            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new AdException( "New {schemaClassName} name is not specified.", AdStatusType.MissingInput );
            }

            parentPath = string.IsNullOrWhiteSpace( parentPath ) ? GetDomainDistinguishedName() : parentPath.Replace( "LDAP://", "" );
            string childPath = $"{name},{parentPath}";

            DirectoryEntry parent = GetDirectoryEntry( parentPath );

            if ( parent != null )
            {
                if ( !IsExistingDirectoryEntry( childPath ) )
                {
                    child = parent.Children.Add( name, schemaClassName );
                    SetProperties( child, properties );
                    if (saveOnCreate)
                        child.CommitChanges();
                }
                else
                    throw new AdException( $"New {schemaClassName} already exists.", AdStatusType.AlreadyExists );

            }
            else
            {
                throw new AdException( $"Parent {schemaClassName} does not exist.", AdStatusType.DoesNotExist );
            }

            return child;
        }

        public static DirectoryEntry ModifyDirectoryEntry(string schemaClassName, string identity, Dictionary<String, List<String>> properties = null, bool saveOnModify = true)
        {
            DirectoryEntry entry = GetDirectoryEntry( identity, schemaClassName );
            return ModifyDirectoryEntry( entry, properties, saveOnModify );
        }

        public static DirectoryEntry ModifyDirectoryEntry(DirectoryEntry entry, Dictionary<String, List<String>> properties = null, bool saveOnModify = true)
        {
            SetProperties( entry, properties );
            if (saveOnModify)
                entry.CommitChanges();
            return entry;
        }

        public static void DeleteDirectoryEntry(string schemaClassName, string identity)
        {
            DirectoryEntry entry = GetDirectoryEntry( identity, schemaClassName );
            if ( entry == null )
                throw new AdException( $"{schemaClassName} [{identity}] cannot be found", AdStatusType.DoesNotExist );
            else
                DeleteDirectoryEntry( entry );
        }

        public static void DeleteDirectoryEntry(DirectoryEntry entry, bool isDryRun = false)
        {
            if ( entry != null )
            {
                if ( !isDryRun )
                {
                    entry.DeleteTree();
                    entry.CommitChanges();
                }
            }
        }

        public static DirectoryEntry Move(string identity, string destination)
        {
            DirectoryEntry fromDE = GetDirectoryEntry( identity );
            DirectoryEntry toDE = GetDirectoryEntry( destination );

            fromDE.MoveTo( toDE );

            return GetDirectoryEntry( identity );
        }

        public static bool IsExistingDirectoryEntry(string identity)
        {
            return GetDirectoryEntry( identity ) != null;
        }

        public static DirectoryEntry GetDirectoryEntry(string identity, string objectClass = null)
        {
            string searchString = null;

            if ( String.IsNullOrWhiteSpace( identity ) )
                return null;

            identity = identity.Replace( "LDAP://", "" );

            if ( IsDistinguishedName( identity ) )
                searchString = $"(distinguishedName={identity})";
            else if ( IsGuid( identity ) )
                searchString = $"(objectGuid={GetGuidSearchBytes( identity )})";
            else if ( IsSid( identity ) )
                searchString = $"(objectSid={identity}";
            else
                searchString = $"(|(name={identity})(userPrincipalName={identity})(sAMAccountName={identity}))";

            if ( objectClass != null )
                searchString = $"(&(objectClass={objectClass}){searchString})";

            List<DirectoryEntry> results = GetDirectoryEntries( searchString );

            if ( results.Count > 1 )
                throw new AdException( $"Multiple Objects Found With Identity [{identity}].", AdStatusType.MultipleMatches );

            if ( results.Count > 0 )
                return results[0];
            else
                return null;

        }

        public static List<DirectoryEntry> GetDirectoryEntries(string filter, string searchBase = null)
        {
            List<DirectoryEntry> entries = new List<DirectoryEntry>();
            if ( String.IsNullOrWhiteSpace( searchBase ) )
                searchBase = GetDomainDistinguishedName();
            if ( !searchBase.StartsWith( "LDAP://" ) )
                searchBase = "LDAP://" + searchBase;

            using ( DirectoryEntry root = new DirectoryEntry( searchBase ) )
            using ( DirectorySearcher searcher = new DirectorySearcher( root ) )
            {
                searcher.Filter = filter;
                searcher.SearchScope = SearchScope.Subtree;
                searcher.ReferralChasing = ReferralChasingOption.All;

                SearchResultCollection results = searcher.FindAll();
                foreach ( SearchResult result in results )
                    entries.Add( result.GetDirectoryEntry() );
            }

            return entries;
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
                        if ( de.Properties[name].Value != null && replaceExisting )
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
                if ( valueStr != null )
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

        public static void AddProperty(Dictionary<String, List<String>> properties, string name, string value, bool overwriteExisting = false)
        {
            if ( !String.IsNullOrWhiteSpace( name ) && value != null )
            {
                if ( properties == null )
                    properties = new Dictionary<string, List<string>>();

                List<String> values = new List<string>();
                values.Add( value );
                AddProperty( properties, name, values, overwriteExisting );
            }
        }

        public static void AddProperty(Dictionary<String, List<String>> properties, string name, List<String> values, bool overwriteExisting = false)
        {
            if ( !String.IsNullOrWhiteSpace( name ) && values != null )
            {
                if ( properties == null )
                    properties = new Dictionary<string, List<string>>();

                bool exists = properties.ContainsKey( name );
                if ( exists && overwriteExisting )
                    properties[name] = values;
                else if ( !exists )
                    properties.Add( name, values );
            }
        }
    }
}
