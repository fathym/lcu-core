using LCU.Graphs.Registry.Enterprises.IDE;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IdeSettingsState
	{
		[DataMember]
		public virtual List<IDEActivity> Activities { get; set; }

		[DataMember]
		public virtual IdeSettingsAddNew AddNew { get; set; }

		[DataMember]
		public virtual IdeSettingsArchitechture Arch { get; set; }

		[DataMember]
		public virtual IdeSettingsConfig Config { get; set; }

		[DataMember]
		public virtual string EditActivity { get; set; }

		[DataMember]
		public virtual string EditSection { get; set; }

		[DataMember]
		public virtual string EditSectionAction { get; set; }

		[DataMember]
		public virtual Dictionary<string, List<string>> LCUSolutionOptions { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }

		[DataMember]
		public virtual List<IdeSettingsSectionAction> SectionActions { get; set; }

		[DataMember]
		public virtual string SideBarEditActivity { get; set; }

		[DataMember]
		public virtual List<string> SideBarSections { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IdeSettingsAddNew
	{
		[DataMember]
		public virtual bool Activity { get; set; }

		[DataMember]
		public virtual bool LCU { get; set; }

		[DataMember]
		public virtual bool SectionAction { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IdeSettingsArchitechture
	{
		[DataMember]
		public virtual string EditLCU { get; set; }

		[DataMember]
		public virtual List<LowCodeUnitConfig> LCUs { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IdeSettingsConfig
	{
		[DataMember]
		public virtual string ConfigLCU { get; set; }

		[DataMember]
		public virtual List<string> Files { get; set; }

		[DataMember]
		public virtual List<string> LCUFiles { get; set; }

		[DataMember]
		public virtual List<IdeSettingsConfigSolution> LCUSolutions { get; set; }

		[DataMember]
		public virtual List<IdeSettingsConfigSolution> Solutions { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IdeSettingsConfigSolution
	{
		[DataMember]
		public virtual string Element { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}

	[Serializable]
	[DataContract]
	public class IdeSettingsSectionAction
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual string Group { get; set; }

		[DataMember]
		public virtual string Name { get; set; }
	}
}
