using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using LCU.Graphs.Registry.Enterprises.Edges;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	public class ApplicationGraph : LCUGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public ApplicationGraph(LCUGraphConfig graphConfig, ILogger<ApplicationGraph> logger)
			: base(graphConfig, logger)
		{ }
		#endregion

		#region API Methods
		public virtual async Task<Status> AddDefaultApp(string entLookup, Guid appId)
		{
			var defApps = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Offers>()
				.OfType<DefaultApplications>()
				.Where(da => da.Registry == entLookup)
				.FirstOrDefaultAsync();

			await ensureEdgeRelationship<Consumes>(defApps.ID, appId);

			return Status.Success;
		}

		public virtual async Task<Status> CreateDefaultApps(string entLookup)
		{
			var ent = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.FirstOrDefaultAsync();

			var dropDefaults = await g.V<Enterprise>(ent.ID)
				.Out<Offers>()
				.BothE().Where(__ => __.OutV<DefaultApplications>())
				.Drop();

			var defApps = await g.AddV(new DefaultApplications()
			{
				ID = Guid.NewGuid(),
				Registry = entLookup,
				EnterpriseLookup = entLookup
			})
			.FirstOrDefaultAsync();

			await ensureEdgeRelationship<Consumes>(ent.ID, defApps.ID);

			await ensureEdgeRelationship<Manages>(ent.ID, defApps.ID);

			await ensureEdgeRelationship<Owns>(ent.ID, defApps.ID);

			return Status.Success;
		}

		public virtual async Task<List<DAFApplicationConfiguration>> GetDAFApplications(string entLookup, Guid appId)
		{
			var dafApps = await g.V<Application>(appId)
				.Out<Provides>()
				.OfType<DAFApplicationConfiguration>()
				.Where(da => da.Registry == $"{entLookup}|{appId}")
				.Where(da => da.ApplicationID == appId)
				.Order(_ => _.By(da => da.Priority))
				.ToListAsync();

			return dafApps;
		}

		public virtual async Task<Status> HasDefaultApps(string entLookup)
		{
			var defApps = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Owns>()
				.OfType<DefaultApplications>()
				.Where(e => e.Registry == entLookup)
				.FirstOrDefaultAsync();

			return defApps != null ? Status.Success : Status.NotLocated;
		}

		public virtual async Task<Status> IsDefaultApp(string entLookup, Guid appId)
		{
			var defApp = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Owns>()
				.OfType<DefaultApplications>()
				.Where(e => e.Registry == entLookup)
				.Out<Consumes>()
				.OfType<Application>()
				.V(appId)
				.FirstOrDefaultAsync();

			return defApp != null ? Status.Success : Status.NotLocated;
		}

		public virtual async Task<List<Application>> ListApplications(string entLookup)
		{
			var apps = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Consumes>()
				.OfType<Application>()
				.Order(_ => _.By(a => a.Priority))
				.ToListAsync();

			return apps;
		}

		public virtual async Task<List<Application>> LoadByEnterprise(string entLookup, string host, string container = null)
		{
			var appsQuery = g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Consumes>()
				.OfType<Application>()
				.Where(a => a.Hosts.Contains(host));

			//	TODO:  Turn on and enable as part of next wave of data apps
			//if (!container.IsNullOrEmpty())
			//	appsQuery = appsQuery.Where(a => a.Container == container);

			var apps = await appsQuery
				.Order(_ => _.By(a => a.Priority))
				.ToListAsync();

			return apps;
		}

		public virtual async Task<List<Application>> LoadDefaultApplications(string entLookup)
		{
			var defApps = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Offers>()
				.OfType<DefaultApplications>()
				.Out<Consumes>()
				.OfType<Application>()
				.Order(_ => _.By(da => da.Priority))
				.ToListAsync();

			return defApps;
		}

		public virtual async Task<Status> RemoveDAFApplication(string entLookup, DAFApplicationConfiguration config)
		{
			var dropQuery = g.V<DAFApplicationConfiguration>(config.ID)
					.Where(da => da.Registry == $"{entLookup}|{config.ApplicationID}")
					.Where(da => da.ApplicationID == config.ApplicationID);

			if (!config.Lookup.IsNullOrEmpty())
				dropQuery = dropQuery.Where(da => da.Lookup == config.Lookup);

			var existingResult = await dropQuery.Drop();

			return Status.Success;
		}

		public virtual async Task<Status> RemoveDefaultApp(string entLookup, Guid appId)
		{
			var dropResult = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == entLookup)
				.Where(e => e.Registry == entLookup)
				.Out<Offers>()
				.OfType<DefaultApplications>()
				.Where(da => da.Registry == entLookup)
				.BothE().Where(__ => __.InV().V(appId))
				.Drop();

			return Status.Success;
		}

		public virtual async Task<Application> Save(Application application)
		{
			var existingApp = await g.V<Application>(application.ID)
				.Where(e => e.EnterpriseLookup == application.EnterpriseLookup)
				.Where(e => e.Registry == application.EnterpriseLookup)
				.FirstOrDefaultAsync();

			if (existingApp == null)
			{
				if (application.ID.IsEmpty())
					application.ID = Guid.NewGuid();

				application = await g.AddV(application).FirstOrDefaultAsync();
			}
			else
				//if (existingAppResult != null)
				//{
				//	var dropAccessRightsQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
				//		.HasId(existingAppResult.ID)
				//		.Properties<Vertex>("AccessRights").Drop();

				//	await Submit(dropAccessRightsQuery);

				//	var dropHostsQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
				//		.HasId(existingAppResult.ID)
				//		.Properties<Vertex>("Hosts").Drop();

				//	await Submit(dropHostsQuery);

				//	var dropLicensesQuery = g.V().HasLabel(EntGraphConstants.AppVertexName)
				//		.HasId(existingAppResult.ID)
				//		.Properties<Vertex>("Licenses").Drop();

				//	await Submit(dropLicensesQuery);
				//}

				application = await g.V<Application>(existingApp.ID)
						.Update(application)
						.FirstOrDefaultAsync();

			var ent = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == application.EnterpriseLookup)
				.Where(e => e.Registry == application.EnterpriseLookup)
				.FirstOrDefaultAsync();

			await ensureEdgeRelationship<Consumes>(ent.ID, application.ID);

			await ensureEdgeRelationship<Manages>(ent.ID, application.ID);

			await ensureEdgeRelationship<Owns>(ent.ID, application.ID);

			return application;
		}

		public virtual async Task<DAFApplicationConfiguration> SaveDAFApplication(string entLookup, DAFApplicationConfiguration dafApp)
		{
			var existingDafApp = await g.V<DAFApplicationConfiguration>(dafApp.ID)
				.Where(da => da.Registry == $"{entLookup}|{dafApp.ApplicationID}")
				.Where(da => da.ApplicationID == dafApp.ApplicationID)
				.FirstOrDefaultAsync();

			if (existingDafApp == null)
			{
				if (dafApp.ID.IsEmpty())
					dafApp.ID = Guid.NewGuid();

				dafApp = await g.AddV(dafApp).FirstOrDefaultAsync();
			}
			else
				dafApp = await g.V<DAFApplicationConfiguration>(existingDafApp.ID)
					.Update(dafApp)
					.FirstOrDefaultAsync();

			var app = await g.V<Application>(dafApp.ApplicationID)
				.Where(a => a.Registry == entLookup)
				.FirstOrDefaultAsync();

			await ensureEdgeRelationship<Provides>(app.ID, dafApp.ID);

			return dafApp;
		}

		public virtual async Task<Status> SeedDefault(string sourceEntLookup, string targetEntLookup)
		{
			var defaultApp = await g.V<DefaultApplications>()
				.Where(da => da.EnterpriseLookup == sourceEntLookup)
				.Where(da => da.Registry == sourceEntLookup)
				.FirstOrDefaultAsync();

			var ent = await g.V<Enterprise>()
				.Where(e => e.EnterpriseLookup == targetEntLookup)
				.Where(e => e.Registry == targetEntLookup)
				.FirstOrDefaultAsync(); ;

			await ensureEdgeRelationship<Offers>(ent.ID, defaultApp.ID);

			return Status.Success;
		}
		#endregion

		#region Helpers
		#endregion
	}
}
