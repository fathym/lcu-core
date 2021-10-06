using Fathym;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LCU.Graphs
{
    public static class LCUGraphExtensions
    {
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