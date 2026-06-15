// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Expressão de mapeamento.
    /// Implementação fluente para configuração.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
    {
        private readonly MapperConfiguration _config;
        private int _maxDepth = 10;

        /// <summary>Construtor.</summary>
        public MappingExpression(MapperConfiguration config) => _config = config;

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> ForMember<TMember>(string destinationProperty, Expression<Func<TSource, TMember>> mappingExpression, Func<TSource, bool>? condition = null)
        {
            var key = (typeof(TSource), typeof(TDestination));

            var expressions = _config.CustomMappingExpressions.GetOrAdd(key, _ => new ConcurrentDictionary<string, LambdaExpression>());
            expressions[destinationProperty] = mappingExpression;

            var mappings = _config.CustomMappings.GetOrAdd(key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
            var compiled = mappingExpression.Compile(); // CS8604: TSource is expected to be non-null here by design
            mappings[destinationProperty] = (src, _) => compiled((TSource)src)!;

            if (condition != null)
            {
                var conditions = _config.ConditionalMappings.GetOrAdd(key, _ => new ConcurrentDictionary<string, Func<object, bool>>());
                conditions[destinationProperty] = src => condition((TSource)src);
            }

            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> ForMember(string destinationProperty, string sourceProperty, Func<TSource, bool>? condition = null)
        {
            var key = (typeof(TSource), typeof(TDestination));

            var mappings = _config.PropertyMappings.GetOrAdd(key, _ => new ConcurrentDictionary<string, string>());
            mappings[destinationProperty] = sourceProperty;

            if (condition != null)
            {
                var conditions = _config.ConditionalMappings.GetOrAdd(key, _ => new ConcurrentDictionary<string, Func<object, bool>>());
                conditions[destinationProperty] = src => condition((TSource)src);
            }

            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options)
        {
            if (destinationMember.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Expressão deve ser uma expressão de membro.", nameof(destinationMember));

            var propertyName = memberExpression.Member.Name;
            var memberConfig = new MemberConfigurationExpression<TSource, TDestination, TMember>(_config, propertyName);
            options(memberConfig);

            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var reverseMapping = new MappingExpression<TDestination, TSource>(_config);
            var key = (typeof(TSource), typeof(TDestination));

            if (_config.PropertyMappings.TryGetValue(key, out var mappings))
            {
                foreach (var mapping in mappings)
                    reverseMapping.ForMember(mapping.Value, mapping.Key);
            }

            if (_config.CustomMappingExpressions.TryGetValue(key, out var customMappings))
            {
                foreach (var mapping in customMappings)
                {
                    if (mapping.Value.Body is MemberExpression memberExpression)
                    {
                        var sourceMember = memberExpression.Member.Name;
                        reverseMapping.ForMember(sourceMember, mapping.Key);
                    }
                }
            }

            return reverseMapping;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember)
        {
            if (destinationMember.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Expressão deve ser uma expressão de membro.", nameof(destinationMember));

            _config.IgnoredProperties.TryAdd((typeof(TSource), typeof(TDestination), memberExpression.Member.Name), 0);
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes)
        {
            _config.ConstructorSelection[(typeof(TSource), typeof(TDestination))] = parameterTypes;
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeAction)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var actions = _config.BeforeMapActions.GetOrAdd(key, _ => new List<Action<object, object>>());
            lock (actions)
            {
                actions.Add((src, dest) => beforeAction((TSource)src, (TDestination)dest));
            }
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterAction)
        {
            var key = (typeof(TSource), typeof(TDestination));
            var actions = _config.AfterMapActions.GetOrAdd(key, _ => new List<Action<object, object>>());
            lock (actions)
            {
                actions.Add((src, dest) => afterAction((TSource)src, (TDestination)dest));
            }
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor)
        {
            _config.CustomConstructors[(typeof(TSource), typeof(TDestination))] = src => constructor((TSource)src)!;
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
        {
            // 0. Validação de Hierarquia (Garante que a herança de mapeamento é logicamente válida)
            if (!typeof(TSourceBase).IsAssignableFrom(typeof(TSource)))
                throw new ArgumentException($"O tipo de origem '{typeof(TSource).Name}' deve ser derivado de '{typeof(TSourceBase).Name}'.", nameof(TSourceBase));
            if (!typeof(TDestinationBase).IsAssignableFrom(typeof(TDestination)))
                throw new ArgumentException($"O tipo de destino '{typeof(TDestination).Name}' deve ser derivado de '{typeof(TDestinationBase).Name}'.", nameof(TDestinationBase));

            var baseKey = (typeof(TSourceBase), typeof(TDestinationBase));
            var currentKey = (typeof(TSource), typeof(TDestination));

            // 1. Mapeamentos de propriedade por nome (Simples)
            if (_config.PropertyMappings.TryGetValue(baseKey, out var baseMappings))
            {
                var currentMappings = _config.PropertyMappings.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, string>());
                foreach (var mapping in baseMappings.Where(m => !currentMappings.ContainsKey(m.Key)))
                    currentMappings.TryAdd(mapping.Key, mapping.Value);
            }

            // 2. Mapeamentos customizados (Síncronos e Assíncronos)
            if (_config.CustomMappings.TryGetValue(baseKey, out var baseCustomMaps))
            {
                var currentCustomMaps = _config.CustomMappings.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
                foreach (var mapping in baseCustomMaps.Where(m => !currentCustomMaps.ContainsKey(m.Key)))
                    currentCustomMaps.TryAdd(mapping.Key, mapping.Value);
            }

            if (_config.AsyncCustomMappings.TryGetValue(baseKey, out var baseAsyncCustomMaps))
            {
                var currentAsyncCustomMaps = _config.AsyncCustomMappings.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, Func<object, IMapper, CancellationToken, Task<object>>>());
                foreach (var mapping in baseAsyncCustomMaps.Where(m => !currentAsyncCustomMaps.ContainsKey(m.Key)))
                    currentAsyncCustomMaps.TryAdd(mapping.Key, mapping.Value);
            }

            // 3. Expressões Lambda (Fundamentais para o ProjectTo e herança de lógica)
            if (_config.CustomMappingExpressions.TryGetValue(baseKey, out var baseCustomExprs))
            {
                var currentCustomExprs = _config.CustomMappingExpressions.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, LambdaExpression>());
                foreach (var mapping in baseCustomExprs.Where(m => !currentCustomExprs.ContainsKey(m.Key)))
                    currentCustomExprs.TryAdd(mapping.Key, mapping.Value);
            }

            // 4. Condições de mapeamento (Síncronas e Assíncronas)
            if (_config.ConditionalMappings.TryGetValue(baseKey, out var baseConditions))
            {
                var currentConditions = _config.ConditionalMappings.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, Func<object, bool>>());
                foreach (var mapping in baseConditions.Where(m => !currentConditions.ContainsKey(m.Key)))
                    currentConditions.TryAdd(mapping.Key, mapping.Value);
            }

            if (_config.AsyncConditionalMappings.TryGetValue(baseKey, out var baseAsyncConditions))
            {
                var currentAsyncConditions = _config.AsyncConditionalMappings.GetOrAdd(currentKey, _ => new ConcurrentDictionary<string, Func<object, CancellationToken, Task<bool>>>());
                foreach (var mapping in baseAsyncConditions.Where(m => !currentAsyncConditions.ContainsKey(m.Key)))
                    currentAsyncConditions.TryAdd(mapping.Key, mapping.Value);
            }

            // 5. Propriedades ignoradas
            var baseIgnored = _config.IgnoredProperties.Keys.Where(k => k.Source == typeof(TSourceBase) && k.Target == typeof(TDestinationBase));
            foreach (var ignored in baseIgnored)
            {
                _config.IgnoredProperties.TryAdd((typeof(TSource), typeof(TDestination), ignored.PropertyName), 0);
            }

            // 6. Value Resolvers (Síncronos e Assíncronos)
            var baseResolvers = _config.ValueResolvers.Keys.Where(k => k.Source == typeof(TSourceBase) && k.Target == typeof(TDestinationBase));
            foreach (var key in baseResolvers)
            {
                if (_config.ValueResolvers.TryGetValue(key, out var res))
                {
                    _config.ValueResolvers.TryAdd((typeof(TSource), typeof(TDestination), key.Property), res);
                }
            }

            var baseAsyncResolvers = _config.AsyncValueResolvers.Keys.Where(k => k.Source == typeof(TSourceBase) && k.Target == typeof(TDestinationBase));
            foreach (var key in baseAsyncResolvers)
            {
                if (_config.AsyncValueResolvers.TryGetValue(key, out var res))
                {
                    _config.AsyncValueResolvers.TryAdd((typeof(TSource), typeof(TDestination), key.Property), res);
                }
            }

            // 7. Ações de Ciclo de Vida (Before/After Map)
            if (_config.BeforeMapActions.TryGetValue(baseKey, out var before))
            {
                var currentBefore = _config.BeforeMapActions.GetOrAdd(currentKey, _ => new List<Action<object, object>>());
                lock (currentBefore)
                {
                    currentBefore.InsertRange(0, before); // Base run first
                }
            }

            if (_config.AfterMapActions.TryGetValue(baseKey, out var after))
            {
                var currentAfter = _config.AfterMapActions.GetOrAdd(currentKey, _ => new List<Action<object, object>>());
                lock (currentAfter)
                {
                    currentAfter.InsertRange(0, after); // Base run first
                }
            }

            // 8. Construtores e Lógica de Instanciação
            if (_config.CustomConstructors.TryGetValue(baseKey, out var ctor))
            {
                _config.CustomConstructors.TryAdd(currentKey, ctor);
            }

            if (_config.ConstructorSelection.TryGetValue(baseKey, out var ctorSel))
            {
                _config.ConstructorSelection.TryAdd(currentKey, ctorSel);
            }

            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> ValidateMemberList(MemberListType memberList = MemberListType.Destination)
        {
            _config.ValidateMemberList = memberList;
            return this;
        }

        /// <inheritdoc/>
        public IMappingExpression<TSource, TDestination> MaxDepth(int depth)
        {
            _maxDepth = depth;
            if (_config.MaxDepth > depth) _config.MaxDepth = depth;
            return this;
        }
    }

    

}
