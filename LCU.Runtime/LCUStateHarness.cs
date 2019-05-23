using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fathym;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.IDE;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LCU.Runtime
{
    public abstract class LCUStateHarness<TState>
    {
		#region Fields
		protected readonly ApplicationGraph appGraph;

		protected readonly EnterpriseGraph entGraph;

		protected readonly LCUStateDetails details;

		const string lcuPathRoot = "_lcu";

		protected readonly ILogger log;

		protected readonly HttpRequest req;

		protected readonly TState state;
        #endregion

        #region Constructors
        public LCUStateHarness(HttpRequest req, ILogger log, TState state)
        {
            this.details = req.LoadStateDetails();

            this.log = log;

            this.req = req;

			this.state = state;

			appGraph = req.LoadGraph<ApplicationGraph>(log);

			entGraph = req.LoadGraph<EnterpriseGraph>(log);
		}
		#endregion

		#region API Methods
		public virtual TState Eject()
        {
            return state;
        }

        public virtual async Task<TState> WhenAll(params Task<TState>[] stateActions)
        {
            var states = await stateActions.WhenAll();

            return state;
        }
		#endregion

		#region Helpers
		protected async Task<Status> ensureApplication(LowCodeUnitConfig lcu)
		{
			var apps = await appGraph.ListApplications(details.EnterpriseAPIKey);

			var lcuApp = apps?.FirstOrDefault(a => a.PathRegex == $"/{lcuPathRoot}/{lcu.Lookup}*");

			if (lcuApp == null)
			{
				lcuApp = await appGraph.Save(new Application()
				{
					Name = lcu.Lookup,
					PathRegex = $"/{lcuPathRoot}/{lcu.Lookup}*",
					Priority = apps.IsNullOrEmpty() ? 5000 : apps.First().Priority + 500,
					Hosts = new List<string>() { details.Host },
					EnterprisePrimaryAPIKey = details.EnterpriseAPIKey
				});
			}

			if (lcuApp != null)
			{
				var dafApps = await appGraph.GetDAFApplications(details.EnterpriseAPIKey, lcuApp.ID);

				var dafApp = dafApps?.FirstOrDefault(a => a.Metadata["BaseHref"].ToString() == $"/{lcuPathRoot}/{lcu.Lookup}/");

				if (dafApp == null)
					dafApp = new DAFViewConfiguration()
					{
						ApplicationID = lcuApp.ID,
						BaseHref = $"/{lcuPathRoot}/{lcu.Lookup}/",
						NPMPackage = lcu.NPMPackage,
						PackageVersion = lcu.PackageVersion,
						Priority = 10000
					}.JSONConvert<DAFApplicationConfiguration>();
				else
				{
					dafApp.Metadata["NPMPackage"] = lcu.NPMPackage;

					dafApp.Metadata["PackageVersion"] = lcu.PackageVersion;
				}

				var view = dafApp.JSONConvert<DAFViewConfiguration>();

				var status = await unpackView(view, details.EnterpriseAPIKey);

				if (status)
				{
					dafApp = appGraph.SaveDAFApplication(details.EnterpriseAPIKey, view.JSONConvert<DAFApplicationConfiguration>()).Result;

					if (dafApp != null)
						lcu.PackageVersion = dafApp.Metadata["PackageVersion"].ToString();
				}
				else
					return status;
			}

			return Status.Success;
		}

		protected virtual async Task<Status> unpackView(DAFViewConfiguration viewApp, string entApiKey)
		{
			if (viewApp.PackageVersion != "dev-stream")
			{
				log.LogInformation($"Unpacking view: {viewApp.ToJSON()}");

				var ent = await entGraph.LoadByPrimaryAPIKey(entApiKey);

				var client = new HttpClient();

				var npmUnpackUrl = Environment.GetEnvironmentVariable("NPM-PUBLIC-URL");

				var npmUnpackCode = Environment.GetEnvironmentVariable("NPM-PUBLIC-CODE");

				var npmUnpack = $"{npmUnpackUrl}/api/npm-unpack?code={npmUnpackCode}&pkg={viewApp.NPMPackage}&version={viewApp.PackageVersion}";

				npmUnpack += $"&applicationId={viewApp.ApplicationID}&enterpriseId={ent.ID}";

				log.LogInformation($"Running NPM Unpack at: {npmUnpack}");

				var response = await client.GetAsync(npmUnpack);

				var statusStr = await response.Content.ReadAsStringAsync();

				log.LogInformation($"NPM Unpack Response: {statusStr}");

				var status = statusStr.IsNullOrEmpty() || statusStr.StartsWith("<") ? Status.GeneralError.Clone(statusStr) : statusStr.FromJSON<Status>();

				if (status)
					viewApp.PackageVersion = status.Metadata["Version"].ToString();

				log.LogInformation($"NPM Unpacked: {status.ToJSON()}");

				return status;
			}
			else
				return Status.Success.Clone("Success", new { PackageVersion = viewApp.PackageVersion });
		}
		#endregion
	}
}