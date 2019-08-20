using Gremlin.Net.CosmosDb;
using Gremlin.Net.CosmosDb.Serialization;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure.IO.GraphSON;
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
	public abstract class LCUGraph : ILCUGraph
	{
		#region Fields
		protected LCUGraphConfig config;

		protected GremlinServer server;
		#endregion

		#region Properties
		public virtual List<string> ListProperties { get; set; }
		#endregion

		#region Constructors
		public LCUGraph(LCUGraphConfig config)
		{
			this.config = config;

			ListProperties = new List<string>();

			var username = $"/dbs/{config.Database}/colls/{config.Graph}";

			server = createServer(config, username);
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

		protected virtual async Task<T> withClient<T>(Func<GremlinClient, Task<T>> action)
		{
			using (var client = new GremlinClient(server, new GraphSON2Reader(), new GraphSON2Writer(),
				GremlinClient.GraphSON2MimeType))
			{
				return await action(client);
			}
			//using (var client = new GraphClient(config.Host, config.Database, config.Graph, config.APIKey))
			//{
			//	return await action(client);
			//}
		}

		protected virtual async Task withG(Func<GremlinClient, Gremlin.Net.Process.Traversal.GraphTraversalSource, Task> action)
		{
			await withG<object>(async (client, g) =>
			{
				await action(client, g);

				return null;
			});
		}

		protected virtual async Task<T> withG<T>(Func<GremlinClient, Gremlin.Net.Process.Traversal.GraphTraversalSource, Task<T>> action)
		{
			return await withClient<T>(async (client) =>
			{
				var g = new Gremlin.Net.CosmosDb.GraphTraversalSource(new DriverRemoteConnection(client));

				return await action(client, g.G());
			});
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
