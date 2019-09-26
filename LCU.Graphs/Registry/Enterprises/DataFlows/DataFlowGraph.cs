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

				var result = await SubmitFirst<DataFlow>(query);

				return result;
			});
		}

		public virtual async Task<ModulePackSetup> LoadModulePackSetup(string apiKey, string envLookup,
			string dfLookup, string mdlPckLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var mpQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has("Lookup", dfLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModulePackVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", mdlPckLookup);

				var setup = new ModulePackSetup();

				setup.Pack = await SubmitFirst<ModulePack>(mpQuery);

				var mdQuery = g.V((setup.Pack.ID, registry))
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModuleDisplayVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var msResults = await Submit<ModuleDisplay>(mdQuery);

				setup.Displays = msResults.ToList();

				var moQuery = g.V((setup.Pack.ID, registry))
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.ModuleOptionVertexName)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var moResults = await Submit<ModuleOption>(moQuery);

				setup.Options = moResults.ToList();

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

				var existingResult = await SubmitFirst<DataFlow>(existingQuery);

				var query = existingResult == null ?
					g.AddV(EntGraphConstants.DataFlowVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

				query = query
					.Property("Name", dataFlow.Name ?? "")
					.Property("Description", dataFlow.Description ?? "")
					.Property("Lookup", dataFlow.Lookup ?? "")
					.Property("Output", dataFlow.Output ?? new DataFlowOutput()
					{
						Modules = new List<Module>(),
						Streams = new List<ModuleStream>()
					});

				query.SideEffect(__.Properties<string>("ModulePacks").Drop());

				dataFlow.ModulePacks.Each(mp => query = query.Property(Cardinality.List, "ModulePacks", mp));

				var dfResult = await SubmitFirst<DataFlow>(query);

				await ensureEdgeRelationships(g, envResult.ID, dfResult.ID);

				return dfResult;
			});
		}

		public virtual async Task<ModulePack> UnpackModulePack(string apiKey, string envLookup, string dfLookup,
			ModulePackSetup module)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}|DataFlow";

				var dfQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DataFlowVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry)
					.Has("Lookup", dfLookup);

				var dfResult = await SubmitFirst<DataFlow>(dfQuery);

				var existingQuery = dfQuery
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

				await module.Displays.Each(async display =>
				{
					await unpackModuleDisplay(g, registry, apiKey, dfResult.ID, mpResult, display);
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
				.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
				.Property("Active", true)
				.Property("Visible", true) : existingQuery;

			query = query
				.Property("ControlType", option.ControlType)
				.Property("Description", option.Description ?? "")
				.Property("IncomingConnectionLimit", option.IncomingConnectionLimit)
				.Property("ModuleType", option.ModuleType ?? "")
				.Property("Name", option.Name ?? "")
				.Property("OutgoingConnectionLimit", option.OutgoingConnectionLimit);

			query.SideEffect(__.Properties<string>("IncomingConnectionTypes").Drop());

			option.IncomingConnectionTypes.Each(ict =>
				query = query.Property(Cardinality.List, "IncomingConnectionTypes", ict));

			query.SideEffect(__.Properties<string>("OutgoingConnectionTypes").Drop());

			option.OutgoingConnectionTypes.Each(oct =>
				query = query.Property(Cardinality.List, "OutgoingConnectionTypes", oct));

			var moResult = await SubmitFirst<ModuleOption>(query);

			await ensureEdgeRelationships(g, dataFlowId, moResult.ID);

			return moResult;
		}

		protected virtual async Task<ModuleDisplay> unpackModuleDisplay(GraphTraversalSource g, string registry,
			string apiKey, Guid dataFlowId, ModulePack modulePack, ModuleDisplay display)
		{
			var existingQuery = g.V(modulePack.ID)
				.Out(EntGraphConstants.OwnsEdgeName)
				.HasLabel(EntGraphConstants.ModuleDisplayVertexName)
				.Has(EntGraphConstants.RegistryName, registry)
				.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
				.Has("ModuleType", display.ModuleType);

			var existingResult = await SubmitFirst<ModuleDisplay>(existingQuery);

			//	In case the Module Display has been updated, only create new, and don't update if it exists
			if (existingResult == null)
			{
				var query = g.AddV(EntGraphConstants.ModuleDisplayVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Property("Category", display.Category)
					.Property("Element", display.Element)
					.Property("Height", display.Height)
					.Property("Icon", display.Icon)
					.Property("ModuleType", display.ModuleType)
					.Property("Shape", display.Shape)
					.Property("Width", display.Width);

				var msResult = await SubmitFirst<ModuleDisplay>(query);

				await ensureEdgeRelationships(g, dataFlowId, msResult.ID);

				return msResult;
			}
			else
				return existingResult;
		}
		#endregion
	}
}
