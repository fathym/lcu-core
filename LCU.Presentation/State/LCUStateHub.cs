using Fathym;
using LCU.Graphs.Registry.Enterprises.State;
using LCU.Presentation.Enterprises;
using LCU.Presentation.State.ReqRes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Presentation.State
{
	public class LCUStateHub : Hub
	{
		#region Fields
		protected readonly LCUStateManager stateMgr;
		#endregion

		#region Constructors
		public LCUStateHub(LCUStateManager stateMgr)
		{
			this.stateMgr = stateMgr;
		}
		#endregion

		#region Runtime
		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();
		}
		#endregion

		#region API Methods
		public virtual async Task ConnectToState(ConnectToStateRequest request)
		{
			await groupClient(request.State, request.Key);

			Context.Items["Environment"] = request.Environment;

			if (request.ShouldSend.HasValue && request.ShouldSend.Value)
				await sendState(Context.GetHttpContext(), request.State, request.Key);
		}

		public virtual async Task ExecuteAction(ExecuteActionRequest request)
		{
			await handleAction(Context.GetHttpContext(), request.State, request.Key, request.Type,
				request.Arguments);
		}
		#endregion

		#region Helpers
		protected virtual async Task<string> buildGroupName(string stateName, string stateKey,
			LCUStateConfiguration stateCfg = null)
		{
			var context = Context.GetHttpContext();

			var entApiKey = loadEntApiKey(context);

			if (stateCfg == null)
				stateCfg = await loadStateConfig(context, stateName);

			var username = stateCfg.UseUsername ? loadUsername(context) : null;

			return $"{entApiKey}|{stateName}|{stateKey}|{username}";
		}

		protected virtual async Task groupClient(string stateName, string stateKey)
		{
			var groupName = await buildGroupName(stateName, stateKey);

			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

		protected virtual async Task handleAction(HttpContext context, string stateName, string stateKey, string actionName,
			MetadataModel arguments)
		{
			var stateCfg = await loadStateConfig(context, stateName);

			if (stateCfg?.Actions != null && stateCfg.Actions.ContainsKey(actionName))
			{
				var username = stateCfg.UseUsername ? loadUsername(context) : null;

				var action = stateCfg.Actions[actionName];

				var proxiedAddress = loadProxiedAddress(context, action, stateCfg, stateName, stateKey, username);

				if (!proxiedAddress.IsNullOrEmpty())
				{
					var client = loadSecureProxyClient(context, action, stateCfg);

					//	TODO:  serialize response and do something with it on top of returning the state
					var actionResp = await client.PostAsJsonAsync(proxiedAddress, arguments);

					var responseBody = await actionResp.Content.ReadAsStringAsync();
				}
			}

			await sendState(context, stateName, stateKey);
		}

		protected virtual HttpClient loadSecureProxyClient(HttpContext context, LCUStateAction action, LCUStateConfiguration stateCfg)
		{
			var client = new HttpClient();

			client.Timeout = new TimeSpan(1, 0, 0);

			var env = loadEnvironment(stateCfg);

			var security = action.Security;

			if (security.IsNullOrEmpty() && !env.IsNullOrEmpty())
				security = stateCfg.Environments[env].Security;

			if (!security.IsNullOrEmpty())
			{
				var securityParts = security.Split('~');

				var securityKey = securityParts[0];

				var securityValue = securityParts[1];

				if (!securityKey.IsNullOrEmpty() && !securityValue.IsNullOrEmpty())
					client.DefaultRequestHeaders.Add(securityKey, securityValue);
			}

			return client;
		}

		protected virtual string loadEnvironment(LCUStateConfiguration stateCfg)
		{
			var env = Context.Items.ContainsKey("Environment") ? Context.Items["Environment"]?.ToString() : null;

			if (env.IsNullOrEmpty())
				env = stateCfg.ActiveEnvironment;

			return env;
		}

		protected virtual string loadProxiedAddress(HttpContext context, LCUStateAction action,
			LCUStateConfiguration stateCfg, string stateName, string stateKey, string username)
		{
			var apiRoot = action.APIRoot;

			var env = loadEnvironment(stateCfg);

			if (!apiRoot.StartsWith("http") && !env.IsNullOrEmpty())
				apiRoot = $"{stateCfg.Environments[env].ServerAPIRoot}{action.APIRoot}";

			var security = action.Security;

			if (security.IsNullOrEmpty() && !env.IsNullOrEmpty())
				security = stateCfg.Environments[env].Security;

			var proxiedAddress = context.GetProxiedAddress(new API.DAFAPIContext()
			{
				APIRoot = apiRoot,
				InboundPath = context.Request.Path.ToString(),
				Methods = new List<string> { context.Request.Method },
				Security = security
			}, "");

			if (!proxiedAddress.IsNullOrEmpty())
			{
				var entApiKey = loadEntApiKey(context);

				var appEntApiKey = loadAppEntApiKey(context);

				var host = loadEntHost(context);

				var uriBldr = new UriBuilder(proxiedAddress);

				uriBldr.Query += $"&entApiKey={entApiKey}";

				uriBldr.Query += $"&appEntApiKey={appEntApiKey}";

				uriBldr.Query += $"&state={stateName}";

				uriBldr.Query += $"&key={stateKey}";

				uriBldr.Query += $"&host={host}";

				if (!username.IsNullOrEmpty())
					uriBldr.Query += $"&username={username}";

				proxiedAddress = uriBldr.ToString();

				return proxiedAddress;
			}
			else
				return null;
		}

		protected virtual string loadEntApiKey(HttpContext context)
		{
			var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			return context.User?.Claims?.FirstOrDefault(c => c.Type == "lcu-ent-api-key")?.Value ?? entCtxt?.PrimaryAPIKey;
		}

		protected virtual string loadAppEntApiKey(HttpContext context)
		{
			var appCtxt = context.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(context));

			return context.User?.Claims?.FirstOrDefault(c => c.Type == "lcu-app-ent-api-key")?.Value ??
				appCtxt?.EnterprisePrimaryAPIKey;
		}

		protected virtual string loadEntHost(HttpContext context)
		{
			var entCtxt = context.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			return entCtxt?.Host ?? context.User?.Claims?.FirstOrDefault(c => c.Type == "lcu-host")?.Value;
		}

		protected virtual async Task<JToken> loadState(HttpContext context, string stateName, string stateKey)
		{
			var entApiKey = loadEntApiKey(context);

			var stateCfg = await loadStateConfig(context, stateName);

			var username = stateCfg.UseUsername ? loadUsername(context) : null;

			var stateRef = stateMgr.LoadStateRef(entApiKey, stateName, stateKey, username);

			//	TODO:  Update to proxy to some configured API
			var state = await stateMgr.LoadState<JToken>(stateRef);

			//	TODO:  Need a more sophisticated way (i think)... certainly needed a different way to pass in 'path'
			//		Concept should be like Redux Reducers?
			//if (state != null && !path.IsNullOrEmpty())
			//{
			//	var dotPath = path.Replace('/', '.');

			//	state = state.SelectToken($"$.{dotPath}");
			//}

			return state;
		}

		protected virtual async Task<LCUStateConfiguration> loadStateConfig(HttpContext context, string stateName)
		{
			var entApiKey = loadAppEntApiKey(context) ?? loadEntApiKey(context);

			return await stateMgr.LoadStateConfig(entApiKey, stateName);
		}

		protected virtual string loadUsername(HttpContext context)
		{
			return context.LoadUserID();
		}

		protected virtual async Task sendState(HttpContext context, string stateName, string stateKey)
		{
			var state = await loadState(context, stateName, stateKey);

			var request = new ReceiveStateRequest();

			LCUStateConfiguration stateCfg = null;

			if (state != null)
				request.State = state.JSONConvert<IDictionary<string, object>>();
			else
			{
				stateCfg = await loadStateConfig(context, stateName);

				request.State = stateCfg?.DefaultValue;
			}

			var groupName = await buildGroupName(stateName, stateKey, stateCfg);

			await Clients.Group(groupName).SendAsync($"ReceiveState{stateName}{stateKey}", request.JSONConvert<object>());
		}
		#endregion
	}
}
