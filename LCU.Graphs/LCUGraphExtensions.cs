using ExRam.Gremlinq.Core;
using Fathym;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Graphs
{
    public static class LCUGraphExtensions
    {
        public static IVertexGremlinQuery<TVertex> Archived<TVertex>(this IVertexGremlinQuery<TVertex> query, bool? archived = false)
            where TVertex : LCUVertex
        {
            if (!archived.HasValue)
                return query;
            else
                return query.Where(e => e.Archived == archived.Value);
        }

        public static GraphTraversal<Vertex, Vertex> AttachMetadataProperties<T>(this GraphTraversal<Vertex, Vertex> query, T entity)
            where T : MetadataModel
        {
            var properties = entity.Metadata;

            foreach (string key in properties.Keys)
            {
                query = query.Property(key, properties[key].ToString());
            }

            return query;
        }

        public static GraphTraversal<Vertex, Vertex> AttachList<T>(this GraphTraversal<Vertex, Vertex> query, string propertyName, List<T> entities,
            bool distinct = true)
        {
            var isFirst = true;

            if (distinct)
                entities = entities?.Distinct().ToList();

            entities?.Each(entity =>
            {
                if (isFirst)
                    query = query.Property(propertyName, entity);
                else
                    query = query.Property(Cardinality.List, propertyName, entity);

                isFirst = false;
            });

            return query;
        }

        public static IVertexGremlinQuery<TVertex> CreatedSince<TVertex>(this IVertexGremlinQuery<TVertex> query, DateTime? createdSince = null)
            where TVertex : LCUVertex
        {
            if (!createdSince.HasValue)
                return query;
            else
                return query.Where(e => e.Created >= createdSince);
        }

        public static IVertexGremlinQuery<TVertex> UpdatedSince<TVertex>(this IVertexGremlinQuery<TVertex> query, DateTime? updatedSince = null)
            where TVertex : LCUVertex
        {
            if (!updatedSince.HasValue)
                return query;
            else
                return query.Where(e => e.Updated >= updatedSince);
        }

        public static IVertexGremlinQuery<TVertex> Registry<TVertex>(this IVertexGremlinQuery<TVertex> query, string registry)
            where TVertex : LCUVertex
        {
            if (registry.IsNullOrEmpty())
                return query;
            else
                return query.Where(e => e.Registry == registry);
        }

        public static IVertexGremlinQuery<TVertex> Tenant<TVertex>(this IVertexGremlinQuery<TVertex> query, string tenantLookup)
            where TVertex : LCUVertex
        {
            if (tenantLookup.IsNullOrEmpty())
                return query;
            else
                return query.Where(e => e.TenantLookup == tenantLookup);
        }
    }
}

namespace LCU
{
    public static class CandidateExtensions
    {
        public static string SecondToLastToEnd(this string value, string lookup)
        {
            return value.Substring(value.Substring(0, value.LastIndexOf(lookup)).LastIndexOf(lookup) + 1);
        }
    }
}

namespace ExRam.Gremlinq.Core
{
    public static class CandidateExtensions
    {
        public static async ValueTask<List<TElement>> ToListAsync<TElement>(this IGremlinQueryBase<TElement> query, CancellationToken ct = default)
        {
            var results = await query.ToArrayAsync();

            return results.ToList();
        }
    }
}