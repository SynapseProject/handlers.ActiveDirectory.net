using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

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
        public bool SmartcardLogonRequired { get; set; }
        public bool DelegationPermitted { get; set; }
        public string HomeDirectory { get; set; }
        public string ScriptPath { get; set; }
        public bool PasswordNotRequired { get; set; }
        public bool PasswordNeverExpires { get; set; }
        public bool UserCannotChangePassword { get; set; }
        public bool AllowReversiblePasswordEncryption { get; set; }
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

        public UserPrincipal GetUserPrincipal()
        {
            if ( !String.IsNullOrWhiteSpace( this.DistinguishedName ) )
            {
                Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
                Match match = regex.Match( this.DistinguishedName );
                if ( match.Success )
                {
                    this.Name = match.Groups[1]?.Value?.Trim();
                    this.Path = match.Groups[2]?.Value?.Trim();
                }
            }

            this.Path = Path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( this.Path );
            UserPrincipal user = new UserPrincipal( context );

            user.UserPrincipalName = this.UserPrincipalName ?? this.Name;
            user.SamAccountName = this.SamAccountName ?? this.Name;
            user.DisplayName = this.DisplayName;
            user.Description = this.Description;
            user.Name = this.Name;

            if (this.Enabled != null)
                user.Enabled = this.Enabled;
            user.PermittedLogonTimes = this.PermittedLogonTimes;
            user.AccountExpirationDate = this.AccountExpirationDate;
            user.SmartcardLogonRequired = this.SmartcardLogonRequired;
            user.DelegationPermitted = this.DelegationPermitted;
            user.HomeDirectory = this.HomeDirectory;
            user.ScriptPath = this.ScriptPath;
            user.PasswordNotRequired = this.PasswordNotRequired;
            user.PasswordNeverExpires = this.PasswordNeverExpires;
            user.UserCannotChangePassword = this.UserCannotChangePassword;
            user.AllowReversiblePasswordEncryption = this.AllowReversiblePasswordEncryption;
            user.HomeDrive = this.HomeDrive;

            user.GivenName = this.GivenName;
            user.MiddleName = this.MiddleName;
            user.Surname = this.Surname;
            user.EmailAddress = this.EmailAddress;
            user.VoiceTelephoneNumber = this.VoiceTelephoneNumber;
            user.EmployeeId = this.EmployeeId;

            user.SetPassword( Password );

            return user;
        }

    }
}
