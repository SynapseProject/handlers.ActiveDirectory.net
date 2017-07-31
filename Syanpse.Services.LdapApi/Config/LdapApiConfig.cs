using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Synapse.Core.Utilities;


/// <summary>
/// Holds the startup config for the LdapApi; written as an independent class (not using .NET config) for cross-platform compatibility.
/// </summary>
public class LdapApiConfig
{
    public LdapApiConfig() { }

    public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( LdapApiConfig ).Assembly.Location )}";
    public static readonly string FileName = $"{CurrentPath}\\Synapse.Services.LdapApi.config.yaml";


    public PlanConfig Plans { get; set; } = new PlanConfig();


    public void Serialize()
    {
        YamlHelpers.SerializeFile( FileName, this, serializeAsJson: false, emitDefaultValues: true );
    }

    public static LdapApiConfig Deserialze()
    {
        if ( !File.Exists( FileName ) )
            throw new FileNotFoundException( $"Could not find {FileName}" );

        return YamlHelpers.DeserializeFile<LdapApiConfig>( FileName );
    }

    public static LdapApiConfig DeserializeOrNew()
    {
        LdapApiConfig config = null;

        if ( !File.Exists( FileName ) )
        {
            config = new LdapApiConfig();

            config.Plans.User.Query = @"QueryUser";
            config.Plans.User.Create = @"CreateUser";
            config.Plans.User.Delete = @"DeleteUser";
            config.Plans.User.Modify = @"ModifyUser";
            config.Plans.User.AddToGroup = @"AddUserToGroup";
            config.Plans.User.RemoveFromGroup = @"RemoveUserFromGroup";

            config.Plans.Group.Query = @"QueryGroup";
            config.Plans.Group.Create = @"CreateGroup";
            config.Plans.Group.Delete = @"DeleteGroup";
            config.Plans.Group.Modify = @"ModifyGroup";
            config.Plans.Group.AddToGroup = @"AddGroupToGroup";
            config.Plans.Group.RemoveFromGroup = @"RemoveGroupFromGroup";

            config.Plans.OrganizationalUnit.Query = @"QueryOrgUnit";
            config.Plans.OrganizationalUnit.Create = @"CreateOrgUnit";
            config.Plans.OrganizationalUnit.Delete = @"DeleteOrgUnit";
            config.Plans.OrganizationalUnit.Modify = @"ModifyOrgUnit";

            config.Serialize();
        }
        else
        {
            config = YamlHelpers.DeserializeFile<LdapApiConfig>( FileName );
        }

        return config;
    }
}

public class PlanConfig
{
    public UserPlans User { get; set; } = new UserPlans();
    public GroupPlans Group { get; set; } = new GroupPlans();
    public OrgUnitPlans OrganizationalUnit { get; set; } = new OrgUnitPlans();
}

public class UserPlans
{
    public string Query { get; set; }
    public string Create { get; set; }
    public string Delete { get; set; }
    public string Modify { get; set; }
    public string AddToGroup { get; set; }
    public string RemoveFromGroup { get; set; }
}

public class GroupPlans
{
    public string Query { get; set; }
    public string Create { get; set; }
    public string Delete { get; set; }
    public string Modify { get; set; }
    public string AddToGroup { get; set; }
    public string RemoveFromGroup { get; set; }
}

public class OrgUnitPlans
{
    public string Query { get; set; }
    public string Create { get; set; }
    public string Delete { get; set; }
    public string Modify { get; set; }
}

