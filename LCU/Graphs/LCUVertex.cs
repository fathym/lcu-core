using ExRam.Gremlinq.Core.GraphElements;
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

        //[DataMember]
        ////[JsonExtensionData]
        //public new virtual IDictionary<string, JToken> Metadata { get; set; }

        //[DataMember(Name = "Metadata")]
        ////[JsonIgnore]
        //public new virtual string MetadataProxy
        //{
        //    get
        //    {
        //        return Metadata.ToJSON();
        //    }

        //    set
        //    {
        //        Metadata = value.FromJSON<IDictionary<string, JToken>>();
        //    }
        //}

        [DataMember]
        public virtual string Label { get; set; }

        [DataMember]
        public virtual string Registry { get; set; }
    }
}
