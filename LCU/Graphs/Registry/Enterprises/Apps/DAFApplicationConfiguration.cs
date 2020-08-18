using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class DAFApplicationConfiguration : LCUVertex
	{
		[DataMember]
        public virtual string ApplicationID { get; set; }

        [DataMember]
		public virtual string Lookup { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }

	}
}
