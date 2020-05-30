using Fathym;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	public interface IIDEGraph
	{
		Task<Status> AddSideBarSection(string activityLookup, string section, string entLookup, string container);

		Task<Status> DeleteActivity(string activityLookup, string entLookup, string container);

		Task<Status> DeleteLCU(string lcuLookup, string entLookup, string container);

		Task<Status> DeleteSectionAction(string activityLookup, string section, string action, string group, string entLookup, string container);

		Task<Status> DeleteSideBarSection(string activityLookup, string section, string entLookup, string container);

		Task<IDEContainerSettings> EnsureIDESettings(IDEContainerSettings settings);

		Task<IDEActivity> GetActivity(string activityLookup, string entLookup, string container);

		Task<LowCodeUnitSetupConfig> GetLCU(string lcuLookup, string entLookup, string container);

		Task<IdeSettingsConfigSolution> GetLCUSolution(string lcuLookup, string solution, string entLookup, string container);

		Task<ModulePackSetup> GetModulePackSetup(string lcuLookup, string entLookup, string container);

		Task<IDESideBarAction> GetSectionAction(string activityLookup, string section, string action, string group, string entLookup, string container);

		Task<List<IDEActivity>> ListActivities(string entLookup, string container);

		Task<List<ModulePackSetup>> ListModulePackSetups(string entLookup, string container);

		Task<List<LowCodeUnitSetupConfig>> ListLCUs(string entLookup, string container);

		Task<List<IdeSettingsConfigSolution>> ListLCUSolutions(string lcuLookup, string entLookup, string container);

		Task<List<IDESideBarAction>> ListSectionActions(string activityLookup, string section, string entLookup, string container);

		Task<List<string>> ListSideBarSections(string activityLookup, string entLookup, string container);

		Task<IDEActivity> SaveActivity(IDEActivity activity, string entLookup, string container);

		Task<LowCodeUnitSetupConfig> SaveLCU(LowCodeUnitSetupConfig lcu, string entLookup, string container);

		Task<Status> SaveLCUCapabilities(string lcuLookup, List<string> files, List<IdeSettingsConfigSolution> solutions, ModulePackSetup modules, 
			string entLookup, string container);

		Task<IDESideBarAction> SaveSectionAction(string activityLookup, IDESideBarAction action, string entLookup, string container);
	}
}