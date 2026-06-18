using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Cache
{
    /// <summary>
    /// Implementação em memória do cache distribuído.
    /// Útil para testes e ambientes single-server.
    /// </summary>
    public class InMemoryDistributedCache : IDistributedMapperCache
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

        private class CacheItem
        {
            public object? Value { get; set; }
            public DateTime Expiration { get; set; }
        }

        /// <summary>Obtém valor do cache.</summary>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out var item) && item.Expiration > DateTime.UtcNow)
            {
                return Task.FromResult((T?)item.Value);
            }

            _cache.TryRemove(key, out _);
            return Task.FromResult(default(T));
        }

        /// <summary>Armazena valor no cache.</summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var item = new CacheItem
            {
                Value = value,
                Expiration = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(10))
            };

            _cache[key] = item;
            return Task.CompletedTask;
        }

        /// <summary>Remove valor do cache.</summary>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }
}