using Gremlin.Net.CosmosDb;
using Gremlin.Net.CosmosDb.Serialization;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
using LCU.Graphs.Registry.Enterprises;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LCU.Graphs
{
	//	TODO:  Investigate shift to https://github.com/ExRam/ExRam.Gremlinq or another
	public class LCUGraph : ILCUGraph
	{
		#region Fields
		protected readonly GremlinClientPoolManager clientPool;
		#endregion

		#region Properties
		public virtual List<string> ListProperties { get; set; }
		#endregion

		#region Constructors
		public LCUGraph(GremlinClientPoolManager clientPool)
		{
			ListProperties = new List<string>();

			this.clientPool = clientPool;
		}
		#endregion

		#region API Methods
		public virtual async Task Submit(ITraversal traversal)
		{
			await Submit<dynamic>(traversal);
		}

		public virtual async Task Submit(string script)
		{
			await Submit<dynamic>(script);
		}

		public virtual async Task<ResultSet<T>> Submit<T>(ITraversal traversal)
		{
			return await Submit<T>(traversal.ToGremlinQuery());
		}

		public virtual async Task<ResultSet<T>> Submit<T>(string script)
		{
			return await withClient(async (client) =>
			{
				var res = await client.SubmitAsync<JToken>(script);

				var vals = res?.SelectMany(ta =>
				{
					var tokenArray = ta as JArray;

					return tokenArray.Select(token =>
					{
						if (token.Type == JTokenType.Object)
						{
							var newVal = mapGraphObjectProperties(token);

							return newVal.JSONConvert<T>();
						}
						else
						{
							return token.ToObject<T>();
						}
					});
				})?.ToList();

				return new ResultSet<T>(vals, res.StatusAttributes);
			});
		}

		public virtual async Task<T> SubmitFirst<T>(ITraversal traversal)
			where T : class
		{
			var resSet = await Submit<T>(traversal.ToGremlinQuery());

			return resSet?.FirstOrDefault();
		}
		#endregion

		#region Helpers
		protected virtual GremlinServer createServer(LCUGraphConfig config, string username)
		{
			return new GremlinServer(config.Host, config.Port, config.EnableSSL, username, config.APIKey);
		}

		protected virtual async Task ensureEdgeRelationships(Gremlin.Net.Process.Traversal.GraphTraversalSource g, 
			Guid parentId, Guid childId, string edgeToCheckBuy = EntGraphConstants.OwnsEdgeName,
			List<string> edgesToCreate = null)
		{
			if (edgesToCreate.IsNullOrEmpty())
				edgesToCreate = new List<string>()
				{
					EntGraphConstants.ConsumesEdgeName,
					EntGraphConstants.OwnsEdgeName,
					EntGraphConstants.ManagesEdgeName
				};

			var edgeResult = await SubmitFirst<dynamic>(g.V(parentId).Out(edgeToCheckBuy).HasId(childId));

			if (edgeResult == null)
			{
				var edgeQueries = edgesToCreate.Select(eq => g.V(parentId).AddE(eq).To(g.V(childId))).ToList();

				await edgeQueries.Each(async edgeQuery => await Submit(edgeQuery));
			}
		}

		protected virtual IDictionary<string, object> mapGraphObjectProperties(JToken token)
		{
			var val = token.JSONConvert<Dictionary<string, dynamic>>(serializer());

			var newVal = new Dictionary<string, object>();

			if (val.ContainsKey("properties"))
			{
				var props = ((object)val["properties"]).JSONConvert<Dictionary<string, JToken>>();

				newVal.Add("id", val["id"]);

				props.Each(prop =>
				{
					var propVals = prop.Value.ToObject<object[]>();

					if (!ListProperties.Any(lp => lp == prop.Key) && propVals.Length == 1)
						newVal.Add(prop.Key,
							propVals[0].JSONConvert<Dictionary<string, dynamic>>()["value"]);
					else if (ListProperties.Any(lp => lp == prop.Key) || propVals.Length > 1)
						newVal.Add(prop.Key,
							propVals.Select(pv => pv.JSONConvert<Dictionary<string, dynamic>>()["value"]).ToList());
				});
			}

			return newVal;
		}

		protected virtual async Task withClient(Func<GremlinClient, Task> action)
		{
			await withClient<object>(async (client) =>
			{
				await action(client);

				return null;
			});
		}

		protected virtual async Task<T> withClient<T>(Func<GremlinClient, Task<T>> action, string clientId = null)
		{
			using (var client = clientPool.LoadClient(clientId))
			{
				return await action(client);
			}
		}

		protected virtual async Task withG(Func<GremlinClient, Gremlin.Net.Process.Traversal.GraphTraversalSource, Task> action, string clientId = null)
		{
			await withG<object>(async (client, g) =>
			{
				await action(client, g);

				return null;
			}, clientId);
		}

		protected virtual async Task<T> withG<T>(Func<GremlinClient, Gremlin.Net.Process.Traversal.GraphTraversalSource, Task<T>> action, string clientId = null)
		{
			return await withClient<T>(async (client) =>
			{
				var g = new Gremlin.Net.CosmosDb.GraphTraversalSource(new DriverRemoteConnection(client));

				return await action(client, g.G());
			}, clientId);
		}
		#endregion

		protected virtual JsonSerializerSettings serializer()
		{
			return new JsonSerializerSettings
			{
				Converters = new JsonConverter[]
				{
					new TreeJsonConverter(),
					new IEdgeJsonConverter(),
					new ElementJsonConverter(),
					new IVertexJsonConverter(),
					new IsoDateTimeConverter
					{
						DateTimeStyles = DateTimeStyles.AdjustToUniversal
					}
				},
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			};
		}
	}
}
