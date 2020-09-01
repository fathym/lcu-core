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

        [DataMember]
        public virtual string Label { get; set; }

        [DataMember]
        public virtual string Registry { get; set; }



        // Migration properties
        [Obsolete]
        [DataMember]
        public virtual string EnterpriseAPIKey { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string PrimaryAPIKey { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string APIRoot { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string InboundPath { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string Methods { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string Security { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string BaseHref { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string DAFApplicationID { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string DAFApplicationRoot { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string NPMPackage { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string PackageVersion { get; set; }

        [Obsolete]
        [DataMember]
        public virtual MetadataModel StateConfig { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string Redirect { get; set; }

        [Obsolete]
        [DataMember]
        public virtual string[] Section { get; set; }
    }
}
