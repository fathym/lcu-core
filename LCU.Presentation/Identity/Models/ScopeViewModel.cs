using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class ScopeViewModel
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string DisplayName { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public bool Emphasize { get; set; }

		[DataMember]
		public bool Required { get; set; }

		[DataMember]
		public bool Checked { get; set; }
	}
}
