using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdGroup : AdObject
    {
        // Settable Principal Fields
        public string UserPrincipalName { get; set; }
        public string SamAccountName { get; set; }
//        public string DisplayName { get; set; }       // Does Not Seem To Work On Groups
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
            String name = String.Empty;
            String path = String.Empty;

            if ( !String.IsNullOrWhiteSpace( this.Identity ) )
            {
                Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
                Match match = regex.Match( this.Identity );
                if ( match.Success )
                {
                    name = match.Groups[1]?.Value?.Trim();
                    path = match.Groups[2]?.Value?.Trim();
                }
            }

            path = path.Replace( "LDAP://", "" );
            PrincipalContext context = DirectoryServices.GetPrincipalContext( path );
            GroupPrincipal group = new GroupPrincipal( context );

            group.SamAccountName = this.SamAccountName ?? name;
            group.Description = this.Description;
            group.Name = name;

            if (this.IsSecurityGroup != null)
                group.IsSecurityGroup = this.IsSecurityGroup;

            if ( this.Scope != null )
                group.GroupScope = this.Scope;

            return group;
        }


    }
}
