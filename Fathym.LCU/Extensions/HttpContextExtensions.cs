using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static async Task CopyProxyHttpResponse(this HttpContext context, HttpResponseMessage responseMessage, Stream responseStream = null)
        {
            if (responseStream == null)
                responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var header in responseMessage.Headers)
                response.Headers[header.Key] = header.Value.ToArray();

            foreach (var header in responseMessage.Content.Headers)
                response.Headers[header.Key] = header.Value.ToArray();

            response.Headers.Remove("transfer-encoding");

            if (responseStream.CanSeek)
                responseStream.Seek(0, SeekOrigin.Begin);

            await responseStream.CopyToAsync(response.Body).ConfigureAwait(false);
        }

        public static HttpRequestMessage CreateProxyHttpRequest(this HttpContext context, string uriString)
        {
            var uri = new Uri(uriString);
            var request = context.Request;

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in request.Headers)
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }

        public static string GetProxiedAddress(this HttpContext context, string inboundPath, string apiRoot, string security, string path)
        {
            var apiPath = !inboundPath.IsNullOrEmpty() ? Regex.Replace(path, $"^{inboundPath}", String.Empty) : path;

            var proxyPath = loadProxyAPIUri(apiPath, apiRoot, context.Request.QueryString.ToString());

            if (!proxyPath.IsNullOrEmpty())
                proxyPath = loadSecurity(proxyPath, security, context);

            return proxyPath;
        }

        public static string LoadUserID(this HttpContext context)
        {
            return context.User?.Claims?.FirstOrDefault(c => c.Type == "emails")?.Value.Split(",").First();
        }

        public static async Task<HttpResponseMessage> SendProxyHttpRequest(this HttpContext context, string proxiedAddress, TimeSpan? timeout = null)
        {
            var proxiedRequest = context.CreateProxyHttpRequest(proxiedAddress);

            return await new HttpClient()
            {
                Timeout = timeout ?? TimeSpan.FromMinutes(60)
            }.SendAsync(proxiedRequest, HttpCompletionOption.ResponseHeadersRead);//, context.RequestAborted);
        }

        public static async Task SetJSONResponse(this HttpContext context, JsonNode body)
        {
            var response = context.Response;

            response.StatusCode = StatusCodes.Status200OK;

            response.Headers["Content-Type"] = "application/json";

            response.Headers.Remove("transfer-encoding");

            using (var streamWriter = new StreamWriter(response.Body))
                await streamWriter.WriteAsync(body.AsObject().ToJSON());
        }

        #region Helpers
        private static string loadProxyAPIUri(string apiPath, string apiRoot, string query)
        {
            var apiUri = new UriBuilder($"{apiRoot.TrimEnd('/')}/{apiPath.TrimStart('/')}");

            apiUri.Query = query;

            return apiUri.ToString();
        }

        private static string loadSecurity(string proxyPath, string security, HttpContext context)
        {
            if (!security.IsNullOrEmpty())
            {
                var securityParts = security.Split('~');

                if (securityParts.Length >= 2)
                {
                    var securityKey = securityParts[0];

                    var isQuerySecurity = securityKey.StartsWith("?");

                    if (isQuerySecurity)
                        securityKey = securityKey.Substring(1);

                    var securityValue = securityParts[1];

                    if (!isQuerySecurity)
                    {
                        if (securityValue.StartsWith("@SharedAccessSignature="))
                        {
                            //securityValue = SharedAccessSignatureTokenProvider.GetSharedAccessSignature("ide", securityValue.Replace("@SharedAccessSignature=", ""), 
                            //    proxyPath, TimeSpan.FromMinutes(60));

                            throw new NotSupportedException("@SharedAccessSignature= is not supported for API Proxy");
                        }

                        if (!securityKey.IsNullOrEmpty() && !securityValue.IsNullOrEmpty())
                            context.Request.Headers[securityKey] = securityValue;
                    }
                    else
                    {
                        var uri = new UriBuilder(proxyPath);

                        var query = HttpUtility.ParseQueryString(uri.Query);

                        query[securityKey] = securityValue;

                        uri.Query = query.ToString();

                        proxyPath = uri.Query;
                    }
                }
            }

            return proxyPath;
        }
        #endregion
    }
}
