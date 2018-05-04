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
        public OrganizationalUnitObject(DirectoryEntry ou, bool getAccessRules = false, bool getObjectProperties = true, bool loadSchema = true)
        {
            SetPropertiesFromOrganizationalUnit( ou, getAccessRules, getObjectProperties, true, loadSchema );
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

        public void SetPropertiesFromOrganizationalUnit(DirectoryEntry ou, bool getAccessRules, bool getObjectProperties, bool getParent, bool loadSchema)
        {
            if( ou == null ) return;

            SetPropertiesFromDirectoryEntry( ou, loadSchema, getAccessRules, getObjectProperties, getParent );

            DistinguishedName = ou.Properties["distinguishedName"].Value.ToString();
        }
    }
}