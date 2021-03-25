using LCU.Graphs.Registry.Enterprises;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace LCU.Presentation.Enterprises
{
    [Serializable]
	public class ApplicationLookupConfiguration
	{
		public virtual List<string> AccessRights { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public virtual AllAnyTypes AccessRightsAllAny { get; set; } = AllAnyTypes.Any;

		public virtual bool IsPrivate{ get; set; }

		public virtual bool IsReadOnly { get; set; }

		public virtual bool IsTriggerSignIn { get; set; }

		public virtual List<string> Licenses { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public virtual AllAnyTypes LicensesAllAny { get; set; } = AllAnyTypes.All;

		public virtual string PathRegex { get; set; }

		public virtual string QueryRegex { get; set; }

		public virtual string UserAgentRegex { get; set; }
	}
}
