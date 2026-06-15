// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Core;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Configuração de membro.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        private readonly MapperConfiguration _config;
        private readonly string _propertyName;
        private readonly (Type Source, Type Target) _key;

        /// <summary>Construtor.</summary>
        public MemberConfigurationExpression(MapperConfiguration config, string propertyName)
        {
            _config = config;
            _propertyName = propertyName;
            _key = (typeof(TSource), typeof(TDestination));
        }

        /// <inheritdoc/>
        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var expressions = _config.CustomMappingExpressions.GetOrAdd(_key, _ => new ConcurrentDictionary<string, LambdaExpression>());
            expressions[_propertyName] = sourceMember;

            var compiled = sourceMember.Compile();
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
            mappings[_propertyName] = (src, _) => compiled((TSource)src)!;
        }

        /// <inheritdoc/>
        public void MapFrom(Func<TSource, TDestination, TMember> resolver)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
            mappings[_propertyName] = (src, _) => resolver((TSource)src, default!)!;
        }

        /// <inheritdoc/>
        public void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new()
        {
            var resolver = new TResolver();
            MapFrom(resolver);
        }

        /// <inheritdoc/>
        public void MapFromAsync<TAsyncResolver>() where TAsyncResolver : IAsyncValueResolver<TSource, TDestination, TMember>, new()
        {
            var resolver = new TAsyncResolver();
            MapFromAsync(resolver);
        }

        /// <inheritdoc/>
        public void MapFrom(IValueResolver<TSource, TDestination, TMember> resolver)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());

            mappings[_propertyName] = (src, mapper) =>
            {
                var source = (TSource)src;
                var destination = default(TDestination)!;
                var destMember = default(TMember)!;
                var context = new ResolutionContext(mapper);
                return resolver.Resolve(source, destination, destMember, context)!;
            };
        }

        /// <inheritdoc/>
        public void MapFromAsync(IAsyncValueResolver<TSource, TDestination, TMember> resolver)
        {
            var asyncMappings = _config.AsyncCustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, CancellationToken, Task<object>>>());

            asyncMappings[_propertyName] = async (src, mapper, cancellationToken) =>
            {
                var source = (TSource)src;
                var destination = default(TDestination)!;
                var destMember = default(TMember)!;
                var context = new ResolutionContext(mapper);
                var result = await resolver.ResolveAsync(source, destination, destMember, context, cancellationToken).ConfigureAwait(false);
                return result!;
            };
        }

        /// <inheritdoc/>
        public void Ignore() => _config.IgnoredProperties.TryAdd((typeof(TSource), typeof(TDestination), _propertyName), 0);

        /// <inheritdoc/>
        public void Condition(Func<TSource, bool> condition)
        {
            var conditions = _config.ConditionalMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, bool>>());
            conditions[_propertyName] = src => condition((TSource)src);
        }

        /// <inheritdoc/>
        public void ConditionAsync(Func<TSource, CancellationToken, Task<bool>> condition)
        {
            var conditions = _config.AsyncConditionalMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, CancellationToken, Task<bool>>>());
            conditions[_propertyName] = async (src, token) => await condition((TSource)src, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void NullSubstitute(object value)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
            var originalMapping = mappings.TryGetValue(_propertyName, out var existing) ? existing : null;
            mappings[_propertyName] = (src, mapper) =>
            {
                var result = originalMapping?.Invoke(src, mapper);
                return result ?? value;
            };
        }

        /// <inheritdoc/>
        public void UseDestinationValue() { } // Implementação simplificada

        /// <inheritdoc/>
        public void ConvertUsing(Func<TMember, object> converter)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, IMapper, object>>());
            var originalMapping = mappings.TryGetValue(_propertyName, out var existing) ? existing : null;
            mappings[_propertyName] = (src, mapper) =>
            {
                var value = originalMapping?.Invoke(src, mapper);
                return converter((TMember)value!)!;
            };
        }

        /// <inheritdoc/>
        public void MapFromSourceMember(string sourceMemberName)
        {
            var mappings = _config.PropertyMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, string>());
            mappings[_propertyName] = sourceMemberName;
        }
    }

    

}
