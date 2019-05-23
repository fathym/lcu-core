using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	[Serializable]
	[DataContract]
	public class LCUAppsState
	{
		[DataMember]
		public virtual Application ActiveApp { get; set; }

		[DataMember]
		public virtual List<DAFApplicationConfiguration> ActiveDAFApps { get; set; }

		[DataMember]
		public virtual string ActiveAppType { get; set; }

		[DataMember]
		public virtual List<Application> Apps { get; set; }

		[DataMember]
		public virtual List<AppPriorityModel> AppPriorities { get; set; }

		[DataMember]
		public virtual string AppsNavState { get; set; }

		[DataMember]
		public virtual List<Application> DefaultApps { get; set; }

		[DataMember]
		public virtual bool DefaultAppsEnabled { get; set; }

		[DataMember]
		public virtual bool IsAppsSettings { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }
	}

	[Serializable]
	[DataContract]
	public class AppPriorityModel
	{
		[DataMember]
		public virtual Guid AppID { get; set; }

		[DataMember]
		public virtual bool IsDefault { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string Path { get; set; }

		[DataMember]
		public virtual int Priority { get; set; }
	}
}
