using Fathym;
using Fathym.Design.Factory;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.State;
using LCU.Presentation.State;
using LCU.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
	public static class LCURuntimeHttpRequestExtensions
	{
		public static LCUStateDetails LoadStateDetails(this HttpRequest req)
		{
			return new LCUStateDetails()
			{
				EnterpriseAPIKey = req.Query["entApiKey"],
				ApplicationEnterpriseAPIKey = req.Query["appEntApiKey"],
				Host = req.Query["host"],
				StateKey = req.Query["key"],
				StateName = req.Query["state"],
				Username = req.Query["username"]
			};
		}

		public static LCUStateConfiguration LoadStateConfig(this HttpRequest req)
		{
			return (LCUStateConfiguration)req.HttpContext.Items["LCU:StateConfig"];
		}

		public static async Task<T> LoadStateArguments<T>(this HttpRequest req)
		{
			var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

			return requestBody.FromJSON<T>();
		}

		public static TMgr Manage<TState, TMgr>(this HttpRequest req, TState state, ILogger log)
			where TMgr : LCUStateHarness<TState>
		{
			return new ActivatorFactory<TMgr>().Create(req, log, state);
		}

		public static async Task<IActionResult> Manage<TArgs, TState, TMgr>(this HttpRequest req, ILogger log,
			Func<TMgr, TArgs, Task<TState>> action)
			where TState : class
			where TMgr : LCUStateHarness<TState>
		{
			return await req.WithState<TArgs, TState>(log, async (details, reqData, state, stateMgr) =>
			{
				var mgr = req.Manage<TState, TMgr>(state, log);

				return await action(mgr, reqData);
			});
		}

		public static async Task<IActionResult> WithState<TArgs, TState>(this HttpRequest req, ILogger log,
			Func<LCUStateDetails, TArgs, TState, LCUStateManager, Task<TState>> action,
			string storageConnection = "LCU-STORAGE-CONNECTION")
			where TState : class
		{
			log.LogInformation("Executing a request with State boundary.");

			var status = Status.Initialized;

			try
			{
				var stateDetails = req.LoadStateDetails();

				var reqData = await req.LoadStateArguments<TArgs>();

				var storageAcc = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(storageConnection));

				var stateMgr = new LCUStateManager(storageAcc);

				var stateCfg = await stateMgr.LoadStateConfig(stateDetails.ApplicationEnterpriseAPIKey, stateDetails.StateName);

				req.HttpContext.Items["LCU:StateConfig"] = stateCfg;

				var stateRef = stateMgr.LoadStateRef(stateDetails.EnterpriseAPIKey, stateDetails.StateName, stateDetails.StateKey,
					stateDetails.Username);

				var state = await stateMgr.LoadState<TState>(stateRef);

				if (state == null)
					state = stateCfg?.DefaultValue?.JSONConvert<TState>();

				log.LogInformation($"Current state: {state.ToJSON()}");

				state = await action(stateDetails, reqData, state, stateMgr);

				log.LogInformation($"Target state: {state.ToJSON()}");

				if (req.HttpContext.Items.ContainsKey("LCU:Status"))
					status = (Status)req.HttpContext.Items["LCU:Status"];
				else
					status = Status.Success;

				if (status)
					await stateMgr.SaveState(stateRef, state);
			}
			catch (Exception ex)
			{
				status = Status.GeneralError.Clone(ex.ToString());
			}

			log.LogInformation($"State boundary request executed: {status.ToJSON()}");

			return new JsonResult(status);
		}
	}

	public class LCUStateDetails
	{
		public virtual string ApplicationEnterpriseAPIKey { get; set; }

		public virtual string EnterpriseAPIKey { get; set; }

		public virtual string Host { get; set; }

		public virtual string StateKey { get; set; }

		public virtual string StateName { get; set; }

		public virtual string Username { get; set; }
	}
}
