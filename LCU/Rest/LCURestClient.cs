using LCU.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Rest
{
	public class LCURestClient : IDisposable
	{
		#region Fields
		protected readonly IBearerTokenProvider bearerTokenProvider;

		protected readonly ILogger logger;

		protected readonly HttpClient web;
		#endregion

		#region Constructors
		public LCURestClient(string apiRoot, ILogger logger, string bearerToken = null)
			: this(apiRoot, logger, new StaticBearerTokenProvider(bearerToken))
		{
			bearerTokenProvider = null;
		}

		public LCURestClient(string apiRoot, ILogger logger, IBearerTokenProvider bearerTokenProvider)
		{
			this.bearerTokenProvider = bearerTokenProvider;

			this.logger = logger;

			//	TODO: Figure out how to get the TimeoutHandler to honor a SetTimeout from here?
			//var handler = new TimeoutHandler
			//{
			//	InnerHandler = new LoggingHandler(new HttpClientHandler(), this.logger)
			//};

			var handler = new LoggingHandler(new HttpClientHandler(), this.logger);

			web = new HttpClient(handler);

			SetTimeout(TimeSpan.FromMinutes(60));

			ensureBearerTokenFromProvider().Wait();

			web.BaseAddress = new Uri(apiRoot);
		}
		#endregion

		#region API Methods
		public virtual void ClearAuthorization()
		{
			web.DefaultRequestHeaders.Authorization = null;
		}

		public virtual async Task<T> Delete<T>(string requestUri)
			where T : class
		{
			await ensureBearerTokenFromProvider();

			var response = await web.DeleteAsync(requestUri);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<T>();
		}

		public virtual void Dispose()
		{
			web.Dispose();
		}

		public virtual async Task<T> Get<T>(string requestUri)
			where T : class
		{
			await ensureBearerTokenFromProvider();

			var respStr = await web.GetStringAsync(requestUri);

			return respStr?.FromJSON<T>();
		}

		public virtual async Task<TResp> Patch<TReq, TResp>(string requestUri, TReq request)
			where TResp : class
		{
			await ensureBearerTokenFromProvider();

			var response = await web.PatchAsJsonAsync(requestUri, request);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<TResp>();
		}

		public virtual async Task<TResp> Post<TReq, TResp>(string requestUri, TReq request)
			where TResp : class
		{
			await ensureBearerTokenFromProvider();

			var response = await web.PostAsJsonAsync(requestUri, request);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<TResp>();
		}

		public virtual async Task<TResp> Put<TReq, TResp>(string requestUri, TReq request)
			where TResp : class
		{
			await ensureBearerTokenFromProvider();

			var response = await web.PutAsJsonAsync(requestUri, request);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<TResp>();
		}

		public virtual async Task<HttpResponseMessage> Send(HttpRequestMessage request)
		{
			await ensureBearerTokenFromProvider();

			var response = await web.SendAsync(request);

			return response;
		}

		public virtual void SetAuthorization(string token, string key)
		{
			web.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(key, token);
		}

		public virtual void SetTimeout(TimeSpan timeout)
		{
			web.Timeout = timeout;
		}

		public virtual async Task<TResp> With<TResp>(Func<HttpClient, Task<TResp>> action)
			where TResp : class
		{
			await ensureBearerTokenFromProvider();

			return await action(web);
		}
		#endregion

		#region Helpers
		protected virtual async Task ensureBearerTokenFromProvider()
		{
			if (bearerTokenProvider != null)
			{
				var bearerToken = await bearerTokenProvider.Load();

				if (!bearerToken.IsNullOrEmpty())
					SetAuthorization(bearerToken, "Bearer");
				else
					ClearAuthorization();
			}
		}
		#endregion
	}
}
