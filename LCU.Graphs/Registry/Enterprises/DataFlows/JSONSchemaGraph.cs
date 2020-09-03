//using Fathym;
//using Fathym.Business.Models;
//using Gremlin.Net.Process.Traversal;
//using LCU.Graphs.Registry.Enterprises.Provisioning;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LCU.Graphs.Registry.Enterprises.DataFlows
//{
//	public class JSONSchemaGraph : LCUGraph, IJSONSchemaGraph
//	{
//		#region Properties
//		#endregion

//		#region Constructors
//		public JSONSchemaGraph(GremlinClientPoolManager clientPool)
//			: base(clientPool)
//		{
			
//		}
//		#endregion

//		#region API Methods

//		public virtual async Task<List<BusinessModel<Guid>>> FetchJSONSchemas(string entLookup, IEnumerable<string> schemaIds)
//		{
//			return await withG(async (client, g) =>
//			{
//                var query = g.V(schemaIds);

//				var results = await Submit<BusinessModel<Guid>>(query);

//                return results.ToList();

//            }, entLookup);
//		}

//        public virtual async Task<List<BusinessModel<Guid>>> ListJSONSchemas(string entLookup, string envLookup)
//        {
//            return await withG(async (client, g) =>
//            {
//                var registry = $"{entLookup}|{envLookup}|JSONSchemaMap";

//                var query = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
//                    .Has(EntGraphConstants.RegistryName, entLookup)
//                    .Has("EnterpriseLookup", entLookup)
//                    .Out(EntGraphConstants.ConsumesEdgeName)
//                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
//                    .Has(EntGraphConstants.RegistryName, entLookup)
//                    .Has(EntGraphConstants.EnterpriseLookupName, entLookup)
//                    .Has("Lookup", envLookup)
//                    .Out(EntGraphConstants.UsesEdgeName)
//                    .HasLabel(EntGraphConstants.SemanticProfileVertexName)
//                    .Out(EntGraphConstants.UsesEdgeName)
//                    .HasLabel(EntGraphConstants.JSONSchemaMapVertexName)
//                    .Has(EntGraphConstants.RegistryName, registry);

//                var results = await Submit<BusinessModel<Guid>>(query);

//                return results.ToList();
//            }, entLookup);
//        }

//        public virtual async Task<Status> SaveJSONSchema(string entLookup, string envLookup, string lookup, 
//            string name, string description, string schemaPath)
//        {
//            return await withG(async (client, g) =>
//            {
//                var registry = $"{entLookup}|{envLookup}|JSONSchemaMap";

//                var envQuery = g.V().HasLabel(EntGraphConstants.EnterpriseVertexName)
//                    .Has(EntGraphConstants.RegistryName, entLookup)
//                    .Has("EnterpriseLookup", entLookup)
//                    .Out(EntGraphConstants.OwnsEdgeName)
//                    .HasLabel(EntGraphConstants.EnvironmentVertexName)
//                    .Has(EntGraphConstants.RegistryName, entLookup)
//                    .Has(EntGraphConstants.EnterpriseLookupName, entLookup)
//                    .Has("Lookup", envLookup);

//                var envResult = await SubmitFirst<LCUEnvironment>(envQuery);

//                var semanticQuery = envQuery
//                    .Out(EntGraphConstants.UsesEdgeName)
//                    .HasLabel(EntGraphConstants.SemanticProfileVertexName);

//                var semResult = await SubmitFirst<dynamic>(semanticQuery);

//                var existingQuery = semanticQuery
//                    .Out(EntGraphConstants.UsesEdgeName)
//                    .HasLabel(EntGraphConstants.JSONSchemaMapVertexName)
//                    .Has("Lookup", lookup);

//                var existingResult = await SubmitFirst<MetadataModel>(existingQuery);

//                var query = existingResult == null ?
//                    g.AddV(EntGraphConstants.JSONSchemaMapVertexName)
//                    .Property(EntGraphConstants.RegistryName, registry)
//                    .Property(EntGraphConstants.EnterpriseLookupName, entLookup) : existingQuery;

//                query = query
//                    .Property("Name", name ?? "")
//                    .Property("Description", description ?? "")
//                    .Property("Lookup", lookup ?? "")
//                    .Property("SchemaPath", schemaPath ?? "");

//                var result = await SubmitFirst<MetadataModel>(query);

//                await ensureEdgeRelationships(g, envResult.ID, semResult.ID, EntGraphConstants.UsesEdgeName, new List<string>() { EntGraphConstants.UsesEdgeName });

//                await ensureEdgeRelationships(g, semResult.ID, new Guid(result.Metadata["id"].ToString()), EntGraphConstants.UsesEdgeName, new List<string>() { EntGraphConstants.UsesEdgeName });

//                var status = Status.Success;

//                status.Metadata.Add("ID", result.Metadata["id"].ToString());

//                return status;
//            }, entLookup);
//        }

//        #endregion

//        #region Helpers

      
//        #endregion
//    }
//}
