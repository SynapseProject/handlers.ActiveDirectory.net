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
            String path = DirectoryServices.GetDomainDistinguishedName();

            if ( DirectoryServices.IsDistinguishedName(this.Identity) )
            {
                Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
                Match match = regex.Match( this.Identity );
                if ( match.Success )
                {
                    name = match.Groups[1]?.Value?.Trim();
                    path = match.Groups[2]?.Value?.Trim();
                }
            }
            else if ( String.IsNullOrWhiteSpace( this.Identity ) )
                throw new AdException( "Unable To Create User Principal From Given Input.", AdStatusType.MissingInput );

            path = path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( path );
            UserPrincipal user = new UserPrincipal( context );

            user.UserPrincipalName = this.UserPrincipalName ?? name;
            user.SamAccountName = this.SamAccountName ?? name;
            user.DisplayName = this.DisplayName;
            user.Description = this.Description;
            user.Name = name;

            if (this.Enabled != null)
                user.Enabled = this.Enabled;
            user.PermittedLogonTimes = this.PermittedLogonTimes;
            user.AccountExpirationDate = this.AccountExpirationDate;
            user.SmartcardLogonRequired = this.SmartcardLogonRequired ?? false;
            user.DelegationPermitted = this.DelegationPermitted ?? false;
            user.HomeDirectory = this.HomeDirectory;
            user.ScriptPath = this.ScriptPath;
            user.PasswordNotRequired = this.PasswordNotRequired ?? false;
            user.PasswordNeverExpires = this.PasswordNeverExpires ?? false;
            user.UserCannotChangePassword = this.UserCannotChangePassword ?? false;
            user.AllowReversiblePasswordEncryption = this.AllowReversiblePasswordEncryption ?? false;
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
