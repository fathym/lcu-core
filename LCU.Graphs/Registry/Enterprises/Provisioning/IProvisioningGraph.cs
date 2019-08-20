using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	public interface IProvisioningGraph
	{
		Task<LCUEnvironment> GetEnvironment(string apiKey, string lookup);

		Task<SourceControl> GetSourceControl(string apiKey, string envLookup);

		Task<List<LCUEnvironment>> ListEnvironments(string apiKey);

		Task<LCUEnvironment> SaveEnvironment(LCUEnvironment env);

		Task<SourceControl> SaveSourceControl(string apiKey, string envLookup, SourceControl sc);
	}
}