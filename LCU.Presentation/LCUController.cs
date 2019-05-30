﻿using Fathym;
using Fathym.Design;
using Fathym.Presentation.MVC;
using LCU.Graphs.Registry.Enterprises.Identity;
using LCU.Presentation.API;
using LCU.Presentation.DFS;
using LCU.Presentation.Enterprises;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace LCU.Presentation
{
	public class LCUController : FathymController
	{
		#region Fields
		protected readonly CloudBlobClient blobClient;

		protected readonly IDistributedCache cache;

		protected const string defaultBaseHrefSearch = @"<base [^>]*href=""(.*?)""(>|/>| />)";

		protected const string defaultLcuRegSearch = @"<script id=""lcu-reg"">*.</script>";

		protected readonly string devOpsAppId;

		protected readonly string devOpsAppClientSecret;

		protected readonly string devOpsAppSecret;

		protected readonly CloudBlobContainer fsContainer;

		protected readonly string gitHubAppClientId;

		protected readonly string gitHubAppClientSecret;

		protected readonly GitHubClient gitHubClient;

		protected readonly IdentityGraph idGraph;

		protected readonly IMemoryCache memCache;
		#endregion

		#region Properties
		#endregion

		#region Constructors
		public LCUController(IConfiguration config, IDistributedCache cache, IMemoryCache memCache, CloudStorageAccount storageAccount,
			IdentityGraph idGraph)
		{
			this.cache = cache;

			this.memCache = memCache;

			blobClient = storageAccount.CreateCloudBlobClient();

			devOpsAppId = config["LCU-DEV-OPS-APP-ID"];

			devOpsAppClientSecret = config["LCU-DEV-OPS-APP-CLIENT-SECRET"];

			devOpsAppSecret = config["LCU-DEV-OPS-APP-SECRET"];

			fsContainer = blobClient.GetContainerReference("filesystem");

			gitHubAppClientId = config["LCU-GIT-HUB-CLIENT-ID"];

			gitHubAppClientSecret = config["LCU-GIT-HUB-CLIENT-SECRET"];

			gitHubClient = new GitHubClient(new ProductHeaderValue("LOW-CODE-UNIT-DAF"));

			this.idGraph = idGraph;
		}
		#endregion

		#region API Methods
		public virtual async Task<HttpResponseMessage> API(string prefix, string path)
		{
			try
			{
				var dafApisCtxt = HttpContext.ResolveContext<DAFAPIsContext>(DAFAPIsContext.Lookup);

				if (dafApisCtxt != null)
				{
					var dafApiCtxt = resolveDafApiContext(dafApisCtxt, path, HttpContext.Request.Method);

					var proxiedAddress = HttpContext.GetProxiedAddress(dafApiCtxt, path);

					if (!proxiedAddress.IsNullOrEmpty())
						//	TODO:  Previously we used the below commented out code to write proxied response directly to response in middleware...  
						//	await context.CopyProxyHttpResponse(proxiedResponse).ConfigureAwait(false);
						//	Just noting as i haven't been able to test this yet
						return await HttpContext.SendProxyHttpRequest(proxiedAddress).ConfigureAwait(false);
					else
						return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
				}
				else
					//	TODO:  Previously, if a dafApisCtxt was not found, then we passed on to the next middleware, now this terminates in MVC...
					//				Are we good with the "all apis on a '/api/{....}' construct?  If not, do we forward to some other handler for this case?
					return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
			}
			catch (Exception e)
			{
				//	TODO:  Log

				return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
			}
		}

		//[Authorize]
		public virtual async Task<IActionResult> DevOpsAuthorize(string code, string state, string error = null)
		{
			var expectedState = HttpContext.Session.GetString("DEVOPS:CSRF:State");

			var redirectUri = HttpContext.Session.GetString("DEVOPS:RedirectURI");

			redirectUri = redirectUri ?? "/";

			if (code.IsNullOrEmpty())
				return Redirect(redirectUri);

			if (state != expectedState)
				throw new InvalidOperationException("SECURITY FAIL!");

			HttpContext.Session.Remove("DEVOPS:CSRF:State");

			HttpContext.Session.Remove("DEVOPS:RedirectURI");

			await processDevOpsAuthTokenForAccessToken(code);

			return Redirect(redirectUri);
		}

		//[Authorize]
		public virtual async Task<IActionResult> DevOpsOAuth(string redirectUri = null)
		{
			string csrf = Guid.NewGuid().ToString();

			HttpContext.Session.SetString("DEVOPS:CSRF:State", csrf);

			HttpContext.Session.SetString("DEVOPS:RedirectURI", csrf);

			var req = HttpContext.Request;

			var callBackUrl = $"{req.Scheme}://{req.Host}/.devops/authorize";

			var scopes = new StringBuilder();
			scopes.Append("vso.build_execute vso.code_full vso.code_status vso.connected_server vso.dashboards ");
			scopes.Append("vso.dashboards_manage vso.entitlements vso.extension.data_write vso.extension_manage ");
			scopes.Append("vso.gallery_acquire vso.gallery_manage vso.graph_manage vso.identity_manage vso.loadtest_write ");
			scopes.Append("vso.machinegroup_manage vso.memberentitlementmanagement_write vso.notification_diagnostics ");
			scopes.Append("vso.notification_manage vso.packaging_manage vso.profile_write vso.project_manage vso.release_manage ");
			scopes.Append("vso.security_manage vso.serviceendpoint_manage vso.symbols_manage vso.taskgroups_manage ");
			scopes.Append("vso.test_write vso.tokenadministration vso.tokens vso.variablegroups_manage vso.wiki_write vso.work_full");

			var oauthLoginUrl = new StringBuilder("https://app.vssps.visualstudio.com/oauth2/authorize?");
			oauthLoginUrl.Append($"client_id={devOpsAppId}");
			oauthLoginUrl.Append($"&response_type=Assertion");
			oauthLoginUrl.Append($"&state={csrf}");
			oauthLoginUrl.Append($"&scope={scopes.ToString()}");
			oauthLoginUrl.Append($"&redirect_uri={callBackUrl}");

			return Redirect(oauthLoginUrl.ToString());
		}

		[Authorize]
		public virtual async Task<IActionResult> DevOpsRefresh()
		{
			//	TODO:  This needs to be set to wherever the user is coming from?
			var redirectUri = HttpContext.Session.GetString("DEVOPS:RedirectURI");

			redirectUri = redirectUri ?? "/";

			HttpContext.Session.Remove("DEVOPS:RedirectURI");

			var entCtxt = HttpContext.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			var refreshToken = await idGraph.RetrieveThirdPartyAccessToken(entCtxt.PrimaryAPIKey, HttpContext.LoadUserID(), "AZURE-DEV-OPS-REFRESH");

			await processDevOpsAuthTokenForAccessToken(refreshToken);

			return Redirect(redirectUri);
		}

		[Authorize]
		public virtual async Task<IActionResult> DevStream(string path)
		{
			var devStreamFilePathHeader = HttpContext.Request.Headers["f-dev-stream-file-path"].ToString();

			var dfsCtxt = HttpContext.ResolveContext<DFSContext>(DFSContext.Lookup);

			if (dfsCtxt != null)
			{
				var appDirectory = fsContainer.GetDirectoryReference(dfsCtxt.DFSRoot);

				var file = HttpContext.Request.Form.Files.FirstOrDefault();

				if (file != null)
				{
					var cd = new ContentDisposition(file.ContentDisposition);

					cd.FileName = devStreamFilePathHeader ?? cd.FileName;

					var fileContents = new MemoryStream();

					await file.CopyToAsync(fileContents);

					fileContents.Seek(0, SeekOrigin.Begin);

					var status = await saveToDistributedFileSystem(appDirectory, dfsCtxt, cd, fileContents);

					return new ContentResult()
					{
						Content = status.Message,
						ContentType = "text/plain",
						StatusCode = StatusCodes.Status200OK
					};
				}
				else
					return new ContentResult()
					{
						Content = "No file provided for the dev-stream",
						ContentType = "text/plain",
						StatusCode = StatusCodes.Status400BadRequest
					};
			}
			else
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
		}

		public virtual async Task<IActionResult> DFS(string path)
		{
			var devStreamHeader = HttpContext.Request.Headers["f-dev-stream"];

			if (devStreamHeader.IsNullOrEmpty())
			{
				var dfsCtxt = HttpContext.ResolveContext<DFSContext>(DFSContext.Lookup);

				if (dfsCtxt != null)
				{
					var fileRes = await loadFile(path, dfsCtxt);

					var file = fileRes.Item1;

					var target = fileRes.Item2;

					file = await handleDefaultFile(target, file, dfsCtxt);

					return deliverFile(target, file);
				}
				else
					return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
			else
				return await DevStream(path);
		}

		[Authorize]
		public virtual async Task<IActionResult> GitHubAuthorize(string code, string state, string redirectUri = null)
		{
			redirectUri = redirectUri ?? "/";

			if (code.IsNullOrEmpty())
				return Redirect(redirectUri);

			var expectedState = HttpContext.Session.GetString("GITHUB:CSRF:State");

			if (state != expectedState)
				throw new InvalidOperationException("SECURITY FAIL!");

			HttpContext.Session.Remove("GITHUB:CSRF:State");

			var request = new OauthTokenRequest(gitHubAppClientId, gitHubAppClientSecret, code);

			var token = await gitHubClient.Oauth.CreateAccessToken(request);

			var entCtxt = HttpContext.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			await idGraph.SetThirdPartyAccessToken(entCtxt.PrimaryAPIKey, HttpContext.LoadUserID(), "GIT-HUB", token.AccessToken);

			return Redirect(redirectUri);
		}

		[Authorize]
		public virtual async Task<IActionResult> GitHubOAuth(string redirectUri = null)
		{
			string csrf = Guid.NewGuid().ToString();

			HttpContext.Session.SetString("GITHUB:CSRF:State", csrf);

			var req = HttpContext.Request;

			var request = new OauthLoginRequest(gitHubAppClientId)
			{
				Scopes = { "user", "notifications", "repo" },
				State = csrf,
				RedirectUri = new Uri($"{req.Scheme}://{req.Host}/.github/authorize?redirectUri={redirectUri}")
			};

			var oauthLoginUrl = gitHubClient.Oauth.GetGitHubLoginUrl(request);

			return Redirect(oauthLoginUrl.ToString());
		}

		[Authorize]
		public virtual async Task<IActionResult> Logout()
		{
			return SignOut("Cookies", "oidc");
		}
		#endregion

		#region Helpers
		protected virtual IActionResult deliverFile(string target, byte[] file)
		{
			//	TODO:  Need to do a better job of managing this / Now that this is in MVC... Should we be doing differently??
			HttpContext.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
			{
				Public = true,
				MaxAge = TimeSpan.FromMinutes(10)
			};

			HttpContext.Response.Headers[HeaderNames.Vary] = new string[] { "Accept-Encoding" };

			string contentType;
			new FileExtensionContentTypeProvider().TryGetContentType(target, out contentType);

			if (String.IsNullOrEmpty(contentType))
				contentType = "text/plain";

			return new FileContentResult(file, contentType);
		}

		protected virtual Dictionary<string, string> generateRequestPostData(string authToken)
		{
			var req = HttpContext.Request;

			var callBackUrl = $"{req.Scheme}://{req.Host}/.devops/authorize";

			return new Dictionary<string, string>()
			{
				{ "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
				{ "client_assertion", devOpsAppClientSecret },
				{ "grant_type", "refresh_token" },
				{ "assertion", authToken },
				{ "redirect_uri", callBackUrl }
			};
		}

		protected virtual async Task<byte[]> handleDefaultFile(string target, byte[] file, DFSContext dfsCtxt)
		{
			var appCtxt = HttpContext.ResolveContext<ApplicationContext>(ApplicationContext.CreateLookup(HttpContext));

			if (target.EndsWith(dfsCtxt.DefaultFile))
				using (var streamRdr = new StreamReader(new MemoryStream(file)))
				{
					var fileStr = await streamRdr.ReadToEndAsync();

					var newBase = $"<base href=\"{dfsCtxt.AppRoot}\">";

					var baseHrefRegex = new Regex(defaultBaseHrefSearch);

					if (baseHrefRegex.IsMatch(fileStr))
						fileStr = baseHrefRegex.Replace(fileStr, newBase);
					else
						fileStr = fileStr.Replace("</head>", $"{newBase}</head>");

					var lcuRegRegex = new Regex(defaultLcuRegSearch);


					var config = new
					{
						Application = new
						{
							ID = appCtxt.ID,
							EnterprisePrimaryAPIKey = appCtxt.EnterprisePrimaryAPIKey
						},
						State = new
						{

						}
					}.ToJSON();

					var lcu = new StringBuilder(@"<script id=""lcu-reg"">");
					lcu.AppendLine($"\twindow.LCU = {config};");
					lcu.AppendLine("</script>");

					if (lcuRegRegex.IsMatch(fileStr))
						fileStr = lcuRegRegex.Replace(fileStr, lcu.ToString());
					else
						fileStr = fileStr.Replace("</head>", $"{lcu.ToString()}</head>");

					file = Encoding.UTF8.GetBytes(fileStr);
				}

			return file;
		}

		protected virtual async Task<Tuple<byte[], string>> loadFile(string path, DFSContext dfsCtxt)
		{
			var entCtxt = HttpContext.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

			var entId = dfsCtxt.EnterpriseID;

			var appId = dfsCtxt.ApplicationID;

			var chains = DesignOutline.Instance.Chain<Tuple<byte[], string>>()
				.AddResponsibilities(new Func<Task<Tuple<byte[], string>>>[] { });

			//	Check for DFS Files from DFS Root
			if (!dfsCtxt.DFSRoot.IsNullOrEmpty())
				chains = chains.AddResponsibilities(
					() => loadFileFromDirectory(path, entId, appId, dfsCtxt.DFSRoot, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(path, entId, Guid.Empty, dfsCtxt.DFSRoot, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(path, Guid.Empty, Guid.Empty, dfsCtxt.DFSRoot, dfsCtxt.AppRoot)
				);

			//	Check for DFS Files from Actual and App Ent ID
			chains = chains.AddResponsibilities(
				() => loadFileFromDirectory(path, entCtxt.ID, appId, String.Empty, String.Empty),
				() => loadFileFromDirectory(path, entCtxt.ID, Guid.Empty, String.Empty, String.Empty),
				() => loadFileFromDirectory(path, entId, appId, String.Empty, String.Empty),
				() => loadFileFromDirectory(path, entId, Guid.Empty, String.Empty, String.Empty),
				() => loadFileFromDirectory(path, Guid.Empty, Guid.Empty, String.Empty, String.Empty)
			);

			//	Check for Default File from Cascading DFS Structure
			if (!dfsCtxt.DFSRoot.IsNullOrEmpty())
				chains = chains.AddResponsibilities(
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, entId, appId, dfsCtxt.DFSRoot, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, entId, appId, String.Empty, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, entId, Guid.Empty, dfsCtxt.DFSRoot, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, entId, Guid.Empty, String.Empty, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, Guid.Empty, Guid.Empty, dfsCtxt.DFSRoot, dfsCtxt.AppRoot),
					() => loadFileFromDirectory(dfsCtxt.DefaultFile, Guid.Empty, Guid.Empty, String.Empty, dfsCtxt.AppRoot)
				);

			chains = chains.SetShouldContinue(val =>
			{
				return val == null || val.Item1.IsNullOrEmpty();
			});

			return await chains.Run();
		}

		protected virtual async Task<Tuple<byte[], string>> loadFileFromDirectory(string path, Guid entId, Guid appId, string dfsRoot,
			string appRoot)
		{
			var dirPath = new StringBuilder("");

			if (!entId.IsEmpty())
			{
				dirPath.Append(entId);

				if (!appId.IsEmpty())
				{
					dirPath.Append("/");

					dirPath.Append(appId);
				}
			}

			if (!dfsRoot.IsNullOrEmpty())
			{
				if (dirPath.Length > 0)
					dirPath.Append("/");

				dirPath.Append(dfsRoot);
			}

			var appDirectory = fsContainer.GetDirectoryReference(dirPath.ToString());

			var target = await loadFileTarget(path, appDirectory, appRoot);

			//var cacheKey = $"{dfsCtxt.DFSRoot}/{dfsCtxt.AppRoot}/{target}";

			//var file = await cache.GetOrCreateAsync(memCache, cacheKey, async (entry, options) =>
			//{
			//	entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(dfsCtxt.CacheSeconds));

			//	options.SetAbsoluteExpiration(TimeSpan.FromSeconds(dfsCtxt.CacheSeconds));

			//	return await loadFromDistributedFileSystem(appDirectory, target, dfsCtxt);
			//});

			if (!target.IsNullOrEmpty())
			{
				var f = await loadFromDistributedFileSystem(appDirectory, target);

				return new Tuple<byte[], string>(f, target);
			}
			else
				return null;
		}

		protected virtual async Task<string> loadFileTarget(string path, CloudBlobDirectory appDirectory, string appRoot)
		{
			string target = path ?? String.Empty;

			var appRootCheck = appRoot?.Trim('/');

			if (!appRootCheck.IsNullOrEmpty() && target.StartsWith(appRootCheck))
				target = target.Substring(appRootCheck.Length);

			target = target.TrimStart('/');

			return target;
		}

		protected virtual async Task<byte[]> loadFromDistributedFileSystem(CloudBlobDirectory appDirectory, string target)
		{
			var appBlob = appDirectory.GetBlockBlobReference(target);

			var bytes = new byte[0];

			if (await appBlob.ExistsAsync())
			{
				await appBlob.FetchAttributesAsync();

				bytes = new byte[appBlob.Properties.Length];

				await appBlob.DownloadToByteArrayAsync(bytes, 0);
			}

			return bytes;
		}

		protected virtual async Task processDevOpsAuthTokenForAccessToken(string authToken)
		{
			var req = HttpContext.Request;

			var callBackUrl = $"{req.Scheme}://{req.Host}/.devops/authorize";

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token");

			requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var form = generateRequestPostData(authToken);

			requestMessage.Content = new FormUrlEncodedContent(form);

			var httpClient = new HttpClient();

			var responseMessage = await httpClient.SendAsync(requestMessage);

			if (responseMessage.IsSuccessStatusCode)
			{
				var body = await responseMessage.Content.ReadAsStringAsync();

				var tokenRes = body.FromJSON<JObject>();

				var entCtxt = HttpContext.ResolveContext<EnterpriseContext>(EnterpriseContext.Lookup);

				await idGraph.SetThirdPartyAccessToken(entCtxt.PrimaryAPIKey, HttpContext.LoadUserID(), "AZURE-DEV-OPS", tokenRes["access_token"].ToString());

				await idGraph.SetThirdPartyAccessToken(entCtxt.PrimaryAPIKey, HttpContext.LoadUserID(), "AZURE-DEV-OPS-REFRESH", tokenRes["refresh_token"].ToString());
			}
			else
			{
				//	TODO:  Handle Error
			}
		}

		protected virtual DAFAPIContext resolveDafApiContext(DAFAPIsContext dafApisCtxt, string path, string method)
		{
			return dafApisCtxt.APIs.FirstOrDefault(api =>
			{
				return path.StartsWith(api.InboundPath) && (api.Methods.IsNullOrEmpty() || api.Methods.Contains(method.ToUpper()));
			});
		}

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
