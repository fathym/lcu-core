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
    public class DAFAppPointerApplicationDetails
    {
        [DataMember]
        public virtual string DAFApplicationID { get; set; }

        [DataMember]
        public virtual string DAFApplicationRoot { get; set; }
    }
}
