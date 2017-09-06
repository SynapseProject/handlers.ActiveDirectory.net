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
    public class AdGroup : AdObject
    {
        // Settable Principal Fields
        public string SamAccountName { get; set; }
        public string Description { get; set; }

        // Settable GroupPrincipalFields
        public GroupScope? Scope { get; set; }
        public bool? IsSecurityGroup { get; set; }

        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.Group;
        }


        public GroupPrincipal CreateGroupPrincipal()
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
                throw new AdException( "Unable To Create Group Principal From Given Input.", AdStatusType.MissingInput );


            path = path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( path );
            GroupPrincipal group = new GroupPrincipal( context );

            group.Name = name;
            this.SamAccountName = this.SamAccountName ?? name;
            if (this.Properties?.Count > 0)
                group.Save();   // Group Must Exist Before Properties Can Be Updated

            UpdateGroupPrincipal( group );

            return group;
        }

        public void UpdateGroupPrincipal(GroupPrincipal group)
        {
            if ( this.SamAccountName != null )
                group.SamAccountName = SetValueOrNull( this.SamAccountName );
            if ( this.Description != null )
                group.Description = SetValueOrNull( this.Description );

            if ( this.IsSecurityGroup != null )
                group.IsSecurityGroup = this.IsSecurityGroup;

            if ( this.Scope != null )
                group.GroupScope = this.Scope;

            if ( group.GetUnderlyingObjectType() == typeof( DirectoryEntry ) && this.Properties?.Count > 0 )
                DirectoryServices.SetProperties( (DirectoryEntry)group.GetUnderlyingObject(), this.Properties );
        }

    }
}
