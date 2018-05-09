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
        public static void CreateOrganizationUnit(string distinguishedName, Dictionary<String, List<String>> properties, bool isDryRun = false )
        {
            CreateDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), distinguishedName, properties );
        }

        public static void ModifyOrganizationUnit(string identity, Dictionary<String, List<String>> properties, bool isDryRun = false)
        {
            ModifyDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), identity, properties );
        }

        public static void DeleteOrganizationUnit(string identity, bool isDryRun = false)
        {
            DeleteDirectoryEntry( AdObjectType.OrganizationalUnit.ToString(), identity );
        }

        public static OrganizationalUnitObject GetOrganizationalUnit(string identity, bool getAccessRules, bool getObjectProperties, bool loadSchema)
        {
            OrganizationalUnitObject ouo = null;

            DirectoryEntry orgUnit = GetDirectoryEntry( identity, AdObjectType.OrganizationalUnit.ToString() );
            if (orgUnit != null)
                ouo = new OrganizationalUnitObject( orgUnit, getAccessRules, getObjectProperties, loadSchema );

            return ouo;
        }

    }
}