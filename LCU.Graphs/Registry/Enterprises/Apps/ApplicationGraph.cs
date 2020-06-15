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
		public ApplicationGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			ListProperties.Add("AccessRights");

			ListProperties.Add("Hosts");

			ListProperties.Add("Licenses");
		}
		#endregion

		#region API Methods
		public virtual async Task<Status> AddDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey);

				var defAppsResult = await SubmitFirst<BusinessModel<Guid>>(defAppsQuery);

				await ensureEdgeRelationships(g, defAppsResult.ID, appId,
					edgeToCheckBuy: EntGraphConstants.ConsumesEdgeName, edgesToCreate: new List<string>()
					{
						EntGraphConstants.ConsumesEdgeName
					});

				return Status.Success;
			}, apiKey);
		}

		public virtual async Task<Status> CreateDefaultApps(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				var dropDefaultsQuery = g.V(entResult.ID)
					.Out(EntGraphConstants.OffersEdgeName)
					.BothE().Where(__.OutV().HasLabel(EntGraphConstants.DefaultAppsVertexName))
					.Drop();

				await Submit(dropDefaultsQuery);

				var defAppsQuery = g.AddV(EntGraphConstants.DefaultAppsVertexName)
					.Property(EntGraphConstants.RegistryName, apiKey)
					.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey);

				var dafAppsResult = await SubmitFirst<BusinessModel<Guid>>(defAppsQuery);

				await ensureEdgeRelationships(g, entResult.ID, dafAppsResult.ID);

				return Status.Success;
			}, apiKey);
		}

		public virtual async Task<DAFApplicationConfiguration> GetDAFApplication(Guid dafAppId)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V(dafAppId);

				var appAppResult = await SubmitFirst<DAFApplicationConfiguration>(query);

				return appAppResult;
			}, dafAppId.ToString());
		}

		public virtual async Task<Status> HasDefaultApps(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey);

				var defAppsResult = await SubmitFirst<BusinessModel<Guid>>(defAppsQuery);

				return defAppsResult != null ? Status.Success : Status.NotLocated;
			}, apiKey);
		}

		public virtual async Task<Status> IsDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var defAppsQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OwnsEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.HasId(appId);

				var appsResult = await SubmitFirst<Application>(defAppsQuery);

				return appsResult != null ? Status.Success : Status.NotLocated;
			}, apiKey);
		}

		public virtual async Task<List<Application>> ListApplications(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				var
				query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<List<DAFApplicationConfiguration>> ListDAFApplications(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V(appId)
					.Out(EntGraphConstants.ProvidesEdgeName)
					.HasLabel(EntGraphConstants.DAFAppVertexName)
					.Has("ApplicationID", appId)
					.Has(EntGraphConstants.RegistryName, $"{apiKey}|{appId}")
					.Order().By("Priority", Order.Decr);

				var appAppResults = await Submit<DAFApplicationConfiguration>(query);

				return appAppResults.ToList();
			}, apiKey);
		}

		public virtual async Task<List<Application>> LoadByEnterprise(string apiKey, string host, string container)
		{
			return await withG(async (client, g) =>
			{
				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Has("Hosts", host)
					//.Has("Container", container)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<List<Application>> LoadDefaultApplications(string apiKey)
		{
			return await withG(async (client, g) =>
			{
				//	TODO:  Need to support attaching Enterprise to appropriate DefaultApplications node through some edge so this is pull not as a global default, but enterprise default

				var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Out(EntGraphConstants.ConsumesEdgeName)
					.HasLabel(EntGraphConstants.AppVertexName)
					.Order().By("Priority", Order.Decr);

				var results = await Submit<Application>(query);

				return results.ToList();
			}, apiKey);
		}

		public virtual async Task<Status> RemoveDAFApplication(string apiKey, DAFApplicationConfiguration config)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(config.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has(EntGraphConstants.RegistryName, $"{apiKey}|{config.ApplicationID}");

				if (!config.Lookup.IsNullOrEmpty())
					existingQuery = existingQuery.Has("Lookup", config.Lookup);

				existingQuery = existingQuery.Drop();

				var existingResult = await SubmitFirst<DAFApplicationConfiguration>(existingQuery);

				return Status.Success;
			}, apiKey);
		}

		public virtual async Task<Status> RemoveDefaultApp(string apiKey, Guid appId)
		{
			return await withG(async (client, g) =>
			{
				var dropQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.Has("PrimaryAPIKey", apiKey)
					.Out(EntGraphConstants.OffersEdgeName)
					.HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has(EntGraphConstants.RegistryName, apiKey)
					.BothE().Where(__.InV().HasId(appId))
					.Drop();

				await Submit(dropQuery);

				return Status.Success;
			}, apiKey);
		}

		public virtual async Task<Application> Save(Application application)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(application.ID)
						.Has(EntGraphConstants.EnterpriseAPIKeyName, application.EnterpriseAPIKey)
						.Has(EntGraphConstants.RegistryName, application.EnterpriseAPIKey);

				var existingAppResult = await SubmitFirst<Application>(existingQuery);

				if (existingAppResult != null)
				{
					var dropAccessRightsQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(existingAppResult.ID)
						.Properties<Vertex>("AccessRights").Drop();

					await Submit(dropAccessRightsQuery);

					var dropHostsQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(existingAppResult.ID)
						.Properties<Vertex>("Hosts").Drop();

					await Submit(dropHostsQuery);

					var dropLicensesQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(existingAppResult.ID)
						.Properties<Vertex>("Licenses").Drop();

					await Submit(dropLicensesQuery);
				}

				var query = existingAppResult == null ?
					g.AddV(EntGraphConstants.AppVertexName)
						.Property(EntGraphConstants.RegistryName, application.EnterpriseAPIKey)
						.Property(EntGraphConstants.EnterpriseAPIKeyName, application.EnterpriseAPIKey) :
					g.V().HasLabel(EntGraphConstants.AppVertexName)
						.HasId(existingAppResult.ID);

				query = query
					.Property("Container", application.Container ?? "")
					.Property("Description", application.Description ?? "")
					.Property("IsPrivate", application.IsPrivate)
					.Property("IsReadOnly", application.IsReadOnly)
					.Property("Name", application.Name ?? "")
					.Property("PathRegex", application.PathRegex ?? "")
					.Property("QueryRegex", application.QueryRegex ?? "")
					.Property("UserAgentRegex", application.UserAgentRegex ?? "")
					.Property("Priority", application.Priority)
					.AttachList("AccessRights", application.AccessRights)
					.AttachList("Hosts", application.Hosts)
					.AttachList("Licenses", application.Licenses);

				var appResult = await SubmitFirst<Application>(query);

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, application.EnterpriseAPIKey)
					.Has("PrimaryAPIKey", application.EnterpriseAPIKey);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				await ensureEdgeRelationships(g, entResult.ID, appResult.ID);

				return appResult;
			}, application.ID.ToString());
		}

		public virtual async Task<DAFApplicationConfiguration> SaveDAFApplication(string apiKey, DAFApplicationConfiguration config)
		{
			return await withG(async (client, g) =>
			{
				var existingQuery = g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(config.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has(EntGraphConstants.RegistryName, $"{apiKey}|{config.ApplicationID}");

				var existingAppResult = await SubmitFirst<DAFApplicationConfiguration>(existingQuery);

				var query = existingAppResult == null ?
					g.AddV(EntGraphConstants.DAFAppVertexName)
						.Property("ApplicationID", config.ApplicationID)
						.Property(EntGraphConstants.RegistryName, $"{apiKey}|{config.ApplicationID}")
						.Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) :
					g.V().HasLabel(EntGraphConstants.DAFAppVertexName)
						.HasId(existingAppResult.ID)
						.Has("ApplicationID", config.ApplicationID)
						.Has(EntGraphConstants.RegistryName, $"{apiKey}|{config.ApplicationID}");

				query = query.Property("Lookup", config.Lookup ?? "");

				query = query.Property("Priority", config.Priority);

				if (config.Metadata.ContainsKey("BaseHref"))
				{
					query.Property("BaseHref", config.Metadata["BaseHref"])
						.Property("NPMPackage", config.Metadata["NPMPackage"])
						.Property("PackageVersion", config.Metadata["PackageVersion"])
						.Property("StateConfig", config.Metadata.ContainsKey("StateConfig") ? config.Metadata["StateConfig"] : "");
				}
				else if (config.Metadata.ContainsKey("APIRoot"))
				{
					query.Property("APIRoot", config.Metadata["APIRoot"])
						.Property("InboundPath", config.Metadata["InboundPath"])
						.Property("Methods", config.Metadata["Methods"])
						.Property("Security", config.Metadata["Security"]);
				}
				else if (config.Metadata.ContainsKey("Redirect"))
				{
					query.Property("Redirect", config.Metadata["Redirect"]);
				}

				var appAppResult = await SubmitFirst<DAFApplicationConfiguration>(query);

				var appQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
					.HasId(config.ApplicationID)
					.Has(EntGraphConstants.RegistryName, apiKey);

				var appResult = await SubmitFirst<Application>(appQuery);

				await ensureEdgeRelationships(g, appResult.ID, appAppResult.ID,
					edgeToCheckBuy: EntGraphConstants.ProvidesEdgeName, edgesToCreate: new List<string>()
					{
						EntGraphConstants.ProvidesEdgeName
					});

				return appAppResult;
			}, apiKey);
		}

		public virtual async Task<Status> SeedDefault(string sourceApiKey, string targetApiKey)
		{
			return await withG(async (client, g) =>
			{
				var defaultsQuery = g.V().HasLabel(EntGraphConstants.DefaultAppsVertexName)
					.Has(EntGraphConstants.RegistryName, sourceApiKey);

				var defaultApps = await Submit<BusinessModel<Guid>>(defaultsQuery);

				var defaultApp = defaultApps.FirstOrDefault();

				var entQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
					.Has(EntGraphConstants.RegistryName, targetApiKey)
					.Has("PrimaryAPIKey", targetApiKey);

				var entResult = await SubmitFirst<Enterprise>(entQuery);

				await ensureEdgeRelationships(g, entResult.ID, defaultApp.ID,
					edgeToCheckBuy: EntGraphConstants.OffersEdgeName, edgesToCreate: new List<string>()
					{
						EntGraphConstants.OffersEdgeName
					});

				return Status.Success;
			}, targetApiKey);
		}
		#endregion

		#region Helpers
		#endregion
	}
}
