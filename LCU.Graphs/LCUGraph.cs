using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Gremlin.Net.Structure;
using LCU.Graphs.Registry.Enterprises;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace LCU.Graphs
{
	public class LCUGraph : ILCUGraph
	{
		#region Fields
		#endregion

		#region Properties
		public virtual IGremlinQuerySource G { get; protected set; }
		#endregion

		#region Constructors
		public LCUGraph(LCUGraphConfig graphConfig, ILogger logger)
		{
			G = g.ConfigureEnvironment(env =>
			{
				return env
					.UseLogger(logger)
					.UseModel(GraphModel.FromBaseTypes<LCUVertex, LCUEdge>(lookup =>
					{
						return lookup.IncludeAssembliesOfBaseTypes();
					}))
					.UseCosmosDb(builder =>
					{
						return builder.ConfigureWebSocket(builder =>
						{
							return builder.ConfigureQueryLoggingOptions(o =>
							{
								return o.SetQueryLoggingVerbosity(QueryLoggingVerbosity.None);
							});
						})
						.At(graphConfig.Host, graphConfig.Database, graphConfig.Graph)
						.AuthenticateBy(graphConfig.APIKey);
					});
			});
		}
		#endregion

		#region API Methods
		#endregion

		#region Helpers
		//protected virtual async Task ensureEdgeRelationships(Gremlin.Net.Process.Traversal.GraphTraversalSource g, 
		//	Guid parentId, Guid childId, string edgeToCheckBuy = EntGraphConstants.OwnsEdgeName,
		//	List<string> edgesToCreate = null)
		//{
		//	if (edgesToCreate.IsNullOrEmpty())
		//		edgesToCreate = new List<string>()
		//		{
		//			EntGraphConstants.ConsumesEdgeName,
		//			EntGraphConstants.OwnsEdgeName,
		//			EntGraphConstants.ManagesEdgeName
		//		};

		//	var edgeResult = await SubmitFirst<dynamic>(g.V(parentId).Out(edgeToCheckBuy).HasId(childId));

		//	if (edgeResult == null)
		//	{
		//		var edgeQueries = edgesToCreate.Select(eq => g.V(parentId).AddE(eq).To(g.V(childId))).ToList();

		//		await edgeQueries.Each(async edgeQuery => await Submit(edgeQuery));
		//	}
		//}

		#endregion
	}
}
