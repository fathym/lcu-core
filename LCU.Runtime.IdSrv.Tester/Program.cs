using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.Runtime.IdSrv.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			Work(args).Wait();
	}

		static async Task Work(string[] args)
		{
			Console.WriteLine("Hello World!");

			var idClient = new HttpClient();

			var disco = await idClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest()
			{
				Address = "http://localhost:52235/.identity"
			});

			if (disco.IsError)
			{
				Console.WriteLine(disco.Error);

				return;
			}

			// request token
			var tokenResponse = await idClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
			{
				Address = disco.TokenEndpoint,
				ClientId = "client",
				ClientSecret = "secret",
				Scope = "api1"
			});

			if (tokenResponse.IsError)
			{
				Console.WriteLine(tokenResponse.Error);
				return;
			}

			Console.WriteLine(tokenResponse.Json);
		}
	}
}

 