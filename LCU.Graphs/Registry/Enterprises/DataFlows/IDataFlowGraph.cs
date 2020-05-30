using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface IDataFlowGraph
	{
		Task<Status> DeleteDataFlow(string entLookup, string envLookup, string dfLookup);

		Task<DataFlow> GetDataFlow(string entLookup, string envLookup, string dfLookup);

		Task<ModulePackSetup> LoadModulePackSetup(string entLookup, string envLookup,
			string dfLookup, string mdlPckLookup);

		Task<List<DataFlow>> ListDataFlows(string entLookup, string envLookup);

		Task<DataFlow> SaveDataFlow(string entLookup, string envLookup, DataFlow dataFlow);

		Task<ModulePack> UnpackModulePack(string entLookup, string envLookup, string dfLookup,
			ModulePackSetup module);
	}
}