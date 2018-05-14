using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

using Synapse.Core;
using Synapse.Core.Utilities;
using Synapse.ActiveDirectory.Core;
using Synapse.Handlers.ActiveDirectory;

public class ActiveDirectoryHandler : HandlerRuntimeBase
{
    ActiveDirectoryHandlerConfig config = null;
    HandlerConfig handlerConfig = null;
    IRoleManager roleManager = new DefaultRoleManager();

    HandlerStartInfo startInfo = null;
    string requestUser = null;

    ActiveDirectoryHandlerResults results = new ActiveDirectoryHandlerResults();
    bool isDryRun = false;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<ActiveDirectoryHandlerConfig>( config );
        if ( handlerConfig == null )
        {
            this.handlerConfig = HandlerConfig.DeserializeOrNew();
            this.roleManager = AssemblyLoader.Load<IRoleManager>( handlerConfig.RoleManager.Name, @"Synapse.ActiveDirectory.Core:DefaultRoleManager" );
            this.roleManager.Initialize( handlerConfig?.RoleManager?.Config );
        }
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        int cheapSequence = 0;
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = int.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        this.startInfo = startInfo;
        requestUser = WhoAmI();
        isDryRun = startInfo.IsDryRun;

        try
        {
            //deserialize the Parameters from the Action declaration
            ActiveDirectoryHandlerParameters parameters = YamlHelpers.Deserialize<ActiveDirectoryHandlerParameters>( startInfo.Parameters );

            OnLogMessage( "Execute", $"Running Handler As User [{System.Security.Principal.WindowsIdentity.GetCurrent().Name}]" );
            OnLogMessage( "Execute", $"Request User : [{requestUser}]" );

            if (startInfo.IsDryRun && config.Action != ActionType.Search)
            {
                ProcessActiveDirectoryObjects(parameters.Users, ProcessDryRun);
                ProcessActiveDirectoryObjects(parameters.Groups, ProcessDryRun);
                ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessDryRun);
            }
            else
            {
                switch (config.Action)
                {
                    case ActionType.Get:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessGet);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessGet);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessGet);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessGet);
                        break;
                    case ActionType.Create:
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessCreate);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessCreate);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessCreate);
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessCreate);
                        break;
                    case ActionType.Modify:
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessModify);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessModify);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessModify);
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessModify);
                        break;
                    case ActionType.Delete:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessDelete);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessDelete);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessDelete);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessDelete);
                        break;
                    case ActionType.AddToGroup:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessGroupAdd);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessGroupAdd);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessGroupAdd);
                        break;
                    case ActionType.RemoveFromGroup:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessGroupRemove);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessGroupRemove);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessGroupRemove);
                        break;
                    case ActionType.AddAccessRule:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessAccessRules);
                        break;
                    case ActionType.RemoveAccessRule:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessAccessRules);
                        break;
                    case ActionType.SetAccessRule:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessAccessRules);
                        break;
                    case ActionType.PurgeAccessRules:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessAccessRules);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessAccessRules);
                        break;
                    case ActionType.AddRole:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessRoles);
                        break;
                    case ActionType.RemoveRole:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessRoles);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessRoles);
                        break;
                    case ActionType.Search:
                        ProcessSearchRequests(parameters.SearchRequests, startInfo.IsDryRun);
                        break;
                    case ActionType.Move:
                        ProcessActiveDirectoryObjects(parameters.Users, ProcessMove);
                        ProcessActiveDirectoryObjects(parameters.Groups, ProcessMove);
                        ProcessActiveDirectoryObjects(parameters.Computers, ProcessMove);
                        ProcessActiveDirectoryObjects(parameters.OrganizationalUnits, ProcessMove);
                        break;
                    default:
                        throw new AdException($"Unknown Action {config.Action} Specified", AdStatusType.NotSupported);
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch ( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        if (string.IsNullOrWhiteSpace(result.ExitData?.ToString()))
            result.ExitData = results.Serialize( config.OutputType, config.PrettyPrint );

        if (!config.SuppressOutput)
            OnProgress( __context, result.ExitData?.ToString(), result.Status, sequence: cheapSequence++, ex: exc );

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: int.MaxValue, ex: exc );

        return result;
    }

    public override object GetConfigInstance()
    {
        ActiveDirectoryHandlerConfig config = new ActiveDirectoryHandlerConfig();

        config.Action = ActionType.Get;
        config.RunSequential = false;
        config.ReturnGroupMembership = true;
        config.ReturnAccessRules = true;
        config.ReturnObjectProperties = true;
        config.ReturnObjects = true;
        config.SuppressOutput = false;
        config.UseUpsert = true;
        config.OutputType = SerializationFormat.Yaml;
        config.PrettyPrint = true;

        return config;
    }

    public override object GetParametersInstance()
    {
        ActiveDirectoryHandlerParameters parms = new ActiveDirectoryHandlerParameters();

        parms.Users = new List<AdUser>();
        AdUser user = new AdUser();
        user.Identity = "cn=mfox,ou=FamousActors,dc=sandbox,dc=local";
        parms.Users.Add(user);

        parms.Groups = new List<AdGroup>();
        AdGroup group = new AdGroup();
        group.Identity = "cn=BackToTheFuture,ou=Movies,dc=sandbox,dc=local";
        parms.Groups.Add(group);

        parms.OrganizationalUnits = new List<AdOrganizationalUnit>();
        AdOrganizationalUnit ou = new AdOrganizationalUnit();
        ou.Identity = "ou=Movies,dc=sandbox,dc=local";
        parms.OrganizationalUnits.Add(ou);

        parms.SearchRequests = new List<AdSearchRequest>();
        AdSearchRequest search = new AdSearchRequest();
        search.SearchBase = "ou=Synapse,dc=sandbox,dc=local";
        search.Filter = "(objectClass=User)";
        search.ReturnAttributes = new List<string>();
        search.ReturnAttributes.Add("Name");
        search.ReturnAttributes.Add("objectGUID");
        parms.SearchRequests.Add(search);


        return parms;
    }

    private void ProcessActiveDirectoryObjects(IEnumerable<AdObject> objs, Action<AdObject, bool> processFunction)
    {
        if ( objs != null )
        {
            if ( config.RunSequential )
            {
                foreach ( AdObject obj in objs )
                    processFunction( obj, config.ReturnObjects );
            }
            else
                Parallel.ForEach( objs, obj =>
                {
                    processFunction( obj, config.ReturnObjects );
                } );
        }
    }

    private void ProcessGet(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        try
        {
            roleManager.CanPerformActionOrException( requestUser, ActionType.Get, obj.Identity );
            GetObject( result, obj, returnObject );
        }
        catch (AdException ade)
        {
            ProcessActiveDirectoryException( result, ade, ActionType.Get );
        }

        results.Add( result );
    }

    private void GetObject(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true, bool returnStatus = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            object adObject = GetActiveDirectoryObject( obj );
            if ( returnObject )
                result.Object = adObject;
            if ( returnStatus )
                result.Statuses.Add( status );

        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "GetObject", e.Message );
            OnLogMessage( "GetObject", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }
    }

    private object GetActiveDirectoryObject(AdObject obj)
    {
        switch ( obj.Type )
        {
            case AdObjectType.User:
                AdUser user = (AdUser)obj;
                UserPrincipalObject upo = null;
                upo = DirectoryServices.GetUser( user.Identity, config.ReturnGroupMembership, config.ReturnAccessRules, config.ReturnObjectProperties );

                // User Might Have Been Renamed, Look Up By "Name" If Provided
                if (upo == null && !String.IsNullOrEmpty(user.Name))
                    upo = DirectoryServices.GetUser( user.Name, config.ReturnGroupMembership, config.ReturnAccessRules, config.ReturnObjectProperties);

                if ( upo == null )
                    throw new AdException( $"User [{user.Identity}] Was Not Found.", AdStatusType.DoesNotExist );
                return upo;
            case AdObjectType.Group:
                AdGroup group = (AdGroup)obj;
                GroupPrincipalObject gpo = null;
                gpo = DirectoryServices.GetGroup( group.Identity, config.ReturnGroupMembership, config.ReturnAccessRules, config.ReturnObjectProperties );

                // Group Might Have Been Renamed, Look Up By "Name" If Provided
                if (gpo == null && !String.IsNullOrEmpty(group.Name))
                    gpo = DirectoryServices.GetGroup(group.Name, config.ReturnGroupMembership, config.ReturnAccessRules, config.ReturnObjectProperties);

                if ( gpo == null )
                    throw new AdException( $"Group [{group.Identity}] Was Not Found.", AdStatusType.DoesNotExist );
                return gpo;
            case AdObjectType.OrganizationalUnit:
                AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                DirectoryEntryObject ouo = null;
                ouo = DirectoryServices.GetOrganizationalUnit(ou.Identity, config.ReturnAccessRules, config.ReturnObjectProperties, config.LoadSchema);

                // Group Might Have Been Renamed, Look Up By "Name" If Provided
                if (ouo == null && !String.IsNullOrEmpty(ou.Name))
                    ouo = DirectoryServices.GetOrganizationalUnit(ou.Name, config.ReturnAccessRules, config.ReturnObjectProperties, config.LoadSchema);

                if (ouo == null)
                    throw new AdException($"Organizational Unit [{ou.Identity}] Was Not Found.", AdStatusType.DoesNotExist);
                return ouo;
            case AdObjectType.Computer:
                AdComputer computer = (AdComputer)obj;
                DirectoryEntryObject co = null;
                co = DirectoryServices.GetComputer(computer.Identity, config.ReturnAccessRules, config.ReturnObjectProperties, config.LoadSchema);

                // Group Might Have Been Renamed, Look Up By "Name" If Provided
                if (co == null && !String.IsNullOrEmpty(computer.Name))
                    co = DirectoryServices.GetComputer(computer.Name, config.ReturnAccessRules, config.ReturnObjectProperties, config.LoadSchema);

                if (co == null)
                    throw new AdException($"Computer [{computer.Identity}] Was Not Found.", AdStatusType.DoesNotExist);
                return co;
            default:
                throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
        }
    }

    private void ProcessCreate(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            string statusAction = "Created";

            String idOnly = null;
            String domain = DirectoryServices.GetDomain(obj.Identity, out idOnly);

            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    UserPrincipal up = null;
                    if ( config.UseUpsert && DirectoryServices.IsExistingUser( obj.Identity ) )
                    {
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Modify, obj.Identity );
                        up = DirectoryServices.GetUserPrincipal( idOnly, domain );
                        if ( up == null )
                            throw new AdException( $"User [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        user.UpdateUserPrincipal( up );
                        statusAction = "Modified";
                    }
                    else if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                    {
                        String path = DirectoryServices.GetParentPath( obj.Identity );
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Create, path );
                        up = user.CreateUserPrincipal();
                    }
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );

                    DirectoryServices.SaveUser( up, isDryRun );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Identity + "] " + statusAction + "." );
                    result.Statuses.Add( status );
                    if ( user.MemberOf != null )
                        AddToGroup( result, user, false );
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    GroupPrincipal gp = null;
                    if ( config.UseUpsert && DirectoryServices.IsExistingGroup( idOnly, domain ) )
                    {
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Modify, obj.Identity );
                        gp = DirectoryServices.GetGroupPrincipal( idOnly, domain );
                        if ( gp == null )
                            throw new AdException( $"Group [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        group.UpdateGroupPrincipal( gp );
                        statusAction = "Modified";
                    }
                    else if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                    {
                        String path = DirectoryServices.GetParentPath( obj.Identity );
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Create, path );
                        gp = group.CreateGroupPrincipal();
                    }
                    else
                        throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );

                    DirectoryServices.SaveGroup( gp, isDryRun );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Identity + "] " + statusAction + "." );
                    result.Statuses.Add( status );
                    if ( group.MemberOf != null )
                        AddToGroup( result, group, false );
                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if (!String.IsNullOrWhiteSpace(ou.ManagedBy))
                    {
                        if (ou.Properties == null)
                            ou.Properties = new Dictionary<string, List<string>>();

                        if (!ou.Properties.ContainsKey("managedBy"))
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName(ou.ManagedBy);
                            if (distinguishedName == null)
                                distinguishedName = ou.ManagedBy;

                            List<String> values = new List<string>() { distinguishedName };
                            ou.Properties.Add("managedBy", values);
                        }
                    }

                    if (config.UseUpsert && DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Modify, obj.Identity);
                        if (!String.IsNullOrWhiteSpace(ou.Description))
                            DirectoryServices.AddProperty(ou.Properties, "description", ou.Description);
                        DirectoryServices.ModifyOrganizationUnit(ou.Identity, ou.Properties, isDryRun);
                        statusAction = "Modified";
                    }
                    else if (DirectoryServices.IsDistinguishedName(ou.Identity))
                    {
                        String path = DirectoryServices.GetParentPath(obj.Identity);
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Create, path);
                        if (!String.IsNullOrWhiteSpace(ou.Description))
                            DirectoryServices.AddProperty(ou.Properties, "description", ou.Description);
                        DirectoryServices.CreateOrganizationUnit(ou.Identity, ou.Properties, isDryRun);
                    }
                    else
                        throw new AdException($"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput);

                    OnLogMessage("ProcessCreate", obj.Type + " [" + obj.Identity + "] " + statusAction + ".");
                    result.Statuses.Add(status);
                    break;
                case AdObjectType.Computer:
                    AdComputer comp = (AdComputer)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if (!String.IsNullOrWhiteSpace(comp.ManagedBy))
                    {
                        if (comp.Properties == null)
                            comp.Properties = new Dictionary<string, List<string>>();

                        if (!comp.Properties.ContainsKey("managedBy"))
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName(comp.ManagedBy);
                            if (distinguishedName == null)
                                distinguishedName = comp.ManagedBy;

                            List<String> values = new List<string>() { distinguishedName };
                            comp.Properties.Add("managedBy", values);
                        }
                    }

                    if (config.UseUpsert && DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Modify, obj.Identity);
                        if (!String.IsNullOrWhiteSpace(comp.Description))
                            DirectoryServices.AddProperty(comp.Properties, "description", comp.Description);
                        DirectoryServices.ModifyComputer(comp.Identity, comp.Properties, isDryRun);
                        statusAction = "Modified";
                    }
                    else if (DirectoryServices.IsDistinguishedName(comp.Identity))
                    {
                        String path = DirectoryServices.GetParentPath(obj.Identity);
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Create, path);
                        if (!String.IsNullOrWhiteSpace(comp.Description))
                            DirectoryServices.AddProperty(comp.Properties, "description", comp.Description);
                        DirectoryServices.CreateComputer(comp.Identity, comp.Properties, isDryRun);
                    }
                    else
                        throw new AdException($"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput);

                    OnLogMessage("ProcessCreate", obj.Type + " [" + obj.Identity + "] " + statusAction + ".");
                    result.Statuses.Add(status);
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if (!String.IsNullOrWhiteSpace(obj.Name))
            {
                DirectoryEntry de = DirectoryServices.Rename(obj.Identity, obj.Name);
                obj.Identity = de.Properties["distinguishedName"].Value.ToString().Replace("LDAP://", "");
            }

            if (returnObject)
                result.Object = GetActiveDirectoryObject(obj);

        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }

        results.Add( result );

    }

    private void ProcessModify(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            string statusAction = "Modified";

            String idOnly = null;
            String domain = DirectoryServices.GetDomain(obj.Identity, out idOnly);

            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    UserPrincipal up = null;
                    if ( config.UseUpsert && !DirectoryServices.IsExistingUser( obj.Identity ) )
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                        {
                            String path = DirectoryServices.GetParentPath( obj.Identity );
                            roleManager.CanPerformActionOrException( requestUser, ActionType.Create, path );
                            up = user.CreateUserPrincipal();
                            statusAction = "Created";
                        }
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For User Creation.", AdStatusType.MissingInput );
                    }
                    else
                    {
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Modify, obj.Identity );
                        up = DirectoryServices.GetUserPrincipal( idOnly, domain );
                        if ( up == null )
                            throw new AdException( $"User [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        user.UpdateUserPrincipal( up );
                    }

                    DirectoryServices.SaveUser( up, isDryRun );

                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Identity + "] " + statusAction + "." );
                    if ( user.MemberOf != null )
                        ProcessGroupAdd( user, false );
                    result.Statuses.Add( status );
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    GroupPrincipal gp = null;
                    if ( config.UseUpsert && !DirectoryServices.IsExistingGroup( idOnly, domain ) )
                    {
                        if ( DirectoryServices.IsDistinguishedName( obj.Identity ) )
                        {
                            String path = DirectoryServices.GetParentPath( obj.Identity );
                            roleManager.CanPerformActionOrException( requestUser, ActionType.Create, path );
                            gp = group.CreateGroupPrincipal();
                            statusAction = "Created";
                        }
                        else
                            throw new AdException( $"Identity [{obj.Identity}] Must Be A Distinguished Name For Group Creation.", AdStatusType.MissingInput );
                    }
                    else
                    {
                        roleManager.CanPerformActionOrException( requestUser, ActionType.Modify, obj.Identity );
                        gp = DirectoryServices.GetGroupPrincipal( idOnly, domain );
                        if ( gp == null )
                            throw new AdException( $"Group [{obj.Identity}] Not Found.", AdStatusType.DoesNotExist );
                        group.UpdateGroupPrincipal( gp );
                    }

                    DirectoryServices.SaveGroup( gp, isDryRun );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Identity + "] " + statusAction + "." );
                    result.Statuses.Add( status );
                    if (group.MemberOf != null)
                        ProcessGroupAdd(group, false);
                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if (!String.IsNullOrWhiteSpace(ou.ManagedBy))
                    {
                        if (ou.Properties == null)
                            ou.Properties = new Dictionary<string, List<string>>();

                        if (!ou.Properties.ContainsKey("managedBy"))
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName(ou.ManagedBy);
                            if (distinguishedName == null)
                                distinguishedName = ou.ManagedBy;

                            List<String> values = new List<string>() { distinguishedName };
                            ou.Properties.Add("managedBy", values);
                        }
                    }

                    if (config.UseUpsert && !DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        if (DirectoryServices.IsDistinguishedName(obj.Identity))
                        {
                            String path = DirectoryServices.GetParentPath(obj.Identity);
                            roleManager.CanPerformActionOrException(requestUser, ActionType.Create, path);
                            if (!String.IsNullOrWhiteSpace(ou.Description))
                                DirectoryServices.AddProperty(ou.Properties, "description", ou.Description);
                            DirectoryServices.CreateOrganizationUnit(obj.Identity, ou.Properties, isDryRun);
                            statusAction = "Created";
                        }
                        else
                            throw new AdException($"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput);
                    }
                    else
                    {
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Modify, obj.Identity);
                        if (!String.IsNullOrWhiteSpace(ou.Description))
                            DirectoryServices.AddProperty(ou.Properties, "description", ou.Description);
                        DirectoryServices.ModifyOrganizationUnit(ou.Identity, ou.Properties, isDryRun);
                    }

                    OnLogMessage("ProcessModify", obj.Type + " [" + obj.Identity + "] " + statusAction + ".");
                    result.Statuses.Add(status);
                    break;
                case AdObjectType.Computer:
                    AdComputer comp = (AdComputer)obj;

                    // Get DistinguishedName from User or Group Identity for ManagedBy Property
                    if (!String.IsNullOrWhiteSpace(comp.ManagedBy))
                    {
                        if (comp.Properties == null)
                            comp.Properties = new Dictionary<string, List<string>>();

                        if (!comp.Properties.ContainsKey("managedBy"))
                        {
                            String distinguishedName = DirectoryServices.GetDistinguishedName(comp.ManagedBy);
                            if (distinguishedName == null)
                                distinguishedName = comp.ManagedBy;

                            List<String> values = new List<string>() { distinguishedName };
                            comp.Properties.Add("managedBy", values);
                        }
                    }

                    if (config.UseUpsert && !DirectoryServices.IsExistingDirectoryEntry(obj.Identity))
                    {
                        if (DirectoryServices.IsDistinguishedName(obj.Identity))
                        {
                            String path = DirectoryServices.GetParentPath(obj.Identity);
                            roleManager.CanPerformActionOrException(requestUser, ActionType.Create, path);
                            if (!String.IsNullOrWhiteSpace(comp.Description))
                                DirectoryServices.AddProperty(comp.Properties, "description", comp.Description);
                            DirectoryServices.CreateComputer(obj.Identity, comp.Properties, isDryRun);
                            statusAction = "Created";
                        }
                        else
                            throw new AdException($"Identity [{obj.Identity}] Must Be A Distinguished Name For Organizational Unit Creation.", AdStatusType.MissingInput);
                    }
                    else
                    {
                        roleManager.CanPerformActionOrException(requestUser, ActionType.Modify, obj.Identity);
                        if (!String.IsNullOrWhiteSpace(comp.Description))
                            DirectoryServices.AddProperty(comp.Properties, "description", comp.Description);
                        DirectoryServices.ModifyComputer(comp.Identity, comp.Properties, isDryRun);
                    }

                    OnLogMessage("ProcessModify", obj.Type + " [" + obj.Identity + "] " + statusAction + ".");
                    result.Statuses.Add(status);
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if (!String.IsNullOrWhiteSpace(obj.Name))
            {
                String newName = null;
                String newDomain = DirectoryServices.GetDomain(obj.Name, out newName);
                DirectoryEntry de = DirectoryServices.Rename(obj.Identity, newName);
                obj.Identity = de.Properties["distinguishedName"].Value.ToString().Replace("LDAP://", "");
            }

            if ( returnObject )
                result.Object = GetActiveDirectoryObject( obj );

        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }

        results.Add( result );

    }

    private void ProcessDelete(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            roleManager.CanPerformActionOrException( requestUser, ActionType.Delete, obj.Identity );
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    DirectoryServices.DeleteUser( user.Identity );
                    result.Statuses.Add( status );
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    DirectoryServices.DeleteGroup( group.Identity, isDryRun );
                    result.Statuses.Add( status );
                    break;
                case AdObjectType.OrganizationalUnit:
                    AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                    DirectoryServices.DeleteOrganizationUnit(ou.Identity);
                    result.Statuses.Add(status);
                    break;
                case AdObjectType.Computer:
                    AdComputer comp = (AdComputer)obj;
                    DirectoryServices.DeleteComputer(comp.Identity);
                    result.Statuses.Add(status);
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            String message = $"{obj.Type} [{obj.Identity}] Deleted.";
            OnLogMessage( "ProcessDelete", message );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }

        results.Add( result );

    }

    private void ProcessGroupAdd(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        AddToGroup( result, obj, returnObject );
        results.Add( result );
    }

    private void ProcessAccessRules(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            roleManager.CanPerformActionOrException( requestUser, config.Action, obj.Identity );

            // Get Target DirectoryEntry For Rules
            DirectoryEntry de = null;
            if ( obj.Type == AdObjectType.User || obj.Type == AdObjectType.Group )
            {
                String id = null;
                String domain = DirectoryServices.GetDomain(obj.Identity, out id);
                Principal principal = DirectoryServices.GetPrincipal( id, domain );
                if ( principal == null )
                    throw new AdException( $"Principal [{obj.Identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
                else if ( principal.GetUnderlyingObjectType() == typeof( DirectoryEntry ) )
                    de = (DirectoryEntry)principal.GetUnderlyingObject();
                else
                    throw new AdException( $"AddAccessRule Not Available For Object Type [{principal.GetUnderlyingObjectType()}]", AdStatusType.NotSupported );
            }
            else
            {
                de = DirectoryServices.GetDirectoryEntry( obj.Identity );
                if ( de == null )
                    throw new AdException( $"DirectoryEntry [{obj.Identity}] Can Not Be Found.", AdStatusType.DoesNotExist );
            }

            // Add Rules To Target DirectoryEntry
            foreach ( AdAccessRule rule in obj.AccessRules )
            {
                String message = String.Empty;
                switch ( config.Action )
                {
                    case ActionType.AddAccessRule:
                        DirectoryServices.AddAccessRule( de, rule.Identity, rule.Rights, rule.Type, rule.Inheritance );
                        message = $"{rule.Type} [{rule.Rights}] Rule Added To {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.RemoveAccessRule:
                        DirectoryServices.DeleteAccessRule( de, rule.Identity, rule.Rights, rule.Type, rule.Inheritance );
                        message = $"{rule.Type} [{rule.Rights}] Rule Deleted From {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.SetAccessRule:
                        DirectoryServices.SetAccessRule( de, rule.Identity, rule.Rights, rule.Type, rule.Inheritance );
                        message = $"{rule.Type} [{rule.Rights}] Rule Set On {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    case ActionType.PurgeAccessRules:
                        DirectoryServices.PurgeAccessRules( de, rule.Identity );
                        message = $"All Rules Purged On {obj.Type} [{obj.Identity}] For Identity [{rule.Identity}].";
                        break;
                    default:
                        throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
                }

                result.Statuses.Add( status );
                OnLogMessage( "ProcessAccessRules", message );
            }

            if ( returnObject )
                result.Object = GetActiveDirectoryObject( obj );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }

        results.Add( result );

    }

    private void ProcessRoles(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            roleManager.CanPerformActionOrException( requestUser, config.Action, obj.Identity );
            foreach (AdRole role in obj.Roles)
            {
                string message = string.Empty;
                switch ( config.Action )
                {
                    case ActionType.AddRole:
                        roleManager.AddRole( role.Principal, role.Name, obj.Identity );
                        message = $"Role [{role.Name}] Has Been Added To {obj.Type} [{obj.Identity}] For Principal [{role.Principal}].";
                        break;
                    case ActionType.RemoveRole:
                        roleManager.RemoveRole( role.Principal, role.Name, obj.Identity );
                        message = $"Role [{role.Name}] Has Been Removed From {obj.Type} [{obj.Identity}] For Principal [{role.Principal}].";
                        break;
                }

                result.Statuses.Add( status );
                OnLogMessage( "ProcessAccessRules", message );
            }

            if ( returnObject )
                result.Object = GetActiveDirectoryObject( obj );

        }
        catch (AdException ade)
        {
            ProcessActiveDirectoryException( result, ade, config.Action );
        }

        results.Add( result );

    }

    private void AddToGroup(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = ActionType.AddToGroup,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    if ( user.MemberOf != null )
                    {
                        foreach ( string userGroup in user.MemberOf )
                        {
                            try
                            {
                                roleManager.CanPerformActionOrException( requestUser, ActionType.AddToGroup, userGroup );
//                                DirectoryServices.AddUserToGroup( user.Identity, userGroup, isDryRun );
                                DirectoryServices.AddToGroup(userGroup, user.Identity, "user", isDryRun);
                                String userMessage = $"{obj.Type} [{user.Identity}] Added To Group [{userGroup}].";
                                OnLogMessage( "ProcessGroupAdd", userMessage );
                                status.Message = userMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch ( AdException ldeUserEx )
                            {
                                ProcessActiveDirectoryException( result, ldeUserEx, status.ActionId );
                            }
                        }
                    }
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    if ( group.MemberOf != null )
                    {
                        foreach ( string groupGroup in group.MemberOf )
                        {
                            try
                            {
                                roleManager.CanPerformActionOrException( requestUser, ActionType.AddToGroup, groupGroup );
//                                DirectoryServices.AddGroupToGroup( group.Identity, groupGroup, isDryRun );
                                DirectoryServices.AddToGroup(groupGroup, group.Identity, "group", isDryRun);
                                String groupMessage = $"{obj.Type} [{group.Identity}] Added To Group [{groupGroup}].";
                                OnLogMessage( "ProcessGroupAdd", groupMessage );
                                status.Message = groupMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch ( AdException ldeGroupEx )
                            {
                                ProcessActiveDirectoryException( result, ldeGroupEx, status.ActionId );
                            }
                        }
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if ( returnObject )
                GetObject( result, obj, true, false );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupAdd", e.Message );
            OnLogMessage( "ProcessGroupAdd", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }

    }

    private void ProcessGroupRemove(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        RemoveFromGroup( result, obj, returnObject );
        results.Add( result );
    }

    private void RemoveFromGroup(ActiveDirectoryObjectResult result, AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = ActionType.RemoveFromGroup,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            switch ( obj.Type )
            {
                case AdObjectType.User:
                    AdUser user = (AdUser)obj;
                    if ( user.MemberOf != null )
                    {
                        foreach ( string userGroup in user.MemberOf )
                        {
                            try
                            {
                                roleManager.CanPerformActionOrException( requestUser, ActionType.RemoveFromGroup, userGroup );
                                //DirectoryServices.RemoveUserFromGroup( user.Identity, userGroup, isDryRun );
                                DirectoryServices.RemoveFromGroup(userGroup, user.Identity, "user", isDryRun);
                                String userMessage = $"{obj.Type} [{user.Identity}] Removed From Group [{userGroup}].";
                                OnLogMessage( "ProcessGroupRemove", userMessage );
                                status.Message = userMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch (AdException ade)
                            {
                                ProcessActiveDirectoryException( result, ade, status.ActionId );
                            }
                        }
                    }
                    break;
                case AdObjectType.Group:
                    AdGroup group = (AdGroup)obj;
                    if ( group.MemberOf != null )
                    {
                        foreach ( string groupGroup in group.MemberOf )
                        {
                            try
                            {
                                roleManager.CanPerformActionOrException( requestUser, ActionType.RemoveFromGroup, groupGroup );
                                //DirectoryServices.RemoveGroupFromGroup( group.Identity, groupGroup, isDryRun );
                                DirectoryServices.RemoveFromGroup(groupGroup, group.Identity, "group", isDryRun);
                                String groupMessage = $"{obj.Type} [{group.Identity}] Removed From Group [{groupGroup}].";
                                OnLogMessage( "ProcessGroupRemove", groupMessage );
                                status.Message = groupMessage;
                                result.Statuses.Add( new ActiveDirectoryStatus( status ) );
                            }
                            catch ( AdException ade )
                            {
                                ProcessActiveDirectoryException( result, ade, status.ActionId );
                            }

                        }
                    }
                    break;
                default:
                    throw new AdException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported );
            }

            if ( returnObject )
                GetObject( result, obj, true, false );
        }
        catch ( AdException ex )
        {
            ProcessActiveDirectoryException( result, ex, status.ActionId );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupRemove", e.Message );
            OnLogMessage( "ProcessGroupRemove", e.StackTrace );
            AdException le = new AdException( e );
            ProcessActiveDirectoryException( result, le, status.ActionId );
        }
    }

    private void ProcessActiveDirectoryException(ActiveDirectoryObjectResult result, AdException ex, ActionType action)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = action,
            StatusId = ex.Type,
            Message = ex.Message,
        };

        OnLogMessage( "Exception", ex.Message );
        result.Statuses.Add( status );
    }

    private void ProcessSearchRequests(IEnumerable<AdSearchRequest> requests, bool dryRun)
    {
        if ( requests != null )
        {
            if ( config.RunSequential )
            {
                foreach ( AdSearchRequest request in requests )
                    ProcessSearchRequest( request, dryRun );
            }
            else
                Parallel.ForEach( requests, request =>
                {
                    ProcessSearchRequest( request, dryRun );
                } );
        }
    }

    private void ProcessSearchRequest(AdSearchRequest request, bool dryRun)
    {
        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = ActionType.Search,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };


        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = AdObjectType.SearchResults,
        };

        try
        {
            string searchBase = request.SearchBase;
            if ( String.IsNullOrWhiteSpace( request.SearchBase ) )
                searchBase = DirectoryServices.GetDomainDistinguishedName();
            roleManager.CanPerformActionOrException( requestUser, ActionType.Search, searchBase );

            String filter = request.Filter;
            if ( request.Parameters != null )
                foreach ( RegexParameters param in request.Parameters )
                    filter = Regex.Replace( filter, param.Find, param.ReplaceWith );

            OnLogMessage( "ProcessSearchRequest", $"Executing Search.  Filter String: [{filter}].  Search Base: [{searchBase}]." );
            if (!dryRun)
            {
                SearchResultsObject searchResults = DirectoryServices.Search(searchBase, filter, request.ReturnAttributes?.ToArray());
                result.Object = searchResults;
            }
            result.Statuses.Add( status );
        } 
        catch (AdException ade)
        {
            ProcessActiveDirectoryException( result, ade, ActionType.Search );
        }

        results.Add( result );
    }

    private string WhoAmI()
    {
        string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

        if ( DirectoryServices.IsExistingUser( startInfo.RequestUser ) )
            user = startInfo.RequestUser;

        // TODO : Remove Domain Check / Removal Once Support For Multiple Domains Is Added.
        if ( user.Contains( @"\" ) )
            user = user.Substring( user.IndexOf(@"\") + 1 );

        return user;
    }

    private void ProcessDryRun(AdObject obj, bool returnObject = false)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            string identity = obj.Identity;
            if (config.Action == ActionType.Create && DirectoryServices.IsDistinguishedName(identity))
                identity = DirectoryServices.GetParentPath(identity);

            roleManager.CanPerformActionOrException(requestUser, config.Action, identity);
            if (returnObject)
            {
                switch (obj.Type)
                {
                    case AdObjectType.User:
                        AdUser user = (AdUser)obj;
                        UserPrincipalObject upo = new UserPrincipalObject();
                        upo.Name = "DryRun Name";
                        upo.DistinguishedName = $"cn=DryRunName,dc=sandbox,dc=local";
                        upo.Guid = Guid.NewGuid();
                        upo.Sid = "S-1-2-34-1234567890-1234567890-1234567890-1234";
                        result.Object = upo;
                        break;
                    case AdObjectType.Group:
                        AdGroup group = (AdGroup)obj;
                        GroupPrincipalObject gpo = new GroupPrincipalObject();
                        gpo.Name = "DryRun Name";
                        gpo.DistinguishedName = $"cn=DryRunName,dc=sandbox,dc=local";
                        gpo.Guid = Guid.NewGuid();
                        gpo.Sid = "S-1-2-34-1234567890-1234567890-1234567890-1234";
                        result.Object = gpo;
                        break;
                    case AdObjectType.OrganizationalUnit:
                        AdOrganizationalUnit ou = (AdOrganizationalUnit)obj;
                        DirectoryEntryObject ouo = new DirectoryEntryObject();
                        ouo.Name = "DryRun Name";
                        ouo.DistinguishedName = $"ou=DryRunName,dc=sandbox,dc=local";
                        ouo.Guid = Guid.NewGuid();
                        result.Object = ouo;
                        break;
                    default:
                        throw new AdException("Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", AdStatusType.NotSupported);
                }
            }

            result.Statuses.Add(status);

        }
        catch (AdException ade)
        {
            ProcessActiveDirectoryException(result, ade, config.Action);
        }

        results.Add(result);

    }

    private void ProcessMove(AdObject obj, bool returnObject = true)
    {
        ActiveDirectoryObjectResult result = new ActiveDirectoryObjectResult()
        {
            TypeId = obj.Type,
            Identity = obj.Identity
        };

        ActiveDirectoryStatus status = new ActiveDirectoryStatus()
        {
            ActionId = config.Action,
            StatusId = AdStatusType.Success,
            Message = "Success",
        };

        try
        {
            DirectoryEntry de = DirectoryServices.Move(obj.Identity, obj.MoveTo);
            OnLogMessage("ProcessMove", $"{obj.Type} [{obj.Identity}] Moved To [{obj.MoveTo}]");

            if (returnObject)
            {
                obj.Identity = de.Properties["distinguishedName"].Value.ToString().Replace("LDAP://", "");
                result.Object = GetActiveDirectoryObject(obj);
            }

            result.Statuses.Add(status);
        }
        catch (AdException ex)
        {
            ProcessActiveDirectoryException(result, ex, status.ActionId);
        }
        catch (Exception e)
        {
            OnLogMessage("ProcessCreate", e.Message);
            OnLogMessage("ProcessCreate", e.StackTrace);
            AdException le = new AdException(e);
            ProcessActiveDirectoryException(result, le, status.ActionId);
        }

        results.Add(result);

    }


}