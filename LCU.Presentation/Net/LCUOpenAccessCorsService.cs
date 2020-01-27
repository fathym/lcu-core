using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LCU.Presentation.Net
{
	public class LCUOpenAccessCorsService : CorsService, ICorsService
	{
		public LCUOpenAccessCorsService(IOptions<CorsOptions> options, ILoggerFactory loggerFactory)
			: base(options, loggerFactory)
		{ } 

		public override void EvaluateRequest(HttpContext context, CorsPolicy policy, CorsResult result)
		{
			base.EvaluateRequest(context, policy, result);

			//if (result.AllowedOrigin == "*")
				result.AllowedOrigin = context.Request.Headers["Origin"].ToString().TrimEnd('/');
		}

		public override void EvaluatePreflightRequest(HttpContext context, CorsPolicy policy, CorsResult result)
		{
			base.EvaluatePreflightRequest(context, policy, result);

			//if (result.AllowedOrigin == "*")
				result.AllowedOrigin = context.Request.Headers["Origin"].ToString().TrimEnd('/');
		}
	}
}
