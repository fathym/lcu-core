using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using LCU.Logging;
using Microsoft.Extensions.Logging;
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
		protected readonly ILogger<IDEGraph> logger;
		#endregion

		#region Constructors
		public IDEGraph(LCUGraphConfig config, ILogger<IDEGraph> logger)
			: base(config)
		{
			ListProperties.Add("Hosts");

			this.logger = logger;
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Action", action)
					.Has("Group", group)
					.Has("Section", section)
					.Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, settings.EnterpriseAPIKey)
					.Has(EntGraphConstants.RegistryName, settings.EnterpriseAPIKey);

				var ideResult = await SubmitFirst<IDEContainerSettings>(existingIdeQuery);

				if (ideResult == null)
				{
					var ideQuery = g.AddV(EntGraphConstants.IDEContainerVertexName)
						.Property("Container", settings.Container)
						.Property(EntGraphConstants.RegistryName, settings.EnterpriseAPIKey)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, settings.EnterpriseAPIKey);

					ideResult = await SubmitFirst<IDEContainerSettings>(ideQuery);
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry);

				var result = await SubmitFirst<IDEActivity>(query);

				return result;
			});
		}

		public virtual async Task<LowCodeUnitSetupConfig> GetLCU(string lcuLookup, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var dropActivityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ManagesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has(EntGraphConstants.RegistryName, registry);

				var lcu = await SubmitFirst<LowCodeUnitSetupConfig>(dropActivityQuery);

				return lcu;
			});
		}

		public virtual async Task<IdeSettingsConfigSolution> GetLCUSolution(string lcuLookup, string solution, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Values<string>("Solutions");

				var results = await Submit<string>(query);

				var slnCfgs = results?.FirstOrDefault()?.FromJSON<List<IdeSettingsConfigSolution>>();

				return slnCfgs.FirstOrDefault(sc => sc.Name == solution);
			});
		}

		public virtual async Task<IDESideBarAction> GetSectionAction(string activityLookup, string section, string action, string group, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Section", section)
					.Has("Action", action)
					.Has("Group", group)
					.Has(EntGraphConstants.RegistryName, registry);

				var result = await SubmitFirst<IDESideBarAction>(query);

				return result;
			});
		}

		public virtual async Task<List<IDEActivity>> ListActivities(string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName);

				var results = await Submit<IDEActivity>(query);

				return results?.ToList();
			});
		}

		public virtual async Task<List<string>> ListLCUFiles(string lcuLookup, string host)
		{
			var client = new HttpClient(new LoggingHandler(new HttpClientHandler(), logger));

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
			//         .Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
			//         .Has(EntGraphConstants.RegistryName, entApiKey)
			//         .Out(IDEGraphConstants.ConsumesEdgeName)
			//         .HasLabel(IDEGraphConstants.LCUConfigVertexName)
			//         .Has("Lookup", lcuLookup)
			//         .Has(EntGraphConstants.RegistryName, registry)
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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Values<string>("Solutions");

				var results = await Submit<string>(query);

				return results?.FirstOrDefault()?.FromJSON<List<IdeSettingsConfigSolution>>();
			});
		}

		public virtual async Task<List<LowCodeUnitSetupConfig>> ListLCUs(string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName);

				var results = await Submit<LowCodeUnitSetupConfig>(query);

				return results?.ToList();
			});
		}

		public virtual async Task<List<IDESideBarAction>> ListSectionActions(string activityLookup, string section, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var query = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.SectionActionVertexName)
					.Has("Section", section)
					.Has(EntGraphConstants.RegistryName, registry);

				var results = await Submit<IDESideBarAction>(query);

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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Values<string>("Section")
					.Dedup();

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
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey);

				var ideResult = await SubmitFirst<IDEContainerSettings>(ideQuery);

				var registry = $"{entApiKey}|{container}";

				var existingActivityQuery = g.V(ideResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.ActivityVertexName)
						.Has("Lookup", activity.Lookup)
						.Has(EntGraphConstants.RegistryName, registry);

				var existingActivityResult = await SubmitFirst<BusinessModel<Guid>>(existingActivityQuery);

				var saveQuery = existingActivityResult != null ? g.V(existingActivityResult.ID) :
					g.AddV(EntGraphConstants.ActivityVertexName)
						.Property("Lookup", activity.Lookup)
						.Property(EntGraphConstants.RegistryName, registry)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

				saveQuery = saveQuery
					.Property("Title", activity.Title)
					.Property("Icon", activity.Icon)
					.Property("IconSet", activity.IconSet ?? "");

				var activityResult = await SubmitFirst<BusinessModel<Guid>>(saveQuery);

				await ensureEdgeRelationships(g, ideResult.ID, activityResult.ID);

				return activityResult.JSONConvert<IDEActivity>();
			});
		}

		public virtual async Task<LowCodeUnitSetupConfig> SaveLCU(LowCodeUnitSetupConfig lcu, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var ideQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey);

				var ideResult = await SubmitFirst<IDEContainerSettings>(ideQuery);

				var registry = $"{entApiKey}|{container}";

				var existingLCUQuery = g.V(ideResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.LCUConfigVertexName)
						.Has("Lookup", lcu.Lookup)
						.Has(EntGraphConstants.RegistryName, registry)
						.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

				var existingLCUResult = await SubmitFirst<BusinessModel<Guid>>(existingLCUQuery);

				var saveQuery = existingLCUResult != null ? g.V(existingLCUResult.ID) :
					g.AddV(EntGraphConstants.LCUConfigVertexName)
						.Property("Lookup", lcu.Lookup)
						.Property(EntGraphConstants.RegistryName, registry)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

				saveQuery = saveQuery
					.Property("NPMPackage", lcu.NPMPackage)
					.Property("PackageVersion", lcu.PackageVersion);

				var lcuResult = await SubmitFirst<BusinessModel<Guid>>(saveQuery);

				await ensureEdgeRelationships(g, ideResult.ID, lcuResult.ID);

				return lcuResult.JSONConvert<LowCodeUnitSetupConfig>();
			});
		}

		public virtual async Task<Status> SaveLCUCapabilities(string lcuLookup, List<string> files, List<IdeSettingsConfigSolution> solutions, string entApiKey, string container)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var saveQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.LCUConfigVertexName)
					.Has("Lookup", lcuLookup)
					.Has(EntGraphConstants.RegistryName, registry)
					.Property("CapabilityFiles", files)
					.Property("Solutions", solutions);

				var lcuResult = await SubmitFirst<BusinessModel<Guid>>(saveQuery);

				return Status.Success;
			});
		}

		public virtual async Task<IDESideBarAction> SaveSectionAction(string activityLookup, IDESideBarAction action, string entApiKey, string container)
		{
			return await base.withG((async (client, g) =>
			{
				var registry = $"{entApiKey}|{container}";

				var activityQuery = g.V().HasLabel(EntGraphConstants.IDEContainerVertexName)
					.Has("Container", container)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entApiKey)
					.Has(EntGraphConstants.RegistryName, entApiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.ActivityVertexName)
					.Has("Lookup", activityLookup)
					.Has(EntGraphConstants.RegistryName, registry);

				var activityResult = await SubmitFirst<BusinessModel<Guid>>(activityQuery);

				var existingSecActQuery = g.V(activityResult.ID)
						.Out(EntGraphConstants.ConsumesEdgeName)
						.HasLabel(EntGraphConstants.SectionActionVertexName)
						.Has("Action", action.Action)
						.Has("Group", action.Group)
						.Has("Section", action.Section)
						.Has(EntGraphConstants.RegistryName, registry);

				var existingSecActResult = await SubmitFirst<BusinessModel<Guid>>(existingSecActQuery);

				var saveQuery = existingSecActResult != null ? g.V(existingSecActResult.ID) :
					g.AddV(EntGraphConstants.SectionActionVertexName)
						.Property("Action", (object)action.Action)
						.Property("Group", (object)action.Group)
						.Property("Section", action.Section)
						.Property(EntGraphConstants.RegistryName, registry)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, entApiKey);

				saveQuery = saveQuery
					.Property("Title", action.Title);

				var secActResult = await SubmitFirst<BusinessModel<Guid>>(saveQuery);

				await ensureEdgeRelationships(g, activityResult.ID, secActResult.ID);

				return secActResult.JSONConvert<IDESideBarAction>();
			}));
		}
		#endregion

		#region Helpers
		#endregion
	}
}
