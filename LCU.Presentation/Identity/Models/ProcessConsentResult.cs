using System;
using System.Runtime.Serialization;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class ProcessConsentResult
	{
		[DataMember]
		public virtual bool IsRedirect => RedirectUri != null;

		[DataMember]
		public virtual string RedirectUri { get; set; }

		[DataMember]
		public virtual string ClientId { get; set; }

		[DataMember]
		public virtual bool ShowView => ViewModel != null;

		[DataMember]
		public virtual ConsentViewModel ViewModel { get; set; }

		[DataMember]
		public virtual bool HasValidationError => ValidationError != null;

		[DataMember]
		public virtual string ValidationError { get; set; }
	}
}
