using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class DirectoryEntryObject
    {
        public DirectoryEntryObject() { }
        public DirectoryEntryObject(DirectoryEntry de)
        {
            SetPropertiesFromDirectoryEntry( de );
        }

        //
        // Summary:
        //     Gets the GUID of the System.DirectoryServices.DirectoryEntry.
        //
        // Returns:
        //     A System.Guid structure that represents the GUID of the System.DirectoryServices.DirectoryEntry.
        public Guid Guid { get; internal set; }
        //
        // Summary:
        //     Gets the name of the object as named with the underlying directory service.
        //
        // Returns:
        //     The name of the object as named with the underlying directory service.
        public string Name { get; internal set; }
        //
        // Summary:
        //     Gets the GUID of the System.DirectoryServices.DirectoryEntry, as returned from
        //     the provider.
        //
        // Returns:
        //     A System.Guid structure that represents the GUID of the System.DirectoryServices.DirectoryEntry,
        //     as returned from the provider.
        public string NativeGuid { get; internal set; }
        ////
        //// Summary:
        ////     Gets the native Active Directory Service Interfaces (ADSI) object.
        ////
        //// Returns:
        ////     The native ADSI object.
        //public object NativeObject { get; internal set; }
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
        //public DirectoryEntryConfiguration Options { get; internal set; }
        //
        // Summary:
        //     Gets this entry's parent in the Active Directory Domain Services hierarchy.
        //
        // Returns:
        //     A System.DirectoryServices.DirectoryEntry object that represents the parent of
        //     this entry.
        public DirectoryEntryObject Parent { get; internal set; }
        ////
        //// Summary:
        ////     Sets the password to use when authenticating the client.
        ////
        //// Returns:
        ////     The password to use when authenticating the client.
        //public string Password { internal get; set; }
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
        public PropertyCollection Properties { get; internal set; }
        //
        // Summary:
        //     Gets the name of the schema class for this System.DirectoryServices.DirectoryEntry
        //     object.
        //
        // Returns:
        //     The name of the schema class for this System.DirectoryServices.DirectoryEntry
        //     object.
        public string SchemaClassName { get; internal set; }
        //
        // Summary:
        //     Gets the schema object for this entry.
        //
        // Returns:
        //     A System.DirectoryServices.DirectoryEntry object that represents the schema class
        //     for this entry.
        public DirectoryEntryObject SchemaEntry { get; internal set; }
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

        public void SetPropertiesFromDirectoryEntry(DirectoryEntry de)
        {
            if( de == null ) return;

            Guid = de.Guid;
            Name = de.Name;
            NativeGuid = de.NativeGuid;
            Parent = new DirectoryEntryObject( de.Parent );
            Path = de.Path;
            Properties = de.Properties;
            SchemaClassName = de.SchemaClassName;
            SchemaEntry = new DirectoryEntryObject( de.SchemaEntry );
            UsePropertyCache = de.UsePropertyCache;
            Username = de.Username;
        }
    }
}