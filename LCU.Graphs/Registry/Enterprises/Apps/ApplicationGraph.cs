using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	public class ApplicationGraph : LCUGraph, IApplicationGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public ApplicationGraph(LCUGraphConfig config)
			: base(config)
		{
			ListProperties.Add("Hosts");
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> AddDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has("Registry", apiKey);

				var defAppsResults = await Submit<BusinessModel<Guid>>(defAppsQuery);

				var defAppsResult = defAppsResults.FirstOrDefault();

				//	TODO: Add edge if not exists

				var edgeQueries = new List<GraphTraversal<Vertex, Edge>>()
				{
					g.V(defAppsResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(appId))
				};

				foreach (var edgeQuery in edgeQueries)
				{
					await Submit(edgeQuery);
				}

				return Status.Success;
			});
		}

		public virtual async Task<Status> CreateDefaultApps(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey);

				var entResults = await Submit<Enterprise>(entQuery);

				var entResult = entResults.FirstOrDefault();

				var dropDefaultsQuery = g.V(entResult.ID)
					.Out(EntGraphConstants.OffersEdgeName)
					.BothE().Where(__.OutV().HasLabel(EntGraphConstants.DefaultAppsVertexName))
					.Drop();

				await Submit(dropDefaultsQuery);

				var defAppsQuery = g.AddV(EntGraphConstants.DefaultAppsVertexName)
					.Property("Registry", apiKey);

				var defAppsResults = await Submit<BusinessModel<Guid>>(defAppsQuery);

				var defAppsResult = defAppsResults.FirstOrDefault();

				var edgeQueries = new List<GraphTraversal<Vertex, Edge>>()
				{
					g.V(entResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(defAppsResult.ID)),
					g.V(entResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(defAppsResult.ID)),
					g.V(entResult.ID).AddE(EntGraphConstants.OffersEdgeName).To(g.V(defAppsResult.ID))
				};

				foreach (var edgeQuery in edgeQueries)
				{
					await Submit(edgeQuery);
				}

				return Status.Success;
			});
		}

		public virtual async Task<List<DAFApplicationConfiguration>> GetDAFApplications(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V(appId)
					.Out(EntGraphConstants.ProvidesEdgeName)
					.HasLabel(EntGraphConstants.DAFAppVertexName)
					.Has("ApplicationID", appId)
					.Has("Registry", $"{apiKey}|{appId}")
					.Order().By("Priority", Order.Decr);

				var appAppResults = await Submit<DAFApplicationConfiguration>(query);

				return appAppResults.ToList();
			});
		}

		public virtual async Task<Status> HasDefaultApps(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has("Registry", apiKey);

				var defAppsResults = await Submit<BusinessModel<Guid>>(defAppsQuery);

				var defAppsResult = defAppsResults.FirstOrDefault();

				return defAppsResult != null ? Status.Success : Status.NotLocated;
			});
		}

		public virtual async Task<Status> IsDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has("Registry", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.HasId(appId);

				var defAppsResults = await Submit<BusinessModel<Guid>>(defAppsQuery);

				var defAppsResult = defAppsResults.FirstOrDefault();

				return defAppsResult != null ? Status.Success : Status.NotLocated;
			});
		}

		public virtual async Task<List<Application>> ListApplications(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var 
				query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			});
		}

		public virtual async Task<List<Application>> LoadByEnterprise(string apiKey, string host, string container)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Has("Hosts", host)
					//.Has("Container", container)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			});
		}

		public virtual async Task<List<Application>> LoadDefaultApplications(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				//	TODO:  Need to support attaching Enterprise to appropriate DefaultApplications node through some edge so this is pull not as a global default, but enterprise default

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			});
		}

		public virtual async Task<Status> RemoveDAFApplication(string apiKey, DAFApplicationConfiguration config)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(config.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has("Registry", $"{apiKey}|{config.ApplicationID}")
						.Drop();

				var existingResults = await Submit<DAFApplicationConfiguration>(existingQuery);

				var existingAppResult = existingResults.FirstOrDefault();

				return Status.Success;
			});
		}

		public virtual async Task<Status> RemoveDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var dropQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has("Registry", apiKey)
					.BothE().Where(__.InV().HasId(appId))
					.Drop();

				await Submit(dropQuery);

				return Status.Success;
			});
		}

		public virtual async Task<Application> Save(Application application)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(application.ID)
						.Has("EnterprisePrimaryAPIKey", application.EnterprisePrimaryAPIKey)
						.Has("Registry", application.EnterprisePrimaryAPIKey);

				var existingResults = await Submit<Application>(existingQuery);

				var existingAppResult = existingResults.FirstOrDefault();

				var query = existingAppResult == null ?
					g.AddV(EntGraphConstants.AppVertexName)
					.Property("EnterprisePrimaryAPIKey", application.EnterprisePrimaryAPIKey)
					.Property("Registry", application.EnterprisePrimaryAPIKey) :
					g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(existingAppResult.ID)
						.Has("EnterprisePrimaryAPIKey", application.EnterprisePrimaryAPIKey)
						.Has("Registry", application.EnterprisePrimaryAPIKey);

				query = query
					.Property("Container", application.Container ?? "")
					.Property("IsPrivate", application.IsPrivate)
					.Property("IsReadOnly", application.IsReadOnly)
					.Property("Name", application.Name ?? "")
					.Property("PathRegex", application.PathRegex ?? "")
					.Property("QueryRegex", application.QueryRegex ?? "")
					.Property("UserAgentRegex", application.UserAgentRegex ?? "")
					.Property("Priority", application.Priority);

				application.Hosts.Each(host =>
				{
					query = query.Property(Cardinality.List, "Hosts", host, new object[] { });
				});

				var appResults = await Submit<Application>(query);

				var appResult = appResults.FirstOrDefault();

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", application.EnterprisePrimaryAPIKey)
					.Has("PrimaryAPIKey", application.EnterprisePrimaryAPIKey);

				var entResults = await Submit<Enterprise>(entQuery);

				var entResult = entResults.FirstOrDefault();

				var edgeResults = await Submit<Application>(g.V(entResult.ID).Out(EntGraphConstants.OwnsEdgeName).HasId(appResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(entResult.ID).AddE(EntGraphConstants.ConsumesEdgeName).To(g.V(appResult.ID)),
						g.V(entResult.ID).AddE(EntGraphConstants.OwnsEdgeName).To(g.V(appResult.ID)),
						g.V(entResult.ID).AddE(EntGraphConstants.ManagesEdgeName).To(g.V(appResult.ID))
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return appResult;
			});
		}

		public virtual async Task<DAFApplicationConfiguration> SaveDAFApplication(string apiKey, DAFApplicationConfiguration config)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(config.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has("Registry", $"{apiKey}|{config.ApplicationID}");

				var existingResults = await Submit<DAFApplicationConfiguration>(existingQuery);

				var existingAppResult = existingResults.FirstOrDefault();

				var query = existingAppResult == null ?
					g.AddV(EntGraphConstants.DAFAppVertexName)
						.Property("ApplicationID", config.ApplicationID)
						.Property("Registry", $"{apiKey}|{config.ApplicationID}") :
					g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(existingAppResult.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has("Registry", $"{apiKey}|{config.ApplicationID}");

				query = query.Property("Priority", config.Priority);

				if (config.Metadata.ContainsKey("BaseHref"))
				{
					query.Property("BaseHref", config.Metadata["BaseHref"])
						.Property("NPMPackage", config.Metadata["NPMPackage"])
						.Property("PackageVersion", config.Metadata["PackageVersion"]);
				}
				else if (config.Metadata.ContainsKey("APIRoot"))
				{
					query.Property("APIRoot", config.Metadata["APIRoot"])
						.Property("InboundPath", config.Metadata["InboundPath"])
						.Property("Methods", config.Metadata["Methods"])
						.Property("Security", config.Metadata["Security"]);
				}

				var appAppResults = await Submit<DAFApplicationConfiguration>(query);

				var appAppResult = appAppResults.FirstOrDefault();

				var appQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
					.HasId(config.ApplicationID)
					.Has("Registry", apiKey);

				var appResults = await Submit<Application>(appQuery);

				var appResult = appResults.FirstOrDefault();

				var edgeResults = await Submit<DAFApplicationConfiguration>(g.V(appResult.ID).Out(EntGraphConstants.ProvidesEdgeName).HasId(appAppResult.ID));

				var edgeResult = edgeResults.FirstOrDefault();

				if (edgeResult == null)
				{
					var edgeQueries = new[] {
						g.V(appResult.ID).AddE(EntGraphConstants.ProvidesEdgeName).To(g.V(appAppResult.ID)),
					};

					foreach (var edgeQuery in edgeQueries)
						await Submit(edgeQuery);
				}

				return appAppResult;
			});
		}

		public virtual async Task<Status> SeedDefault(string sourceApiKey, string targetApiKey)
		{
			return await withG(async (client, g) =>
			{
				var defaultsQuery = g.V().HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has("Registry", sourceApiKey);

				var defaultApps = await Submit<BusinessModel<Guid>>(defaultsQuery);

				var defaultApp = defaultApps.FirstOrDefault();

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has("Registry", targetApiKey)
					.Has("PrimaryAPIKey", targetApiKey);

				var entResults = await Submit<Enterprise>(entQuery);

				var entResult = entResults.FirstOrDefault();

				var edgeQueries = new List<GraphTraversal<Vertex, Edge>>()
				{
					g.V(entResult.ID).AddE(EntGraphConstants.OffersEdgeName).To(g.V(defaultApp.ID))
				};

				foreach (var edgeQuery in edgeQueries)
				{
					await Submit(edgeQuery);
				}

				return Status.Success;
			});
		}
		#endregion

		#region Helpers
		#endregion
	}
}
