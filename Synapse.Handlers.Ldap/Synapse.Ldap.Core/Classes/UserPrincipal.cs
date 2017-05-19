using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Ldap.Core
{
    public class UserPrincipalObject : SecurityPrincipalObject
    {
        public UserPrincipalObject() { }
        public UserPrincipalObject(UserPrincipal up)
        {
            SetPropertiesFromUserPrincipal( up );
        }


        //
        // Summary:
        //     Gets or sets the e-mail address for this account.
        //
        // Returns:
        //     The e-mail address of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string EmailAddress { get; set; }
        //
        // Summary:
        //     Gets or sets the employee ID for this user principal.
        //
        // Returns:
        //     The employee ID of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string EmployeeId { get; set; }
        //
        // Summary:
        //     Gets or sets the given name for the user principal.
        //
        // Returns:
        //     The given name of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string GivenName { get; set; }
        //
        // Summary:
        //     Gets or sets the middle name for the user principal.
        //
        // Returns:
        //     The middle name of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string MiddleName { get; set; }
        //
        // Summary:
        //     Gets or sets the surname for the user principal.
        //
        // Returns:
        //     The surname of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string Surname { get; set; }
        //
        // Summary:
        //     Gets or sets the voice telephone number for the user principal.
        //
        // Returns:
        //     The voice telephone number of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string VoiceTelephoneNumber { get; set; }


        public static UserPrincipalObject FromUserPrincipal(UserPrincipal up)
        {
            return new UserPrincipalObject( up );
        }

        public void SetPropertiesFromUserPrincipal(UserPrincipal up)
        {
            if( up == null ) return;

            SetPropertiesFromAuthenticablePrincipal( up );

            EmailAddress = up.EmailAddress;
            EmployeeId = up.EmployeeId;
            GivenName = up.GivenName;
            MiddleName = up.MiddleName;
            Surname = up.Surname;
            VoiceTelephoneNumber = up.VoiceTelephoneNumber;
        }
    }
}