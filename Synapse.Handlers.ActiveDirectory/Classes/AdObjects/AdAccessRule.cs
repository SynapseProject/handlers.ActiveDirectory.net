﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.DirectoryServices;
using System.Security.AccessControl;

using Synapse.ActiveDirectory.Core;

namespace Synapse.Handlers.ActiveDirectory
{
    public class AdAccessRule
    {
        [XmlElement]
        public string Identity { get; set; }
        [XmlElement]
        public AccessControlType Type { get; set; }
        [XmlElement]
        public ActiveDirectoryRights Rights { get; set; }
        [XmlElement]
        public ActiveDirectorySecurityInheritance Inheritance { get; set; } = ActiveDirectorySecurityInheritance.None;
    }
}
