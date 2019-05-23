using LCU.Presentation.State;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace LCU.Presentation.State
{
	public class LCUStateMiddleware
	{
		#region Fields
		protected readonly IDistributedCache cache;

		protected readonly IMemoryCache memCache;

		protected readonly RequestDelegate next;

		protected readonly LCUStateManager stateMgr;
		#endregion

		#region Constructors
		public LCUStateMiddleware(RequestDelegate next, IDistributedCache cache, IMemoryCache memCache, LCUStateManager stateMgr)
		{
			this.cache = cache;

			this.memCache = memCache;

			this.next = next;

			this.stateMgr = stateMgr;

		}
		#endregion

		#region API Methods
		public virtual async Task Invoke(HttpContext context)
		{
			//if (HttpMethods.IsGet(context.Request.Method))
			//	await sendState(context);
			//else if (HttpMethods.IsPost(context.Request.Method))
			//	await handleAction(context);
			//else if (context.WebSockets.IsWebSocketRequest)
			//	await handleSocket(context);
			//else
			await next(context);

			//	TODO:  Should handle web sockets, and then socket should listen for blob changes

			//	TODO:  Ideally this would all happen in a per call ordering, so we should probably post to a queue here 
			//		and have the logic for invoking actions run in a separate process from that queue... We should also then ensure 
			//		some level of state sequence monitoring that prevents calls that are out of date from succeeding...  and lots more...

		}
		#endregion

	}
}
