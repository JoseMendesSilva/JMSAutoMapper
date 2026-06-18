#if false
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Abstractions
{
    /// <summary>
    /// Mantém o estado e cache de instâncias durante o ciclo de vida de uma operação de mapeamento.
    /// </summary>
    public class ResolutionContext
    {
        private readonly MapperConfiguration _configuration;
        private readonly ConcurrentDictionary<object, object> _instanceCache = new();
        private readonly ConcurrentDictionary<object, Task<object>> _asyncInstanceCache = new();

        public ResolutionContext(MapperConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) return default!;
            if (_instanceCache.TryGetValue(source, out var cached)) return (TDestination)cached;

            var mapper = _configuration.CreateMapper();
            var result = mapper.Map<TSource, TDestination>(source);
            if (result != null) _instanceCache[source] = result!;
            return result;
        }

        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
        {
            if (source == null) return default!;
            if (_asyncInstanceCache.TryGetValue(source, out var cachedTask)) return (TDestination)await cachedTask;

            var mapper = _configuration.CreateMapper();
            return await mapper.MapAsync<TSource, TDestination>(source, cancellationToken).ConfigureAwait(false);
        }
    }
}
#endif
