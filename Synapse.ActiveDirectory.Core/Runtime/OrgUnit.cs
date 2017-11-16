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
        public static void CreateOrganizationUnit(string distinguishedName, string description, Dictionary<String, List<String>> properties, bool isDryRun = false )
        {
            CreateDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), distinguishedName, properties );
        }

        public static void ModifyOrganizationUnit(string identity, string description, Dictionary<String, List<String>> properties, bool isDryRun = false)
        {
            AddProperty( properties, "description", description );
            ModifyDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), identity, properties );
        }

        public static void DeleteOrganizationUnit(string identity, bool isDryRun = false)
        {
            DeleteDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), identity );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string identity, bool getAccessRules, bool getObjectProperties)
        {
            DirectoryEntry orgUnit = GetDirectoryEntry( identity, AdObjectType.OrganizationalUnit.ToString() );
            return new OrganizationalUnitObject( orgUnit, getAccessRules, getObjectProperties );
        }

    }
}