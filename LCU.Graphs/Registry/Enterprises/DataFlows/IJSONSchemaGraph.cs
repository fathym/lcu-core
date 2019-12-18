using Fathym;
using Fathym.Business.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public interface IJSONSchemaGraph
	{
		Task<List<BusinessModel<Guid>>> FetchJSONSchemas(string apiKey, IEnumerable<string> schemaIds);

        Task<List<BusinessModel<Guid>>> ListJSONSchemas(string apiKey, string envLookup);

        Task<Status> SaveJSONSchema(string apiKey, string envLookup, string lookup,
            string name, string description, string schemaPath);

    }
}