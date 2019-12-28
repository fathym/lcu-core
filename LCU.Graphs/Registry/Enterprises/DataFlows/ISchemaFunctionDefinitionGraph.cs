using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface ISchemaFunctionDefinitionGraph
	{
        Task<List<SchemaFunctionDefinition>> ListSchemaFunctionDefinitions(string apiKey);
    }
}