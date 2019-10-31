using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	public class ProvisioningGraph : LCUGraph, IProvisioningGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public ProvisioningGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			ListProperties.Add("Hosts");
		}
		#endregion

		#region API Methods
		public virtual async Task<LCUEnvironment> GetEnvironment(string apiKey, string lookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", lookup);

				var result = await SubmitFirst<LCUEnvironment>(query);

				return result;
			}, apiKey);
		}

		public virtual async Task<MetadataModel> GetEnvironmentSettings(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var result = await SubmitFirst<MetadataModel>(query);

				if (result.Metadata.ContainsKey("Registry"))
					result.Metadata.Remove("Registry");

				if (result.Metadata.ContainsKey("EnterpriseAPIKey"))
					result.Metadata.Remove("EnterpriseAPIKey");

				if (result.Metadata.ContainsKey("id"))
					result.Metadata.Remove("id");

				return result;
			}, apiKey);
		}

		public virtual async Task<SourceControl> GetSourceControl(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry);

				var result = await SubmitFirst<SourceControl>(query);

				return result;
			}, apiKey);
		}

		public virtual async Task<List<LCUEnvironment>> ListEnvironments(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<LCUEnvironment>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<LCUEnvironment> SaveEnvironment(LCUEnvironment env)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
						.Has(EntGraphConstants.RegistryName, env.EnterpriseAPIKey)
						.Has(EntGraphConstants.EnterpriseAPIKeyName, env.EnterpriseAPIKey)
						.Has("Lookup", env.Lookup);

				var existingEnvResult = await SubmitFirst<LCUEnvironment>(existingQuery);

				var query = existingEnvResult == null ?
					g.AddV(EntGraphConstants.EnvironmentVertexName)
					.Property(EntGraphConstants.RegistryName, env.EnterpriseAPIKey)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, env.EnterpriseAPIKey) : existingQuery;

				query = query
					.Property("Lookup", env.Lookup ?? "")
					.Property("Name", env.Name ?? "");

				var envResult = await SubmitFirst<LCUEnvironment>(query);

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, env.EnterpriseAPIKey)
					.Has("PrimaryAPIKey", env.EnterpriseAPIKey);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				await ensureEdgeRelationships(g, entResult.ID, envResult.ID);

				return envResult;
			});
		}

		public virtual async Task<MetadataModel> SaveEnvironmentSettings(string apiKey, string envLookup, MetadataModel settings)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", apiKey)
					.Has("EnterpriseAPIKey", apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has("Registry", apiKey)
					.Has("EnterpriseAPIKey", apiKey);

				var existingEnvSetResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var query = existingEnvSetResult == null ?
					g.AddV(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Property("EnterpriseAPIKey", apiKey)
					.Property("Registry", apiKey) : existingQuery;

				settings.Metadata.Each(md =>
				{
					query = query.Property(md.Key, md.Value?.ToString() ?? "");
				});

				var envSetResult = await SubmitFirst<BusinessModel<Guid>>(query);

				var envQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", apiKey)
					.Has("EnterpriseAPIKey", apiKey)
					.Has("Lookup", envLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				await ensureEdgeRelationships(g, envResult.ID, envSetResult.ID);

				return envSetResult;
			}, apiKey);
		}

		public virtual async Task<SourceControl> SaveSourceControl(string apiKey, string envLookup, SourceControl sc)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

				var existingQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, registry);

				var existingSCResult = await SubmitFirst<SourceControl>(existingQuery);

				var query = existingSCResult == null ?
					g.AddV(EntGraphConstants.SourceControlVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

				query = query
					.Property("Name", sc.Name ?? "")
					.Property("Organization", sc.Organization ?? "")
					.Property("Repository", sc.Repository ?? "");

				var scResult = await SubmitFirst<SourceControl>(query);

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
					.Has(EntGraphConstants.RegistryName, apiKey);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				await ensureEdgeRelationships(g, envResult.ID, scResult.ID);

				return scResult;
			}, apiKey);
		}
		#endregion

		#region Helpers
		#endregion
	}
}
