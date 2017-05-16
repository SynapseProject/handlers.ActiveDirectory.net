using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Synapse.Core;


public class LdapHandler : HandlerRuntimeBase
{
    ConnectionInfo _dsn = null;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        _dsn = DeserializeOrNew<ConnectionInfo>( config );
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
        LdapHandlerParameters parms = DeserializeOrNew<LdapHandlerParameters>( startInfo.Parameters );

        using( OdbcConnection connection = new OdbcConnection( _dsn.ConnectionString ) )
        {
            try
            {
                //if IsDryRun == true, test if ConnectionString is valid and works.
                if( startInfo.IsDryRun )
                {
                    OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                    result.ExitData = connection.State;
                    result.Message = msg =
                        $"Connection test successful! Connection string: {_dsn.ConnectionString}";
                }
                //else, select data as declared in Parameters.QueryString
                else
                {

                    //populate the Handler result
                    result.ExitData = "data";
                }
            }
            //something wnet wrong: hand-back the Exception and mark the execution as Failed
            catch( Exception ex )
            {
                exc = ex;
                result.Status = StatusType.Failed;
                result.ExitData = msg =
                    ex.Message;
            }
            finally
            {
                connection.Close();
            }

            //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
            OnProgress( __context, msg, result.Status, sequence: Int32.MaxValue, ex: exc );

            return result;
        }
    }

    public override object GetConfigInstance()
    {
        throw new NotImplementedException();
    }

    public override object GetParametersInstance()
    {
        throw new NotImplementedException();
    }
}

public class ConnectionInfo
{
    public string ConnectionString { get; set; }
}

public enum SerializationFormat
{
    Json,
    Xml
}

public class LdapHandlerParameters
{
    public string QueryString { get; set; }
    public SerializationFormat ReturnFormat { get; set; }
}