﻿using ExRam.Gremlinq.Core.GraphElements;
using Fathym;
using Fathym.Business.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs
{
    [DataContract]
    public class LCUVertex //: MetadataModel// BusinessModel<Guid>
    {
        [DataMember]
        public virtual string EnterpriseLookup { get; set; }

        [DataMember]
        public virtual Guid ID { get; set; }

        [DataMember]
        public virtual string Label { get; set; }

        //[DataMember]
        //public virtual string PartitionKey { get; set; }

        [DataMember]
        public virtual string Registry { get; set; }
    }
}
