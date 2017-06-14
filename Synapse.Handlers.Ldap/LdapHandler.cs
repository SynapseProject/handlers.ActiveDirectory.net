using System;

using Newtonsoft.Json;

using Synapse.Core;
using Synapse.Ldap.Core;
using Synapse.Handlers.Ldap;

public class LdapHandler : HandlerRuntimeBase
{
    LdapHanderConfig config = null;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<LdapHanderConfig>( config );
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

        //deserialize the Parameters from the Action declaration
        LdapHanderParameters parms = DeserializeOrNew<LdapHanderParameters>( startInfo.Parameters );

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
                        // TODO : Implement Me
                        break;
                    case ActionType.Create:
                        // TODO : Implement Me
                        break;
                    case ActionType.Modify:
                        // TODO : Implement Me
                        break;
                    case ActionType.Delete:
                        // TODO : Implement Me
                        break;
                    case ActionType.AddToGroup:
                        // TODO : Implement Me
                        break;
                    case ActionType.RemoveFromGroup:
                        // TODO : Implement Me
                        break;
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

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: Int32.MaxValue, ex: exc );

        return result;
    }

    public override object GetConfigInstance()
    {
        return new ConnectionInfo() { LdapRoot = "LDAP://" };
    }

    public override object GetParametersInstance()
    {
        return new SecurityPrincipalQueryParameters() { Type = ObjectClass.Group, Action = ActionType.Create, ReturnFormat = Synapse.Ldap.Core.SerializationFormat.Xml };
    }
}