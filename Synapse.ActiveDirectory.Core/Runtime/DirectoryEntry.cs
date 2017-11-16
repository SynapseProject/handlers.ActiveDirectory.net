using System;
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
        public static DirectoryEntry CreateDirectoryEntry(string schemaClassName, string distinguishedName, Dictionary<String, List<String>> properties = null)
        {
            DirectoryEntry entry = null;

            Regex regex = new Regex( @"(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
            {
                string name = match.Groups[1]?.Value?.Trim();
                string parentPath = match.Groups[2]?.Value?.Trim();
                entry = CreateDirectoryEntry( schemaClassName, name, parentPath, properties );
            }
            else
                throw new AdException( $"Unable To Locate {schemaClassName} Name In Distinguished Name [{distinguishedName}]." );

            return entry;
        }

        private static DirectoryEntry CreateDirectoryEntry(string schemaClassName, string name, string parentPath, Dictionary<String, List<String>> properties = null)
        {
            DirectoryEntry child = null;

            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new AdException( "New {schemaClassName} name is not specified.", AdStatusType.MissingInput );
            }

            parentPath = string.IsNullOrWhiteSpace( parentPath ) ? GetDomainDistinguishedName() : parentPath.Replace( "LDAP://", "" );
            string childPath = $"{name},{parentPath}";

            DirectoryEntry parent = GetDirectoryEntry( parentPath, schemaClassName );

            if ( parent != null )
            {
                if ( !IsExistingDirectoryEntry( childPath ) )
                {
                    child = parent.Children.Add( name, schemaClassName );
                    SetProperties( child, properties );
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

        public static DirectoryEntry ModifyDirectoryEntry(string schemaClassName, string identity, Dictionary<String, List<String>> properties = null)
        {
            DirectoryEntry entry = GetDirectoryEntry( identity, schemaClassName );
            return ModifyDirectoryEntry( entry, properties );
        }

        public static DirectoryEntry ModifyDirectoryEntry(DirectoryEntry entry, Dictionary<String, List<String>> properties = null)
        {
            SetProperties( entry, properties );
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


    }
}
