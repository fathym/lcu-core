﻿using LCU.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Rest
{
	public class LCURestClient : IDisposable
	{
		#region Fields
		protected readonly ILogger logger;

		protected readonly HttpClient web;
		#endregion

		#region Constructors
		public LCURestClient(string apiRoot, ILogger logger, string bearerToken = null)
		{
			this.logger = logger;

			var handler = new TimeoutHandler
			{
				InnerHandler = new LoggingHandler(new HttpClientHandler(), this.logger)
			};

			web = new HttpClient(handler);

			SetTimeout(Timeout.InfiniteTimeSpan);

			if (!bearerToken.IsNullOrEmpty())
				web.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

			web.BaseAddress = new Uri(apiRoot);
		}
		#endregion

		#region API Methods
		public virtual async Task<T> Delete<T>(string requestUri)
			where T : class
		{
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
			var respStr = await web.GetStringAsync(requestUri);

			return respStr?.FromJSON<T>();
		}

		public virtual async Task<TResp> Post<TReq, TResp>(string requestUri, TReq request)
			where TResp : class
		{
			var response = await web.PostAsJsonAsync(requestUri, request);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<TResp>();
		}

		public virtual async Task<TResp> Put<TReq, TResp>(string requestUri, TReq request)
			where TResp : class
		{
			var response = await web.PutAsJsonAsync(requestUri, request);

			var respStr = await response.Content.ReadAsStringAsync();

			return respStr?.FromJSON<TResp>();
		}

		public virtual void SetTimeout(TimeSpan timeout)
		{
			web.Timeout = timeout;
		}

		public virtual async Task<TResp> With<TResp>(Func<HttpClient, Task<TResp>> action)
			where TResp : class
		{
			return await action(web);
		}
		#endregion
	}
}
