using System;

using Newtonsoft.Json;

using Synapse.Core;
using Synapse.Ldap.Core;

public class LdapHandler : HandlerRuntimeBase
{
    ConnectionInfo _ldap = null;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        _ldap = DeserializeOrNew<ConnectionInfo>( config );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        //declare/initialize method-scope variables
        int cheapSequence = 0; //used to order message flowing out from the Handler
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = Int32.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        //deserialize the Parameters from the Action declaration
        SecurityPrincipalQueryParameters parms = DeserializeOrNew<SecurityPrincipalQueryParameters>( startInfo.Parameters );

        try
        {
            //if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                result.ExitData = _ldap.LdapRoot;
                result.Message = msg =
                    $"Connection test successful! Connection string: {_ldap.LdapRoot}";
            }
            //else, select data as declared in Parameters.QueryString
            else
            {

                //populate the Handler result
                //result.ExitData = DirectoryServices.GetObjectDistinguishedName( ObjectClass.User, parms.Name, _ldap.LdapRoot );
                result.ExitData = DirectoryServices.GetUser( parms.Name, parms.IncludeGroups );
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