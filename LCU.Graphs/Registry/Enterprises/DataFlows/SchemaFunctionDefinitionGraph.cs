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

        public virtual async Task<SchemaFunctionDefinition> SaveSchemaFunctionDefinitionDefinition(string apiKey, SchemaFunctionDefinition schemaFunctionDefinition)
        {
            return await withG(async (client, g) =>
            {
                var registry = $"{apiKey}|SchemaFunctionDefinition";

                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.SchemaFunctionDefinitionVertexName)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Lookup", schemaFunctionDefinition.Lookup);

                var existingResult = await SubmitFirst<MetadataModel>(existingQuery);

                var query = existingResult == null ?
                    g.AddV(EntGraphConstants.SchemaFunctionDefinitionVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

                query = query
                    .Property("Active", schemaFunctionDefinition.Active)
                    .Property("AllowDifferentIncommingTypes", schemaFunctionDefinition.AllowDifferentIncommingTypes)
                    .Property("AllowMultipleIncomming", schemaFunctionDefinition.AllowMultipleIncomming)
                    .Property("Description", schemaFunctionDefinition.Description ?? "")
                    .Property("FunctionType", schemaFunctionDefinition.FunctionType ?? "")
                    .Property("Lookup", schemaFunctionDefinition.Lookup ?? "")
                    .Property("MaxProperties", schemaFunctionDefinition.MaxProperties)
                    .Property("MinProperties", schemaFunctionDefinition.MinProperties)
                    .Property("Name", schemaFunctionDefinition.Name ?? "")
                    .Property("ReturnType", schemaFunctionDefinition.ReturnType ?? "")
                    .Property("SQL", schemaFunctionDefinition.SQL ?? "")
                    .Property("SQLBoolean", schemaFunctionDefinition.SQLBoolean ?? "");

                schemaFunctionDefinition.AllowedIncommingTypes.Each(item =>
                {
                    query = query.Property(Cardinality.List, "AllowedIncommingTypes", item, new object[] { });
                });

                if (schemaFunctionDefinition.AllowedIncommingTypes.Count() == 1)
                    query = query.Property(Cardinality.List, "AllowedIncommingTypes", "filler", new object[] { });

                var sfdResult = await SubmitFirst<SchemaFunctionDefinition>(query);

                return sfdResult;
            }, apiKey);
        }

        #endregion

        #region Helpers


        #endregion
    }
}
