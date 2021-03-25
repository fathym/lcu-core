using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
	[DataContract]
	public class ApplicationLookupConfiguration
	{
		[DataMember]
		public virtual List<string> AccessRights { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public virtual AllAnyTypes AccessRightsAllAny { get; set; } = AllAnyTypes.Any;

		[DataMember]
		public virtual bool IsPrivate { get; set; }

		[DataMember]
		public virtual bool IsReadOnly { get; set; }

		[DataMember]
		public virtual bool IsTriggerSignIn { get; set; }

		[DataMember]
		public virtual List<string> Licenses { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public virtual AllAnyTypes LicensesAllAny { get; set; } = AllAnyTypes.All;

		[DataMember]
		public virtual string PathRegex { get; set; }

		[DataMember]
		public virtual string QueryRegex { get; set; }

		[DataMember]
		public virtual string UserAgentRegex { get; set; }
	}
}
