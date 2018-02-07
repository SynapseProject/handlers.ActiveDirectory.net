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

        // Settable DirectoryEntry Properties
        public string ManagedBy { get; set; }

        [XmlArrayItem(ElementName = "Group")]
        public List<string> Groups { get; set; }

        public override AdObjectType GetADType()
        {
            return AdObjectType.Group;
        }


        public GroupPrincipal CreateGroupPrincipal()
        {
            GroupPrincipal group = DirectoryServices.CreateGroupPrincipal( this.Identity, this.SamAccountName );
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

            // Get DistinguishedName from User or Group Identity for ManagedBy Property
            if ( this.ManagedBy != null && group.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
            {
                String distinguishedName = DirectoryServices.GetDistinguishedName( this.ManagedBy );
                if ( distinguishedName == null )
                    distinguishedName = this.ManagedBy;     // Cant' Find As User Or Group, Pass Raw Value (Might Be ~null~)
                DirectoryServices.SetProperty( (DirectoryEntry)group.GetUnderlyingObject(), "managedby", distinguishedName );
            }

            if ( group.GetUnderlyingObjectType() == typeof( DirectoryEntry ) && this.Properties?.Count > 0 )
                DirectoryServices.SetProperties( (DirectoryEntry)group.GetUnderlyingObject(), this.Properties );
        }

    }
}
