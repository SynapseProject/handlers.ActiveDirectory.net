using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public class OrganizationalUnitObject : DirectoryEntryObject, ICloneable
    {
        public OrganizationalUnitObject() { }
        public OrganizationalUnitObject(DirectoryEntry ou, bool getAccessRules = false)
        {
            SetPropertiesFromOrganizationalUnit( ou, getAccessRules );
        }

        public string DistinguishedName { get; set; }

        public static OrganizationalUnitObject FromOrganizationalUnit(DirectoryEntry ou)
        {
            return new OrganizationalUnitObject( ou );
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void SetPropertiesFromOrganizationalUnit(DirectoryEntry ou, bool getAccessRules)
        {
            if( ou == null ) return;

            SetPropertiesFromDirectoryEntry( ou, true, getAccessRules );

            DistinguishedName = ou.Properties["distinguishedName"].Value.ToString();
        }
    }
}