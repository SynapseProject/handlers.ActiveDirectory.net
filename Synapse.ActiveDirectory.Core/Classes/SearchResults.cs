using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Synapse.ActiveDirectory.Core
{
    public class SearchResults
    {
        public List<SearchResultRow> Results { get; set; }
    }

    public class SearchResultRow
    {
        public string Path { get; set; }
        public SerializableDictionary<string, List<string>> Properties { get; set; }
    }
}
