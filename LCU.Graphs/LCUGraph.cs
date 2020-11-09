using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Core.AspNet;
using ExRam.Gremlinq.Providers.WebSocket;
using Fathym;
using Fathym.Design;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.Graphs.Registry.Enterprises.DataFlows;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Graphs.Registry.Enterprises.Identity;
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
                                cs.Add(new GenericGraphElementPropertySerializer<MetadataModel>());

                                cs.Add(new GenericGraphElementPropertySerializer<Audit>());

                                cs.Add(new GenericGraphElementPropertySerializer<DataFlowOutput>());

                                cs.Add(new GenericGraphElementPropertySerializer<ModulePackSetup>());

                                cs.Add(new GenericGraphElementPropertySerializer<IdeSettingsConfigSolution[]>());

                                cs.Add(new GenericGraphElementPropertySerializer<AccessConfiguration[]>());

                                cs.Add(new GenericGraphElementPropertySerializer<AccessRight[]>());

                                cs.Add(new GenericGraphElementPropertySerializer<Provider[]>());

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
                    })
                    .ConfigureModel(m => m.ConfigureNativeTypes(t => t.Add(typeof(Guid))));
            });
        }
        #endregion

        #region API Methods
        public virtual async Task EnsureEdgeRelationship<TEdge>(Guid fromId, Guid toId)
            where TEdge : new()
        {
            var outEdges = await g.V(fromId)
                .Out<TEdge>()
                .OfType<LCUVertex>()
                .ToListAsync();

            var existing = outEdges.FirstOrDefault(oe => oe.ID == toId);

            if (existing == null)
                await g.V(fromId)
                    .AddE<TEdge>()
                    .To(__ => __.V(toId))
                    .FirstOrDefaultAsync();
        }

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

        protected virtual async Task withCommonGraphBoundary(Func<Task> action)
        {
            var result = await withCommonGraphBoundary(async () =>
            {
                await action();

                return Status.Success;
            });
        }

        protected virtual async Task<T> withCommonGraphBoundary<T>(Func<Task<T>> action)
        {
            T result = default(T);

            await DesignOutline.Instance.Retry()
                .SetActionAsync(async () =>
                {
                    try
                    {
                        result = await action();

                        return false;
                    }
                    catch (Exception ex)
                    {
                        var retriable = false;

                        var retriableExceptionCodes = new List<int>() { 409, 412, 429, 1007, 1008 };

                        if (ex is ResponseException rex)
                        {
                            var code = rex.StatusAttributes["x-ms-status-code"].As<int>();

                            retriable = retriableExceptionCodes.Contains(code);

                            if (retriable && rex.StatusAttributes.ContainsKey("x-ms-retry-after-ms"))
                            {
                                var retryMsWait = rex.StatusAttributes["x-ms-retry-after-ms"].As<int>();

                                await Task.Delay(retryMsWait);
                            }
                        }

                        if (!retriable)
                            throw;

                        return retriable;
                    }
                })
                .SetCycles(10)
                .SetThrottle(25)
                .SetThrottleScale(2)
                .Run();

            return result;
        }
        #endregion
    }


}
