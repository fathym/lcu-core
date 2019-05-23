using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Runtime.Tests
{
	[TestClass]
	public class WebApps
	{
		[TestMethod]
		public async Task AddHostName()
		{
			var host = "forge.fathym.com";

			var rootHost = host.Substring(host.Substring(0, host.LastIndexOf(".")).LastIndexOf(".") + 1);

			Assert.AreEqual("fathym.com", rootHost);

			var subHost = host.Substring(0, host.IndexOf(rootHost) - 1);

			Assert.AreEqual("forge", subHost);

			host = "sub.forge.fathym.com";

			rootHost = host.Substring(host.Substring(0, host.LastIndexOf(".")).LastIndexOf(".") + 1);

			Assert.AreEqual("fathym.com", rootHost);

			subHost = host.Substring(0, host.IndexOf(rootHost) - 1);

			Assert.AreEqual("sub.forge", subHost);

			//string subscription = "7c091771-9597-4ae3-b3fd-46c78af15dfb";
			//string client = "1928792a-760e-4ac3-8ce2-cc64bf60b2f9";
			//string key = "+L?|^_A*.B+-).][5&C){yM->Rt*-=|[:-";
			//string tenant = "6dcbebd0-f8d0-4a9d-89e5-5873e8146b0a";

			//var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(client, key, tenant, AzureEnvironment.AzureGlobalCloud);

			//var azure = Azure
			//	.Configure()
			//	.WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
			//	.Authenticate(credentials)
			//	.WithSubscription(subscription);

			////List the hostname
			//var webApp = azure.WebApps.GetByResourceGroup("lcu-prd", "lcu-prd");

			//webApp.Update().DefineHostnameBinding()
			//	.WithThirdPartyDomain("5280software.com").WithSubDomain("test")
			//	.WithDnsRecordType(Microsoft.Azure.Management.AppService.Fluent.Models.CustomHostNameDnsRecordType.CName)
			//	.Attach()
			//	.Apply();

			//var hostNameBindings = await webApp.GetHostNameBindingsAsync();

			//var hb = hostNameBindings.First();

			////webApp.VerifyDomainOwnership()
		}
	}
}
