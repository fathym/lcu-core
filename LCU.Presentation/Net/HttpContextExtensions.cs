using Fathym.Design.Factory;
using LCU.Graphs;
using LCU.Presentation.API;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
	public static class HttpContextExtensions
	{
		public static async Task CopyProxyHttpResponse(this HttpContext context, HttpResponseMessage responseMessage)
		{
			var response = context.Response;

			response.StatusCode = (int)responseMessage.StatusCode;

			foreach (var header in responseMessage.Headers)
				response.Headers[header.Key] = header.Value.ToArray();

			foreach (var header in responseMessage.Content.Headers)
				response.Headers[header.Key] = header.Value.ToArray();

			response.Headers.Remove("transfer-encoding");

			using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
				await responseStream.CopyToAsync(response.Body, 81920, context.RequestAborted).ConfigureAwait(false);
		}

		public static HttpRequestMessage CreateProxyHttpRequest(this HttpContext context, string uriString)
		{
			var uri = new Uri(uriString);
			var request = context.Request;

			var requestMessage = new HttpRequestMessage();
			var requestMethod = request.Method;
			if (!HttpMethods.IsGet(requestMethod) &&
				!HttpMethods.IsHead(requestMethod) &&
				!HttpMethods.IsDelete(requestMethod) &&
				!HttpMethods.IsTrace(requestMethod))
			{
				var streamContent = new StreamContent(request.Body);
				requestMessage.Content = streamContent;
			}

			// Copy the request headers.
			if (requestMessage.Content != null)
				foreach (var header in request.Headers)
					if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
						requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());

			requestMessage.Headers.Host = uri.Authority;
			requestMessage.RequestUri = uri;
			requestMessage.Method = new HttpMethod(request.Method);

			return requestMessage;
		}

		public static string GetProxiedAddress(this HttpContext context, DAFAPIContext dafApiCtxt, string path)
		{
			if (dafApiCtxt != null)
			{
				var apiPath = path.ToString().Replace(dafApiCtxt.InboundPath, String.Empty).TrimStart('/');

				var proxyPath = loadProxyAPIUri(apiPath, dafApiCtxt.APIRoot, context.Request.QueryString.ToString());

				if (!proxyPath.IsNullOrEmpty())
					loadSecurity(dafApiCtxt.Security, context);

				return proxyPath;
			}
			else
				return null;
		}

		public static TGraph LoadGraph<TGraph>(this HttpRequest req, ILogger log = null)
			where TGraph : LCUGraph
		{
			var regGraphConfig = req.LoadGraphConfig();

			if (log != null)
				log.LogInformation($"Loaded graph configuration: {regGraphConfig.ToJSON()}");

			return new ActivatorFactory<TGraph>().Create(regGraphConfig);
		}

		public static LCUGraphConfig LoadGraphConfig(this HttpRequest req)
		{
			return new LCUGraphConfig()
			{
				APIKey = Environment.GetEnvironmentVariable("LCU-GRAPH-API-KEY"),
				Host = Environment.GetEnvironmentVariable("LCU-GRAPH-HOST"),
				Database = Environment.GetEnvironmentVariable("LCU-GRAPH-DATABASE"),
				Graph = Environment.GetEnvironmentVariable("LCU-GRAPH")
			};
		}

		public static string LoadUserID(this HttpContext context)
		{
			return context.User?.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
		}

		public static async Task<HttpResponseMessage> SendProxyHttpRequest(this HttpContext context, string proxiedAddress)
		{
			var proxiedRequest = context.CreateProxyHttpRequest(proxiedAddress);

			return await new HttpClient().SendAsync(proxiedRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
		}

		public static async Task SetJSONResponse(this HttpContext context, JToken body)
		{
			var response = context.Response;

			response.StatusCode = StatusCodes.Status200OK;

			response.Headers["Content-Type"] = "application/json";

			response.Headers.Remove("transfer-encoding");

			using (var streamWriter = new StreamWriter(response.Body))
				await streamWriter.WriteAsync(body.ToObject<object>().ToJSON());
		}

		#region Helpers
		private static string loadProxyAPIUri(string apiPath, string apiRoot, string query)
		{
			var apiUri = new UriBuilder($"{apiRoot}/{apiPath}");

			apiUri.Query = query;

			return apiUri.ToString();
		}

		private static void loadSecurity(string security, HttpContext context)
		{
			if (!security.IsNullOrEmpty())
			{
				var securityParts = security.Split('~');

				var securityKey = securityParts[0];

				var securityValue = securityParts[1];

				if (!securityKey.IsNullOrEmpty() && !securityValue.IsNullOrEmpty())
					context.Request.Headers[securityKey] = securityValue;
			}
		}
		#endregion
	}
}
