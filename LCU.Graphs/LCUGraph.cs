using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Core.AspNet;
using ExRam.Gremlinq.Providers.WebSocket;
using Fathym;
using Gremlin.Net.Structure;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
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
    public class LCUGraph
    {
        #region Fields
        #endregion

        #region Properties
        public virtual IGremlinQuerySource g { get; protected set; }
        #endregion

        #region Constructors
        public LCUGraph(LCUGraphConfig graphConfig, ILogger logger)
        {
            g = GremlinQuerySource.g.ConfigureEnvironment(env =>
            {
                var graphModel = GraphModel.FromBaseTypes<LCUVertex, LCUEdge>(lookup =>
                {
                    return lookup.IncludeAssembliesOfBaseTypes();
                });

                return env
                    .UseLogger(logger)
                    .UseModel(graphModel.ConfigureProperties(model =>
                    {
                        return model
                            .ConfigureElement<LCUVertex>(conf =>
                            {
                                return conf
                                    .IgnoreOnUpdate(x => x.Registry);
                            })
                            .ConfigureCustomSerializers(cs =>
                            {
                                cs.Add(new GraphElementPropertySerializer(
                                    pi =>
                                    {
                                        return pi.PropertyType == typeof(MetadataModel);
                                    },
                                    obj =>
                                    {
                                        return new Dictionary<string, string>()
                                        {
                                            { "", obj.ToJSON() }
                                        };
                                    },
                                    type =>
                                    {
                                        return type == typeof(MetadataModel);
                                    },
                                    token =>
                                    {
                                        return token[0]["value"].ToString().FromJSON<MetadataModel>();
                                    })
                                );

                                return cs;
                            });
                    }))
                    .UseCosmosDb(builder =>
                    {
                        return builder
                            .At(new Uri(graphConfig.Host), graphConfig.Database, graphConfig.Graph)
                            .AuthenticateBy(graphConfig.APIKey)
                            .ConfigureWebSocket(builder =>
                            {
                                return builder;
                            });
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

        protected virtual async Task ensureEdgeRelationship<TEdge>(Guid fromId, Guid toId)
            where TEdge : new()
        {
            var existing = await g.V(fromId)
                .Out<TEdge>()
                .V(toId)
                .FirstOrDefaultAsync();

            if (existing == null)
                await g.V(fromId)
                    .AddE<TEdge>()
                    .To(__ => __.V(toId))
                    .FirstOrDefaultAsync();
        }
        #endregion
    }
}
