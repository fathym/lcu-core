using System;
using System.Runtime.Serialization;

namespace LCU.Presentation.Identity.Models
{
	[Serializable]
	[DataContract]
	public class DeviceAuthorizationInputModel : ConsentInputModel
	{
		[DataMember]
		public string UserCode { get; set; }
	}
}
