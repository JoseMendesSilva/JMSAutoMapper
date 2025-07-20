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
        T Map<T>(object? source);

        // Coleções padrão
        IEnumerable<T> MapIEnumerable<T>(object? source);
        List<T> MapList<T>(object? source);
        ICollection<T> MapICollection<T>(object? source);
        IReadOnlyList<T> MapIReadOnlyList<T>(object? source);
        IReadOnlyCollection<T> MapIReadOnlyCollection<T>(object? source);
        T[] MapArray<T>(object? source);
        HashSet<T> MapHashSet<T>(object? source);

        // Dicionários
        Dictionary<TKey, TValue> MapDictionary<TKey, TValue>(object? source)
            where TKey : notnull;

        // Coleções imutáveis
        ImmutableList<T> MapImmutableList<T>(object? source);
        ImmutableDictionary<TKey, TValue> MapImmutableDictionary<TKey, TValue>(object? source)
            where TKey : notnull;
        ImmutableArray<T> MapImmutableArray<T>(object? source);
        ImmutableQueue<T> MapImmutableQueue<T>(object? source);
        ImmutableStack<T> MapImmutableStack<T>(object? source);

        // Métodos assíncronos
        Task<T> MapAsync<T>(object? source);
        Task<IEnumerable<T>> MapIEnumerableAsync<T>(object? source, int? maxDegreeOfParallelism = null);
        IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;

        


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
        IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember);
    }

    public static class MapperExtensions
    {
        public static IServiceCollection AddJMSMapper(this IServiceCollection services, Action<MapperConfiguration>? configure = null, Action<ProfileConfiguration>? configureProfiles = null)
        {
            var config = new MapperConfiguration();
            configure?.Invoke(config);

            var profileConfig = new ProfileConfiguration(config);
            configureProfiles?.Invoke(profileConfig);

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                return new JMSMapper(config, logger);
            });

            return services;
        }
    }

    public class ProfileConfiguration
    {
        private readonly MapperConfiguration _config;

        public ProfileConfiguration(MapperConfiguration config)
        {
            _config = config;
        }

        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            _config.AddProfile<TProfile>();
        }
    }

    public abstract class Profile
    {
        internal MapperConfiguration Configuration { get; private set; }

        public Profile()
        {
            Configuration = new MapperConfiguration();
            Configure();
        }

        public virtual void Configure() { }

        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Configuration.CreateMap<TSource, TDestination>();
        }
    }

    public class MapperConfiguration
    {
        internal Dictionary<(Type Source, Type Target), Dictionary<string, Func<object, object>>> CustomMappings { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, string>> PropertyMappings { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, Func<object, bool>>> ConditionalMappings { get; } = new();
        internal HashSet<(Type Source, Type Target, string PropertyName)> IgnoredProperties { get; } = new();

        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            var profile = new TProfile();
            // Merge configurations from the profile
            foreach (var customMapping in profile.Configuration.CustomMappings)
            {
                CustomMappings[customMapping.Key] = customMapping.Value;
            }
            foreach (var propertyMapping in profile.Configuration.PropertyMappings)
            {
                PropertyMappings[propertyMapping.Key] = propertyMapping.Value;
            }
            foreach (var conditionalMapping in profile.Configuration.ConditionalMappings)
            {
                ConditionalMappings[conditionalMapping.Key] = conditionalMapping.Value;
            }
            foreach (var ignoredProperty in profile.Configuration.IgnoredProperties)
            {
                IgnoredProperties.Add(ignoredProperty);
            }
        }

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

        public IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember)
        {
            if (destinationMember.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a member expression.", nameof(destinationMember));
            }

            var propertyName = memberExpression.Member.Name;
            _config.IgnoredProperties.Add((typeof(TSource), typeof(TDestination), propertyName));

            return this;
        }
    }

    public abstract class MapperBase : IMapper
    {
        public abstract IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
            where TSource : class
            where TDestination : class;

        private readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new();
        protected readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _compiledMappers = new();
        protected readonly ConcurrentDictionary<Type, MethodInfo> _mapObjectMethodCache = new();
        protected readonly Action<string, Exception>? _logger;

        protected MapperBase(Action<string, Exception>? logger = null)
        {
            _logger = logger;
        }

        public T Map<T>(object? source)
        {
            if (source == null) return default!;
            var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return MapObject<T>(source, mappedObjects).Result; // Blocking call for sync Map
        }

        protected abstract Task<T> MapObject<T>(object source, Dictionary<object, object> mappedObjects);

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
            catch (Exception ex)
            {
                _logger?.Invoke($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);
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

        public IEnumerable<TDestination> MapIEnumerable<TDestination>(object? source)
        {
            if (source == null) return Enumerable.Empty<TDestination>();
            if (source is not IEnumerable enumerable)
                throw new ArgumentException("Source must be a collection", nameof(source));

            return enumerable.Cast<object>()
                .Select(item => Map<TDestination>(item))
                .Where(result => result != null)
                .ToList();
        }

        public List<T> MapList<T>(object? source)
        {
            if (source == null) return new List<T>();
            if (source is not IEnumerable enumerable)
                throw new ArgumentException("Source must be a collection", nameof(source));

            return enumerable.Cast<object>()
                .Select(item => Map<T>(item))
                .Where(result => result != null)
                .ToList();
        }

        public ICollection<T> MapICollection<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToList();
        }

        public IReadOnlyList<T> MapIReadOnlyList<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToList();
        }

        public IReadOnlyCollection<T> MapIReadOnlyCollection<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToList();
        }

        public T[] MapArray<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToArray();
        }

        public HashSet<T> MapHashSet<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return new HashSet<T>(enumerable);
        }

        public Dictionary<TKey, TValue> MapDictionary<TKey, TValue>(object? source)
            where TKey : notnull
        {
            if (source == null) return new Dictionary<TKey, TValue>();
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

        public ImmutableList<T> MapImmutableList<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToImmutableList();
        }

        public ImmutableDictionary<TKey, TValue> MapImmutableDictionary<TKey, TValue>(object? source)
            where TKey : notnull
        {
            return MapDictionary<TKey, TValue>(source).ToImmutableDictionary();
        }

        public ImmutableArray<T> MapImmutableArray<T>(object? source)
        {
            return MapIEnumerable<T>(source).ToImmutableArray();
        }

        public ImmutableQueue<T> MapImmutableQueue<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return ImmutableQueue.CreateRange(enumerable);
        }

        public ImmutableStack<T> MapImmutableStack<T>(object? source)
        {
            var enumerable = MapIEnumerable<T>(source);
            return ImmutableStack.CreateRange(enumerable);
        }

        public async Task<T> MapAsync<T>(object? source)
        {
            if (source == null) return default!;
            var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return await MapObject<T>(source, mappedObjects);
        }

        public async Task<IEnumerable<T>> MapIEnumerableAsync<T>(object? source, int? maxDegreeOfParallelism = null)
        {
            if (source == null) return Enumerable.Empty<T>();
            if (source is not IEnumerable enumerable)
                throw new ArgumentException("Source must be a collection", nameof(source));

            var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

            var query = enumerable.Cast<object>();

            if (maxDegreeOfParallelism.HasValue && maxDegreeOfParallelism.Value > 0)
            {
                return await Task.WhenAll(query.AsParallel().WithDegreeOfParallelism(maxDegreeOfParallelism.Value)
                    .Select(item => MapObject<T>(item, mappedObjects)))
                    .ContinueWith(t => t.Result.Where(result => result != null).ToList() as IEnumerable<T>);
            }
            else
            {
                return await Task.WhenAll(query.Select(item => MapObject<T>(item, mappedObjects)))
                    .ContinueWith(t => t.Result.Where(result => result != null).ToList() as IEnumerable<T>);
            }
        }

        
        


    }

    public class JMSMapper : MapperBase
    {
        private static MapperConfiguration? _staticConfig;
        private readonly MapperConfiguration? _instanceConfig;

        public JMSMapper(MapperConfiguration? config = null, Action<string, Exception>? logger = null) : base(logger)
        {
            _instanceConfig = config;
        }

        public static void SetConfiguration(MapperConfiguration config)
        {
            _staticConfig = config;
        }

        private MapperConfiguration? GetActiveConfig() => _instanceConfig ?? _staticConfig;

        public override IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
            where TSource : class
            where TDestination : class
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);

            var config = GetActiveConfig();

            // Create a parameter expression for the source type
            var parameter = Expression.Parameter(sourceType, "source");

            // Build the projection expression
            var visitor = new ProjectionExpressionVisitor(config!, sourceType, targetType, parameter);
            var projectionBody = visitor.Visit(parameter);

            // Create the lambda expression: source => new TDestination { ... }
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(projectionBody, parameter);

            // Use the Expression.Call to create the Select method call on the IQueryable
            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)
                )
            );
        }

        protected override async Task<T> MapObject<T>(object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                {
                    throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{typeof(T).Name}'.");
                }
                return default!; // Para tipos de referência ou tipos de valor anuláveis, retorna null.
            }

            if (mappedObjects.TryGetValue(source, out var existing))
                return (T)existing;

            var sourceType = source.GetType();
            var targetType = typeof(T);

            var mapper = _compiledMappers.GetOrAdd((sourceType, targetType), key => CreateMapperDelegate(key.Source, key.Target));

            var result = await ((Func<object, Dictionary<object, object>, Task<object>>)mapper)(source, mappedObjects);
            return (T)result;
        }

        private async Task<object?> MapObject(Type targetType, object source, Dictionary<object, object> mappedObjects)
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

            return await ((Func<object, Dictionary<object, object>, Task<object>>)mapper)(source, mappedObjects);
        }

        private Delegate CreateMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");

            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");

            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var expressions = new List<Expression>
            {
                Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType))
            };

            // Try to find a suitable constructor
            ConstructorInfo? bestConstructor = null;
            var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length);

            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.All(p => sourceProperties.Any(sp => string.Equals(sp.Name, p.Name, StringComparison.OrdinalIgnoreCase) && p.ParameterType.IsAssignableFrom(sp.PropertyType))))
                {
                    bestConstructor = ctor;
                    break;
                }
            }

            if (targetType.IsValueType)
            {
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
            }
            else if (bestConstructor != null)
            {
                var constructorParameters = bestConstructor.GetParameters();
                var arguments = new Expression[constructorParameters.Length];

                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    var param = constructorParameters[i];
                    var sourceProperty = sourceProperties.First(sp => string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));
                    var sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    arguments[i] = Expression.Convert(sourcePropertyAccess, param.ParameterType);
                }
                expressions.Add(Expression.Assign(resultVar, Expression.New(bestConstructor, arguments)));
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
            var ignoredProperties = config?.IgnoredProperties;

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapObjectMethod = typeof(JMSMapper).GetMethod("MapObject", BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(Type), typeof(object), typeof(Dictionary<object, object>) })!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            foreach (var targetProperty in targetProperties)
            {
                if (!targetProperty.CanWrite) continue;

                // Check if the property should be ignored
                if (ignoredProperties?.Contains((sourceType, targetType, targetProperty.Name)) == true)
                {
                    continue;
                }

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
                        var mapCollectionHelperCall = Expression.Call(
                            thisInstance,
                            mapCollectionHelperMethod,
                            Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)),
                            Expression.Constant(targetProperty.PropertyType),
                            mappedObjectsParam
                        );
                        // Access the Result property of the Task<object>
                        var getCollectionResult = Expression.Property(mapCollectionHelperCall, "Result");
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(getCollectionResult, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType))
                    {
                        // Call the async MapObject and get its Result synchronously within the expression tree
                        var mapObjectCall = Expression.Call(
                            thisInstance,
                            mapObjectMethod,
                            Expression.Constant(targetProperty.PropertyType),
                            Expression.Convert(sourcePropertyAccess, typeof(object)),
                            mappedObjectsParam
                        );
                        // Access the Result property of the Task<object>
                        var getResult = Expression.Property(mapObjectCall, "Result");
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(getResult, targetProperty.PropertyType));
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

            // Wrap the final result in Task.FromResult
            expressions.Add(Expression.Call(typeof(Task).GetMethod(nameof(Task.FromResult), BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(typeof(object)), Expression.Convert(resultVar, typeof(object))));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, Dictionary<object, object>, Task<object>>>(body, sourceParam, mappedObjectsParam);
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

        private async Task<object?> MapCollectionHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, Dictionary<object, object> mappedObjects)
        {
            if (sourceEnumerable == null) return null;

            var itemType = GetCollectionItemType(targetCollectionType)!;
            var listType = typeof(List<>).MakeGenericType(itemType);
            var mappedList = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in sourceEnumerable)
            {
                if (item == null) continue;
                var mappedItem = await MapObject(itemType, item, mappedObjects);
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
    private class ProjectionExpressionVisitor : ExpressionVisitor
        {
            private readonly MapperConfiguration _config;
            private readonly Type _sourceType;
            private readonly Type _targetType;
            private readonly ParameterExpression _parameterExpression;

            public ProjectionExpressionVisitor(MapperConfiguration config, Type sourceType, Type targetType, ParameterExpression parameterExpression)
            {
                _config = config;
                _sourceType = sourceType;
                _targetType = targetType;
                _parameterExpression = parameterExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the parameter with the root source parameter
                return node == _parameterExpression ? node : base.VisitParameter(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                // This is where the core projection logic will go
                // For now, just pass through
                return base.VisitMember(node);
            }
        }
    }

    #endregion
}