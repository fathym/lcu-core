using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises
{
	[DataContract]
	public class EnterpriseRegistration : LCUVertex
	{
		[DataMember]
		public virtual string[] Hosts { get; set; }
	}
}
