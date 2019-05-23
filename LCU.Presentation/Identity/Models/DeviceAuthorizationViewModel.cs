using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class DeviceAuthorizationViewModel : ConsentViewModel
	{
		[DataMember]
		public string UserCode { get; set; }

		[DataMember]
		public bool ConfirmUserCode { get; set; }
	}
}
