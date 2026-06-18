#if false
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using JMSAutoMapper.Internals;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Base do motor de mapeamento, integrando diagnóstico e conversores.
    /// </summary>
    public abstract partial class MapperBase : IMapper
    {
        protected internal readonly MapperConfiguration _config;
        protected readonly Action<string, Exception>? _logger;
        protected readonly DiagnosticCollector _diagnostics;
        protected readonly IDistributedMapperCache? _distributedCache;
        protected readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        protected MapperBase(MapperConfiguration config, Action<string, Exception>? logger = null, IDistributedMapperCache? distributedCache = null)
        {
            _config = Guard.ThrowIfNull(config);
            _logger = logger;
            _distributedCache = distributedCache;
            _diagnostics = new DiagnosticCollector(config);
        }

        public abstract T Map<T>(object? source);
        public abstract TDestination Map<TSource, TDestination>(TSource source);
        public abstract TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        public abstract Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
        public abstract Task<T> MapAsync<T>(object? source, CancellationToken cancellationToken = default);
        public abstract IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;
        public abstract IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
        
        public virtual object Map(object source, Type sourceType, Type destinationType)
        {
            if (source == null) return null!;
            var method = typeof(IMapper).GetMethod(nameof(Map), new[] { typeof(object) })?.MakeGenericMethod(destinationType);
            return method?.Invoke(this, new[] { source })!;
        }

        public void AssertConfigurationIsValid() => _config.AssertConfigurationIsValidInternal();

        public DiagnosticInfo GetDiagnostics()
        {
            // Obtém memória aproximada usada pelo processo para o relatório
            long memory = GC.GetTotalMemory(false);
            return _diagnostics.GetInfo(memory);
        }
    }
}
#endif
