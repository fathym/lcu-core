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
		public ProvisioningGraph(LCUGraphConfig config)
			: base(config)
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
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", lookup);

				var results = await Submit<LCUEnvironment>(query);

				return results.FirstOrDefault();
			});
		}

		public virtual async Task<MetadataModel> GetEnvironmentSettings(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", apiKey)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has("Registry", apiKey)
					.Has("EnterprisePrimaryAPIKey", apiKey);

				var results = await Submit<MetadataModel>(query);

				return results.FirstOrDefault();
			});
		}

		public virtual async Task<SourceControl> GetSourceControl(string apiKey, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Registry", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Registry", registry);

				var results = await Submit<SourceControl>(query);

				return results.FirstOrDefault();
			});
		}

		public virtual async Task<List<LCUEnvironment>> ListEnvironments(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<LCUEnvironment>(query);

				return results.ToList();
			});
		}

		public virtual async Task<LCUEnvironment> SaveEnvironment(LCUEnvironment env)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
						.Has("Lookup", env.Lookup)
						.Has("EnterprisePrimaryAPIKey", env.EnterprisePrimaryAPIKey)
						.Has("Registry", env.EnterprisePrimaryAPIKey);

				var existingEnvResults = await Submit<LCUEnvironment>(existingQuery);

				var existingEnvResult = existingEnvResults.FirstOrDefault();

				var query = existingEnvResult == null ?
					g.AddV(EntGraphConstants.EnvironmentVertexName)
					.Property("EnterprisePrimaryAPIKey", env.EnterprisePrimaryAPIKey)
					.Property("Registry", env.EnterprisePrimaryAPIKey) : existingQuery;

				query = query
					.Property("Lookup", env.Lookup ?? "")
					.Property("Name", env.Name ?? "");

				var envResults = await Submit<LCUEnvironment>(query);

				var envResult = envResults.FirstOrDefault();

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", env.EnterprisePrimaryAPIKey)
					.Has("PrimaryAPIKey", env.EnterprisePrimaryAPIKey);

				var entResults = await Submit<Enterprise>(entQuery);

				var entResult = entResults.FirstOrDefault();

				var edgeResults = await Submit<LCUEnvironment>(g.V(entResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(envResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(entResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(envResult.ID)),
						g.V(entResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(envResult.ID)),
						g.V(entResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(envResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return envResult;
			});
		}

		public virtual async Task<MetadataModel> SaveEnvironmentSettings(string apiKey, string envLookup, MetadataModel settings)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", apiKey)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Lookup", envLookup)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has("Registry", apiKey)
					.Has("EnterprisePrimaryAPIKey", apiKey);

				var existingEnvSetResults = await Submit<BusinessModel<Guid>>(existingQuery);

				var existingEnvSetResult = existingEnvSetResults.FirstOrDefault();

				var query = existingEnvSetResult == null ?
					g.AddV(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Property("EnterprisePrimaryAPIKey", apiKey)
					.Property("Registry", apiKey) : existingQuery;

				settings.Metadata.Each(md =>
				{
					query = query.Property(md.Key, md.Value?.ToString() ?? "");
				});

				var envSetResults = await Submit<BusinessModel<Guid>>(query);

				var envSetResult = envSetResults.FirstOrDefault();

				var envQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", apiKey)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Lookup", envLookup);

				var envResults = await Submit<Graphs.Registry.Enterprises.Provisioning.LCUEnvironment>(envQuery);

				var envResult = envResults.FirstOrDefault();

				var edgeResults = await Submit<BusinessModel<Guid>>(g.V(envResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(envSetResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(envResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(envSetResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(envSetResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(envSetResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return envSetResult;
			});
		}

		public virtual async Task<SourceControl> SaveSourceControl(string apiKey, string envLookup, SourceControl sc)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{apiKey}|{envLookup}";

				var existingQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Registry", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Registry", registry);

				var existingSCResults = await Submit<SourceControl>(existingQuery);

				var existingSCResult = existingSCResults.FirstOrDefault();

				var query = existingSCResult == null ?
					g.AddV(EntGraphConstants.SourceControlVertexName)
					.Property("EnterprisePrimaryAPIKey", apiKey)
					.Property("Registry", registry) : existingQuery;

				query = query
					.Property("Name", sc.Name ?? "")
					.Property("Organization", sc.Organization ?? "")
					.Property("Repository", sc.Repository ?? "");

				var scResults = await Submit<SourceControl>(query);

				var scResult = scResults.FirstOrDefault();

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has("EnterprisePrimaryAPIKey", apiKey)
					.Has("Registry", apiKey);

				var envResults = await Submit<LCUEnvironment>(envQuery);

				var envResult = envResults.FirstOrDefault();

				var edgeResults = await Submit<SourceControl>(g.V(envResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(scResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(envResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(scResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(scResult.ID)),
						g.V(envResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(scResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return scResult;
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
