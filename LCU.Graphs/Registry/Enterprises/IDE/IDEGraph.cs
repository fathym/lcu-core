using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	public class IDEGraph : LCUGraph, IIDEGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public IDEGraph(LCUGraphConfig config)
			: base(config)
		{
			ListProperties.Add("Hosts");
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> AddSideBarSection(string activityLookup, string section, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Property(Cardinality.List, "Section", section);

				await Submit(query);

				return Status.Success;
			});
		}

		public virtual async Task<Status> DeleteActivity(string activityLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var dropActivityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Drop();

				await Submit(dropActivityQuery);

				return Status.Success;
			});
		}

		public virtual async Task<Status> DeleteLCU(string lcuLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var dropActivityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has("Registry", registry)
					.Drop();

				await Submit(dropActivityQuery);

				return Status.Success;
			});
		}

		public virtual async Task<Status> DeleteSectionAction(string activityLookup, string section, string action, string group, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var dropActivityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Action", action)
					.Has("Group", group)
					.Has("Section", section)
					.Has("Registry", registry)
					.Drop();

				await Submit(dropActivityQuery);

				return Status.Success;
			});
		}

		public virtual async Task<Status> DeleteSideBarSection(string activityLookup, string section, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Properties<string>("Section")
					.HasValue(section)
					.Drop();

				await Submit(query);

				return Status.Success;
			});
		}

		public virtual async Task<IDEContainerSettings> EnsureIDESettings(IDEContainerSettings settings)
		{
			return await withG(async (client, g) =>
			{
				var existingIdeQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", settings.Container)
					.Has("EnterprisePrimaryAPIKey", settings.EnterprisePrimaryAPIKey)
					.Has("Registry", settings.EnterprisePrimaryAPIKey);

				var existingIdeResults = await Submit<IDEContainerSettings>(existingIdeQuery);

				var ideResult = existingIdeResults.FirstOrDefault();

				if (ideResult == null)
				{
					var ideQuery = g.AddV(EntGraphConstants.IDEContainerVertexName)
						.Property("Container", settings.Container)
						.Property("EnterprisePrimaryAPIKey", settings.EnterprisePrimaryAPIKey)
						.Property("Registry", settings.EnterprisePrimaryAPIKey);

					var newIdeResults = await Submit<IDEContainerSettings>(ideQuery);

					ideResult = newIdeResults.FirstOrDefault();
				}

				return ideResult;
			});
		}

		public virtual async Task<IDEActivity> GetActivity(string activityLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry);

				var results = await Submit<IDEActivity>(query);

				return results?.FirstOrDefault();
			});
		}

		public virtual async Task<LowCodeUnitConfig> GetLCU(string lcuLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var dropActivityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has("Registry", registry);

				var lcus = await Submit<LowCodeUnitConfig>(dropActivityQuery);

				return lcus.FirstOrDefault();
			});
		}

		public virtual async Task<IdeSettingsConfigSolution> GetLCUSolution(string lcuLookup, string solution, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has("Registry", registry)
					.Values<string>("Solutions");

				var results = await Submit<string>(query);

				var slnCfgs = results?.FirstOrDefault()?.FromJSON<List<IdeSettingsConfigSolution>>();

				return slnCfgs.FirstOrDefault(sc => sc.Name == solution);
			});
		}

		public virtual async Task<IdeSettingsSectionAction> GetSectionAction(string activityLookup, string section, string action, string group, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Section", section)
					.Has("Action", action)
					.Has("Group", group)
					.Has("Registry", registry);

				var results = await Submit<IdeSettingsSectionAction>(query);

				return results?.FirstOrDefault();
			});
		}

		public virtual async Task<List<IDEActivity>> ListActivities(string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName);

				var results = await Submit<IDEActivity>(query);

				return results?.ToList();
			});
		}

		public virtual async Task<List<string>> ListLCUFiles(string lcuLookup, string host)
		{
			var client = new HttpClient();

			//	TODO: Support HTTPs only once supported
			client.BaseAddress = new Uri($"https://{host}");

			var lcuConfigResp = await client.GetAsync($"/_lcu/{lcuLookup}/lcu.json");

			var lcuConfigStr = await lcuConfigResp.Content.ReadAsStringAsync();

			if (lcuConfigResp.IsSuccessStatusCode && !lcuConfigStr.IsNullOrEmpty() && !lcuConfigStr.StartsWith("<"))
			{
				var lcuConfig = lcuConfigStr.FromJSON<dynamic>();

				var slnsDict = ((JToken)lcuConfig.config.solutions).ToObject<Dictionary<string, dynamic>>();

				return ((JToken)lcuConfig.config.wc).ToObject<List<string>>();
			}

			return new List<string>();

			// return await withG(async (client, g) =>
			// {
			//     var registry = $"{entApiKey}|{container}";

			//     var query = g.V().HasLabel(IDEGraphConstants.IDEContainerVertexName)
			//         .Has("Container", container)
			//         .Has("EnterprisePrimaryAPIKey", entApiKey)
			//         .Has("Registry", entApiKey)
			//         .Out(IDEGraphConstants.ConsumesEdgeName)
			//         .HasLabel(IDEGraphConstants.LCUConfigVertexName)
			//         .Has("Lookup", lcuLookup)
			//         .Has("Registry", registry)
			//         .Values<string>("CapabilityFiles");

			//     var results = await Submit<string>(query);

			//     return results?.FirstOrDefault()?.FromJSON<List<string>>();
			// });
		}

		public virtual async Task<List<IdeSettingsConfigSolution>> ListLCUSolutions(string lcuLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has("Registry", registry)
					.Values<string>("Solutions");

				var results = await Submit<string>(query);

				return results?.FirstOrDefault()?.FromJSON<List<IdeSettingsConfigSolution>>();
			});
		}

		public virtual async Task<List<LowCodeUnitConfig>> ListLCUs(string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName);

				var results = await Submit<LowCodeUnitConfig>(query);

				return results?.ToList();
			});
		}

		public virtual async Task<List<IdeSettingsSectionAction>> ListSectionActions(string activityLookup, string section, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Section", section)
					.Has("Registry", registry);

				var results = await Submit<IdeSettingsSectionAction>(query);

				return results?.ToList();
			});
		}

		public virtual async Task<List<string>> ListSideBarSections(string activityLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry)
					.Values<string>("Section");

				var results = await Submit<string>(query);

				return results.ToList();

				// var results = await Submit<BusinessModel<Guid>>(query);

				// var result = results.FirstOrDefault();

				// var sections = result?.Metadata?["Section"];

				// return sections is JArray ? sections.ToObject<List<string>>() : sections != null ? new List<string>() { sections.ToObject<string>() } : null;
			});
		}

		public virtual async Task<IDEActivity> SaveActivity(IDEActivity activity, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var ideQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey);

				var ideResults = await Submit<IDEContainerSettings>(ideQuery);

				var ideResult = ideResults.FirstOrDefault();

				var registry = $"{entApiKey}|{container}";

				var existingActivityQuery = g.V(ideResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.ActivityVertexName)
						.Has("Lookup", activity.Lookup)
						.Has("Registry", registry);

				var existingActivityResults = await Submit<BusinessModel<Guid>>(existingActivityQuery);

				var existingActivityResult = existingActivityResults.FirstOrDefault();

				var saveQuery = existingActivityResult != null ? g.V(existingActivityResult.ID) :
					g.AddV(EntGraphConstants.ActivityVertexName)
						.Property("Lookup", activity.Lookup)
						.Property("Registry", registry);

				saveQuery = saveQuery
					.Property("Title", activity.Title)
					.Property("Icon", activity.Icon)
					.Property("IconSet", activity.IconSet ?? "");

				var activityResults = await Submit<BusinessModel<Guid>>(saveQuery);

				var activityResult = activityResults.FirstOrDefault();

				if (existingActivityResult == null)
				{
					var edgeQueries = new[] {
						g.V(ideResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(activityResult.ID)),
						g.V(ideResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(activityResult.ID)),
						g.V(ideResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(activityResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return activityResult.JSONConvert<IDEActivity>();
			});
		}

		public virtual async Task<LowCodeUnitConfig> SaveLCU(LowCodeUnitConfig lcu, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var ideQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey);

				var ideResults = await Submit<IDEContainerSettings>(ideQuery);

				var ideResult = ideResults.FirstOrDefault();

				var registry = $"{entApiKey}|{container}";

				var existingLCUQuery = g.V(ideResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.LCUConfigVertexName)
						.Has("Lookup", lcu.Lookup)
						.Has("Registry", registry)
						.Has("EnterprisePrimaryAPIKey", entApiKey);

				var existingLCUResults = await Submit<BusinessModel<Guid>>(existingLCUQuery);

				var existingLCUResult = existingLCUResults.FirstOrDefault();

				var saveQuery = existingLCUResult != null ? g.V(existingLCUResult.ID) :
					g.AddV(EntGraphConstants.LCUConfigVertexName)
						.Property("Lookup", lcu.Lookup)
						.Property("Registry", registry)
						.Property("EnterprisePrimaryAPIKey", entApiKey);

				saveQuery = saveQuery
					.Property("NPMPackage", lcu.NPMPackage)
					.Property("PackageVersion", lcu.PackageVersion);

				var lcuResults = await Submit<BusinessModel<Guid>>(saveQuery);

				var lcuResult = lcuResults.FirstOrDefault();

				if (existingLCUResult == null)
				{
					var edgeQueries = new[] {
						g.V(ideResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(lcuResult.ID)),
						g.V(ideResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(lcuResult.ID)),
						g.V(ideResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(lcuResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return lcuResult.JSONConvert<LowCodeUnitConfig>();
			});
		}

		public virtual async Task<Status> SaveLCUCapabilities(string lcuLookup, List<string> files, List<IdeSettingsConfigSolution> solutions, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var saveQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has("Registry", registry)
					.Property("CapabilityFiles", files)
					.Property("Solutions", solutions);

				var lcuResults = await Submit<BusinessModel<Guid>>(saveQuery);

				var lcuResult = lcuResults.FirstOrDefault();

				return Status.Success;
			});
		}

		public virtual async Task<IdeSettingsSectionAction> SaveSectionAction(string activityLookup, string section, IdeSettingsSectionAction action, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var activityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has("EnterprisePrimaryAPIKey", entApiKey)
					.Has("Registry", entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has("Registry", registry);

				var activityResults = await Submit<BusinessModel<Guid>>(activityQuery);

				var activityResult = activityResults.FirstOrDefault();

				var existingSecActQuery = g.V(activityResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.SectionActionVertexName)
						.Has("Action", action.Action)
						.Has("Group", action.Group)
						.Has("Section", section)
						.Has("Registry", registry);

				var existingSecActResults = await Submit<BusinessModel<Guid>>(existingSecActQuery);

				var existingSecActResult = existingSecActResults.FirstOrDefault();

				var saveQuery = existingSecActResult != null ? g.V(existingSecActResult.ID) :
					g.AddV(EntGraphConstants.SectionActionVertexName)
						.Property("Action", action.Action)
						.Property("Group", action.Group)
						.Property("Section", section)
						.Property("Registry", registry);

				saveQuery = saveQuery
					.Property("Name", action.Name);

				var secActResults = await Submit<BusinessModel<Guid>>(saveQuery);

				var secActResult = secActResults.FirstOrDefault();

				if (existingSecActResult == null)
				{
					var edgeQueries = new[] {
						g.V(activityResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(secActResult.ID)),
						g.V(activityResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(secActResult.ID)),
						g.V(activityResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(secActResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return secActResult.JSONConvert<IdeSettingsSectionAction>();
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
