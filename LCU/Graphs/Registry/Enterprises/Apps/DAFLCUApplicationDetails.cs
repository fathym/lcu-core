using Fathym;
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
    public class DAFLCUApplicationDetails : MetadataModel
    {
        [DataMember]
        public virtual string Lookup { get; set; }

        [DataMember]
        public virtual string NPMPackage { get; set; }

        [DataMember]
        public virtual string PackageVersion { get; set; }
    }
}
