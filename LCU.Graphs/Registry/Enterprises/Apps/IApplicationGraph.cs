using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
	public interface IApplicationGraph
	{
		Task<Status> AddDefaultApp(string entLookup, Guid appId);

		Task<Status> CreateDefaultApps(string entLookup);

		Task<List<DAFApplicationConfiguration>> GetDAFApplications(string entLookup, Guid appId);

		Task<Status> HasDefaultApps(string entLookup);

		Task<List<Application>> ListApplications(string entLookup);

		Task<List<Application>> LoadByEnterprise(string entLookup, string host, string container);

		Task<List<Application>> LoadDefaultApplications(string entLookup);

		Task<Status> RemoveDAFApplication(string entLookup, DAFApplicationConfiguration config);

		Task<Status> RemoveDefaultApp(string entLookup, Guid appId);

		Task<Application> Save(Application application);

		Task<DAFApplicationConfiguration> SaveDAFApplication(string entLookup, DAFApplicationConfiguration config);

		Task<Status> SeedDefault(string sourceApiKey, string targetApiKey);
	}
}