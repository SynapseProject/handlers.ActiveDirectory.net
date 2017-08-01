using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public class PrincipalObject
    {
        Principal _innerPrincipal = null;

        public PrincipalObject() { }
        public PrincipalObject(Principal p)
        {
            SetPropertiesFromPrincipal( p );
        }

        #region Principal
        //
        // Summary:
        //     Gets the context type enumeraton value that specifies the type of principal context
        //     associated with this principal.
        //
        // Returns:
        //     A System.DirectoryServices.AccountManagement.ContextType enumeration value that
        //     specifies the context type.
        public ContextType ContextType { get; set; }
        //
        // Summary:
        //     Gets or sets the description of the principal.
        //
        // Returns:
        //     The description text for this principal or null if there is no description.
        public string Description { get; set; }
        //
        // Summary:
        //     Gets or sets the display name for this principal.
        //
        // Returns:
        //     The display name for this principal or null if there is no display name.
        public string DisplayName { get; set; }
        //
        // Summary:
        //     Gets the distinguished name (DN) for this principal.
        //
        // Returns:
        //     The DN for this principal or null if there is no DN.
        public string DistinguishedName { get; set; }
        //
        // Summary:
        //     Gets the GUID associated with this principal.
        //
        // Returns:
        //     The Nullable System.Guid associated with this principal or null if there is no
        //     GUID.
        public Guid? Guid { get; set; }
        //
        // Summary:
        //     Gets or sets the name of this principal.
        //
        // Returns:
        //     The name of the principal or null if the name attribute is not set.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The application tried to set the name to null.
        //
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string Name { get; set; }
        //
        // Summary:
        //     Gets or sets the SAM account name for this principal.
        //
        // Returns:
        //     The SAM account name for this principal or null if no name has been set.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The application tried to set the SAM account name to null.
        //
        //   T:System.InvalidOperationException:
        //     The application tried to set the SAM account name on a persisted principal.
        public string SamAccountName { get; set; }
        //
        // Summary:
        //     Gets the Security ID (SID) of the principal.
        //
        // Returns:
        //     The System.Security.Principal.SecurityIdentifier for this principal or null if
        //     there is no SID.
        public string Sid { get; set; }
        //
        // Summary:
        //     Gets the structural object class directory attribute.
        //
        // Returns:
        //     The structural object class directory attribute.
        public string StructuralObjectClass { get; set; }
        //
        // Summary:
        //     Gets or sets the user principal name (UPN) associated with this principal.
        //
        // Returns:
        //     The UPN associated with this principal or null if no if the UPN has not been
        //     set.
        //
        // Exceptions:
        //   T:System.Inval>DidOperationException:
        //     The underlying store does not support this property.
        public string UserPrincipalName { get; set; }
        #endregion

        public List<PrincipalObject> Groups { get; set; } = new List<PrincipalObject>();


        public static PrincipalObject FromPrincipal(Principal p)
        {
            return new PrincipalObject( p );
        }

        public void SetPropertiesFromPrincipal(Principal p)
        {
            _innerPrincipal = p;

            if( p == null ) return;

            ContextType = p.ContextType;
            Description = p.Description;
            DisplayName = p.DisplayName;
            DistinguishedName = p.DistinguishedName;
            Guid = p.Guid;
            Name = p.Name;
            SamAccountName = p.SamAccountName;
            Sid = p.Sid.Value;
            StructuralObjectClass = p.StructuralObjectClass;
            UserPrincipalName = p.UserPrincipalName;
        }

        public void GetGroups()
        {
            PrincipalSearchResult<Principal> sr = _innerPrincipal.GetGroups();
            foreach( Principal p in sr )
                Groups.Add( new PrincipalObject( p ) );
        }
    }
}