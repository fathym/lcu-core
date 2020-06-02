using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Fathym;
using Gremlin.Net.Structure;
using LCU.Graphs.Registry.Enterprises;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
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
		public virtual IGremlinQuerySource g { get; protected set; }
		#endregion

		#region Constructors
		public LCUGraph(LCUGraphConfig graphConfig, ILogger logger)
		{
			g = g.ConfigureEnvironment(env =>
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
		protected virtual Audit buildAudit(string by = null, string description = null)
		{
			by = by ?? "LCU System";

			description = description ?? GetType().FullName;

			return new Audit()
			{
				By = by,
				Description = description
			};
		}
		#endregion
	}
}
