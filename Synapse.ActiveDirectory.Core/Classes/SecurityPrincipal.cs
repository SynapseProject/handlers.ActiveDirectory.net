using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.ActiveDirectory.Core
{
    public class SecurityPrincipalObject : PrincipalObject
    {
        public SecurityPrincipalObject() { }
        public SecurityPrincipalObject(AuthenticablePrincipal ap)
        {
            SetPropertiesFromAuthenticablePrincipal( ap );
        }

        #region AuthenticablePrincipal
        //
        // Summary:
        //     Gets or sets a Nullable System.DateTime that specifies the date and time that
        //     the account expires.
        //
        // Returns:
        //     A System.DateTime that specifies the date and time that the account expires,
        //     or null if the account never expires.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public DateTime? AccountExpirationDate { get; set; }
        //
        // Summary:
        //     Gets the Nullable System.DateTime that specifies the date and time that the account
        //     was locked out.
        //
        // Returns:
        //     A System.DateTime that specifies the date and time that the account was locked
        //     out, or null if no lockout time is set on the account.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public DateTime? AccountLockoutTime { get; }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether reversible password encryption
        //     is enabled for this account.
        //
        // Returns:
        //     true if reversible password encryption is enabled for this account; otherwise
        //     false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public bool AllowReversiblePasswordEncryption { get; set; }
        //
        // Summary:
        //     Gets the number of logon attempts using incorrect credentials for this account.
        //
        // Returns:
        //     The number of logon attempts using incorrect credentials for this account.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public int BadLogonCount { get; }

        ////
        //// Summary:
        ////     Gets a System.X509Certificate2Collection that contains the X509 certificates
        ////     for this account.
        ////
        //// Returns:
        ////     A System.X509Certificate2Collection that contains the X509 certificates for this
        ////     account.
        ////
        //// Exceptions:
        ////   T:System.InvalidOperationException:
        ////     The underlying store does not support this property.
        //public X509Certificate2Collection Certificates { get; }


        //
        // Summary:
        //     Gets or sets a Nullable Boolean value that specifies whether the account may
        //     be delegated.
        //
        // Returns:
        //     true if the account may be delegated; otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public bool DelegationPermitted { get; set; }
        //
        // Summary:
        //     Gets or sets a Nullable Boolean value that specifies whether this account is
        //     enabled for authentication.
        //
        // Returns:
        //     true if the principal is enabled, or null if the account has not been persisted;
        //     otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        //
        //   T:System.ArgumentNullException:
        //     The application tried to set a null value for this property.
        public bool? Enabled { get; set; }
        //
        // Summary:
        //     Gets or sets the home directory for this account.
        //
        // Returns:
        //     The home directory for this account, or null if no home directory exists.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string HomeDirectory { get; set; }
        //
        // Summary:
        //     Gets or sets the home drive for this account.
        //
        // Returns:
        //     The home drive for the account, or null if no home drive exists.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string HomeDrive { get; set; }
        //
        // Summary:
        //     Gets the Nullable System.DateTime that specifies the date and time of the last
        //     incorrect password attempt on this account.
        //
        // Returns:
        //     A Nullable System.DateTime that specifies the date and time of the last incorrect
        //     password attempt on this account, or null if no incorrect password tries are
        //     recorded.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public DateTime? LastBadPasswordAttempt { get; set; }
        //
        // Summary:
        //     Gets the Nullable System.DateTime that specifies the date and time of the last
        //     logon for this account.
        //
        // Returns:
        //     A Nullable System.DateTime that specifies the date and time of the last logon
        //     for this account.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public DateTime? LastLogon { get; set; }
        //
        // Summary:
        //     Gets the Nullable System.DateTime that specifies the last date and time that
        //     the password was set for this account.
        //
        // Returns:
        //     A Nullable System.DateTime that specifies the last date and time that the password
        //     was set for this account.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public DateTime? LastPasswordSet { get; set; }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether the password expires for
        //     this account.
        //
        // Returns:
        //     true if the password expires for this account; otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public bool PasswordNeverExpires { get; set; }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether a password is required for
        //     this account.
        //
        // Returns:
        //     true if a password is required for this account; otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public bool PasswordNotRequired { get; set; }
        //
        // Summary:
        //     Gets or sets the times when the principal can logon.
        //
        // Returns:
        //     The permitted logon times for this account.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public byte[] PermittedLogonTimes { get; set; }

        ////
        //// Summary:
        ////     Gets the list of workstations that this principal is permitted to log into.
        ////
        //// Returns:
        ////     The mutable list of workstations that this principal is permitted to log into.
        ////
        //// Exceptions:
        ////   T:System.InvalidOperationException:
        ////     The underlying store does not support this property.
        //public PrincipalValueCollection<string> PermittedWorkstations { get; }


        //
        // Summary:
        //     Gets or sets the script path for this account.
        //
        // Returns:
        //     A path of the script for this account, or null if there is no script path.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string ScriptPath { get; set; }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether a smartcard is required to
        //     log on to the account.
        //
        // Returns:
        //     true if a smartcard is required to log on to this account; otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public bool SmartcardLogonRequired { get; set; }
        //
        // Summary:
        //     Gets or sets a Boolean value that specifies whether the user can change the password
        //     for this account. Do not use this with a System.DirecoryServices.AccountManagement.ComputerPrincipal.
        //
        // Returns:
        //     true if the user is not permitted to change the password; otherwise false.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        //
        //   T:System.NotSupportedException:
        //     This principal object is not a user.
        public bool UserCannotChangePassword { get; set; }
        #endregion


        public static SecurityPrincipalObject FromAuthenticablePrincipal(AuthenticablePrincipal ap)
        {
            return new SecurityPrincipalObject( ap );
        }

        public void SetPropertiesFromAuthenticablePrincipal(AuthenticablePrincipal ap)
        {
            if( ap == null ) return;

            SetPropertiesFromPrincipal( ap );

            AccountExpirationDate = ap.AccountExpirationDate;
            AllowReversiblePasswordEncryption = ap.AllowReversiblePasswordEncryption;
            DelegationPermitted = ap.DelegationPermitted;
            Enabled = ap.Enabled;
            HomeDirectory = ap.HomeDirectory;
            HomeDrive = ap.HomeDrive;
            LastBadPasswordAttempt = ap.LastBadPasswordAttempt;
            LastLogon = ap.LastLogon;
            LastPasswordSet = ap.LastPasswordSet;
            PasswordNeverExpires = ap.PasswordNeverExpires;
            PasswordNotRequired = ap.PasswordNotRequired;
            PermittedLogonTimes = ap.PermittedLogonTimes;
            ScriptPath = ap.ScriptPath;
            SmartcardLogonRequired = ap.SmartcardLogonRequired;
            UserCannotChangePassword = ap.UserCannotChangePassword;
        }
    }
}