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
	public class JSONSchemaGraph : LCUGraph, IJSONSchemaGraph
	{
		#region Properties
		#endregion

		#region Constructors
		public JSONSchemaGraph(GremlinClientPoolManager clientPool)
			: base(clientPool)
		{
			
		}
		#endregion

		#region API Methods

		public virtual async Task<List<BusinessModel<Guid>>> FetchJSONSchemas(string apiKey, IEnumerable<string> schemaIds)
		{
			return await withG(async (client, g) =>
			{
                var query = g.V(schemaIds);

				var results = await Submit<BusinessModel<Guid>>(query);

                return results.ToList();

            }, apiKey);
		}

        public virtual async Task<List<BusinessModel<Guid>>> ListJSONSchemas(string apiKey, string envLookup)
        {
            return await withG(async (client, g) =>
            {
                var registry = $"{apiKey}|{envLookup}|JSONSchemaMap";

                var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                    .Has(EntGraphConstants.RegistryName, apiKey)
                    .Has("PrimaryAPIKey", apiKey)
                    .Out(EntGraphConstants.ConsumesEdgeName)
                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
                    .Has(EntGraphConstants.RegistryName, apiKey)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
                    .Has("Lookup", envLookup)
                    .Out(EntGraphConstants.UsesEdgeName)
                    .HasLabel(EntGraphConstants.SemanticProfileVertexName)
                    .Out(EntGraphConstants.UsesEdgeName)
                    .HasLabel(EntGraphConstants.JSONSchemaMapVertexName)
                    .Has(EntGraphConstants.RegistryName, registry);

                var results = await Submit<BusinessModel<Guid>>(query);

                return results.ToList();
            }, apiKey);
        }

        public virtual async Task<Status> SaveJSONSchema(string apiKey, string envLookup, string lookup, 
            string name, string description, string schemaPath)
        {
            return await withG(async (client, g) =>
            {
                var registry = $"{apiKey}|{envLookup}|JSONSchemaMap";

                var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
                    .Has(EntGraphConstants.RegistryName, apiKey)
                    .Has("PrimaryAPIKey", apiKey)
                    .Out(EntGraphConstants.OwnsEdgeName)
                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
                    .Has(EntGraphConstants.RegistryName, apiKey)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, apiKey)
                    .Has("Lookup", envLookup);

                var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

                var semanticQuery = envQuery
                    .Out(EntGraphConstants.UsesEdgeName)
                    .HasLabel(EntGraphConstants.SemanticProfileVertexName);

                var semResult = await SubmitFirst<dynamic>(semanticQuery);

                var existingQuery = semanticQuery
                    .Out(EntGraphConstants.UsesEdgeName)
                    .HasLabel(EntGraphConstants.JSONSchemaMapVertexName)
                    .Has("Lookup", lookup);

                var existingResult = await SubmitFirst<MetadataModel>(existingQuery);

                var query = existingResult == null ?
                    g.AddV(EntGraphConstants.JSONSchemaMapVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, apiKey) : existingQuery;

                query = query
                    .Property("Name", name ?? "")
                    .Property("Description", description ?? "")
                    .Property("Lookup", lookup ?? "")
                    .Property("SchemaPath", schemaPath ?? "");

                var result = await SubmitFirst<MetadataModel>(query);

                await ensureEdgeRelationships(g, envResult.ID, semResult.ID, EntGraphConstants.UsesEdgeName, new List<string>() { EntGraphConstants.UsesEdgeName });

                await ensureEdgeRelationships(g, semResult.ID, new Guid(result.Metadata["id"].ToString()), EntGraphConstants.UsesEdgeName, new List<string>() { EntGraphConstants.UsesEdgeName });

                var status = Status.Success;

                status.Metadata.Add("ID", result.Metadata["id"].ToString());

                return status;
            }, apiKey);
        }

        #endregion

        #region Helpers

      
        #endregion
    }
}
