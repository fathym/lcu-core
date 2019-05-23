using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
	public static class LCUDistributedCachingExtensions
	{
		public static async Task<TItem> GetOrCreateAsync<TItem>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, Task<TItem>> factory,
			CancellationToken token = default(CancellationToken))
		{
			var itemStr = await cache.GetStringAsync(key, token);

			var item = itemStr.IsNullOrEmpty() ? default(TItem) : itemStr.FromJSON<TItem>();

			if (item == null)
			{
				var options = new DistributedCacheEntryOptions();

				item = await factory(options);

				await cache.SetStringAsync(key, item.ToJSON(), options);
			}

			return item;
		}

		public static async Task<TItem> GetOrCreateAsync<TItem>(this IDistributedCache cache, IMemoryCache memCache, string key, Func<ICacheEntry, DistributedCacheEntryOptions, Task<TItem>> factory,
			CancellationToken token = default(CancellationToken))
		{
			return await memCache.GetOrCreateAsync(key, async (entry) =>
			{
				return await cache.GetOrCreateAsync(key, async (options) =>
				{
					return await factory(entry, options);
				});
			});
		}
	}
}
