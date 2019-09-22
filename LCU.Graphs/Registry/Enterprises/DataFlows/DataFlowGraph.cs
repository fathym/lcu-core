using Fathym;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public class DataFlowGraph : LCUGraph, IDataFlowGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public DataFlowGraph(LCUGraphConfig config)
			: base(config)
		{
			ListProperties.Add("Hosts");
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> DeleteDataFlow(string apiKey, string envLookup, string dfLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", dfLookup)
					.Drop();

				await Submit(query);

				return Status.Success;
			});
		}

		public virtual async Task<DataFlow> GetDataFlow(string apiKey, string envLookup, string dfLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", dfLookup);

				var results = await Submit<DataFlow>(query);

				return results.FirstOrDefault();
			});
		}

		public virtual async Task<ModulePackSetup> LoadModulePackSetup(string apiKey, string envLookup,
			string dfLookup, string mdlPckLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				var dfQuery = envQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has("Lookup", dfLookup);

				var dfResult = await SubmitFirst<DataFlow>(dfQuery);

				var mpQuery = envQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModulePackVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", mdlPckLookup);

				var setup = new ModulePackSetup();

				setup.Pack = await SubmitFirst<ModulePack>(mpQuery);

				var moQuery = g.V((setup.Pack.ID, registry))
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModuleOptionVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var moResults = await Submit<ModuleOption>(moQuery);

				setup.Options = moResults.ToList();

				var msQuery = g.V((setup.Pack.ID, registry))
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModuleStyleVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var msResults = await Submit<ModuleStyle>(moQuery);

				setup.Styles = msResults.ToList();

				return new ModulePackSetup()
				{
				};
			});
		}

		public virtual async Task<List<DataFlow>> ListDataFlows(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var results = await Submit<DataFlow>(query);

				return results.ToList();
			});
		}

		public virtual async Task<DataFlow> SaveDataFlow(string apiKey, string envLookup, DataFlow dataFlow)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				var existingQuery = envQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has("Lookup", dataFlow.Lookup);

				var existingSCResult = await SubmitFirst<DataFlow>(existingQuery);

				var query = existingSCResult == null ?
					g.AddV(EntGraphConstants.DataFlowVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

				query = query
					.Property("Name", dataFlow.Name ?? "")
					.Property("Description", dataFlow.Description ?? "")
					.Property("Lookup", dataFlow.Lookup ?? "");

				var dfResult = await SubmitFirst<DataFlow>(query);

				var edgeResult = await SubmitFirst<DataFlow>(g.V(envResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(dfResult.ID));

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(envResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(dfResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(dfResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(dfResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return dfResult;
			});
		}

		public virtual async Task<ModulePack> UnpackModulePack(string apiKey, string envLookup, string dfLookup,
			ModulePackSetup module)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				var dfQuery = envQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has("Lookup", dfLookup);

				var dfResult = await SubmitFirst<DataFlow>(dfQuery);

				var existingQuery = envQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModulePackVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", module.Pack.Lookup);

				var existingResult = await SubmitFirst<ModulePack>(existingQuery);

				var query = existingResult == null ?
					g.AddV(EntGraphConstants.ModulePackVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

				query = query
					.Property("Name", module.Pack.Name ?? "")
					.Property("Description", module.Pack.Description ?? "")
					.Property("Lookup", module.Pack.Lookup ?? "");

				var mpResult = await SubmitFirst<ModulePack>(query);

				await ensureEdgeRelationships(g, dfResult.ID, mpResult.ID);

				//	TODO:  Drop any previous Module Options for the Enterprise and reload new
				var dropOptionsQuery = existingQuery
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModuleOptionVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Drop();

				await module.Options.Each(async option =>
				{
					await unpackModuleOption(g, registry, apiKey, dfResult.ID, mpResult, option);
				});

				await module.Styles.Each(async style =>
				{
					await unpackModuleStyle(g, registry, apiKey, dfResult.ID, mpResult, style);
				});

				return mpResult;
			});
		}
		#endregion

		#region Helpers
		protected virtual async Task<ModuleOption> unpackModuleOption(GraphTraversalSource g, string registry,
			string apiKey, Guid dataFlowId, ModulePack modulePack, ModuleOption option)
		{
			var existingQuery = g.V(modulePack.ID)
				.Out(EntGraphConstants.OwnsEdgeName)
				.HasLabel(EntGraphConstants.ModuleOptionVertexName)
				.Has(EntGraphConstants.RegistryName, registry)
				.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
				.Has("ModuleType", option.ModuleType);

			var existingResult = await SubmitFirst<ModuleOption>(existingQuery);

			var query = existingResult == null ?
				g.AddV(EntGraphConstants.ModuleOptionVertexName)
				.Property(EntGraphConstants.RegistryName, registry)
				.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

			query = query
				.Property("Name", option.Name ?? "")
				.Property("Description", option.Description ?? "")
				.Property("ModuleType", option.ModuleType ?? "");

			var moResult = await SubmitFirst<ModuleOption>(query);

			await ensureEdgeRelationships(g, dataFlowId, moResult.ID);

			return moResult;
		}

		protected virtual async Task<ModuleStyle> unpackModuleStyle(GraphTraversalSource g, string registry,
			string apiKey, Guid dataFlowId, ModulePack modulePack, ModuleStyle style)
		{
			var existingQuery = g.V(modulePack.ID)
				.Out(EntGraphConstants.OwnsEdgeName)
				.HasLabel(EntGraphConstants.ModuleStyleVertexName)
				.Has(EntGraphConstants.RegistryName, registry)
				.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
				.Has("ModuleType", style.ModuleType);

			var existingResult = await SubmitFirst<ModuleStyle>(existingQuery);

			//	In case the Module Style has been updated, only create new, and don't update if it exists
			if (existingResult == null)
			{
				var query = g.AddV(EntGraphConstants.ModuleStyleVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Property("Height", style.Height)
					.Property("ModuleType", style.ModuleType)
					.Property("Shape", style.Shape)
					.Property("Width", style.Width);

				var msResult = await SubmitFirst<ModuleStyle>(query);

				await ensureEdgeRelationships(g, dataFlowId, msResult.ID);

				return msResult;
			}
			else
				return existingResult;
		}
		#endregion
	}
}
