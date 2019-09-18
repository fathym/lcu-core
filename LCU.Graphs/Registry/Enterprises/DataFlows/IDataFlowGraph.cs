using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface IDataFlowGraph
	{
		Task<DataFlow> GetDataFlow(string apiKey, string envLookup, string dfLookup);

		Task<DataFlow> SaveDataFlow(string apiKey, string envLookup, DataFlow dataFlow);
	}
}