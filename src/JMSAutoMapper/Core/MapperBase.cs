// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Conversion;
using JMSAutoMapper.DependencyInjection; // Adicionado para MapperExtensions
using JMSAutoMapper.Diagnostics;
using JMSAutoMapper.Expressions;
using JMSAutoMapper.Reflection;
using JMSAutoMapper.Validation;
using JMSAutoMapper.Cache;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ReferenceEqualityComparer = JMSAutoMapper.Reflection.ReferenceEqualityComparer;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Classe base abstrata para mapeadores.
    /// Contém a lógica comum de mapeamento.
    /// </summary>
    public abstract class MapperBase : IMapper
    {
        /// <summary>Configuração do mapeador.</summary>
        protected readonly MapperConfiguration _config;
        /// <summary>Logger para registro de erros.</summary>
        protected readonly Action<string, Exception>? _logger;
        /// <summary>Pool de expressões.</summary>
        protected readonly ExpressionPool _expressionPool;
        /// <summary>Cache distribuído.</summary>
        protected readonly IDistributedMapperCache? _distributedCache;
        /// <summary>Cache de metadados de tipos.</summary>
        protected readonly ConcurrentDictionary<Type, TypeMetadata> _typeMetadataCache = new();
        /// <summary>Cache de mapeadores compilados.</summary>
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledMappers = new();
        /// <summary>Cache de mapeadores com destino.</summary>
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledMappersWithDestination = new();
        /// <summary>Cache de delegados assíncronos.</summary>
        protected readonly ConcurrentDictionary<Type, Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>> _mapComplexTypeAsyncDelegates = new();
        /// <summary>Cache de mapeadores assíncronos.</summary>
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledAsyncMappers = new();
        /// <summary>Cache de delegados para tipos complexos.</summary>
        protected readonly ConcurrentDictionary<Type, Func<object, Dictionary<object, object>, object>> _mapComplexTypeDelegates = new();
        /// <summary>Coletor de diagnósticos.</summary>
        protected readonly DiagnosticCollector _diagnostics;
        /// <summary>Cache estático para objetos imutáveis.</summary>
        protected readonly ConcurrentDictionary<string, Task<object>> _staticCache = new();
        /// <summary>Locks para sincronização de cache.</summary>
        protected readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

        /// <summary>Coletor interno de diagnósticos de performance.</summary>
        protected class DiagnosticCollector
        {
            private readonly MapperConfiguration _config;
            private long _totalMapsExecuted;
            private long _totalMapTimeMs;
            private long _cacheHits;
            private long _cacheMisses;
            private long _totalTimeSavedByCache;
            private readonly ConcurrentQueue<string> _recentErrors = new();
            private readonly ConcurrentQueue<string> _slowMappings = new();

            /// <summary>Inicializa o coletor.</summary>
            public DiagnosticCollector(MapperConfiguration config) => _config = config;

            /// <summary>Registra a execução de um mapeamento.</summary>
            public void RecordMap(string sourceType, string targetType, long elapsedMs)
            {
                if (!_config.EnableDiagnostics) return;

                Interlocked.Increment(ref _totalMapsExecuted);
                Interlocked.Add(ref _totalMapTimeMs, elapsedMs);

                if (elapsedMs > 100)
                {
                    _slowMappings.Enqueue($"{sourceType} -> {targetType}: {elapsedMs}ms");
                    while (_slowMappings.Count > 10) _slowMappings.TryDequeue(out _);
                }
            }

            /// <summary>Registra um acerto no cache.</summary>
            public void RecordCacheHit(long timeSavedMs)
            {
                Interlocked.Increment(ref _cacheHits);
                Interlocked.Add(ref _totalTimeSavedByCache, timeSavedMs);
            }

            /// <summary>Registra uma falha no cache.</summary>
            public void RecordCacheMiss()
            {
                Interlocked.Increment(ref _cacheMisses);
            }

            /// <summary>Registra um erro ocorrido durante o mapeamento.</summary>
            public void RecordError(Exception ex, string context)
            {
                if (!_config.EnableDiagnostics) return;

                _recentErrors.Enqueue($"{context}: {ex.Message}");
                while (_recentErrors.Count > 10) _recentErrors.TryDequeue(out _);
            }

            /// <summary>Gera o relatório de diagnóstico.</summary>
            public DiagnosticInfo GetInfo(long memoryUsed)
            {
                return new DiagnosticInfo
                {
                    TotalMapsExecuted = _totalMapsExecuted,
                    AverageMapTimeMs = _totalMapsExecuted > 0 ? (double)_totalMapTimeMs / _totalMapsExecuted : 0,
                    RecentErrors = _recentErrors.ToList(),
                    SlowMappings = _slowMappings.ToList(),
                    ErrorCount = _recentErrors.Count,
                    MemoryUsedBytes = memoryUsed,
                    CacheStats = new CacheStatistics
                    {
                        CacheHits = _cacheHits,
                        CacheMisses = _cacheMisses,
                        AverageTimeSavedMs = _cacheHits > 0 ? (double)_totalTimeSavedByCache / _cacheHits : 0
                    }
                };
            }
        }

        /// <summary>Construtor.</summary>
        protected MapperBase(MapperConfiguration config, Action<string, Exception>? logger = null, IDistributedMapperCache? distributedCache = null)
        {
            _config = config;
            _logger = logger;
            _expressionPool = new ExpressionPool();
            _distributedCache = distributedCache;
            _diagnostics = new DiagnosticCollector(config);
        }

        /// <inheritdoc/>
        public abstract IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;

        /// <inheritdoc/>
        public abstract IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);

        /// <inheritdoc/>
        public virtual T Map<T>(object? source)
        {
            var stopwatch = Stopwatch.StartNew();
            var targetType = typeof(T);

            try
            {
                if (source == null)
                {
                    // Se o destino for uma coleção, retorna uma instância vazia
                    if (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string))
                    {
                        if (targetType.IsGenericType)
                        {
                            var def = targetType.GetGenericTypeDefinition();
                            // Verifica se é uma das coleções imutáveis que não possuem construtor padrão
                            if (def == typeof(ImmutableArray<>) || def == typeof(ImmutableList<>) || 
                                def == typeof(ImmutableDictionary<,>) || def == typeof(ImmutableQueue<>) || 
                                def == typeof(ImmutableStack<>))
                            {
                                var emptyProp = targetType.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);
                                if (emptyProp != null) return (T)emptyProp.GetValue(null)!;
                                
                                var emptyField = targetType.GetField("Empty", BindingFlags.Public | BindingFlags.Static);
                                if (emptyField != null) return (T)emptyField.GetValue(null)!;
                            }
                        }

                        if (targetType.IsInterface)
                        {
                            var itemType = GetCollectionItemType(targetType) ?? typeof(object);
                            return (T)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
                        }
                        return (T)Activator.CreateInstance(targetType)!;
                    }

                    if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                        throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{typeof(T).Name}'.");
                    return default!;
                }

                // 1. Tentar Mapeador de Coleção Pré-compilado (Alta Performance)
                // A lógica de mapeamento de coleção foi movida para JMSMapper.MapObject
                // e será tratada lá para melhor consistência com o pipeline de Expression Trees.

                var sourceType = source.GetType();

                if (targetType.IsAssignableFrom(sourceType) || IsSimpleType(targetType))
                    return (T)ConvertValue(source, targetType)!;

                var mappedObjects = ObjectPoolProvider.GetDictionary();
                try {
                    return MapObject<T>(source, mappedObjects);
                }
                finally {
                    ObjectPoolProvider.ReturnDictionary(mappedObjects);
                }
            }
            finally
            {
                stopwatch.Stop();
                _diagnostics.RecordMap(typeof(T).Name, source?.GetType().Name ?? "null", stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>Obtém o tipo de item de uma coleção.</summary>
        protected Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            
            // Se for dicionário, retornamos null para que seja tratado por lógica específica de dicionário
            if (typeof(IDictionary).IsAssignableFrom(collectionType)) return null;

            if (collectionType.IsGenericType) return collectionType.GetGenericArguments().FirstOrDefault();

            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                var ienumerable = collectionType.GetInterface("IEnumerable`1");
                if (ienumerable != null) return ienumerable.GetGenericArguments().FirstOrDefault();
            }

            return null;
        }

        /// <inheritdoc/>
        public virtual async Task<T> MapAsync<T>(object? source, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var targetType = typeof(T);
            try
            {
                if (source == null)
                {
                    var isNonNullableValueType = targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null;
                    if (isNonNullableValueType)
                    {
                        if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Throw)
                            throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{targetType.Name}'.");
                        
                        return default!; // Default e Ignore no nível raiz para tipos de valor retornam default(T)
                    }
                    return default!;
                }

                var sourceType = source.GetType();

                // Verificar cache se habilitado
                if (_distributedCache != null && _config.EnableDistributedCache)
                {
                    // Gerar chave CONSISTENTE baseada nos tipos e no objeto
                    // Esta é a versão CORRIGIDA que garante a mesma chave para o mesmo objeto
                    var cacheKey = CacheKeyGenerator.GenerateKey(sourceType, targetType, source);

                    // Tentar cache
                    var cached = await _distributedCache.GetAsync<T>(cacheKey, cancellationToken).ConfigureAwait(false);
                    if (cached != null)
                    {
                        _diagnostics.RecordCacheHit(stopwatch.ElapsedMilliseconds);
                        return cached;
                    }

                    _diagnostics.RecordCacheMiss();

                    // Mapear e cachear
                    var result = await PerformMappingAsync<T>(source, targetType, sourceType, cancellationToken).ConfigureAwait(false);

                    // Usar tempo de expiração apropriado
                    var expiration = TimeSpan.FromMinutes(
                        CacheKeyGenerator.GetExpirationForType(sourceType));

                    await _distributedCache.SetAsync(cacheKey, result, expiration, cancellationToken).ConfigureAwait(false);

                    return result;
                }

                _diagnostics.RecordCacheMiss();
                var mappedObjects = ObjectPoolProvider.GetDictionary();
                try {
                    return await MapObjectAsync<T>(source, new ConcurrentDictionary<object, object>(mappedObjects, ReferenceEqualityComparer.Instance), cancellationToken).ConfigureAwait(false);
                }
                finally {
                    ObjectPoolProvider.ReturnDictionary(mappedObjects);
                }
            }
            finally
            {
                stopwatch.Stop();
                _diagnostics.RecordMap(typeof(T).Name, source?.GetType().Name ?? "null", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<T> PerformMappingAsync<T>(object source, Type targetType, Type sourceType, CancellationToken cancellationToken)
        {
            if (targetType.IsAssignableFrom(sourceType) || IsSimpleType(targetType))
                return (T)ConvertValue(source, targetType)!;

            var mappedObjects = new ConcurrentDictionary<object, object>(ReferenceEqualityComparer.Instance);
            return await MapObjectAsync<T>(source, mappedObjects, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return Map<TDestination>(source);
        }

        /// <inheritdoc/>
        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
        {
            return await MapAsync<TDestination>(source, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null) return destination;
            var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return MapObjectWithDestination(source, destination, mappedObjects);
        }

        /// <inheritdoc/>
        public object Map(object source, Type sourceType, Type destinationType)
        {
            if (source == null) return null!;
            var method = typeof(MapperBase).GetMethod(nameof(Map), new[] { typeof(object) })?.MakeGenericMethod(destinationType);
            return method?.Invoke(this, new[] { source })!;
        }

        /// <summary>Mapeia objeto com cache.</summary>
        protected abstract T MapObject<T>(object source, Dictionary<object, object> mappedObjects);

        /// <summary>Mapeia objeto assincronamente.</summary>
        protected abstract Task<T> MapObjectAsync<T>(object source, ConcurrentDictionary<object, object> mappedObjects, CancellationToken cancellationToken);

        /// <summary>Mapeia objeto com destino existente.</summary>
        protected abstract TDestination MapObjectWithDestination<TSource, TDestination>(TSource source, TDestination destination, Dictionary<object, object> mappedObjects);

        /// <summary>
        /// Converte valor para tipo destino.
        /// VERSÃO CORRIGIDA - Suporte completo para double/decimal e decimal/double.
        /// </summary>
        protected object? ConvertValue(object? value, Type targetType)
        {
            try
            {
                if (value == null)
                {
                    if (Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType) return null;
                    return Activator.CreateInstance(targetType);
                }

                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                var valueType = value.GetType();
                var underlyingValueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

                // Se os tipos são iguais, retorna o valor
                if (underlyingType == underlyingValueType) return value;
                
                // Tenta conversão via Tabela de Conversão Numérica (Centralizada)
                var numericConverted = NumericConversionTable.Convert(value!, underlyingType);
                if (numericConverted != null) return numericConverted;

                // Conversões especiais
                if (underlyingType == typeof(string)) return value.ToString()!;

                // Conversão de enum
                if (underlyingType.IsEnum) return ConvertToEnum(value, underlyingType);
                if (underlyingValueType.IsEnum && underlyingType == typeof(string)) return value.ToString()!;

                // Tenta conversão genérica para outros tipos
                try
                {
                    return Convert.ChangeType(value, underlyingType);
                }
                catch (InvalidCastException)
                {
                    // Se falhar, tenta converter via string
                    var stringValue = value.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        return Convert.ChangeType(stringValue, underlyingType);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);
                _diagnostics.RecordError(ex, $"ConvertValue {value?.GetType().Name}->{targetType.Name}");

                if (_config.ThrowOnConversionError)
                    throw new MappingException($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);

                return null;
            }
        }

        private object ConvertToEnum(object value, Type enumType)
        {
            if (value.GetType().IsEnum) return Enum.ToObject(enumType, (int)value);
            if (value is string stringValue) return Enum.Parse(enumType, stringValue, true);
            
            if (value is int || value is short || value is byte || value is long || 
                value is uint || value is ushort || value is sbyte || value is ulong)
                return Enum.ToObject(enumType, value);
            if (value is decimal || value is double || value is float)
                return Enum.ToObject(enumType, Convert.ToInt32(value));

            throw new InvalidOperationException($"Não é possível converter {value.GetType().Name} para {enumType.Name}");
        }

        /// <summary>Obtém propriedades com cache.</summary>
        protected PropertyInfo[] GetProperties(Type type)
        {
            return _typeMetadataCache.GetOrAdd(type, t => PropertyAccessorCache.GetMetadata(t))
                .PublicReadableProperties
                .Select(p => p.PropertyInfo)
                .ToArray();
        }

        /// <summary>Verifica se é tipo simples (primitivo, string, etc).</summary>
        protected bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.IsPrimitive ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(Guid) ||
                   underlyingType == typeof(TimeSpan) ||
                   underlyingType == typeof(DateTimeOffset);
        }

        /// <summary>
        /// Tenta encontrar um membro na origem seguindo a convenção de achatamento (Flattening).
        /// Ex: Destination.CustomerName -> Source.Customer.Name
        /// </summary>
        internal Expression? GetFlattenedSourceMember(Expression sourceExpr, string targetPropertyName)
        {
            var sourceType = sourceExpr.Type;
            var properties = GetProperties(sourceType);

            foreach (var prop in properties)
            {
                if (targetPropertyName.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var remaining = targetPropertyName.Substring(prop.Name.Length);
                    if (string.IsNullOrEmpty(remaining)) continue;

                    var propertyAccess = Expression.Property(sourceExpr, prop);
                    var result = GetFlattenedSourceMemberRecursive(propertyAccess, remaining);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private Expression? GetFlattenedSourceMemberRecursive(Expression parentExpr, string remainingName)
        {
            var type = parentExpr.Type;
            if (IsSimpleType(type)) return null;

            var properties = GetProperties(type);
            var exactMatch = properties.FirstOrDefault(p => string.Equals(p.Name, remainingName, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                var access = Expression.Property(parentExpr, exactMatch);
                if (type.IsValueType) return access;

                // Null-safe access: parent != null ? parent.Member : default
                return Expression.Condition(
                    Expression.Equal(parentExpr, Expression.Constant(null, type)),
                    Expression.Default(exactMatch.PropertyType),
                    access);
            }

            foreach (var prop in properties)
            {
                if (remainingName.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var subRemaining = remainingName.Substring(prop.Name.Length);
                    var access = Expression.Property(parentExpr, prop);
                    var result = GetFlattenedSourceMemberRecursive(access, subRemaining);

                    if (result != null)
                    {
                        if (type.IsValueType) return result;
                        return Expression.Condition(
                            Expression.Equal(parentExpr, Expression.Constant(null, type)),
                            Expression.Default(result.Type),
                            result);
                    }
                }
            }

            return null;
        }

        /// <summary>Mapeia para IEnumerable.</summary>
        /// <typeparam name="T">Tipo dos elementos da coleção destino.</typeparam>
        /// <param name="source">Coleção de origem.</param>
        /// <returns>IEnumerable com os elementos mapeados.</returns>
        protected IEnumerable<T> MapIEnumerable<T>(object? source)
        {
            if (source == null) return Enumerable.Empty<T>();
            if (source is not IEnumerable enumerable)
                throw new MappingException($"Origem deve ser uma coleção para mapear para {typeof(IEnumerable<T>).Name}.");

            return enumerable.Cast<object>().Select(item => Map<T>(item)).Where(result => result != null).ToList()!;
        }

        /// <summary>Mapeia para List.</summary>
        /// <typeparam name="T">Tipo dos elementos da lista destino.</typeparam>
        /// <param name="source">Coleção de origem.</param>
        /// <returns>List com os elementos mapeados.</returns>
        protected List<T> MapList<T>(object? source)
        {
            if (source == null) return new List<T>();
            if (source is not IEnumerable enumerable)
                throw new MappingException($"Origem deve ser uma coleção para mapear para {typeof(List<T>).Name}.");

            return enumerable.Cast<object>().Select(item => Map<T>(item)).Where(result => result != null).ToList()!;
        }

        /// <summary>Mapeia para ICollection.</summary>
        protected ICollection<T> MapICollection<T>(object? source) => MapIEnumerable<T>(source).ToList();

        /// <summary>Mapeia para IReadOnlyList.</summary>
        protected IReadOnlyList<T> MapIReadOnlyList<T>(object? source) => MapIEnumerable<T>(source).ToList();

        /// <summary>Mapeia para IReadOnlyCollection.</summary>
        protected IReadOnlyCollection<T> MapIReadOnlyCollection<T>(object? source) => MapIEnumerable<T>(source).ToList();

        /// <summary>Mapeia para array.</summary>
        protected T[] MapArray<T>(object? source) => MapIEnumerable<T>(source).ToArray();

        /// <summary>Mapeia para HashSet.</summary>
        protected HashSet<T> MapHashSet<T>(object? source) => new HashSet<T>(MapIEnumerable<T>(source));

        /// <summary>Mapeia para Dictionary.</summary>
        protected Dictionary<TKey, TValue> MapDictionary<TKey, TValue>(object? source) where TKey : notnull
        {
            if (source == null) return new Dictionary<TKey, TValue>();
            if (source is not IDictionary dictionary)
                throw new MappingException($"Origem deve ser um dicionário para mapear para {typeof(Dictionary<TKey, TValue>).Name}.");

            var result = new Dictionary<TKey, TValue>();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is TKey key && entry.Value != null)
                {
                    var value = Map<TValue>(entry.Value);
                    if (value != null) result.Add(key, value);
                }
            }
            return result;
        }

        /// <summary>Mapeia para ImmutableList.</summary>
        protected ImmutableList<T> MapImmutableList<T>(object? source) => MapIEnumerable<T>(source).ToImmutableList();

        /// <summary>Mapeia para ImmutableDictionary.</summary>
        protected ImmutableDictionary<TKey, TValue> MapImmutableDictionary<TKey, TValue>(object? source) where TKey : notnull
            => MapDictionary<TKey, TValue>(source).ToImmutableDictionary();

        /// <summary>Mapeia para ImmutableArray.</summary>
        protected ImmutableArray<T> MapImmutableArray<T>(object? source) => MapIEnumerable<T>(source).ToImmutableArray();

        /// <summary>Mapeia para ImmutableQueue.</summary>
        protected ImmutableQueue<T> MapImmutableQueue<T>(object? source) => ImmutableQueue.CreateRange(MapIEnumerable<T>(source));

        /// <summary>Mapeia para ImmutableStack.</summary>
        protected ImmutableStack<T> MapImmutableStack<T>(object? source) => ImmutableStack.CreateRange(MapIEnumerable<T>(source));

        /// <inheritdoc/>
        public void AssertConfigurationIsValid() => _config.AssertConfigurationIsValidInternal();

        /// <inheritdoc/>
        public DiagnosticInfo GetDiagnostics()
        {
            var memoryUsed = GC.GetTotalMemory(false);
            return _diagnostics.GetInfo(memoryUsed);
        }
    }
}
