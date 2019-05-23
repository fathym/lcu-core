using Fathym;
using Fathym.Design.Factory;
using IdentityServer4;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Identity;
using LCU.Presentation.DAF;
using LCU.Presentation.Enterprises;
using LCU.Presentation.Identity;
using LCU.Presentation.Identity.Core;
using LCU.Presentation.State;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace LCU.Presentation
{
	public static class LCUMiddlewareExtensions
	{
		public static IServiceCollection AddRegistryGraphEnterprisesServices(this IServiceCollection services, LCUGraphConfig graphConfig)
		{
			services.AddSingleton(graphConfig);

			services.AddSingleton<EnterpriseGraph>();

			services.AddSingleton<ApplicationGraph>();

			services.AddSingleton<IdentityGraph>();

			services.AddSingleton<IDEGraph>();

			return services;
		}

		public static IServiceCollection AddLCUCaching(this IServiceCollection services, string cacheConnection)
		{
			var redisConn = ConnectionMultiplexer.Connect(cacheConnection);

			services.AddSingleton(redisConn);

			services.AddMemoryCache();

			services.AddDistributedRedisCache(o =>
			{
				o.Configuration = redisConn.Configuration;
			});

			return services;
		}

		public static IServiceCollection AddLCUSignalR(this IServiceCollection services, string signalRConnection)
		{
			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			})
			.AddAzureSignalR(o => 
			{
				o.ConnectionString = signalRConnection;

				o.ClaimsProvider = context =>
				{
					var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

					var appCtxt = context.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(context));

					var claims = context.User.Claims.ToList();

					claims.Add(new Claim("lcu-ent-api-key", entCtxt.PrimaryAPIKey));

					claims.Add(new Claim("lcu-app-ent-api-key", appCtxt.EnterprisePrimaryAPIKey));

					claims.Add(new Claim("lcu-host", entCtxt.Host));

					return claims;
				};
			});

			return services;
		}

		public static IServiceCollection AddLCUIdentityServer(this IServiceCollection services)
		{
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddResourceStore<LCUResourceStore>()
                .AddClientStore<LCUClientStore>()
                .AddProfileService<LCUProfileService>();

			services.AddSingleton<IUserStore, LCUUserStore>();

			return services;
		}

		public static IServiceCollection AddLCUIdentityServerAuthentication(this IServiceCollection services, string oauthServerUrl,
			string openIdAuthScheme = "oidc")
		{
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

			services.AddAuthentication(options =>
			{
				//options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

				//options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

				options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

				options.DefaultChallengeScheme = openIdAuthScheme;
			})
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddOpenIdConnect(openIdAuthScheme, o =>
			{
				o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

				o.Authority = oauthServerUrl;

				o.RequireHttpsMetadata = false;

				o.ClientId = "lcu";

				o.SaveTokens = true;

				o.Scope.Add(IdentityServerConstants.StandardScopes.OpenId);

				o.Scope.Add(IdentityServerConstants.StandardScopes.Profile);

				o.ClientSecret = "secret";
				o.ResponseType = "code id_token";

				o.GetClaimsFromUserInfoEndpoint = true;

                o.Scope.Clear();
                o.Scope.Add("openid");
				o.Scope.Add("api1");
				o.Scope.Add("offline_access");
  
                o.ClaimActions.Add(new MapAllClaimsAction());
                o.ClaimActions.MapUniqueJsonKey("user_id", "user_id");
				o.ClaimActions.MapUniqueJsonKey("website", "website");
			});
			//.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
			//{
			//	o.Authority = oauthServerUrl;

			//	o.Audience = "api1";

			//	o.RequireHttpsMetadata = false;
			//});

			return services;
		}

		public static IServiceCollection AddLCU(this IServiceCollection services, string storageConnection, string cacheConnection,
			string oauthServerUrl, string signalRConnection, LCUGraphConfig entGraphConfig)
		{
			var storageAccount = CloudStorageAccount.Parse(storageConnection);

			services.AddSingleton(storageAccount);

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddRegistryGraphEnterprisesServices(entGraphConfig);

			services.AddLCUIdentityServer();

			services.AddLCUIdentityServerAuthentication(oauthServerUrl);

			services.AddLCUCaching(cacheConnection);

			services.AddLCUSignalR(signalRConnection);

			services.AddLCUState();

			services.AddMvc(o =>
			{
				//o.Filters.Add(new LCUAuthorizationFilter());
			});

			return services;
		}

		public static IServiceCollection AddLCUState(this IServiceCollection services)
		{
			services.AddSingleton<LCUStateManager>();

			return services;
		}

		public static IApplicationBuilder UseApplicationContext(this IApplicationBuilder app)
		{
			return app.UseMiddleware<ApplicationContextMiddleware>();
		}

		public static IApplicationBuilder UseDAFApplication(this IApplicationBuilder app)
		{
			return app.UseMiddleware<DAFApplicationMiddleware>();
		}

		public static IApplicationBuilder UseLCU(this IApplicationBuilder app)
		{
			app.UseEnterpriseContext();

			app.UseLCUIdentityServer();

			app.UseAuthentication();

			app.UseApplicationContext();

			app.UseDAFApplication();

			app.UseLCUAuthorization();

			app.UseState();

			app.UseLCUMVC();

			return app;
		}

		public static IApplicationBuilder UseLCUMVC(this IApplicationBuilder app)
		{
			app.UseMvc(routes =>
			{
				//routes.MapRoute("dev-stream", "dev-stream/{*path}", defaults: new { controller = "LCU", action = "DevStream" });

				routes.MapRoute("github-oauth", ".github/oauth", defaults: new { controller = "LCU", action = "GitHubOAuth" });

				routes.MapRoute("github-confirm", ".github/authorize", defaults: new { controller = "LCU", action = "GitHubAuthorize" });

				routes.MapRoute("logout", "identity/logout", defaults: new { controller = "LCU", action = "Logout" });

				routes.MapRoute("api", "api/{prefix}/{*path}", defaults: new { controller = "LCU", action = "API" });

				routes.MapRoute("dfs", "{*path}", defaults: new { controller = "LCU", action = "DFS" });
			});

			return app;
		}

		public static IApplicationBuilder UseEnterpriseContext(this IApplicationBuilder app)
		{
			return app.UseMiddleware<EnterpriseContextMiddleware>();
		}

		public static IApplicationBuilder UseLCUAuthorization(this IApplicationBuilder app)
		{
			return app.UseMiddleware<LCUAuthorizationMiddleware>();
		}

		public static IApplicationBuilder UseLCUIdentityServer(this IApplicationBuilder app, string identityPath = "/.identity")
		{
			if (identityPath.IsNullOrEmpty())
			{
				return setupLCUIdentityServer(app);
			}
			else
				return app.Map(identityPath, builder =>
				{
					setupLCUIdentityServer(builder);
				});
		}

		public static IApplicationBuilder UseState(this IApplicationBuilder app, string stateEndpoint = "state/{state}/{key}/{*path}")
		{
			//return app.UseSignalR(routes =>
			//{
			//	routes.MapHub<LCUStateHub>("/state");
			//});

			return app.UseAzureSignalR(routes =>
			{
				routes.MapHub<LCUStateHub>("/state");
			});

			//return app.UseRouter(builder =>
			//{
			//	builder.MapMiddlewareRoute(stateEndpoint, proxyApp =>
			//	{
			//		//proxyApp.UseMiddleware<LCUStateMiddleware>();

			//	});
			//});
		}

		#region Helpers
		private static IApplicationBuilder setupLCUIdentityServer(IApplicationBuilder app)
		{
			app.UseIdentityServer();

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute("lcu-identity", "{action}/{*path}", defaults: new { controller = "LCUIdentity" });

				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});

			return app;
		}
		#endregion
	}
}
