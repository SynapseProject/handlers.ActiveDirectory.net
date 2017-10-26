using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Synapse.Core.Utilities;


/// <summary>
/// Holds the startup config for the ActiveDirectoryApi; written as an independent class (not using .NET config) for cross-platform compatibility.
/// </summary>
public class ActiveDirectoryApiConfig
{
    public ActiveDirectoryApiConfig() { }

    public static readonly string CurrentPath = $"{Path.GetDirectoryName( typeof( ActiveDirectoryApiConfig ).Assembly.Location )}";
    public static readonly string FileName = $"{CurrentPath}\\Synapse.Services.ActiveDirectoryApi.config.yaml";


    public PlanConfig Plans { get; set; } = new PlanConfig();


    public void Serialize()
    {
        YamlHelpers.SerializeFile( FileName, this, serializeAsJson: false, emitDefaultValues: true );
    }

    public static ActiveDirectoryApiConfig Deserialze()
    {
        if ( !File.Exists( FileName ) )
            throw new FileNotFoundException( $"Could not find {FileName}" );

        return YamlHelpers.DeserializeFile<ActiveDirectoryApiConfig>( FileName );
    }

    public static ActiveDirectoryApiConfig DeserializeOrNew()
    {
        ActiveDirectoryApiConfig config = null;

        if ( !File.Exists( FileName ) )
        {
            config = new ActiveDirectoryApiConfig();

            config.Plans.User.Query = @"QueryUser";
            config.Plans.User.Create = @"CreateUser";
            config.Plans.User.Delete = @"DeleteUser";
            config.Plans.User.Modify = @"ModifyUser";
            config.Plans.User.AddToGroup = @"AddUserToGroup";
            config.Plans.User.RemoveFromGroup = @"RemoveUserFromGroup";
            config.Plans.User.AddAccessRule = @"AddAccessRuleToUser";
            config.Plans.User.RemoveAccessRule = @"RemoveAccessRuleFromUser";
            config.Plans.User.SetAccessRule = @"SetAccessRuleOnUser";
            config.Plans.User.PurgeAccessRules = @"PurgeAccessRulesOnUser";

            config.Plans.Group.Query = @"QueryGroup";
            config.Plans.Group.Create = @"CreateGroup";
            config.Plans.Group.Delete = @"DeleteGroup";
            config.Plans.Group.Modify = @"ModifyGroup";
            config.Plans.Group.AddToGroup = @"AddGroupToGroup";
            config.Plans.Group.RemoveFromGroup = @"RemoveGroupFromGroup";
            config.Plans.Group.AddAccessRule = @"AddAccessRuleToGroup";
            config.Plans.Group.RemoveAccessRule = @"RemoveAccessRuleFromGroup";
            config.Plans.Group.SetAccessRule = @"SetAccessRuleOnGroup";
            config.Plans.Group.PurgeAccessRules = @"PurgeAccessRulesOnGroup";

            config.Plans.OrganizationalUnit.Query = @"QueryOrgUnit";
            config.Plans.OrganizationalUnit.Create = @"CreateOrgUnit";
            config.Plans.OrganizationalUnit.Delete = @"DeleteOrgUnit";
            config.Plans.OrganizationalUnit.Modify = @"ModifyOrgUnit";
            config.Plans.OrganizationalUnit.AddAccessRule = @"AddAccessRuleToOrgUnit";
            config.Plans.OrganizationalUnit.RemoveAccessRule = @"RemoveAccessRuleFromOrgUnit";
            config.Plans.OrganizationalUnit.SetAccessRule = @"SetAccessRuleOnOrgUnit";
            config.Plans.OrganizationalUnit.PurgeAccessRules = @"PurgeAccessRulesOnOrgUnit";

            config.Plans.Search = @"Search";

            config.Serialize();
        }
        else
        {
            config = YamlHelpers.DeserializeFile<ActiveDirectoryApiConfig>( FileName );
        }

        return config;
    }
}

public class PlanConfig
{
    public UserPlans User { get; set; } = new UserPlans();
    public GroupPlans Group { get; set; } = new GroupPlans();
    public OrgUnitPlans OrganizationalUnit { get; set; } = new OrgUnitPlans();
    public string Search { get; set; }
}

public class AllPlans
{
    public string Query { get; set; }
    public string Create { get; set; }
    public string Delete { get; set; }
    public string Modify { get; set; }
    public string AddAccessRule { get; set; }
    public string RemoveAccessRule { get; set; }
    public string SetAccessRule { get; set; }
    public string PurgeAccessRules { get; set; }
}

public class UserPlans : AllPlans
{
    public string AddToGroup { get; set; }
    public string RemoveFromGroup { get; set; }
}

public class GroupPlans : AllPlans
{
    public string AddToGroup { get; set; }
    public string RemoveFromGroup { get; set; }
}

public class OrgUnitPlans : AllPlans
{
}


