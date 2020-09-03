using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class DAFApplication : LCUVertex
	{
		[DataMember]
		public virtual string ApplicationID { get; set; }

		[DataMember]
		public virtual MetadataModel Details { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }




        // Migration properties
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
    }
}
