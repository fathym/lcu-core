using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
