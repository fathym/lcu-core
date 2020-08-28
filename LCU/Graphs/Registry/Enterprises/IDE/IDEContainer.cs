using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEContainer : LCUVertex  //BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Container { get; set; }
	}
}
