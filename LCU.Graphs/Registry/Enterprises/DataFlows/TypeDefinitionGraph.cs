using Fathym;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs.Registry.Enterprises.Provisioning;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs.Registry.Enterprises.DataFlows
{
	public class TypeDefinitionGraph : LCUGraph, ITypeDefinitionGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public TypeDefinitionGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			
		}
		#endregion

		#region API Methods

		public virtual async Task<List<TypeDefinition>> ListTypeDefinitions(string apiKey)
		{
			return await withG(async (client, g) =>
			{
                var query = g.V()
                .HasLabel(EntGraphConstants.TypeDefinitionVertexName);

				var results = await Submit<TypeDefinition>(query);

                return results.ToList();

            }, apiKey);
		}

        #endregion

        #region Helpers

      
        #endregion
    }
}
