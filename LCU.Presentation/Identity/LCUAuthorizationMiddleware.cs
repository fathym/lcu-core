using Fathym;
using LCU.Presentation.DFS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace LCU.Presentation.Identity
{
	public class LCUAuthorizationMiddleware
	{
		#region Fields
		protected readonly IAuthorizationService authSvc;

		protected readonly IDistributedCache cache;

		protected readonly IMemoryCache memCache;

		protected readonly RequestDelegate next;
		#endregion

		#region Constructors
		public LCUAuthorizationMiddleware(RequestDelegate next, IDistributedCache cache, IMemoryCache memCache, IAuthorizationService authSvc)
		{
			this.authSvc = authSvc;

			this.cache = cache;

			this.memCache = memCache;

			this.next = next;
		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			var lcuAuthCtxt = context.ResolveContext<LCUAuthorizationContext>(LCUAuthorizationContext.Lookup);

			var authorized = lcuAuthCtxt == null ? true : true;

			if (!authorized && !context.Request.Path.Value.Contains("forge"))
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;

				var properties = new AuthenticationProperties();

				if (!lcuAuthCtxt.Schemes.IsNullOrEmpty())
					foreach (var scheme in lcuAuthCtxt.Schemes)
						await context.ChallengeAsync(scheme, properties);
				else
					await context.ChallengeAsync(properties);
			}
			else
				await next(context);
		}
		#endregion

		#region Helpers
		protected virtual async Task<Status> saveToDistributedFileSystem(CloudBlobDirectory appDirectory, DFSContext dfsCtxt,
			ContentDisposition content, Stream fileContents)
		{
			var appBlob = appDirectory.GetBlockBlobReference(content.FileName);

			await appBlob.UploadFromStreamAsync(fileContents);

			return Status.Success.Clone($"{content.FileName} saved to the dev-stream");
		}
		#endregion
	}
}
