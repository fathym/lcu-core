using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	public interface IApplicationGraph
	{
		Task<Status> AddDefaultApp(string apiKey, Guid appId);

		Task<Status> CreateDefaultApps(string apiKey);

		Task<List<DAFApplicationConfiguration>> GetDAFApplications(string apiKey, Guid appId);

		Task<Status> HasDefaultApps(string apiKey);

		Task<List<Application>> ListApplications(string apiKey);

		Task<List<Application>> LoadByEnterprise(string apiKey, string host, string container);

		Task<List<Application>> LoadDefaultApplications(string apiKey);

		Task<Status> RemoveDAFApplication(string apiKey, DAFApplicationConfiguration config);

		Task<Status> RemoveDefaultApp(string apiKey, Guid appId);

		Task<Application> Save(Application application);

		Task<DAFApplicationConfiguration> SaveDAFApplication(string apiKey, DAFApplicationConfiguration config);

		Task<Status> SeedDefault(string sourceApiKey, string targetApiKey);
	}
}