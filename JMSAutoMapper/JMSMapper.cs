using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace JMSAutoMapper
{
    #region Mapper melhorado

    public interface IMapper
    {
        // Método base
        T? Map<T>(object? source);

        // Coleções padrão
        IEnumerable<T>? MapIEnumerable<T>(object? source);
        List<T>? MapList<T>(object? source);
        ICollection<T>? MapICollection<T>(object? source);
        IReadOnlyList<T>? MapIReadOnlyList<T>(object? source);
        IReadOnlyCollection<T>? MapIReadOnlyCollection<T>(object? source);
        T[]? MapArray<T>(object? source);
        HashSet<T>? MapHashSet<T>(object? source);

        // Dicionários
        Dictionary<TKey, TValue>? MapDictionary<TKey, TValue>(object? source)
            where TKey : notnull;

        // Coleções imutáveis
        ImmutableList<T>? MapImmutableList<T>(object? source);
        ImmutableDictionary<TKey, TValue>? MapImmutableDictionary<TKey, TValue>(object? source)
            where TKey : notnull;
        ImmutableArray<T>? MapImmutableArray<T>(object? source);
        ImmutableQueue<T>? MapImmutableQueue<T>(object? source);
        ImmutableStack<T>? MapImmutableStack<T>(object? source);

        // Métodos assíncronos
        Task<T?> MapAsync<T>(object? source);
        Task<IEnumerable<T>?> MapIEnumerableAsync<T>(object? source, int? maxDegreeOfParallelism = null);


    }

    public interface IMappingExpression<TSource, TDestination>
    {
        IMappingExpression<TSource, TDestination> ForMember<TMember>(
            string destinationProperty,
            Func<TSource, TMember> mappingFunction,
            Func<TSource, bool>? condition = null);

        IMappingExpression<TSource, TDestination> ForMember(
            string destinationProperty,
            string sourceProperty,
            Func<TSource, bool>? condition = null);

        IMappingExpression<TDestination, TSource> ReverseMap();
    }

    public static class MapperExtensions
    {
        public static IServiceCollection AddJMSMapper(this IServiceCollection services, Action<MapperConfiguration>? configure = null)
        {
            var config = new MapperConfiguration();
            configure?.Invoke(config);

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                return new JMSMapper(config, logger);
            });

            return services;
        }
    }

    public class MapperConfiguration
    {
        internal Dictionary<(Type Source, Type Target), Dictionary<string, Func<object, object>>> CustomMappings { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, string>> PropertyMappings { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, Func<object, bool>>> ConditionalMappings { get; } = new();

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return new MappingExpression<TSource, TDestination>(this);
        }
    }

    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
    {
        private readonly MapperConfiguration _config;

        public MappingExpression(MapperConfiguration config)
        {
            _config = config;
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(
            string destinationProperty,
            Func<TSource, TMember> mappingFunction,
            Func<TSource, bool>? condition = null)
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!_config.CustomMappings.TryGetValue(key, out var mappings))
            {
                mappings = new Dictionary<string, Func<object, object>>();
                _config.CustomMappings[key] = mappings;
            }

            mappings[destinationProperty] = src => mappingFunction((TSource)src)!;

            if (condition != null)
            {
                if (!_config.ConditionalMappings.TryGetValue(key, out var conditions))
                {
                    conditions = new Dictionary<string, Func<object, bool>>();
                    _config.ConditionalMappings[key] = conditions;
                }
                conditions[destinationProperty] = src => condition((TSource)src);
            }

            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember(
            string destinationProperty,
            string sourceProperty,
            Func<TSource, bool>? condition = null)
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!_config.PropertyMappings.TryGetValue(key, out var mappings))
            {
                mappings = new Dictionary<string, string>();
                _config.PropertyMappings[key] = mappings;
            }

            mappings[destinationProperty] = sourceProperty;

            if (condition != null)
            {
                if (!_config.ConditionalMappings.TryGetValue(key, out var conditions))
                {
                    conditions = new Dictionary<string, Func<object, bool>>();
                    _config.ConditionalMappings[key] = conditions;
                }
                conditions[destinationProperty] = src => condition((TSource)src);
            }

            return this;
        }

        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var reverseMapping = new MappingExpression<TDestination, TSource>(_config);

            var key = (typeof(TSource), typeof(TDestination));
            if (_config.PropertyMappings.TryGetValue(key, out var mappings))
            {
                foreach (var mapping in mappings)
                {
                    reverseMapping.ForMember(mapping.Value, mapping.Key);
                }
            }

            return reverseMapping;
        }
    }

    public abstract class MapperBase : IMapper
    {
        private readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new();
        protected readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _compiledMappers = new();
        protected readonly ConcurrentDictionary<Type, MethodInfo> _mapObjectMethodCache = new();

        public T? Map<T>(object? source)
        {
            if (source == null) return default;
            var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return MapObject<T>(source, mappedObjects);
        }

        protected abstract T MapObject<T>(object source, Dictionary<object, object> mappedObjects);

        protected object? ConvertValue(object? value, Type targetType)
        {
            if (value == null)
            {
                if (Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType)
                    return null;
                return Activator.CreateInstance(targetType);
            }

            try
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (underlyingType == typeof(string))
                {
                    return value.ToString();
                }

                if (underlyingType.IsInstanceOfType(value))
                {
                    return value;
                }

                if (underlyingType.IsEnum)
                {
                    return ConvertToEnum(value, underlyingType);
                }

                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return null;
            }
        }

        private object ConvertToEnum(object value, Type enumType)
        {
            if (value.GetType().IsEnum)
            {
                return Enum.ToObject(enumType, (int)value);
            }

            if (value is string stringValue)
            {
                return Enum.Parse(enumType, stringValue, true);
            }

            if (value is int || value is short || value is byte ||
                value is long || value is uint || value is ushort ||
                value is sbyte || value is ulong)
            {
                return Enum.ToObject(enumType, value);
            }

            if (value is decimal || value is double || value is float)
            {
                return Enum.ToObject(enumType, Convert.ToInt32(value));
            }

            throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to {enumType.Name}");
        }

        protected PropertyInfo[] GetProperties(Type type)
        {
            if (!_propertyCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _propertyCache[type] = properties;
            }
            return properties;
        }

        public IEnumerable<TDestination>? MapIEnumerable<TDestination>(object? source)
        {
            if (source == null) return null;
            if (source is not IEnumerable enumerable)
                throw new ArgumentException("Source must be a collection", nameof(source));

            return enumerable.Cast<object>()
                .Select(item => Map<TDestination>(item))
                .Where(result => result != null)
                .ToList()!;
        }

        public List<T>? MapList<T>(object? source)
        {
            if (source == null) return null;
            if (source is not IEnumerable enumerable)
                throw new ArgumentException("Source must be a collection", nameof(source));

            return enumerable.Cast<object>()
                .Select(item => Map<T>(item))
                .Where(result => result != null)
                .ToList()!;
        }

        public ICollection<T>? MapICollection<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToList();
        }

        public IReadOnlyList<T>? MapIReadOnlyList<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToList();
        }

        public IReadOnlyCollection<T>? MapIReadOnlyCollection<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToList();
        }

        public T[]? MapArray<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToArray();
        }

        public HashSet<T>? MapHashSet<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return enumerable != null ? new HashSet<T>(enumerable) : null;
        }

        public Dictionary<TKey, TValue>? MapDictionary<TKey, TValue>(object? source)
            where TKey : notnull
        {
            if (source == null) return null;
            if (source is not IDictionary dictionary)
                throw new ArgumentException("Source must be a dictionary", nameof(source));

            var result = new Dictionary<TKey, TValue>();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is TKey key && entry.Value != null)
                {
                    var value = Map<TValue>(entry.Value);
                    if (value != null)
                        result.Add(key, value);
                }
            }
            return result;
        }

        public ImmutableList<T>? MapImmutableList<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToImmutableList();
        }

        public ImmutableDictionary<TKey, TValue>? MapImmutableDictionary<TKey, TValue>(object? source)
            where TKey : notnull
        {
            return MapDictionary<TKey, TValue>(source)?.ToImmutableDictionary();
        }

        public ImmutableArray<T>? MapImmutableArray<T>(object? source)
        {
            return MapIEnumerable<T>(source)?.ToImmutableArray();
        }

        public ImmutableQueue<T>? MapImmutableQueue<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return enumerable != null ? ImmutableQueue.CreateRange(enumerable) : null;
        }

        public ImmutableStack<T>? MapImmutableStack<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return enumerable != null ? ImmutableStack.CreateRange(enumerable) : null;
        }

        public Task<T?> MapAsync<T>(object? source)
        {
            if (source == null) return Task.FromResult<T?>(default);
            return Task.Run(() => Map<T>(source));
        }

        public async Task<IEnumerable<T>?> MapIEnumerableAsync<T>(object? source, int? maxDegreeOfParallelism = null)
        {
            if (source == null) return await Task.FromResult<IEnumerable<T>?>(null);
            if (source is not IEnumerable enumerable)
                return await Task.FromException<IEnumerable<T>?>(new ArgumentException("Source must be a collection", nameof(source)));

            return await Task.Run(() =>
              {
                  var query = enumerable.Cast<object>().AsParallel();
                  if (maxDegreeOfParallelism.HasValue)
                  {
                      query = query.WithDegreeOfParallelism(maxDegreeOfParallelism.Value);
                  }

                  return query
                      .Select(item => Map<T>(item))
                      .Where(result => result != null)
                      .ToList() as IEnumerable<T>;
              });
        }
    }

    public class JMSMapper : MapperBase
    {
        private static MapperConfiguration? _staticConfig;
        private readonly MapperConfiguration? _instanceConfig;
        private readonly Action<string, Exception>? _logger;

        public JMSMapper(MapperConfiguration? config = null, Action<string, Exception>? logger = null)
        {
            _instanceConfig = config;
            _logger = logger;
        }

        public static void SetConfiguration(MapperConfiguration config)
        {
            _staticConfig = config;
        }

        private MapperConfiguration? GetActiveConfig() => _instanceConfig ?? _staticConfig;

        protected override T MapObject<T>(object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return default;

            if (mappedObjects.TryGetValue(source, out var existing))
                return (T)existing;

            var sourceType = source.GetType();
            var targetType = typeof(T);

            var mapper = _compiledMappers.GetOrAdd((sourceType, targetType), key => CreateMapperDelegate(key.Source, key.Target));

            var result = ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
            return (T)result;
        }

        private object? MapObject(Type targetType, object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return null;

            if (mappedObjects.TryGetValue(source, out var existing))
                return existing;

            var sourceType = source.GetType();

            if (IsCollection(sourceType) && IsCollection(targetType))
            {
                return MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);
            }

            var mapper = _compiledMappers.GetOrAdd((sourceType, targetType), key => CreateMapperDelegate(key.Source, key.Target));

            return ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
        }

        private Delegate CreateMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");

            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");

            var expressions = new List<Expression>
            {
                Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType))
            };

            var constructor = targetType.GetConstructor(Type.EmptyTypes);
            if (targetType.IsValueType)
            {
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
            }
            else if (constructor != null)
            {
                expressions.Add(Expression.Assign(resultVar, Expression.New(constructor)));
            }
            else
            {
                var getUninitializedObjectMethod = typeof(FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Static | BindingFlags.Public)!;
                expressions.Add(Expression.Assign(resultVar,
                    Expression.Convert(
                        Expression.Call(getUninitializedObjectMethod, Expression.Constant(targetType)),
                        targetType)));
            }

            expressions.Add(Expression.Assign(
                Expression.Property(mappedObjectsParam, "Item", sourceParam),
                Expression.Convert(resultVar, typeof(object))
            ));

            var config = GetActiveConfig();
            var propertyMappings = config?.PropertyMappings;
            var customMappings = config?.CustomMappings;
            var conditionalMappings = config?.ConditionalMappings;

            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapObjectMethod = typeof(JMSMapper).GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(Type), typeof(object), typeof(Dictionary<object, object>) })!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            foreach (var targetProperty in targetProperties)
            {
                if (!targetProperty.CanWrite) continue;

                Expression? propertyMappingExpression = null;

                var key = (sourceType, targetType);
                if (customMappings?.TryGetValue(key, out var mappings) == true &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = Expression.Invoke(Expression.Constant(mappingFunc), Expression.Convert(sourceVar, typeof(object)));
                    var convertedValueExpression = Expression.Call(
                        baseInstance,
                        convertValueMethod,
                        valueExpression,
                        Expression.Constant(targetProperty.PropertyType)
                    );
                    propertyMappingExpression = Expression.Assign(
                        Expression.Property(resultVar, targetProperty),
                        Expression.Convert(convertedValueExpression, targetProperty.PropertyType)
                    );
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, propertyMappings);
                    var sourceProperty = sourceProperties.FirstOrDefault(p =>
                        string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    if (sourceProperty == null || !sourceProperty.CanRead) continue;

                    var sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    Expression assignment;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mappedCollection = Expression.Call(
                            thisInstance,
                            mapCollectionHelperMethod,
                            Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)),
                            Expression.Constant(targetProperty.PropertyType),
                            mappedObjectsParam
                        );
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mappedCollection, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType))
                    {
                        var mappedObject = Expression.Call(
                            thisInstance,
                            mapObjectMethod,
                            Expression.Constant(targetProperty.PropertyType),
                            Expression.Convert(sourcePropertyAccess, typeof(object)),
                            mappedObjectsParam
                        );
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mappedObject, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(
                            baseInstance,
                            convertValueMethod,
                            Expression.Convert(sourcePropertyAccess, typeof(object)),
                            Expression.Constant(targetProperty.PropertyType)
                        );
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    if (!sourceProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(sourceProperty.PropertyType) != null)
                    {
                        propertyMappingExpression = Expression.IfThen(
                            Expression.NotEqual(sourcePropertyAccess, Expression.Constant(null, sourceProperty.PropertyType)),
                            assignment
                        );
                    }
                    else
                    {
                        propertyMappingExpression = assignment;
                    }
                }

                if (propertyMappingExpression != null)
                {
                    if (conditionalMappings?.TryGetValue(key, out var conditions) == true &&
                        conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionExpression = Expression.Invoke(Expression.Constant(conditionFunc), Expression.Convert(sourceVar, typeof(object)));
                        expressions.Add(Expression.IfThen(conditionExpression, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            expressions.Add(Expression.Convert(resultVar, typeof(object)));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, Dictionary<object, object>, object>>(body, sourceParam, mappedObjectsParam);
            return lambda.Compile();
        }

        private string GetMappedPropertyName(Type sourceType, Type targetType, string targetPropertyName,
            Dictionary<(Type, Type), Dictionary<string, string>>? propertyMappings)
        {
            var key = (sourceType, targetType);
            if (propertyMappings?.TryGetValue(key, out var mappings) == true &&
                mappings.TryGetValue(targetPropertyName, out var sourcePropertyName))
            {
                return sourcePropertyName;
            }
            return targetPropertyName;
        }

        private object? MapCollectionHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, Dictionary<object, object> mappedObjects)
        {
            if (sourceEnumerable == null) return null;

            var itemType = GetCollectionItemType(targetCollectionType)!;
            var listType = typeof(List<>).MakeGenericType(itemType);
            var mappedList = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in sourceEnumerable)
            {
                if (item == null) continue;
                var mappedItem = MapObject(itemType, item, mappedObjects);
                if (mappedItem != null)
                {
                    mappedList.Add(mappedItem);
                }
            }

            if (targetCollectionType.IsArray)
            {
                var array = Array.CreateInstance(itemType, mappedList.Count);
                mappedList.CopyTo(array, 0);
                return array;
            }

            if (targetCollectionType.IsAssignableFrom(listType))
            {
                return mappedList;
            }

            var constructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType) });
            if (constructor != null)
            {
                return constructor.Invoke(new object[] { mappedList });
            }

            return mappedList;
        }

        private Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments().FirstOrDefault();

            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                var ienumerable = collectionType.GetInterface("IEnumerable`1");
                if (ienumerable != null)
                    return ienumerable.GetGenericArguments().FirstOrDefault();
            }

            return null;
        }

        private bool IsCollection(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        private bool IsComplexType(Type type)
        {
            var isSimpleValueType = type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset);

            if (type == typeof(string) || isSimpleValueType || IsCollection(type))
            {
                return false;
            }

            return true;
        }
    }

    #endregion
}