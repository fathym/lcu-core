using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
		public virtual async Task<T> Get<T>(string requestUri)
			where T : class
		{
			var entCtxtStr = await web.GetStringAsync(requestUri);

			return entCtxtStr?.FromJSON<T>();
		}
		#endregion
	}
}
