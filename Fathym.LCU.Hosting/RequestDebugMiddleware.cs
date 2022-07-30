using Fathym.LCU.Hosting.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.LCU.Hosting
{
    public class RequestDebugMiddleware : LCUMiddleware
    {
        #region Fields
        protected readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        protected readonly LCUStartupOptions startupOptions;
        #endregion

        #region Constructors
        public RequestDebugMiddleware(RequestDelegate next, ILogger<RequestDebugMiddleware> logger, IOptions<LCUStartupOptions> startupOptions)
            : base(next, logger)
        {
            recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

            this.startupOptions = startupOptions?.Value ?? throw new ArgumentNullException(nameof(startupOptions));
        }
        #endregion

        #region Helpers
        protected override async Task invoke(HttpContext httpContext)
        {
            logger.LogDebug("Executing debug middleware");

            //var originalBodyStream = httpContext.Response.Body;

            //await using var responseBody = recyclableMemoryStreamManager.GetStream();

            try
            {
                //httpContext.Response.Body = responseBody;

                await logRequest(httpContext);

                await base.invoke(httpContext);

                await logResponse(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an issue debugging the request");

                if (startupOptions.Global.Debug != null && !startupOptions.Global.Debug.ThrowExceptions)
                    await writeException(httpContext, ex);
                else
                    throw;
            }

            //await responseBody.CopyToAsync(originalBodyStream);
        }

        protected virtual async Task logRequest(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            await using var requestStream = recyclableMemoryStreamManager.GetStream();

            await httpContext.Request.Body.CopyToAsync(requestStream);

            var log = new StringBuilder();

            log.AppendLine($"Http Request Information: {httpContext.Request} ");
            log.AppendLine($"\tMethod: {httpContext.Request.Method}");
            log.AppendLine($"\tSchema:{httpContext.Request.Scheme}");
            log.AppendLine($"\tHost: {httpContext.Request.Host}");
            log.AppendLine($"\tPath: {httpContext.Request.Path}");
            log.AppendLine($"\tQueryString: {httpContext.Request.QueryString}");
            log.AppendLine($"\tHeaders: {httpContext.Request.Headers.ToJSON()}");
            log.AppendLine($"\tRequest Body: {readStreamInChunks(requestStream)}");

            logger.LogDebug(log.ToString());

            if (httpContext.Response.Body.CanSeek)
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        protected virtual async Task logResponse(HttpContext httpContext)
        {
            //if (httpContext.Response.Body.CanSeek)
            //    httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            //var text = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            //if (httpContext.Response.Body.CanSeek)
            //    httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            var log = new StringBuilder();

            log.AppendLine($"Http Response Information: {httpContext.Request} ");
            log.AppendLine($"\tMethod: {httpContext.Request.Method}");
            log.AppendLine($"\tSchema:{httpContext.Request.Scheme}");
            log.AppendLine($"\tHost: {httpContext.Request.Host}");
            log.AppendLine($"\tPath: {httpContext.Request.Path}");
            log.AppendLine($"\tQueryString: {httpContext.Request.QueryString}");
            log.AppendLine($"\tHeaders: {httpContext.Request.Headers.ToJSON()}");
            //log.AppendLine($"\tResponse Body: {text}");

            logger.LogDebug(log.ToString());
        }

        protected virtual string readStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }

        protected virtual async Task writeException(HttpContext httpContext, Exception ex)
        {
            var log = new StringBuilder();

            log.AppendLine($"Http Exception Information: {httpContext.Request} ");
            log.AppendLine($"\tMethod: {httpContext.Request.Method}");
            log.AppendLine($"\tSchema:{httpContext.Request.Scheme}");
            log.AppendLine($"\tHost: {httpContext.Request.Host}");
            log.AppendLine($"\tPath: {httpContext.Request.Path}");
            log.AppendLine($"\tQueryString: {httpContext.Request.QueryString}");
            log.AppendLine($"\tHeaders: {httpContext.Request.Headers.ToJSON()}");
            log.AppendLine($"\tException: {ex}");

            logger.LogDebug(log.ToString());

            throw new Exception(log.ToString());
            //await httpContext.Response.Body.WriteAsync(Encoding.Default.GetBytes(log.ToString()));
        }
        #endregion
    }
}
