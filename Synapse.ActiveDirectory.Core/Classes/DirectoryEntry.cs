using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Xml.Serialization;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public class DirectoryEntryObject
    {
        private string VALID_PARENT_CLASS_NAME = @"organizationalUnit";

        public DirectoryEntryObject() { }
        public DirectoryEntryObject(DirectoryEntry de, bool loadSchema = true)
        {
            SetPropertiesFromDirectoryEntry( de, loadSchema );
        }

        //
        // Summary:
        //     Gets the GUID of the System.DirectoryServices.DirectoryEntry.
        //
        // Returns:
        //     A System.Guid structure that represents the GUID of the System.DirectoryServices.DirectoryEntry.
        public Guid Guid { get; set; }
        //
        // Summary:
        //     Gets the name of the object as named with the underlying directory service.
        //
        // Returns:
        //     The name of the object as named with the underlying directory service.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets the GUID of the System.DirectoryServices.DirectoryEntry, as returned from
        //     the provider.
        //
        // Returns:
        //     A System.Guid structure that represents the GUID of the System.DirectoryServices.DirectoryEntry,
        //     as returned from the provider.
        public string NativeGuid { get; set; }
        ////
        //// Summary:
        ////     Gets the native Active Directory Service Interfaces (ADSI) object.
        ////
        //// Returns:
        ////     The native ADSI object.
        //public object NativeObject { get; set; }
        ////
        //// Summary:
        ////     Gets or sets the security descriptor for this entry.
        ////
        //// Returns:
        ////     An System.DirectoryServices.ActiveDirectorySecurity object that represents the
        ////     security descriptor for this directory entry.
        //public ActiveDirectorySecurity ObjectSecurity { get; set; }
        ////
        //// Summary:
        ////     Gets the provider-specific options for this entry.
        ////
        //// Returns:
        ////     A System.DirectoryServices.DirectoryEntryConfiguration object that contains the
        ////     provider-specific options for this entry.
        //public DirectoryEntryConfiguration Options { get; set; }
        //
        // Summary:
        //     Gets this entry's parent in the Active Directory Domain Services hierarchy.
        //
        // Returns:
        //     A System.DirectoryServices.DirectoryEntry object that represents the parent of
        //     this entry.
        public DirectoryEntryObject Parent { get; set; }
        ////
        //// Summary:
        ////     Sets the password to use when authenticating the client.
        ////
        //// Returns:
        ////     The password to use when authenticating the client.
        //public string Password { get; set; }
        //
        // Summary:
        //     Gets or sets the path for this System.DirectoryServices.DirectoryEntry.
        //
        // Returns:
        //     The path of this System.DirectoryServices.DirectoryEntry object. The default
        //     is an empty string ("").
        public string Path { get; set; }
        //
        // Summary:
        //     Gets the Active Directory Domain Services properties for this System.DirectoryServices.DirectoryEntry
        //     object.
        //
        // Returns:
        //     A System.DirectoryServices.PropertyCollection object that contains the properties
        //     that are set on this entry.

        //TODO : Create A Serializable Version Of This
        [XmlArrayItem(ElementName = "Property")]
        public Dictionary<string, List<string>> Properties { get; set; }
        //
        // Summary:
        //     Gets the name of the schema class for this System.DirectoryServices.DirectoryEntry
        //     object.
        //
        // Returns:
        //     The name of the schema class for this System.DirectoryServices.DirectoryEntry
        //     object.
        public string SchemaClassName { get; set; }
        //
        // Summary:
        //     Gets the schema object for this entry.
        //
        // Returns:
        //     A System.DirectoryServices.DirectoryEntry object that represents the schema class
        //     for this entry.
        public DirectoryEntryObject SchemaEntry { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the cache should be committed after each
        //     operation.
        //
        // Returns:
        //     true if the cache should not be committed after each operation; otherwise, false.
        //     The default is true.
        public bool UsePropertyCache { get; set; }
        //
        // Summary:
        //     Gets or sets the user name to use when authenticating the client.
        //
        // Returns:
        //     The user name to use when authenticating the client.
        public string Username { get; set; }


        public static DirectoryEntryObject FromDirectoryEntry(DirectoryEntry de)
        {
            return new DirectoryEntryObject( de );
        }

        public void SetPropertiesFromDirectoryEntry(DirectoryEntry de, bool loadSchema = true)
        {
            if( de == null ) return;

            Guid = de.Guid;
            Name = de.Name;
            NativeGuid = de.NativeGuid;
            if ( de.Parent.SchemaClassName == VALID_PARENT_CLASS_NAME )
            {
                Parent = new DirectoryEntryObject( de.Parent, false );
            }

            if (de.SchemaClassName == VALID_PARENT_CLASS_NAME)
            {
                if ( de.Properties != null )
                {
                    Properties = new Dictionary<string, List<string>>();
                    IDictionaryEnumerator ide = de.Properties.GetEnumerator();
                    while ( ide.MoveNext() )
                    {
                        List<string> propValues = GetPropertyValues( ide.Value );
                        Properties.Add( ide.Key.ToString(), propValues );
                    }
                }
            }
            Path = de.Path;
            SchemaClassName = de.SchemaClassName;
            if (loadSchema)
                SchemaEntry = new DirectoryEntryObject( de.SchemaEntry, false );
            UsePropertyCache = de.UsePropertyCache;
            Username = de.Username;
        }

        private List<string> GetPropertyValues(object values)
        {
            List<string> propValues = new List<string>();

            PropertyValueCollection pvc = (PropertyValueCollection)values;
            IEnumerator pvcValues = pvc.GetEnumerator();
            while (pvcValues.MoveNext())
            {
                Type type = pvcValues.Current.GetType();
                if ( type.FullName == @"System.Byte[]" )
                {
                    byte[] bytes = (byte[])pvcValues.Current;
                    if ( bytes.Length == 16 )
                    {
                        Guid guid = new Guid( bytes );
                        propValues.Add( guid.ToString() );
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

    }
}