using Fathym;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public class SchemaFunctionDefinitionGraph : LCUGraph, ISchemaFunctionDefinitionGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public SchemaFunctionDefinitionGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			
		}
        #endregion

        #region API Methods

        public virtual async Task<List<SchemaFunctionDefinition>> ListSchemaFunctionDefinitions(string apiKey)
        {
            return await withG(async (client, g) =>
            {
                var query = g.V()
                .HasLabel(EntGraphConstants.SchemaFunctionDefinitionVertexName);

                var results = await Submit<SchemaFunctionDefinition>(query);

                return results.ToList();

            }, apiKey);
        }

        #endregion

        #region Helpers


        #endregion
    }
}
