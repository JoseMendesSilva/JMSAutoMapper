using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace JMSAutoMapper
{
    public class SourceAutoMapper
    {

    }
}
// dotnet pack --configuration Release --output D:\NuGetLocal\DeployParaTeste -p:JMSAutoMapper=1.0.12 -p:Authors="José Mendes" -p:Description="Biblioteca avançada de mapeamento de objetos para .NET"

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


        IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;

        


    }

    public interface IMappingExpression<TSource, TDestination>
    {
        IMappingExpression<TSource, TDestination> ForMember<TMember>(
            string destinationProperty,
            System.Linq.Expressions.Expression<Func<TSource, TMember>> mappingExpression,
            Func<TSource, bool>? condition = null);

        IMappingExpression<TSource, TDestination> ForMember(
            string destinationProperty,
            string sourceProperty,
            Func<TSource, bool>? condition = null);

        IMappingExpression<TDestination, TSource> ReverseMap();
        IMappingExpression<TSource, TDestination> Ignore<TMember>(System.Linq.Expressions.Expression<Func<TDestination, TMember>> destinationMember);
        IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes);
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
        internal Dictionary<(Type Source, Type Target), Dictionary<string, System.Linq.Expressions.LambdaExpression>> CustomMappingExpressions { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, string>> PropertyMappings { get; } = new();
        internal Dictionary<(Type Source, Type Target), Dictionary<string, Func<object, bool>>> ConditionalMappings { get; } = new();
        internal HashSet<(Type Source, Type Target, string PropertyName)> IgnoredProperties { get; } = new();
        internal Dictionary<(Type Source, Type Target), Type[]> ConstructorSelection { get; } = new();
        public Func<string, string> NamingConvention { get; set; } = name => name; // Default to no change

        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception when a conversion error occurs.
        /// </summary>
        public bool ThrowOnConversionError { get; set; } = true;

        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            var profile = new TProfile();
            // Merge configurations from the profile
            foreach (var customMapping in profile.Configuration.CustomMappings)
            {
                CustomMappings[customMapping.Key] = customMapping.Value;
            }
            foreach (var customMappingExpression in profile.Configuration.CustomMappingExpressions)
            {
                CustomMappingExpressions[customMappingExpression.Key] = customMappingExpression.Value;
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
            System.Linq.Expressions.Expression<Func<TSource, TMember>> mappingExpression,
            Func<TSource, bool>? condition = null)
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!_config.CustomMappingExpressions.TryGetValue(key, out var expressions))
            {
                expressions = new Dictionary<string, System.Linq.Expressions.LambdaExpression>();
                _config.CustomMappingExpressions[key] = expressions;
            }
            expressions[destinationProperty] = mappingExpression;

            if (!_config.CustomMappings.TryGetValue(key, out var mappings))
            {
                mappings = new Dictionary<string, Func<object, object>>();
                _config.CustomMappings[key] = mappings;
            }
            var compiled = mappingExpression.Compile();
            mappings[destinationProperty] = src => compiled((TSource)src)!;

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

            if (_config.CustomMappingExpressions.TryGetValue(key, out var customMappings))
            {
                foreach (var mapping in customMappings)
                {
                    if (mapping.Value.Body is System.Linq.Expressions.MemberExpression memberExpression)
                    {
                        var sourceMember = memberExpression.Member.Name;
                        reverseMapping.ForMember(sourceMember, mapping.Key);
                    }
                }
            }

            return reverseMapping;
        }

        public IMappingExpression<TSource, TDestination> Ignore<TMember>(System.Linq.Expressions.Expression<Func<TDestination, TMember>> destinationMember)
        {
            if (destinationMember.Body is not System.Linq.Expressions.MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a member expression.", nameof(destinationMember));
            }

            var propertyName = memberExpression.Member.Name;
            _config.IgnoredProperties.Add((typeof(TSource), typeof(TDestination), propertyName));

            return this;
        }

        public IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes)
        {
            _config.ConstructorSelection[(typeof(TSource), typeof(TDestination))] = parameterTypes;
            return this;
        }
    }

        public abstract class MapperBase : IMapper

        {

            public abstract IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)

                where TSource : class

                where TDestination : class;

    

            private readonly Dictionary<Type, System.Reflection.PropertyInfo[]> _propertyCache = new();

            protected readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _compiledMappers = new();

            protected readonly ConcurrentDictionary<Type, System.Reflection.MethodInfo> _mapObjectMethodCache = new();

            protected readonly MapperConfiguration _config;

            protected readonly Action<string, Exception>? _logger;

    

            protected MapperBase(MapperConfiguration config, Action<string, Exception>? logger = null)

            {

                _config = config;

                _logger = logger;

            }

    

            public T Map<T>(object? source)

            {

                if (source == null)

                {

                    if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)

                    {

                        throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{typeof(T).Name}'.");

                    }

                    return default!;

                }

    

                var targetType = typeof(T);

                var sourceType = source.GetType();

    

                // Handle direct mapping of same types, primitive types, or when source is assignable to target

                if (targetType.IsAssignableFrom(sourceType) || targetType.IsPrimitive || targetType == typeof(string) || targetType == typeof(decimal) || targetType == typeof(DateTime) || targetType == typeof(Guid) || targetType == typeof(TimeSpan) || targetType == typeof(DateTimeOffset) || Nullable.GetUnderlyingType(targetType) == sourceType)

                {

                    return (T)ConvertValue(source, targetType)!;

                }

    

                var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

                return MapObject<T>(source, mappedObjects); // Blocking call for sync Map

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

    

                    if (value.GetType().IsEnum && underlyingType == typeof(string))

                    {

                        return value.ToString();

                    }

    

                    return Convert.ChangeType(value, underlyingType);

                }

                catch (Exception ex)

                {

                    _logger?.Invoke($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);

                    if (_config.ThrowOnConversionError)

                        throw new MappingException($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);

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

    

            protected System.Reflection.PropertyInfo[] GetProperties(Type type)

            {

                if (!_propertyCache.TryGetValue(type, out var properties))

                {

                    properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    _propertyCache[type] = properties;

                }

                return properties;

            }

    

                        public IEnumerable<TDestination> MapIEnumerable<TDestination>(object? source)

    

                        {

    

                            if (source == null) return Enumerable.Empty<TDestination>();

    

                            if (source is not IEnumerable enumerable)

    

                                throw new MappingException($"Source must be a collection to map to {typeof(IEnumerable<TDestination>).Name}.");

    

            

    

                            return enumerable.Cast<object>()

    

                                .Select(item => Map<TDestination>(item))

    

                                .Where(result => result != null)

    

                                .ToList();

    

                        }

    

            

    

                        public List<T> MapList<T>(object? source)

    

                        {

    

                            if (source == null) return new List<T>();

    

                            if (source is not IEnumerable enumerable)

    

                                throw new MappingException($"Source must be a collection to map to {typeof(List<T>).Name}.");

    

            

    

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
                    throw new MappingException($"Source must be a dictionary to map to {typeof(Dictionary<TKey, TValue>).Name}.");

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

        }

    public class JMSMapper : MapperBase
    {
        public JMSMapper(MapperConfiguration config, Action<string, Exception>? logger = null) : base(config, logger)
        {
        }

        public override IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
            where TSource : class
            where TDestination : class
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);

            var config = _config;

            var parameter = Expression.Parameter(sourceType, "source");

            var visitor = new ProjectionExpressionVisitor(config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.Visit();

            var lambda = Expression.Lambda<Func<TSource, TDestination>>(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)
                )
            );
        }

        protected override T MapObject<T>(object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                {
                    throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{typeof(T).Name}'.");
                }
                return default!;
            }

            if (mappedObjects.TryGetValue(source, out var existing))
                return (T)existing;

            var sourceType = source.GetType();
            var targetType = typeof(T);

            var mapper = _compiledMappers.GetOrAdd((sourceType, targetType), key => BuildMapperDelegate(key.Source, key.Target));

            var result = ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
            return (T)result;
        }

        private object? MapComplexType(Type targetType, object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return null;

            if (mappedObjects.TryGetValue(source, out var existing))
                return existing;

            var sourceType = source.GetType();

            if (IsCollection(sourceType) && IsCollection(targetType))
            {
                return MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);
            }

            var mapper = _compiledMappers.GetOrAdd((sourceType, targetType), key => BuildMapperDelegate(key.Source, key.Target));

            return ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
        }

        private Delegate BuildMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = System.Linq.Expressions.Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");

            var sourceVar = System.Linq.Expressions.Expression.Variable(sourceType, "source");
            var resultVar = System.Linq.Expressions.Expression.Variable(targetType, "result");

            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var expressions = new List<System.Linq.Expressions.Expression>
            {
                System.Linq.Expressions.Expression.Assign(sourceVar, System.Linq.Expressions.Expression.Convert(sourceParam, sourceType))
            };

            // Constructor selection logic
            System.Reflection.ConstructorInfo? bestConstructor = null;
            if (_config.ConstructorSelection.TryGetValue((sourceType, targetType), out var parameterTypes))
            {
                bestConstructor = targetType.GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, parameterTypes, null);
            }
            else
            {
                var constructors = targetType.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
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
            }

            if (targetType.IsValueType)
            {
                expressions.Add(System.Linq.Expressions.Expression.Assign(resultVar, System.Linq.Expressions.Expression.New(targetType)));
            }
            if (bestConstructor != null)
            {
                var constructorParameters = bestConstructor.GetParameters();
                var arguments = new System.Linq.Expressions.Expression[constructorParameters.Length];
                var sourcePropertiesUsed = new bool[sourceProperties.Length];

                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    var param = constructorParameters[i];
                    System.Reflection.PropertyInfo? sourceProperty = sourceProperties.FirstOrDefault(sp => string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                    if (sourceProperty == null && _config.ConstructorSelection.ContainsKey((sourceType, targetType)))
                    {
                        for (int j = 0; j < sourceProperties.Length; j++)
                        {
                            if (!sourcePropertiesUsed[j] && param.ParameterType.IsAssignableFrom(sourceProperties[j].PropertyType))
                            {
                                sourceProperty = sourceProperties[j];
                                sourcePropertiesUsed[j] = true;
                                break;
                            }
                        }
                    }

                    if (sourceProperty != null)
                    {
                        var sourcePropertyAccess = System.Linq.Expressions.Expression.Property(sourceVar, sourceProperty);
                        arguments[i] = System.Linq.Expressions.Expression.Convert(sourcePropertyAccess, param.ParameterType);
                    }
                    else
                    {
                        arguments[i] = System.Linq.Expressions.Expression.Default(param.ParameterType);
                    }
                }
                expressions.Add(System.Linq.Expressions.Expression.Assign(resultVar, System.Linq.Expressions.Expression.New(bestConstructor, arguments)));
            }
            else
            {
                expressions.Add(System.Linq.Expressions.Expression.Assign(resultVar,
                    System.Linq.Expressions.Expression.Convert(
                        System.Linq.Expressions.Expression.Call(typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(Type) })!, System.Linq.Expressions.Expression.Constant(targetType)),
                        targetType)));
            }

            expressions.Add(System.Linq.Expressions.Expression.Assign(
                System.Linq.Expressions.Expression.Property(mappedObjectsParam, "Item", sourceParam),
                System.Linq.Expressions.Expression.Convert(resultVar, typeof(object))
            ));

            var config = _config;
            var propertyMappings = config.PropertyMappings;
            var customMappings = config.CustomMappings;
            var conditionalMappings = config.ConditionalMappings;
            var ignoredProperties = config.IgnoredProperties;

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var mapObjectMethod = typeof(JMSMapper).GetMethod("MapComplexType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, new[] { typeof(Type), typeof(object), typeof(Dictionary<object, object>) })!;
            var thisInstance = System.Linq.Expressions.Expression.Constant(this);
            var baseInstance = System.Linq.Expressions.Expression.Convert(thisInstance, typeof(MapperBase));

            foreach (var targetProperty in targetProperties)
            {
                if (!targetProperty.CanWrite) continue;

                if (ignoredProperties.Contains((sourceType, targetType, targetProperty.Name)))
                {
                    continue;
                }

                System.Linq.Expressions.Expression? propertyMappingExpression = null;

                var key = (sourceType, targetType);
                if (customMappings.TryGetValue(key, out var mappings) &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = System.Linq.Expressions.Expression.Invoke(System.Linq.Expressions.Expression.Constant(mappingFunc), System.Linq.Expressions.Expression.Convert(sourceVar, typeof(object)));
                    var convertedValueExpression = System.Linq.Expressions.Expression.Call(
                        baseInstance,
                        convertValueMethod,
                        valueExpression,
                        System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType)
                    );
                    propertyMappingExpression = System.Linq.Expressions.Expression.Assign(
                        System.Linq.Expressions.Expression.Property(resultVar, targetProperty),
                        System.Linq.Expressions.Expression.Convert(convertedValueExpression, targetProperty.PropertyType)
                    );
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, propertyMappings, config);
                    var sourceProperty = sourceProperties.FirstOrDefault(p =>
                        string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    if (sourceProperty == null || !sourceProperty.CanRead) continue;

                    var sourcePropertyAccess = System.Linq.Expressions.Expression.Property(sourceVar, sourceProperty);
                    System.Linq.Expressions.Expression assignment;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = System.Linq.Expressions.Expression.Call(
                            thisInstance,
                            mapCollectionHelperMethod,
                            System.Linq.Expressions.Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)),
                            System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType),
                            mappedObjectsParam
                        );
                        assignment = System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Property(resultVar, targetProperty), System.Linq.Expressions.Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = System.Linq.Expressions.Expression.Call(
                            thisInstance,
                            mapObjectMethod,
                            System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType),
                            System.Linq.Expressions.Expression.Convert(sourcePropertyAccess, typeof(object)),
                            mappedObjectsParam
                        );
                        assignment = System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Property(resultVar, targetProperty), System.Linq.Expressions.Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = System.Linq.Expressions.Expression.Call(
                            baseInstance,
                            convertValueMethod,
                            System.Linq.Expressions.Expression.Convert(sourcePropertyAccess, typeof(object)),
                            System.Linq.Expressions.Expression.Constant(targetProperty.PropertyType)
                        );
                        assignment = System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Property(resultVar, targetProperty), System.Linq.Expressions.Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    if (!sourceProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(sourceProperty.PropertyType) != null)
                    {
                        propertyMappingExpression = System.Linq.Expressions.Expression.IfThen(
                            System.Linq.Expressions.Expression.NotEqual(sourcePropertyAccess, System.Linq.Expressions.Expression.Constant(null, sourceProperty.PropertyType)),
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
                    if (conditionalMappings.TryGetValue(key, out var conditions) &&
                        conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionExpression = System.Linq.Expressions.Expression.Invoke(System.Linq.Expressions.Expression.Constant(conditionFunc), System.Linq.Expressions.Expression.Convert(sourceVar, typeof(object)));
                        expressions.Add(System.Linq.Expressions.Expression.IfThen(conditionExpression, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            expressions.Add(System.Linq.Expressions.Expression.Convert(resultVar, typeof(object)));

            var body = System.Linq.Expressions.Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<object, Dictionary<object, object>, object>>(body, sourceParam, mappedObjectsParam);
            return lambda.Compile();
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
                var mappedItem = MapComplexType(itemType, item, mappedObjects);
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

        private string GetMappedPropertyName(Type sourceType, Type targetType, string targetPropertyName,
            Dictionary<(Type, Type), Dictionary<string, string>> propertyMappings, MapperConfiguration config)
        {
            var key = (sourceType, targetType);
            if (propertyMappings.TryGetValue(key, out var mappings) &&
                mappings.TryGetValue(targetPropertyName, out var sourcePropertyName))
            {
                return sourcePropertyName;
            }
            return config.NamingConvention(targetPropertyName);
        }
        private class ProjectionExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly MapperConfiguration _config;
            private readonly Type _sourceType;
            private readonly Type _targetType;
            private readonly System.Linq.Expressions.Expression _sourceExpression;
            private readonly JMSMapper _mapper;
            private readonly HashSet<(Type, Type)> _visited;

            public ProjectionExpressionVisitor(MapperConfiguration config, Type sourceType, Type targetType, System.Linq.Expressions.Expression sourceExpression, JMSMapper mapper, HashSet<(Type, Type)> visited)
            {
                _config = config;
                _sourceType = sourceType;
                _targetType = targetType;
                _sourceExpression = sourceExpression;
                _mapper = mapper;
                _visited = visited;
            }

            public System.Linq.Expressions.Expression Visit()
            {
                if (_visited.Contains((_sourceType, _targetType)))
                {
                    return System.Linq.Expressions.Expression.Default(_targetType);
                }
                _visited.Add((_sourceType, _targetType));

                var bindings = new List<System.Linq.Expressions.MemberBinding>();
                var targetProperties = _targetType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(p => p.CanWrite);

                foreach (var targetProperty in targetProperties)
                {
                    if (_config.IgnoredProperties.Contains((_sourceType, _targetType, targetProperty.Name)))
                        continue;

                    if (_config.CustomMappingExpressions.TryGetValue((_sourceType, _targetType), out var customMaps) && customMaps.TryGetValue(targetProperty.Name, out var lambda))
                    {
                        var body = new ParameterReplacer(lambda.Parameters[0], (System.Linq.Expressions.ParameterExpression)_sourceExpression).Visit(lambda.Body);
                        bindings.Add(System.Linq.Expressions.Expression.Bind(targetProperty, body));
                        continue;
                    }

                    var sourcePropertyName = _mapper.GetMappedPropertyName(_sourceType, _targetType, targetProperty.Name, _config.PropertyMappings, _config);
                    var sourceProperty = _sourceType.GetProperty(sourcePropertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

                    if (sourceProperty != null)
                    {
                        var sourcePropertyAccess = System.Linq.Expressions.Expression.Property(_sourceExpression, sourceProperty);

                        if (_mapper.IsComplexType(targetProperty.PropertyType) && sourceProperty.PropertyType != targetProperty.PropertyType)
                        {
                            var nestedVisitor = new ProjectionExpressionVisitor(_config, sourceProperty.PropertyType, targetProperty.PropertyType, sourcePropertyAccess, _mapper, _visited);
                            var nestedInit = nestedVisitor.Visit();

                            var nullCheck = System.Linq.Expressions.Expression.Equal(sourcePropertyAccess, System.Linq.Expressions.Expression.Constant(null, sourceProperty.PropertyType));
                            var conditional = System.Linq.Expressions.Expression.Condition(nullCheck, System.Linq.Expressions.Expression.Default(targetProperty.PropertyType), nestedInit);

                            bindings.Add(System.Linq.Expressions.Expression.Bind(targetProperty, conditional));
                        }
                        else if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            bindings.Add(System.Linq.Expressions.Expression.Bind(targetProperty, sourcePropertyAccess));
                        }
                        else
                        {
                            try
                            {
                                var converted = System.Linq.Expressions.Expression.Convert(sourcePropertyAccess, targetProperty.PropertyType);
                                bindings.Add(System.Linq.Expressions.Expression.Bind(targetProperty, converted));
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }
                }

                return System.Linq.Expressions.Expression.MemberInit(System.Linq.Expressions.Expression.New(_targetType), bindings);
            }
        }

        private class ParameterReplacer : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly System.Linq.Expressions.ParameterExpression _oldParameter;
            private readonly System.Linq.Expressions.ParameterExpression _newParameter;

            public ParameterReplacer(System.Linq.Expressions.ParameterExpression oldParameter, System.Linq.Expressions.ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override System.Linq.Expressions.Expression VisitParameter(System.Linq.Expressions.ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }

    /// <summary>
    /// Represents errors that occur during the mapping process.
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        public MappingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MappingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public MappingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    #endregion
}

