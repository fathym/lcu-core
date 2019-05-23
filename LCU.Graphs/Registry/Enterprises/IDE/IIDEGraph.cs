﻿using Fathym;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	public interface IIDEGraph
	{
		Task<Status> AddSideBarSection(string activityLookup, string section, string entApiKey, string container);

		Task<Status> DeleteActivity(string activityLookup, string entApiKey, string container);

		Task<Status> DeleteLCU(string lcuLookup, string entApiKey, string container);

		Task<Status> DeleteSectionAction(string activityLookup, string section, string action, string group, string entApiKey, string container);

		Task<Status> DeleteSideBarSection(string activityLookup, string section, string entApiKey, string container);

		Task<IDEContainerSettings> EnsureIDESettings(IDEContainerSettings settings);

		Task<IDEActivity> GetActivity(string activityLookup, string entApiKey, string container);

		Task<LowCodeUnitConfig> GetLCU(string lcuLookup, string entApiKey, string container);

		Task<IdeSettingsConfigSolution> GetLCUSolution(string lcuLookup, string solution, string entApiKey, string container);

		Task<IdeSettingsSectionAction> GetSectionAction(string activityLookup, string section, string action, string group, string entApiKey, string container);

		Task<List<IDEActivity>> ListActivities(string entApiKey, string container);

		Task<List<string>> ListLCUFiles(string lcuLookup, string host);

		Task<List<LowCodeUnitConfig>> ListLCUs(string entApiKey, string container);

		Task<List<IdeSettingsConfigSolution>> ListLCUSolutions(string lcuLookup, string entApiKey, string container);

		Task<List<IdeSettingsSectionAction>> ListSectionActions(string activityLookup, string section, string entApiKey, string container);

		Task<List<string>> ListSideBarSections(string activityLookup, string entApiKey, string container);

		Task<IDEActivity> SaveActivity(IDEActivity activity, string entApiKey, string container);

		Task<LowCodeUnitConfig> SaveLCU(LowCodeUnitConfig lcu, string entApiKey, string container);

		Task<Status> SaveLCUCapabilities(string lcuLookup, List<string> files, List<IdeSettingsConfigSolution> solutions, string entApiKey, string container);

		Task<IdeSettingsSectionAction> SaveSectionAction(string activityLookup, string section, IdeSettingsSectionAction action, string entApiKey, string container);
	}
}