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
		public virtual async Task<LCUEnvironment> GetEnvironment(string entLookup, string lookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", lookup);

				var result = await SubmitFirst<LCUEnvironment>(query);

				return result;
			}, entLookup);
		}

		public virtual async Task<MetadataModel> GetEnvironmentSettings(string entLookup, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup);

				var result = await SubmitFirst<MetadataModel>(query);

				if (result != null)
				{
					if (result.Metadata.ContainsKey("Registry"))
						result.Metadata.Remove("Registry");

					if (result.Metadata.ContainsKey("EnterpriseAPIKey"))
						result.Metadata.Remove("EnterpriseAPIKey");

					if (result.Metadata.ContainsKey("id"))
						result.Metadata.Remove("id");
				}

				return result;
			}, entLookup);
		}

		public virtual async Task<SourceControl> GetSourceControl(string entLookup, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entLookup}|{envLookup}";

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has(EntGraphConstants.RegistryName, registry);

				var result = await SubmitFirst<SourceControl>(query);

				return result;
			}, entLookup);
		}

		public virtual async Task<List<LCUEnvironment>> ListEnvironments(string entLookup)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<LCUEnvironment>(query);

				return results.ToList();
			}, entLookup);
		}

		public virtual async Task<Status> RemoveEnvironment(string entLookup, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var dropQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
						.Has(EntGraphConstants.RegistryName, entLookup)
						.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
						.Has("Lookup", envLookup)
						.Drop();

				await Submit(dropQuery);

				return Status.Success;
			});
		}

		public virtual async Task<Status> RemoveEnvironmentSettings(string entLookup, string envLookup)
		{
			return await withG(async (client, g) =>
			{
				var dropQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Drop();

				await Submit(dropQuery);

				return Status.Success;
			}, entLookup);
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

		public virtual async Task<MetadataModel> SaveEnvironmentSettings(string entLookup, string envLookup, MetadataModel settings)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", entLookup)
					.Has("EnterpriseAPIKey", entLookup)
					.Has("Lookup", envLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Has("Registry", entLookup)
					.Has("EnterpriseAPIKey", entLookup);

				var existingEnvSetResult = await SubmitFirst<BusinessModel<Guid>>(existingQuery);

				var query = existingEnvSetResult == null ?
					g.AddV(EntGraphConstants.EnvironmentVertexName + "Settings")
					.Property("EnterpriseAPIKey", entLookup)
					.Property("Registry", entLookup) : existingQuery;

				settings.Metadata.Each(md =>
				{
					query = query.Property(md.Key, md.Value?.ToString() ?? "");
				});

				var envSetResult = await SubmitFirst<BusinessModel<Guid>>(query);

				var envQuery = g.V().HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Registry", entLookup)
					.Has("EnterpriseAPIKey", entLookup)
					.Has("Lookup", envLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				await ensureEdgeRelationships(g, envResult.ID, envSetResult.ID);

				return envSetResult;
			}, entLookup);
		}

		public virtual async Task<SourceControl> SaveSourceControl(string entLookup, string envLookup, SourceControl sc)
		{
			return await withG(async (client, g) =>
			{
				var registry = $"{entLookup}|{envLookup}";

				var existingQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.SourceControlVertexName)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has(EntGraphConstants.RegistryName, registry);

				var existingSCResult = await SubmitFirst<SourceControl>(existingQuery);

				var query = existingSCResult == null ?
					g.AddV(EntGraphConstants.SourceControlVertexName)
					.Property(EntGraphConstants.RegistryName, registry)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup) : existingQuery;

				query = query
					.Property("Name", sc.Name ?? "")
					.Property("Organization", sc.Organization ?? "")
					.Property("Repository", sc.Repository ?? "");

				var scResult = await SubmitFirst<SourceControl>(query);

				var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, entLookup)
					.Has("PrimaryAPIKey", entLookup)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.EnvironmentVertexName)
					.Has("Lookup", envLookup)
					.Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
					.Has(EntGraphConstants.RegistryName, entLookup);

				var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

				await ensureEdgeRelationships(g, envResult.ID, scResult.ID);

				return scResult;
			}, entLookup);
		}
		#endregion

		#region Helpers
		#endregion
	}
}
