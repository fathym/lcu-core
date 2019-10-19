using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Logging
{
	public class LoggingHandler : DelegatingHandler
	{
		#region Fields
		protected readonly ILogger logger;
		#endregion

		#region Constructors
		public LoggingHandler(HttpMessageHandler innerHandler, ILogger logger)
			: base(innerHandler)
		{
			this.logger = logger;
		}
		#endregion

		#region Helpers
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			logger.LogInformation($"Request: {request.ToString()}");

			if (request.Content != null)
				logger.LogTrace(await request.Content.ReadAsStringAsync());

			var response = await base.SendAsync(request, cancellationToken);

			logger.LogInformation($"Response: {response.ToString()}");

			if (response.Content != null)
				logger.LogTrace(await response.Content.ReadAsStringAsync());

			return response;
		} 
		#endregion
	}
}
