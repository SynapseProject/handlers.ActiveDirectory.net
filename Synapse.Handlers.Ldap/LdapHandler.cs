using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices.AccountManagement;

using Synapse.Core;
using Synapse.Ldap.Core;
using Synapse.Handlers.Ldap;

public class LdapHandler : HandlerRuntimeBase
{
    LdapHandlerConfig config = null;
    LdapHandlerResults results = new LdapHandlerResults();
    bool isDryRun = false;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<LdapHandlerConfig>( config );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        int cheapSequence = 0;
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = Int32.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        isDryRun = startInfo.IsDryRun;

        //deserialize the Parameters from the Action declaration
        Synapse.Handlers.Ldap.LdapHandlerParameters parameters = base.DeserializeOrNew<Synapse.Handlers.Ldap.LdapHandlerParameters>( startInfo.Parameters );

        try
        {
            //if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                result.ExitData = config.LdapRoot;
                result.Message = msg =
                    $"Connection test successful! Connection string: {config.LdapRoot}";
            }
            //else, select data as declared in Parameters.QueryString
            else
            {
                switch( config.Action )
                {
                    case ActionType.Query:
                        ProcessLdapObjects( parameters.Users, ProcessQuery );
                        ProcessLdapObjects( parameters.Groups, ProcessQuery );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessQuery );
                        break;
                    case ActionType.Create:
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessCreate );
                        ProcessLdapObjects( parameters.Groups, ProcessCreate );
                        ProcessLdapObjects( parameters.Users, ProcessCreate );
                        break;
                    case ActionType.Modify:
                        // TODO : Implement Me
                        throw new LdapException( "Not Yet Implemented", LdapStatusType.NotSupported );
                    case ActionType.Delete:
                        ProcessLdapObjects( parameters.Users, ProcessDelete );
                        ProcessLdapObjects( parameters.Groups, ProcessDelete );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessDelete );
                        break;
                    case ActionType.AddToGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupAdd );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupAdd );
                        break;
                    case ActionType.RemoveFromGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupRemove );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupRemove );
                        break;
                    default:
                        throw new LdapException( $"Unknown Action {config.Action} Specified", LdapStatusType.NotSupported );
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        if (String.IsNullOrWhiteSpace(result.ExitData?.ToString()))
            result.ExitData = results.Serialize( config.OutputType, config.PrettyPrint );

        if (!config.SuppressOutput)
            OnProgress( __context, result.ExitData?.ToString(), result.Status, sequence: cheapSequence++, ex: exc );

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: Int32.MaxValue, ex: exc );

        return result;
    }

    // TODO : Implement Me
    public override object GetConfigInstance()
    {
        throw new NotImplementedException();
    }

    // TODO : Implement Me
    public override object GetParametersInstance()
    {
        throw new NotImplementedException();
    }

    private void ProcessLdapObjects(IEnumerable<LdapObject> objs, Action<LdapObject, bool> processFunction)
    {
        if ( objs != null )
        {
            if ( config.RunSequential )
            {
                foreach ( LdapObject obj in objs )
                    processFunction( obj, config.ReturnObjects );
            }
            else
                Parallel.ForEach( objs, obj =>
                {
                    processFunction( obj, config.ReturnObjects );
                } );
        }
    }

    private void ProcessQuery(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Name = obj.Name,
            Path = obj.Path
        };

        try
        {
            object ldapObject = GetLdapObject( obj );
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    if ( returnObject )
                        results.Add( status, (UserPrincipalObject)ldapObject );
                    else
                        results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    if ( returnObject )
                        results.Add( status, (GroupPrincipalObject)ldapObject );
                    else
                        results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    if ( returnObject )
                        results.Add( status, (OrganizationalUnitObject)ldapObject );
                    else
                        results.Add( status, (OrganizationalUnitObject)null );

                    break;
                default:
                    throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
            }
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessQuery", e.Message );
            OnLogMessage( "ProcessQuery", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private object GetLdapObject(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                UserPrincipalObject upo = DirectoryServices.GetUser( user.Name, config.QueryGroupMembership );
                return upo;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                GroupPrincipalObject gpo = DirectoryServices.GetGroup( group.Name, config.QueryGroupMembership );
                return gpo;
            case ObjectClass.OrganizationalUnit:
                LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                OrganizationalUnitObject ouo = DirectoryServices.GetOrganizationalUnit( ou.Name, config.LdapRoot );
                return ouo;
            default:
                throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
        }
    }

    private void ProcessCreate(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Name = obj.Name,
            Path = obj.Path
        };

        try
        {
            object ldapObject = null;

            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    DirectoryServices.CreateUser( user.Path, user.Name, user.Password, user.GivenName, user.Surname, user.Description );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( user.Groups != null )
                        ProcessGroupAdd( user, false );
                    if (returnObject)
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (UserPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    DirectoryServices.CreateGroup( group.Path, group.Name, group.Description, group.Scope, group.IsSecurityGroup, isDryRun );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( group.Groups != null )
                        ProcessGroupAdd( group, false );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (GroupPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                    DirectoryServices.CreateOrganizationUnit( ou.Path, ou.Name );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (OrganizationalUnitObject)ldapObject );
                    }
                    else
                        results.Add( status, (OrganizationalUnitObject)null );
                    break;
                default:
                    throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
            }
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessDelete(LdapObject obj, bool returnObject = false)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Name = obj.Name,
            Path = obj.Path
        };

        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    DirectoryServices.DeleteUser( user.Name );
                    results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    DirectoryServices.DeleteGroup( group.Name, isDryRun );
                    results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                    DirectoryServices.DeleteOrganizationUnit( ou.Name );
                    results.Add( status, (OrganizationalUnitObject)null );
                    break;
                default:
                    throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
            }

            OnLogMessage( "ProcessDelete", obj.Type + " [" + obj.Name + "] Deleted." );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessGroupAdd(LdapObject obj, bool returnObject = true)
    {
        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    foreach ( String userGroup in user.Groups )
                    {
                        DirectoryServices.AddUserToGroup( user.Name, userGroup, isDryRun );
                        OnLogMessage( "ProcessGroupAdd", obj.Type + " [" + obj.Name + "] Added To Group [" + userGroup + "]." );
                    }
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    foreach ( String groupGroup in group.Groups )
                    {
                        DirectoryServices.AddGroupToGroup( group.Name, groupGroup, isDryRun );
                        OnLogMessage( "ProcessGroupAdd", obj.Type + " [" + obj.Name + "] Added To Group [" + groupGroup + "]." );
                    }
                    break;
                default:
                    throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
            }

            if ( returnObject )
                ProcessQuery( obj, true );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupAdd", e.Message );
            OnLogMessage( "ProcessGroupAdd", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }

    }

    private void ProcessGroupRemove(LdapObject obj, bool returnObject = true)
    {
        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    foreach ( String userGroup in user.Groups )
                    {
                        DirectoryServices.RemoveUserFromGroup( user.Name, userGroup, isDryRun );
                        OnLogMessage( "ProcessGroupRemove", obj.Type + " [" + obj.Name + "] Removed From Group [" + userGroup + "]." );
                    }
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    foreach ( String groupGroup in group.Groups )
                    {
                        DirectoryServices.RemoveGroupFromGroup( group.Name, groupGroup, isDryRun );
                        OnLogMessage( "ProcessGroupRemove", obj.Type + " [" + obj.Name + "] Removed From Group [" + groupGroup + "]." );
                    }
                    break;
                default:
                    throw new Exception( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]" );
            }

            if ( returnObject )
                ProcessQuery( obj, true );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupRemove", e.Message );
            OnLogMessage( "ProcessGroupRemove", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessLdapException(LdapException ex, ActionType action, LdapObject obj)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = action,
            Status = ex.Type,
            Message = ex.Message,
            Name = obj.Name,
            Path = obj.Path
        };

        switch ( obj.Type )
        {
            case ObjectClass.User:
                results.Add( status, (UserPrincipalObject)null );
                break;
            case ObjectClass.Group:
                results.Add( status, (GroupPrincipalObject)null );
                break;
            case ObjectClass.OrganizationalUnit:
                results.Add( status, (OrganizationalUnitObject)null );
                break;
            default:
                throw ex;
        }

        OnLogMessage( "Exception", ex.Message );
    }

}