using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
