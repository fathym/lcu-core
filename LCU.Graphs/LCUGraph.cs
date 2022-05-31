using Azure.Storage.Blobs;
using ExRam.Gremlinq.Core;
using Fathym;
using Fathym.Design;
using Gremlin.Net.Driver.Exceptions;
using LCU.Configuration;
using LCU.Graphs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using ExRam.Gremlinq.Providers.CosmosDb;
using ExRam.Gremlinq.Core.Models;
using System.Text.Json;

namespace LCU.Graphs
{
    public class LCUGraph
    {
        #region Fields
        protected readonly IConfiguration config;

        protected readonly BlobContainerClient containerClient;

        protected readonly LCUGraphConfig graphConfig;

        protected readonly ILogger logger;
        #endregion

        #region Properties
        public virtual IGremlinQuerySource g { get; protected set; }
        #endregion

        #region Constructors
        public LCUGraph(LCUGraphConfig graphConfig, IConfiguration config, ILogger logger)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            this.graphConfig = graphConfig ?? throw new ArgumentNullException(nameof(graphConfig));

            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            containerClient = buildGraphAuditContainerClient(graphConfig?.Audit, config);

            g = buildGremlinQuerySource(graphConfig);
        }
        #endregion

        #region API Methods
        public virtual async Task EnsureEdgeRelationship<TFrom, TEdge, TTo>(TFrom from, TTo to)
            where TFrom : LCUVertex
            where TEdge : LCUEdge, new()
            where TTo : LCUVertex
        {
            var existing = await g.V<TFrom>(from.ID)
                .Where(v => v.Registry == from.Registry)
                .Out<TEdge>()
                .OfType<TTo>()
                .Where(v => v.ID == to.ID)
                .Where(v => v.Registry == to.Registry)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var edge = await g.V<TFrom>(from.ID)
                    .Where(v => v.Registry == from.Registry)
                    .AddE(new TEdge()
                    {
                        ID = Guid.NewGuid(),
                        TenantLookup = from.TenantLookup
                    })
                    .To(__ => __.V<TTo>(to.ID).Where(v => v.Registry == to.Registry))
                    .FirstOrDefaultAsync();

                await writeEdgeAudit(edge, description: $"Edge added in {GetType().FullName}",
                    metadata: new JsonObject() { { "AuditType", "Create" } });
            }
        }

        public virtual async Task EnsureEdgeRelationship<TEdge>(Guid fromId, Guid toId, string registry, string toRegistry)
            where TEdge : LCUEdge, new()
        {
            var existingQuery = g.V<LCUVertex>(fromId)
                .Where(v => v.Registry == registry)
                .Out<TEdge>()
                .OfType<LCUVertex>()
                .Where(v => v.ID == toId);

            if (!toRegistry.IsNullOrEmpty())
                existingQuery = existingQuery.Where(v => v.Registry == toRegistry);

            var existing = await existingQuery
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var edge = await g.V(fromId)
                    .AddE(new TEdge()
                    {
                        ID = Guid.NewGuid(),
                        TenantLookup = registry
                    })
                    .To(__ => __.V(toId))
                    .FirstOrDefaultAsync();

                await writeEdgeAudit(edge, description: $"Edge added in {GetType().FullName}",
                    metadata: new JsonObject() { { "AuditType", "Create" } });
            }
        }

        public virtual async Task RemoveEdgeRelationship<TEdge>(Guid fromId, Guid toId, string registry)
            where TEdge : LCUEdge, new()
        {
            var outEdges = await g.V<LCUVertex>(fromId)
                .Registry(registry)
                .OutE<TEdge>()
                .ToListAsync();

            if (outEdges.FirstOrDefault()?.OutV == null)
                throw new ArgumentNullException("This didn't work");

            var existing = outEdges.FirstOrDefault(oe => oe.InV == toId);

            if (existing != null)
            {
                var edge = await g.E<LCUEdge>(existing.ID).FirstOrDefaultAsync();

                await g.E(existing.ID)
                    .Drop();

                await writeEdgeAudit(edge, description: $"Edge deleted in {GetType().FullName}",
                    metadata: new JsonObject() { { "AuditType", "Delete" } });
            }
        }

        public virtual async Task RemoveEdgeRelationships<TEdge>(Guid fromId, string registry)
            where TEdge : LCUEdge, new()
        {
            await g.V<LCUVertex>(fromId)
                .Registry(registry)
                .OutE<TEdge>()
                .Drop();
        }

        public virtual async Task RemoveEdgeRelationships(Guid fromId, string registry)
        {
            await g.V<LCUVertex>(fromId)
                .Registry(registry)
                .OutE()
                .Drop();
        }
        #endregion

        #region Helpers
        protected virtual Audit buildAudit<T>(T detail, string by = null, string description = null,
            JsonObject metadata = null)
        {
            by = by ?? "LCU System";

            description = description ?? GetType().FullName;

            //  TODO:  Use data protection services here to securely write json so sensitive data isn't exposed...
            //      Maybe secured with SALT of the TenantLookup
            var details = detail.ToJSON();

            return new Audit()
            {
                By = by,
                Description = description,
                Details = details,
                Metadata = metadata.JSONConvert<Dictionary<string, JsonElement>>()
            };
        }

        protected virtual string buildAuditPath(string tenantLookup, string auditType, string auditId)
        {
            var datePath = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

            var id = Guid.NewGuid();

            return $"{tenantLookup}/{graphConfig.Database}/{graphConfig.Graph}/{auditType}/{auditId}/{datePath}-{id}";
        }

        protected virtual BlobContainerClient buildGraphAuditContainerClient(LCUBlobStorageConfig blobStrgConfig,
            IConfiguration config)
        {
            if (blobStrgConfig != null)
            {
                var connStr = config[blobStrgConfig.StorageConnectionString] ??
                    blobStrgConfig.StorageConnectionString;

                var containerClient = new BlobContainerClient(connStr, blobStrgConfig.Container);

                containerClient.CreateIfNotExists();

                return containerClient;
            }
            else
                return null;
        }

        protected virtual IGremlinQuerySource buildGremlinQuerySource(LCUGraphConfig graphConfig)
        {
            return GremlinQuerySource.g.ConfigureEnvironment(env =>
            {
                var graphModel = GraphModel.FromBaseTypes<LCUVertex, LCUEdge>(lookup =>
                {
                    return lookup
                        .IncludeAssembliesOfBaseTypes()
                        .IncludeAssembliesFromStackTrace()
                        .IncludeAssembliesFromAppDomain();
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
                            });
                        //.ConfigureCustomSerializers(cs =>
                        //{
                        //    graphConfig.CustomSerializers.Each(csType =>
                        //    {
                        //        var type = Type.GetType(csType);

                        //        if (type != null)
                        //            cs.Add(new GenericGraphElementPropertySerializer(type));
                        //    });

                        //    //cs.Add(new GenericGraphElementPropertySerializer<MetadataModel>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<Audit>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<DataFlowOutput>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<ModulePackSetup>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<IdeSettingsConfigSolution[]>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<AccessConfiguration[]>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<AccessRight[]>());

                        //    //cs.Add(new GenericGraphElementPropertySerializer<Provider[]>());

                        //    return cs;
                        //});
                    }))
                    .ConfigureModel(m => m.ConfigureNativeTypes(t => t.Add(typeof(Guid))));
            })
            .UseCosmosDb(builder =>
            {
                var apiKey = config[graphConfig.APIKey] ?? graphConfig.APIKey;

                return builder
                    .At(new Uri(graphConfig.Host), graphConfig.Database, graphConfig.Graph)
                    .AuthenticateBy(apiKey)
                    .ConfigureWebSocket(builder =>
                    {
                        return builder;
                    });
            });
        }

        protected virtual async Task<T> createOrUpdateVertex<T>(T vertex,
            Func<IVertexGremlinQuery<T>> isExistingFilter = null, Action<T> configureVertex = null,
            bool failOnExists = false, bool failOnNotExists = false)
            where T : LCUVertex
        {
            return await createOrUpdateVertex<LCUVertex, LCUEdge, T>(vertex, null,
                isExistingFilter: isExistingFilter, configureVertex: configureVertex, failOnExists: failOnExists,
                failOnNotExists: failOnNotExists);
        }

        protected virtual async Task<T> createOrUpdateVertex<TParent, TParentEdge, T>(T vertex, Guid? parentId,
            Func<IVertexGremlinQuery<T>> isExistingFilter = null, Action<T> configureVertex = null,
            bool failOnExists = false, bool failOnNotExists = false)
            where T : LCUVertex
            where TParentEdge : LCUEdge, new()
            where TParent : LCUVertex
        {
            logger.LogInformation($"Creating or updating vertex {vertex.GetType().Name}");

            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            if (configureVertex != null)
                configureVertex(vertex);

            if (vertex.TenantLookup.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(vertex.TenantLookup));

            if (vertex.Registry.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(vertex.Registry));

            logger.LogInformation($"Checking for existing vertex {vertex.GetType().Name}");

            var existingBuilder = isExistingFilter == null ? g.V<T>(vertex.ID).Registry(vertex.Registry).Archived() : isExistingFilter();

            var existing = await existingBuilder
                .Registry(vertex.Registry)
                .FirstOrDefaultAsync();

            var vertexName = vertex.GetType().Name;

            string auditType;

            if (existing == null)
            {
                if (failOnNotExists)
                    throw new Exception($"The vertex {vertexName} does not exist");

                if (vertex.ID.IsEmpty())
                    vertex.ID = Guid.NewGuid();

                vertex.Created = DateTime.Now;

                vertex.Updated = DateTime.Now;

                logger.LogInformation($"Creating vertex {vertexName}: {vertex.ID}");

                vertex = await g.AddV(vertex)
                    .FirstOrDefaultAsync();

                auditType = "Create";
            }
            else
            {
                if (failOnExists)
                    throw new Exception($"The vertex for {vertexName} already exists");

                vertex.ID = existing.ID;

                vertex.Label = existing.Label;

                vertex.Registry = existing.Registry;

                vertex.TenantLookup = existing.TenantLookup;

                vertex.Created = existing.Created;

                vertex.Updated = DateTime.Now;

                vertex.Archived = false;

                logger.LogInformation($"Updating vertex {vertexName}: {vertex.ID}");

                vertex = await g.V<T>(vertex.ID)
                    .Update(vertex)
                    .FirstOrDefaultAsync();

                auditType = "Update";
            }

            logger.LogInformation($"Completed vertex create/update for {vertexName}: {vertex.ID}");

            await writeVertexAudit(vertex, description: $"Vertex saved in {GetType().FullName}",
                    metadata: new JsonObject() { { "AuditType", auditType } });

            if (parentId.HasValue)
            {
                var parent = await g.V<TParent>(parentId.Value)
                    .FirstOrDefaultAsync();

                await EnsureEdgeRelationship<TParent, TParentEdge, T>(parent, vertex);
            }

            return vertex;
        }

        protected virtual async Task<Status> deleteVertex<T>(Guid id, string registry,
            Func<IVertexGremlinQuery<T>> isExistingFilter = null)
            where T : LCUVertex
        {
            logger.LogInformation($"Deleting vertex {typeof(T).Name}");

            if (id.IsEmpty())
                throw new ArgumentNullException(nameof(id));

            logger.LogInformation($"Checking for existing vertex {typeof(T).Name} to delete for ID {id}");

            var existingBuilder = isExistingFilter == null ? g.V<T>(id).Registry(registry) : isExistingFilter();

            var existing = await existingBuilder.FirstOrDefaultAsync();

            if (existing != null)
            {
                logger.LogInformation($"Deleting existing vertex: {existing.ID}");

                existing.Archived = true;

                await g.V<T>(id)
                    .Registry(registry)
                    .Update(existing);

                logger.LogInformation($"Completed vertex {typeof(T).Name} delete for ID {id}");

                //await RemoveEdgeRelationships(id, registry);

                await writeVertexAudit(existing, description: $"Vertex deleted in {GetType().FullName}",
                    metadata: new JsonObject() { { "AuditType", "Delete" } });

                return Status.Success;
            }
            else
            {
                logger.LogInformation($"Unable to locate vertex {typeof(T).Name} with ID {id} to delete");

                return Status.GeneralError.Clone($"Unable to locate vertex {typeof(T).Name} with ID {id} to delete");
            }
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
                        logger.LogDebug($"Executing common graph boundary");

                        result = await action();

                        return false;
                    }
                    catch (Exception ex)
                    {
                        var retriable = false;

                        var retriableExceptionCodes = new List<int>() { 409, 412, 429, 1007, 1008 };

                        logger.LogError(ex, $"There was an error while executing the common graph boundary");

                        if (ex is ResponseException rex)
                        {
                            logger.LogInformation($"Determining if exception is retriable");

                            if (rex.StatusAttributes.ContainsKey("x-ms-status-code"))
                            {
                                var code = rex.StatusAttributes["x-ms-status-code"].As<int>();

                                retriable = retriableExceptionCodes.Contains(code);

                                if (retriable && rex.StatusAttributes.ContainsKey("x-ms-retry-after-ms"))
                                {
                                    var retryMsWait = rex.StatusAttributes["x-ms-retry-after-ms"].As<int>();

                                    logger.LogInformation($"Delaying the retry based on headers: {retryMsWait}");

                                    await Task.Delay(retryMsWait);
                                }
                            }
                            else
                                logger.LogInformation(rex.StatusCode.ToString());
                        }

                        var retriableTxt = retriable ? "" : "NOT ";

                        logger.LogInformation($"The common graph boundary exception was determined {retriableTxt}to be retriable");

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

        protected virtual async Task writeEdgeAudit<T>(T edge, string by = null, string description = null,
            JsonObject metadata = null)
            where T : LCUEdge
        {
            try
            {
                logger.LogDebug($"Writing audit for vertex");

                var auditBlobName = buildAuditPath(edge.TenantLookup, edge.GetType().Name, edge.ID.ToString());

                await writeAudit(auditBlobName, edge, by: by, description: description, metadata: metadata);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an unhandled error during the vertex audit process");
            }
        }

        protected virtual async Task writeVertexAudit<T>(T vertex, string by = null, string description = null,
            JsonObject metadata = null)
            where T : LCUVertex
        {
            try
            {
                logger.LogDebug($"Writing audit for vertex");

                var auditBlobName = buildAuditPath(vertex.TenantLookup, vertex.GetType().Name, vertex.ID.ToString());

                await writeAudit(auditBlobName, vertex, by: by, description: description, metadata: metadata);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an unhandled error during the vertex audit process");
            }
        }

        protected virtual async Task writeAudit<T>(string auditBlobName, T detail, string by = null,
            string description = null, JsonObject metadata = null)
        {
            try
            {
                logger.LogDebug($"Writing audit for detail");

                var audit = buildAudit(detail, by: by, description: description, metadata: metadata);

                await writeAudit(auditBlobName, audit);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an unhandled error during the detail audit process");
            }
        }

        protected virtual async Task writeAudit(string auditBlobName, Audit audit)
        {
            try
            {
                if (containerClient != null)
                {
                    logger.LogDebug($"Writing audit");

                    try
                    {
                        logger.LogDebug($"Writing audit for {auditBlobName}: {audit.ToJSON()}");

                        var blobClient = containerClient.GetBlobClient(auditBlobName);

                        var auditDetailsStream = new MemoryStream(Encoding.Default.GetBytes(audit.Details));

                        await blobClient.UploadAsync(auditDetailsStream);

                        var auditMetadata = audit.JSONConvert<Dictionary<string, string>>();

                        auditMetadata.Remove("Details");

                        await blobClient.SetMetadataAsync(auditMetadata);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"There was an error during the audit process for {auditBlobName}: {audit.ToJSON()}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an unhandled error during the audit process");
            }
        }
        #endregion
    }
}
