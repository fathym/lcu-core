using Fathym;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Presentation.API;
using LCU.Presentation.DFS;
using LCU.Presentation.Enterprises;
using LCU.Presentation.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Presentation.DAF
{
	public class DAFApplicationMiddleware
	{
		#region Fields
		protected readonly ApplicationGraph appGraph;

		protected readonly IDistributedCache cache;

		protected readonly IMemoryCache memCache;

		protected readonly RequestDelegate next;
		#endregion

		#region Constructors
		public DAFApplicationMiddleware(RequestDelegate next, ApplicationGraph appGraph, IDistributedCache cache, IMemoryCache memCache)
		{
			this.appGraph = appGraph;

			this.cache = cache;

			this.memCache = memCache;

			this.next = next;
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			var appCtxt = context.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(context));

			var cacheKey = $"{appCtxt.EnterprisePrimaryAPIKey}/{appCtxt.ID}";

			var dafApps = await cache.GetOrCreateAsync(memCache, cacheKey, async (entry, options) =>
			{
				entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(entCtxt.CacheSeconds));

				options.SetAbsoluteExpiration(TimeSpan.FromSeconds(entCtxt.CacheSeconds));

				return await appGraph.GetDAFApplications(appCtxt.EnterprisePrimaryAPIKey, appCtxt.ID);
			});

			if (!dafApps.IsNullOrEmpty())
			{
				if (dafApps.Any(da => da.Metadata.ContainsKey("BaseHref")))
				{
					//	TODO:  Support multiple view resolution?
					var dafView = dafApps.First().JSONConvert<DAFViewConfiguration>();

					var version = context.Request.Headers["f-dev-stream"].IsNullOrEmpty() ? dafView.PackageVersion : "dev-stream";

					var dfsCtxt = new DFSContext()
					{
						ApplicationID = appCtxt.ID,
						AppRoot = dafView.BaseHref,
						CacheSeconds = entCtxt.CacheSeconds,
						DefaultFile = "index.html", //	TODO: Make optional off of view configuration
						DFSRoot = dafView.NPMPackage.IsNullOrEmpty() ? String.Empty : $"{dafView.NPMPackage}/{version}",
						EnterpriseID = appCtxt.EnterpriseID
					};

					context.UpdateContext(DFSContext.Lookup, dfsCtxt);
				}
				else if (dafApps.Any(da => da.Metadata.ContainsKey("APIRoot")))
				{
					var apis = dafApps.Select(dafApp =>
					{
						var dafApi = dafApp.JSONConvert<DAFAPIConfiguration>();

						return new DAFAPIContext()
						{
							APIRoot = dafApi.APIRoot,
							InboundPath = dafApi.InboundPath,
							Methods = dafApi.Methods?.Split(',').ToList(),
							Security = dafApi.Security
						};
					}).ToList();

					context.UpdateContext(DAFAPIsContext.Lookup, new DAFAPIsContext()
					{
						APIs = apis
					});
				}

				context.UpdateContext(LCUAuthorizationContext.Lookup, new LCUAuthorizationContext()
				{
					Schemes = new List<string>() { "oidc" }
				});
			}

			await next(context);
		}
		#endregion
	}
}
