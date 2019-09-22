using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface IDataFlowGraph
	{
		Task<Status> DeleteDataFlow(string apiKey, string envLookup, string dfLookup);

		Task<DataFlow> GetDataFlow(string apiKey, string envLookup, string dfLookup);

		Task<ModulePackSetup> LoadModulePackSetup(string apiKey, string envLookup,
			string dfLookup, string mdlPckLookup);

		Task<List<DataFlow>> ListDataFlows(string apiKey, string envLookup);

		Task<DataFlow> SaveDataFlow(string apiKey, string envLookup, DataFlow dataFlow);

		Task<ModulePack> UnpackModulePack(string apiKey, string envLookup, string dfLookup,
			ModulePackSetup module);
	}
}