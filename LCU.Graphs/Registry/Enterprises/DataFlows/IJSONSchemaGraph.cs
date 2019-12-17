using Fathym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface IJSONSchemaGraph
	{
		Task<List<string>> FetchJSONSchemas(string apiKey, IEnumerable<string> schemaIds);

        Task<Status> SaveJSONSchema(string apiKey, string envLookup, string lookup,
            string name, string description, string schemaPath);

    }
}