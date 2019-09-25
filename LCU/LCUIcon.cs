using Fathym.Business.Models;
using System;
using System.Runtime.Serialization;

namespace LCU
{
	[DataContract]
	public class LCUIcon
	{
		[DataMember]
		public virtual string Icon { get; set; }

		[DataMember]
		public virtual string IconSet { get; set; }
	}
}
