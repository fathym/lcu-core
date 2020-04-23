using Fathym;
using Gremlin.Net.Structure;
using Gremlin.Net.Process.Traversal;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCU.Graphs
{
	public static class LCUGraphExtensions
	{
		public static GraphTraversal<Vertex, Vertex> AttachMetadataProperties<T>(this GraphTraversal<Vertex, Vertex> query, T entity) where T : MetadataModel
		{
			var properties = entity.Metadata;

			foreach (string key in properties.Keys)
			{
				query = query.Property(key, properties[key].ToString());
			}

			return query;
		}

		public static GraphTraversal<Vertex, Vertex> AttachList<T>(this GraphTraversal<Vertex, Vertex> query, string propertyName, List<T> entities)
		{
			query = query.Properties<Vertex>(propertyName).Drop();

			entities.Each(entity =>
			{
				query = query.Property(Cardinality.List, propertyName, entity, new object[] { });
			});

			return query;
		}

	}
}
