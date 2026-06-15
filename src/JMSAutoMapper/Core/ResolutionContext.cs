// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using System.Collections.Concurrent;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Contexto de resolução para mapeamento.
    /// Fornece cache de instâncias durante o processo de mapeamento.
    /// </summary>
    public class ResolutionContext
    {
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<object, object> _instanceCache = new();
        private readonly ConcurrentDictionary<object, Task<object>> _asyncInstanceCache = new();

        /// <summary>Inicializa nova instância do ResolutionContext.</summary>
        public ResolutionContext(IMapper mapper) => _mapper = mapper;

        /// <summary>Mapeia objeto com cache de instâncias (previne loops infinitos).</summary>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) return default!;
            if (_instanceCache.TryGetValue(source, out var cached)) return (TDestination)cached;

            var result = _mapper.Map<TSource, TDestination>(source);
            if (result != null)
            {
                _instanceCache[source] = result!;
            }
            return result;
        }

        /// <summary>Mapeia objeto assincronamente com cache de instâncias.</summary>
        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
        {
            if (source == null) return default!;

            if (_asyncInstanceCache.TryGetValue(source, out var cachedTask))
            {
                var cachedResult = await cachedTask.ConfigureAwait(false);
                return (TDestination)cachedResult;
            }

            var task = _mapper.MapAsync<TSource, TDestination>(source, cancellationToken);

            // Converte Task<TDestination> para Task<object>
            var taskObject = task.ContinueWith(t => (object)t.Result!, cancellationToken);

            _asyncInstanceCache[source] = taskObject;

            var result = await task.ConfigureAwait(false);
            return result;
        }
    }

    
}
