using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class OrganizationalUnitObject : DirectoryEntryObject
    {
        public OrganizationalUnitObject() { }
        public OrganizationalUnitObject(DirectoryEntry ou)
        {
            SetPropertiesFromOrganizationalUnit( ou );
        }

        public string DistinguishedName { get; set; }

        public static OrganizationalUnitObject FromOrganizationalUnit(DirectoryEntry ou)
        {
            return new OrganizationalUnitObject( ou );
        }

        public void SetPropertiesFromOrganizationalUnit(DirectoryEntry ou)
        {
            if( ou == null ) return;

            SetPropertiesFromDirectoryEntry( ou );

            DistinguishedName = ou.Properties["distinguishedName"].Value.ToString();
        }
    }
}