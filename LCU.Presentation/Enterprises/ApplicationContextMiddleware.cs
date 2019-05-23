using Fathym;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Presentation.Enterprises
{
	public class ApplicationContextMiddleware
	{
		#region Fields
		protected const string appContainer = "lcu-applications-container";

		protected readonly ApplicationGraph appGraph;

		protected readonly IDistributedCache cache;

		protected readonly EnterpriseGraph entGraph;

		protected readonly ILogger<ApplicationContextMiddleware> logger;

		protected readonly IMemoryCache memCache;

		protected readonly RequestDelegate next;

		protected readonly SemaphoreSlim readLock;
		#endregion

		#region Constructors
		public ApplicationContextMiddleware(RequestDelegate next, ApplicationGraph appGraph, EnterpriseGraph entGraph,
			IDistributedCache cache, IMemoryCache memCache, ILoggerFactory loggerFactory)
		{
			this.appGraph = appGraph;

			this.cache = cache;

			this.entGraph = entGraph;

			this.logger = loggerFactory.CreateLogger<ApplicationContextMiddleware>();

			this.memCache = memCache;

			this.next = next;

			readLock = new SemaphoreSlim(1, 1);
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var appsCtxt = await ensureApplicationsContext(context);

			await ensureApplicationContext(appsCtxt, context);

			var appCtxt = context.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(context));

			if (appCtxt != null)
				await next(context);
			else
			{
				var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

				throw new Exception($"Application for domain could not be loaded: {entCtxt.ToJSON()}");
			}
		}
		#endregion

		#region Helpers
		protected virtual async Task<ApplicationsContext> ensureApplicationsContext(HttpContext context)
		{
			var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			return await cache.GetOrCreateAsync(memCache, $"{ApplicationsContext.Lookup}|{entCtxt.Host}", async (entry, options) =>
			{
				logger.LogInformation("Loading Applications");

				entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(entCtxt.CacheSeconds));

				options.SetAbsoluteExpiration(TimeSpan.FromSeconds(entCtxt.CacheSeconds));

				var container = context.Request.Headers[appContainer].ToString() ??
							context.Request.Query[appContainer];

				var apps = await appGraph.LoadByEnterprise(entCtxt.PrimaryAPIKey, entCtxt.Host, container);

				if (!entCtxt.PreventDefaultApplications)
				{
					var defaultApps = await appGraph.LoadDefaultApplications(entCtxt.PrimaryAPIKey);

					if (!defaultApps.IsNullOrEmpty())
					{
						defaultApps.Each(da =>
						{
							if (!apps.Any(a => a.ID == da.ID))
								apps.Add(da);
						});

						apps = apps.OrderByDescending(da => da.Priority).ToList();
					}
				}

				if (!apps.IsNullOrEmpty())
				{
					logger.LogInformation("Loaded Applications");

					var entApikeys = apps.Select(app => app.EnterprisePrimaryAPIKey).Distinct();

					var ents = entApikeys.ToDictionary(apiKey => apiKey, apiKey => entGraph.LoadByPrimaryAPIKey(apiKey).Result);

					var appsCtxt = new ApplicationsContext()
					{
						Applications = apps.Select(app =>
						{
							var appConfig = !app.PathRegex.IsNullOrEmpty() &&
								!app.QueryRegex.IsNullOrEmpty() &&
								!app.UserAgentRegex.IsNullOrEmpty() ? null :
								new ApplicationLookupConfiguration()
								{
									IsPrivate = app.IsPrivate,
									IsReadOnly = app.IsReadOnly,
									PathRegex = app.PathRegex,
									QueryRegex = app.QueryRegex,
									UserAgentRegex = app.UserAgentRegex
								};

							return new ApplicationContext()
							{
								ID = app.ID,
								Name = app.Name,
								Priority = app.Priority,
								LookupConfig = appConfig,
								EnterpriseID = ents[app.EnterprisePrimaryAPIKey].ID,
								EnterprisePrimaryAPIKey = ents[app.EnterprisePrimaryAPIKey].PrimaryAPIKey
							};
						}).ToList(),
						Container = container
					};

					logger.LogInformation("Resolved Applications Context: {ApplicationsContext}", appsCtxt);

					return appsCtxt;
				}
				else
					return new ApplicationsContext();
			});
		}

		protected virtual async Task ensureApplicationContext(ApplicationsContext appsCtxt, HttpContext context)
		{
			if (appsCtxt != null && !appsCtxt.Applications.IsNullOrEmpty())
			{
				var apps = appsCtxt.Applications.ToArray();

				var appCtxt = apps.FirstOrDefault(
						(app) =>
						{
							var found = verifyLookupRegexes(context, app);

							if (found && context.User != null)
							{
								//if (found)
								//	found = verifyLookupClaims(context, app.LookupConfig?.Claims);

								//if (found)
								//	found = verifyLookupRoleSets(context, app.LookupConfig?.RoleSets);
							}

							return found;
						});

				if (appCtxt == null)
					appCtxt = appsCtxt.Applications.FirstOrDefault(app => app.LookupConfig == null);

				if (appCtxt != null)
				{
					context.UpdateContext(ApplicationContext.CreateLookup(context), appCtxt);

					logger.LogInformation("Resolved Application Context: {ApplicationContext}", appCtxt);
				}
			}
			else
				throw new Exception("The are no Applications configured for this host container.");
		}

		//protected virtual bool verifyLookupClaims(HttpContext context, List<LookupClaim> claims)
		//{
		//	var verified = false;

		//	if (!claims.IsNullOrEmpty())
		//	{
		//		verified = claims.All((claim) =>
		//		{
		//			var located = false;

		//			var userClaims = context.User.Identity.As<ClaimsIdentity>().Claims;

		//			if (claim.CheckWithout)
		//			{
		//				located = userClaims.IsNullOrEmpty();

		//				if (!located)
		//				{
		//					var userClaim = userClaims.FirstOrDefault(c => c.Type == claim.Type);

		//					located = userClaim == null ||
		//						(!claim.Values.IsNullOrEmpty() && !claim.Values.Contains(userClaim.Value));
		//				}
		//			}
		//			else
		//			{
		//				if (!userClaims.IsNullOrEmpty())
		//				{
		//					var userClaim = userClaims.FirstOrDefault(c => c.Type == claim.Type);

		//					if (userClaim != null)
		//						located = claim.Values.IsNullOrEmpty() || claim.Values.Contains(userClaim.Value);
		//				}
		//			}

		//			return located;
		//		});
		//	}
		//	else
		//		verified = true;

		//	return verified;
		//}

		protected virtual bool verifyLookupRegexes(HttpContext context, ApplicationContext app)
		{
			var verified = false;

			if (context.Request.Query.ContainsKey("lcu-app-id"))
				verified = app.ID == context.Request.Query["lcu-app-id"].ToString().As<Guid>();

			var lookup = app.LookupConfig;

			if (lookup != null && !verified)
			{
				if (lookup.IsReadOnly)
					verified = new[] { "GET", "HEAD" }.Contains(context.Request.Method.ToUpper());
				else
					verified = true;

				if (verified)
				{
					var regexs = new List<Tuple<Regex, string>>();

					if (!lookup.PathRegex.IsNullOrEmpty())
						regexs.Add(new Tuple<Regex, string>(new Regex(lookup.PathRegex), context.Request.Path.Value));

					if (!lookup.QueryRegex.IsNullOrEmpty())
						regexs.Add(new Tuple<Regex, string>(new Regex(lookup.QueryRegex), context.Request.QueryString.Value));

					if (!lookup.UserAgentRegex.IsNullOrEmpty())
						regexs.Add(new Tuple<Regex, string>(new Regex(lookup.UserAgentRegex), context.Request.GetUserAgent()));

					if (!regexs.IsNullOrEmpty())
						verified = regexs.All(set => set.Item1.IsMatch(set.Item2));
				}
			}
			else
				verified = true;

			return verified;
		}

		//protected virtual bool verifyLookupRoleSets(HttpContext context, List<SecurityRoleSet> roleSets)
		//{
		//	var verified = false;

		//	if (!roleSets.IsNullOrEmpty())
		//		verified = roleSets.Any(prs => prs.Roles.All(r => context.User.IsInRole(r)));
		//	else
		//		verified = true;

		//	return verified;
		//}
		#endregion
	}
}
