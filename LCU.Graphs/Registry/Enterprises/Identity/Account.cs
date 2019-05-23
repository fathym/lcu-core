using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Identity
{
	[Serializable]
    [DataContract]
	public class Account : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Email { get; set; }
	}
}
