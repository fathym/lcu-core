using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	public interface IProvisioningGraph
	{
		Task<Environment> GetEnvironment(string apiKey, string lookup);

		Task<SourceControl> GetSourceControl(string apiKey, string envLookup);

		Task<List<Environment>> ListEnvironments(string apiKey);

		Task<Environment> SaveEnvironment(Environment env);

		Task<SourceControl> SaveSourceControl(string apiKey, string envLookup, SourceControl sc);
	}
}