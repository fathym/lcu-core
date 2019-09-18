using Fathym;
using Fathym.Business.Models;
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
		public virtual async Task<DataFlow> GetDataFlow(string apiKey, string envLookup, string dfLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

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

		public virtual async Task<List<DataFlow>> ListDataFlows(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

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

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

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
		#endregion

		#region Helpers
		#endregion
	}
}
