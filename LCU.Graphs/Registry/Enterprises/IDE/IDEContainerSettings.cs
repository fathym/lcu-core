using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEContainerSettings : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Container { get; set; }

		[DataMember]
		public virtual string EnterpriseAPIKey { get; set; }
	}
}
