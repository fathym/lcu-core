using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class ConsentInputModel
	{
		[DataMember]
		public string Button { get; set; }

		[DataMember]
		public IEnumerable<string> ScopesConsented { get; set; }

		[DataMember]
		public bool RememberConsent { get; set; }

		[DataMember]
		public string ReturnURL { get; set; }
	}
}
