﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Rest
{
	public class RestClient
	{
		#region Fields
		protected readonly HttpClient web;
		#endregion

		#region Constructors
		public RestClient(string apiRoot)
		{
			web = new HttpClient();

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

		public virtual async Task<TResp> With<TResp>(Func<HttpClient, Task<TResp>> action)
			where TResp : class
		{
			return await action(web);
		}
		#endregion
	}
}
