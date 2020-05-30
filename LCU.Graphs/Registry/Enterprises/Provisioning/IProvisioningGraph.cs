using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.Provisioning
{
	public interface IProvisioningGraph
	{
		Task<LCUEnvironment> GetEnvironment(string entLookup, string lookup);

		Task<SourceControl> GetSourceControl(string entLookup, string envLookup);

		Task<List<LCUEnvironment>> ListEnvironments(string entLookup);

		Task<LCUEnvironment> SaveEnvironment(LCUEnvironment env);

		Task<SourceControl> SaveSourceControl(string entLookup, string envLookup, SourceControl sc);
	}
}