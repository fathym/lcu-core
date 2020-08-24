﻿using Fathym;
using Fathym.Business.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public class DAFViewApplicationDetails : MetadataModel
    {
        [DataMember]
        public virtual string BaseHref { get; set; }

        [DataMember]
        public virtual string NPMPackage { get; set; }

        [DataMember]
        public virtual string PackageVersion { get; set; }

        [DataMember]
        public virtual MetadataModel StateConfig { get; set; }
    }
}
