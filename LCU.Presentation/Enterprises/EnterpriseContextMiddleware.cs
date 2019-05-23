using Fathym;
using LCU.Graphs.Registry.Enterprises;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Presentation.Enterprises
{
	public class EnterpriseContextMiddleware
	{
		#region Fields
		protected readonly IDistributedCache cache;

		protected readonly IConfiguration config;

		protected readonly EnterpriseGraph entGraph;

		protected readonly ILogger<EnterpriseContextMiddleware> logger;

		protected readonly IMemoryCache memCache;

		protected readonly RequestDelegate next;

		protected readonly SemaphoreSlim readLock;
		#endregion

		#region Constructors
		public EnterpriseContextMiddleware(RequestDelegate next, EnterpriseGraph entGraph, IDistributedCache cache,
			IMemoryCache memCache, IConfiguration config, ILoggerFactory loggerFactory)
		{
			this.cache = cache;

			this.config = config;

			this.entGraph = entGraph;

			this.logger = loggerFactory.CreateLogger<EnterpriseContextMiddleware>();

			this.memCache = memCache;

			this.next = next;

			readLock = new SemaphoreSlim(1, 1);
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var hostLookup = resolveHostLookup(context);

			var entCtxt = await cache.GetOrCreateAsync(memCache, $"{EnterpriseContext.Lookup}|{hostLookup}", async (entry, options) =>
			{
				var entHost = await entGraph.LoadByHost(hostLookup);

				logger.LogInformation("Loaded Enterprise: {Enterprise}", entHost.ID);

				EnterpriseContext ctxt = null;

				var cacheSeconds = 5;

				if (entHost != null)
				{
					ctxt = new EnterpriseContext()
					{
						Host = hostLookup,
						ID = entHost.ID,
						PrimaryAPIKey = entHost.PrimaryAPIKey
					};

					if (entHost.Metadata.ContainsKey("CacheSeconds"))
						cacheSeconds = entHost.Metadata["CacheSeconds"].As<int>();
				}
				else
				{
					//var ent = await entGraph.Create(hostLookup);

					//ctxt = new EnterpriseContext()
					//{
					//	Host = hostLookup,
					//	ID = ent.ID,
					//	PrimaryAPIKey = ent.PrimaryAPIKey
					//};

					//if (ent.Metadata.ContainsKey("CacheSeconds"))
					//	cacheSeconds = ent.Metadata["CacheSeconds"].As<int>();
				}

				if (ctxt != null)
				{
					ctxt.CacheSeconds = cacheSeconds;

					entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(ctxt.CacheSeconds));

					options.SetAbsoluteExpiration(TimeSpan.FromMinutes(ctxt.CacheSeconds));

					logger.LogInformation("Loaded Context: {EnterpriseContext}", ctxt.ToJSON());
				}

				return ctxt;
			});

			if (entCtxt != null)
			{
				context.UpdateContext(EnterpriseContext.Lookup, entCtxt);

				await next(context);
			}
			else
				throw new Exception("Enterprise domain is not configured.");
		}
		#endregion

		#region Helpers
		protected virtual string resolveHostLookup(HttpContext context)
		{
			var hostLookup = context.Request.Host.Host;

			logger.LogInformation("Initial host lookup: {Host}", hostLookup);

			if (hostLookup == "localhost")
				hostLookup = config["LOCALHOST_LOOKUP_OVERRIDE"];
			//	TODO: Not sure how this could actually work given current dev hosting structure
			else if (hostLookup.EndsWith(".local"))
				hostLookup = hostLookup.Replace(".local", ".com");

			logger.LogInformation("Complete host lookup: {Host}", hostLookup);

			return hostLookup;
		}
		#endregion
	}
}
