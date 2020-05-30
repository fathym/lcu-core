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

		public virtual async Task<List<TypeDefinition>> ListTypeDefinitions(string entLookup)
		{
			return await withG(async (client, g) =>
			{
                var query = g.V()
                .HasLabel(EntGraphConstants.TypeDefinitionVertexName);

				var results = await Submit<TypeDefinition>(query);

                return results.ToList();

            }, entLookup);
		}

        public virtual async Task<TypeDefinition> SaveTypeDefinition(string entLookup, TypeDefinition typeDefinition)
        {
            return await withG(async (client, g) =>
            {
                var registry = $"{entLookup}|TypeDefinition";

                var existingQuery = g.V()
                    .HasLabel(EntGraphConstants.TypeDefinitionVertexName)
                    .Has(EntGraphConstants.EnterpriseAPIKeyName, entLookup)
                    .Has(EntGraphConstants.RegistryName, registry)
                    .Has("Lookup", typeDefinition.Lookup);

                var existingResult = await SubmitFirst<MetadataModel>(existingQuery);

                var query = existingResult == null ?
                    g.AddV(EntGraphConstants.TypeDefinitionVertexName)
                    .Property(EntGraphConstants.RegistryName, registry)
                    .Property(EntGraphConstants.EnterpriseAPIKeyName, entLookup) : existingQuery;

                query = query
                    .Property("Active", typeDefinition.Active)
                    .Property("ConversionMethod", typeDefinition.ConversionMethod ?? "")
                    .Property("Description", typeDefinition.Description ?? "")
                    .Property("Lookup", typeDefinition.Lookup ?? "")
                    .Property("Name", typeDefinition.Name ?? "")
                    .Property("TypeName", typeDefinition.TypeName ?? "");

                typeDefinition.AllowedTypeNameConversions.Each(item =>
                {
                    query = query.Property(Cardinality.List, "AllowedTypeNameConversions", item, new object[] { });
                });

                if (typeDefinition.AllowedTypeNameConversions.Count() == 1)
                    query = query.Property(Cardinality.List, "AllowedTypeNameConversions", "filler", new object[] { });

                var tdResult = await SubmitFirst<TypeDefinition>(query);

                return tdResult;
            }, entLookup);
        }

        #endregion

        #region Helpers


        #endregion
    }
}
