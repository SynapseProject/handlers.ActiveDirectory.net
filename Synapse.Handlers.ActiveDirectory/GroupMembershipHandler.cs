using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Synapse.Core;
using Synapse.ActiveDirectory.Core;
using Synapse.Handlers.ActiveDirectory;

public class GroupMembershipHandler : HandlerRuntimeBase
{
    private GroupMembershipHandlerConfig _config;
    private bool _encounteredFailure = false;
    private int _addSectionCount = 0;
    private int _addGroupCount = 0;
    private int _addUserCount = 0;
    private int _deleteSectionCount = 0;
    private int _deleteGroupCount = 0;
    private int _deleteUserCount = 0;
    private int _sequenceNumber = 0;
    private string _context = "Execute";
    private string _mainProgressMsg = "";
    private readonly ExecuteResult _result = new ExecuteResult()
    {
        Status = StatusType.None,
        BranchStatus = StatusType.None,
        Sequence = 0
    };
    private readonly GroupMembershipResponse _response = new GroupMembershipResponse
    {
        Results = new List<Result>()
    };

    public override object GetConfigInstance()
    {
        return new GroupMembershipHandlerConfig()
        {
            DefaultDomain = "xxx"
        };
    }

    public override object GetParametersInstance()
    {
        return new GroupMembershipRequest()
        {
            AddSection = new List<AddSection>()
            {
                new AddSection()
                {
                    Domain = "xxxxxx",
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                },
                new AddSection()
                {
                    Domain = "xxxxxx",
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                },
                new AddSection()
                {
                    Domain = "xxxxxx",
                    Groups = new List<string>()
                    {
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                }
            },
            DeleteSection = new List<DeleteSection>()
            {
                new DeleteSection()
                {
                    Domain = "xxxxxx",
                    Groups = new List<string>()
                    {
                        "xxxxxx",
                        "xxxxxx",
                        "xxxxxx",
                        "xxxxxx"
                    },
                    Users = new List<string>()
                    {
                        "xxxxxx"
                    }
                }
            }
        };
    }

    public override IHandlerRuntime Initialize(string values)
    {
        //deserialize the Config from the Handler declaration
        _config = DeserializeOrNew<GroupMembershipHandlerConfig>( values );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        string message;
        try
        {
            message = "Deserializing incoming request...";
            UpdateProgress( message, StatusType.Initializing );
            string inputParameters = RemoveParameterSingleQuote(startInfo.Parameters);
            GroupMembershipRequest parms = DeserializeOrNew<GroupMembershipRequest>( inputParameters );

            message = "Processing individual child request...";
            UpdateProgress( message, StatusType.Running );
            ProcessAddRequests( parms, startInfo.IsDryRun );
            ProcessDeleteRequests( parms, startInfo.IsDryRun );

            message = "Request has been processed" + (_encounteredFailure ? " with error" : "") + ".";
            UpdateProgress( message, _encounteredFailure ? StatusType.CompletedWithErrors : StatusType.Success );

            _response.Status = message;
            message = "Serializing response...";
            UpdateProgress( message );
            _result.ExitData = JsonConvert.SerializeObject( _response );
        }
        catch ( Exception ex )
        {
            message = $"Execution has been aborted due to: {ex.Message}";
            UpdateProgress( message, StatusType.Failed, 0 );
        }

        message = startInfo.IsDryRun ? "Dry run execution is completed." : "Execution is completed.";
        UpdateProgress( message, StatusType.Any, 0 );
        return _result;
    }

    private void UpdateProgress(string message, StatusType status = StatusType.Any, int seqNum = -1)
    {
        _mainProgressMsg = _mainProgressMsg + Environment.NewLine + message;
        if ( status != StatusType.Any )
        {
            _result.Status = status;
        }
        if ( seqNum == 0 )
        {
            _sequenceNumber = int.MaxValue;
        }
        else
        {
            _sequenceNumber++;
        }
        OnProgress( _context, _mainProgressMsg, _result.Status, _sequenceNumber );
    }

    private void ProcessAddRequests(GroupMembershipRequest parms, bool isDryRun)
    {
        if ( parms?.AddSection != null )
        {
            foreach ( AddSection addsection in parms.AddSection )
            {
                _addSectionCount++;
                foreach ( string group in addsection.Groups )
                {
                    _addGroupCount++;
                    foreach ( string user in addsection.Users )
                    {
                        _addUserCount++;
                        try
                        {
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                + $"Executing add request [{_addSectionCount}/{_addGroupCount}/{_addUserCount}]"
                                + (isDryRun ? " in dry run mode..." : "...");
                            DirectoryServices.AddUserToGroup( user, group, isDryRun, addsection.Domain );
                            Result r = new Result()
                            {
                                User = user,
                                Group = group,
                                Action = "add",
                                ExitCode = 0,
                                Note = isDryRun ? "Dry run has been completed." : "User has been successfully added to the group."
                            };
                            _response.Results.Add( r );
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                + $"Processed add request [{_addSectionCount}/{_addGroupCount}/{_addUserCount}].";
                        }
                        catch ( Exception ex )
                        {
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                + $"Encountered error while processing add request [{_addSectionCount}/{_addGroupCount}/{_addUserCount}].";
                            Result r = new Result()
                            {
                                User = user,
                                Group = group,
                                Action = "add",
                                ExitCode = -1,
                                Note = (isDryRun ? "Dry run has been completed. " : "") + ex.Message
                            };
                            _response.Results.Add( r );
                            _encounteredFailure = true;
                        }
                    }
                    _addUserCount = 0;
                }
                _addGroupCount = 0;
            }
        }
        else
        {
            _mainProgressMsg = _mainProgressMsg + Environment.NewLine + "No add section is found from the incoming request.";
            ++_sequenceNumber;
            OnProgress( _context, _mainProgressMsg, _result.Status, _sequenceNumber );
        }
    }

    private void ProcessDeleteRequests(GroupMembershipRequest parms, bool isDryRun)
    {
        if ( parms?.DeleteSection != null )
        {
            foreach ( DeleteSection deleteSection in parms.DeleteSection )
            {
                _deleteSectionCount++;
                foreach ( string group in deleteSection.Groups )
                {
                    _deleteGroupCount++;
                    foreach ( string user in deleteSection.Users )
                    {
                        _deleteUserCount++;
                        try
                        {
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                               + $"Executing delete request [{_deleteSectionCount}/{_deleteGroupCount}/{_deleteUserCount}]"
                                               + (isDryRun ? " in dry run mode..." : "...");
                            DirectoryServices.RemoveUserFromGroup( user, group, isDryRun, deleteSection.Domain );
                            Result r = new Result()
                            {
                                User = user,
                                Group = group,
                                Action = "delete",
                                ExitCode = 0,
                                Note = isDryRun ? "Dry run has been completed." : "User has been successfully removed from the group."
                            };
                            _response.Results.Add( r );
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                               + $"Processed delete request [{_deleteSectionCount}/{_deleteGroupCount}/{_deleteUserCount}].";
                        }
                        catch ( Exception ex )
                        {
                            _mainProgressMsg = _mainProgressMsg + Environment.NewLine
                                               + $"Encountered error while processing delete request [{_deleteSectionCount}/{_deleteGroupCount}/{_deleteUserCount}].";
                            Result r = new Result()
                            {
                                User = user,
                                Group = group,
                                Action = "delete",
                                ExitCode = -1,
                                Note = (isDryRun ? "Dry run has been completed. " : "") + ex.Message
                            };
                            _response.Results.Add( r );
                            _encounteredFailure = true;
                        }
                    }
                    _deleteUserCount = 0;
                }
                _deleteGroupCount = 0;
            }
        }
        else
        {
            _mainProgressMsg = _mainProgressMsg + Environment.NewLine + "No delete section is found from the incoming request.";
            OnLogMessage( _context, _mainProgressMsg, LogLevel.Error );
        }
    }

    private static string RemoveParameterSingleQuote(string input)
    {
        string output = "";
        if ( !string.IsNullOrWhiteSpace( input ) )
        {
            Regex pattern = new Regex( "'(\r\n|\r|\n|$)" );
            output = input.Replace( ": '", ": " );
            output = pattern.Replace( output, Environment.NewLine );
        }
        return output;
    }
}

