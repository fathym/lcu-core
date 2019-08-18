using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class ConsentViewModel : ConsentInputModel
	{
		[DataMember]
		public string ClientName { get; set; }

		[DataMember]
		public string ClientUrl { get; set; }

		[DataMember]
		public string ClientLogoUrl { get; set; }

		[DataMember]
		public bool AllowRememberConsent { get; set; }

		[DataMember]
		public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

		[DataMember]
		public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
	}
}
