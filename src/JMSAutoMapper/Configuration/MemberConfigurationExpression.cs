using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Configuration
{
    public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        private readonly MapperConfiguration _config;
        private readonly string _propertyName;
        private readonly (Type Source, Type Target) _key;

        public MemberConfigurationExpression(MapperConfiguration config, string propertyName)
        {
            _config = config;
            _propertyName = propertyName;
            _key = (typeof(TSource), typeof(TDestination));
        }

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            var expressions = _config.CustomMappingExpressions.GetOrAdd(_key, _ => new ConcurrentDictionary<string, LambdaExpression>());
            expressions[_propertyName] = sourceMember;

            var compiled = sourceMember.Compile();
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, object>>());
            mappings[_propertyName] = src => compiled((TSource)src)!;
        }

        public void MapFrom(Func<TSource, TDestination, TMember> resolver)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, object>>());
            mappings[_propertyName] = src => resolver((TSource)src, default!)!;
        }

        public void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new()
        {
            var resolver = new TResolver();
            MapFrom(resolver);
        }

        public void MapFromAsync<TAsyncResolver>() where TAsyncResolver : IAsyncValueResolver<TSource, TDestination, TMember>, new()
        {
            var resolver = new TAsyncResolver();
            MapFromAsync(resolver);
        }

        public void MapFrom(IValueResolver<TSource, TDestination, TMember> resolver)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, object>>());
            mappings[_propertyName] = src =>
            {
                var context = new ResolutionContext(_config.CreateMapper());
                return resolver.Resolve((TSource)src, default!, default!, context)!;
            };
        }

        public void MapFromAsync(IAsyncValueResolver<TSource, TDestination, TMember> resolver)
        {
            var asyncMappings = _config.AsyncCustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, CancellationToken, Task<object>>>());
            asyncMappings[_propertyName] = async (src, cancellationToken) =>
            {
                var context = new ResolutionContext(_config.CreateMapper());
                var result = await resolver.ResolveAsync((TSource)src, default!, default!, context, cancellationToken).ConfigureAwait(false);
                return result!;
            };
        }

        public void Ignore() => _config.IgnoredProperties.TryAdd((typeof(TSource), typeof(TDestination), _propertyName), 0);

        public void Condition(Func<TSource, bool> condition)
        {
            var conditions = _config.ConditionalMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, bool>>());
            conditions[_propertyName] = src => condition((TSource)src);
        }

        public void ConditionAsync(Func<TSource, CancellationToken, Task<bool>> condition)
        {
            // Implementação de condição assíncrona
        }

        public void NullSubstitute(object value)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, object>>());
            var originalMapping = mappings.TryGetValue(_propertyName, out var existing) ? existing : null;
            mappings[_propertyName] = src => originalMapping?.Invoke(src) ?? value;
        }

        public void UseDestinationValue() { }

        public void ConvertUsing(Func<TMember, object> converter)
        {
            var mappings = _config.CustomMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, Func<object, object>>());
            var originalMapping = mappings.TryGetValue(_propertyName, out var existing) ? existing : null;
            mappings[_propertyName] = src => converter((TMember)originalMapping?.Invoke(src)!);
        }

        public void MapFromSourceMember(string sourceMemberName)
        {
            var mappings = _config.PropertyMappings.GetOrAdd(_key, _ => new ConcurrentDictionary<string, string>());
            mappings[_propertyName] = sourceMemberName;
        }
    }
}
