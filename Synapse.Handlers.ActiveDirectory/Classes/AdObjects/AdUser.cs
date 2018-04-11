using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;
using System.DirectoryServices;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdUser : AdObject
    {
        // Settable Principal Fields
        public string UserPrincipalName { get; set; }
        public string SamAccountName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        // Settable AuthenticationPrincipal Fields
        public bool? Enabled { get; set; }
        public byte[] PermittedLogonTimes { get; set; }
        public DateTime? AccountExpirationDate { get; set; }
        public bool? SmartcardLogonRequired { get; set; }
        public bool? DelegationPermitted { get; set; }
        public string HomeDirectory { get; set; }
        public string ScriptPath { get; set; }
        public bool? PasswordNotRequired { get; set; }
        public bool? PasswordNeverExpires { get; set; }
        public bool? UserCannotChangePassword { get; set; }
        public bool? AllowReversiblePasswordEncryption { get; set; }
        public string HomeDrive { get; set; }

        // Settable UserPrincipal Fields
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string VoiceTelephoneNumber { get; set; }
        public string EmployeeId { get; set; }

        // Other User Fields
        public string Password { get; set; }
        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.User;
        }

        public UserPrincipal CreateUserPrincipal()
        {
            String name = this.Identity;
            String domain = DirectoryServices.GetDomain(this.Identity, out name);

            UserPrincipal user = null;

            if ( DirectoryServices.IsDistinguishedName(this.Identity) )
                user = DirectoryServices.CreateUserPrincipal( this.Identity, this.UserPrincipalName, this.SamAccountName );

            if (this.Properties?.Count > 0)
                user.Save();    // User Must Exist Before Properties Can Be Updated.

            UpdateUserPrincipal( user );

            return user;
        }

        public void UpdateUserPrincipal( UserPrincipal user )
        {
            if ( this.UserPrincipalName != null )
                user.UserPrincipalName = SetValueOrNull( this.UserPrincipalName );
            if ( this.SamAccountName != null )
                user.SamAccountName = SetValueOrNull( this.SamAccountName );
            if ( this.DisplayName != null )
                user.DisplayName = SetValueOrNull( this.DisplayName );
            if ( this.Description != null )
                user.Description = SetValueOrNull( this.Description );

            if ( this.Enabled != null )
                user.Enabled = this.Enabled;
            if ( this.PermittedLogonTimes != null )
                user.PermittedLogonTimes = this.PermittedLogonTimes;
            if ( this.AccountExpirationDate != null )
                user.AccountExpirationDate = this.AccountExpirationDate;
            if ( this.SmartcardLogonRequired.HasValue)
                user.SmartcardLogonRequired = this.SmartcardLogonRequired.Value;
            if (this.DelegationPermitted.HasValue)
                user.DelegationPermitted = this.DelegationPermitted.Value;
            if ( this.HomeDirectory != null )
                user.HomeDirectory = SetValueOrNull( this.HomeDirectory );
            if ( this.ScriptPath != null )
                user.ScriptPath = SetValueOrNull( this.ScriptPath );
            if ( this.PasswordNotRequired.HasValue )
                user.PasswordNotRequired = this.PasswordNotRequired.Value;
            if ( this.PasswordNeverExpires.HasValue )
                user.PasswordNeverExpires = this.PasswordNeverExpires.Value;
            if ( this.UserCannotChangePassword.HasValue )
                user.UserCannotChangePassword = this.UserCannotChangePassword.Value;
            if ( this.AllowReversiblePasswordEncryption.HasValue )
                user.AllowReversiblePasswordEncryption = this.AllowReversiblePasswordEncryption.Value;
            if ( this.HomeDrive != null )
                user.HomeDrive = SetValueOrNull( this.HomeDrive );

            if ( this.GivenName != null )
                user.GivenName = SetValueOrNull( this.GivenName );
            if ( this.MiddleName != null )
                user.MiddleName = SetValueOrNull( this.MiddleName );
            if ( this.Surname != null )
                user.Surname = (this.Surname == "") ? null : this.Surname;
            if ( this.EmailAddress != null )
                user.EmailAddress = SetValueOrNull(  this.EmailAddress );
            if ( this.VoiceTelephoneNumber != null )
                user.VoiceTelephoneNumber = SetValueOrNull( this.VoiceTelephoneNumber );
            if ( this.EmployeeId != null )
                user.EmployeeId = SetValueOrNull( this.EmployeeId );

            if ( this.Password != null )
                user.SetPassword( Password );

            if (this.Properties?.Count > 0)
                if ( user.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                    DirectoryServices.SetProperties( (DirectoryEntry)user.GetUnderlyingObject(), this.Properties );
        }

    }
}
