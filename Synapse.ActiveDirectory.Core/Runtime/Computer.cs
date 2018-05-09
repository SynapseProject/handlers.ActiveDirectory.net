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
        public static void CreateComputer(string distinguishedName, Dictionary<String, List<String>> properties, bool isDryRun = false )
        {
            CreateDirectoryEntry( AdObjectType.Computer.ToString(), distinguishedName, properties );
        }

        public static void ModifyComputer(string identity, Dictionary<String, List<String>> properties, bool isDryRun = false)
        {
            ModifyDirectoryEntry( AdObjectType.Computer.ToString(), identity, properties );
        }

        public static void DeleteComputer(string identity, bool isDryRun = false)
        {
            DeleteDirectoryEntry( AdObjectType.Computer.ToString(), identity );
        }

        public static DirectoryEntryObject GetComputer(string identity, bool getAccessRules, bool getObjectProperties, bool loadSchema)
        {
            DirectoryEntryObject co = null;

            DirectoryEntry computer = GetDirectoryEntry( identity, AdObjectType.Computer.ToString() );
            if (computer != null)
                co = new DirectoryEntryObject( computer, loadSchema, getAccessRules, getObjectProperties );

            return co;
        }

    }
}