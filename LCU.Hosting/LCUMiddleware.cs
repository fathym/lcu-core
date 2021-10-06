using LCU.Hosting.Monitors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCU.Hosting
{
    public abstract class LCUMiddleware
    {
        #region Fields
        protected readonly MiddlewareTimerMonitor eventSource;

        protected readonly ILogger logger;

        protected readonly RequestDelegate next;
        #endregion

        #region Constructors
        public LCUMiddleware(RequestDelegate next, ILogger logger, MiddlewareTimerMonitor eventSource = null)
        {
            this.eventSource = eventSource;

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }
        #endregion

        #region API Methods
        public virtual async Task Invoke(HttpContext httpContext)
        {
            var stopwatch = new Stopwatch();

            if (eventSource != null)
                stopwatch.Start();

            try
            {
                logger.LogDebug("Invoking middleware");

                await invoke(httpContext);

                logger.LogDebug("Processing middleware response");

                await processResponse(httpContext);

                logger.LogDebug("Invoked middleware");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error invoking the middleware");

                throw;
            }
            finally
            {
                if (eventSource != null)
                {
                    stopwatch.Stop();

                    eventSource?.Request(stopwatch.ElapsedMilliseconds, loadEventSourceArguments(httpContext));
                }
            }
        }
        #endregion

        #region helpers
        protected virtual async Task invoke(HttpContext httpContext)
        {
            logger.LogDebug("Calling next for middleware");

            await next(httpContext);

            logger.LogDebug("Called next for middleware");
        }

        protected virtual object[] loadEventSourceArguments(HttpContext httpContext)
        {
            return new object[]
            {
                httpContext.Request.GetEncodedPathAndQuery(),
                httpContext.TraceIdentifier
            };
        }

        protected virtual async Task processResponse(HttpContext httpContext)
        {
            //  Process response configurations
        }
        #endregion
    }
}
