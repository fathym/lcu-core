using Fathym;
using Fathym.Design;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class LCUDistributedCachingExtensions
    {
        private static ILogger<IDistributedCache> logger = new LoggerFactory().CreateLogger<IDistributedCache>();

        public static async Task<TItem> GetOrCreateAsync<TItem>(this IDistributedCache cache, string key,
            Func<DistributedCacheEntryOptions, Task<TItem>> factory, Func<TItem, Status> shouldCache = null,
            CancellationToken token = default(CancellationToken))
        {
            logger.LogDebug($"Getting or creating key {key} in distributed cache for type {typeof(TItem).Name}");

            var itemStr = await cache.GetStringAsync(key, token: token);

            var item = itemStr.IsNullOrEmpty() ? default(TItem) : itemStr.FromJSON<TItem>();

            if (item == null)
            {
                await DesignOutline.Instance.Retry()
                    .SetActionAsync(async () =>
                    {
                        try
                        {
                            logger.LogDebug($"Calling factory for key {key} in distributed cache for type {typeof(TItem).Name}");

                            var options = new DistributedCacheEntryOptions();

                            item = await factory(options);

                            if (item != null && (shouldCache == null || shouldCache(item)))
                            {
                                logger.LogDebug($"Caching key {key} in distributed cache for type {typeof(TItem).Name}");

                                await cache.SetStringAsync(key, item.ToJSON(), options, token: token);
                            }
                            else
                            {
                                logger.LogDebug($"Loaded value for key {key} could not be cached in distributed cache for type {typeof(TItem).Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex is RedisTimeoutException;
                        }

                        return false;
                    })
                    .SetCycles(5)
                    .SetThrottle(50)
                    .SetThrottleScale(1.5)
                    .Run();
            }

            return item;
        }

        public static async Task<byte[]> GetOrCreateStreamAsync(this IDistributedCache cache, string key,
            Func<DistributedCacheEntryOptions, Task<byte[]>> factory, Func<byte[], Status> shouldCache = null,
            CancellationToken token = default(CancellationToken))
        {
            logger.LogDebug($"Getting or creating key {key} in distributed cache for bytes");

            var item = await cache.GetAsync(key, token: token);

            if (item.IsNullOrEmpty())
            {
                await DesignOutline.Instance.Retry()
                    .SetActionAsync(async () =>
                    {
                        try
                        {
                            logger.LogDebug($"Calling factory for key {key} in distributed cache for bytes");

                            var options = new DistributedCacheEntryOptions();

                            item = await factory(options);

                            if (!item.IsNullOrEmpty() && (shouldCache == null || shouldCache(item)))
                            {
                                logger.LogDebug($"Caching key {key} in distributed cache for bytes");

                                await cache.SetStringAsync(key, item.ToJSON(), options, token: token);
                            }
                            else
                            {
                                logger.LogDebug($"Loaded value for key {key} could not be cached in distributed cache for bytes");
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex is RedisTimeoutException;
                        }

                        return false;
                    })
                    .SetCycles(5)
                    .SetThrottle(50)
                    .SetThrottleScale(1.5)
                    .Run();
            }

            return item;
        }

        public static async Task<TItem> GetOrCreateAsync<TItem>(this IDistributedCache cache, IMemoryCache memCache, string key,
            Func<ICacheEntry, DistributedCacheEntryOptions, Task<TItem>> factory, CancellationToken token = default(CancellationToken))
        {
            bool fromCache = true;

            var cached = await memCache.GetOrCreateAsync(key, async (entry) =>
            {
                fromCache = false;

                return await cache.GetOrCreateAsync(key, async (options) =>
                {
                    return await factory(entry, options);
                }, token: token);
            });

            if (fromCache && cached == null)
            {
                memCache.Remove(key);

                await cache.RemoveAsync(key);

                cached = await cache.GetOrCreateAsync<TItem>(memCache, key, factory, token);
            }

            return cached;
        }

        public static async Task<byte[]> GetOrCreateStreamAsync(this IDistributedCache cache, IMemoryCache memCache, string key,
            Func<ICacheEntry, DistributedCacheEntryOptions, Task<byte[]>> factory, CancellationToken token = default(CancellationToken))
        {
            bool fromCache = true;

            var cached = await memCache.GetOrCreateAsync(key, async (entry) =>
            {
                fromCache = false;

                return await cache.GetOrCreateAsync(key, async (options) =>
                {
                    return await factory(entry, options);
                }, token: token);
            });

            if (fromCache && cached == null)
            {
                memCache.Remove(key);

                await cache.RemoveAsync(key);

                cached = await cache.GetOrCreateStreamAsync(memCache, key, factory, token);
            }

            return cached;
        }
    }
}
