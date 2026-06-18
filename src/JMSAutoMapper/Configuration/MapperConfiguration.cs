using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Internals;
using JMSAutoMapper.Validation;

namespace JMSAutoMapper.Configuration
{
    public partial class MapperConfiguration
    {
        // Dicionários de configuração utilizando as extensões modulares
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, object>>> CustomMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, CancellationToken, Task<object>>>> AsyncCustomMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, LambdaExpression>> CustomMappingExpressions { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, string>> PropertyMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, bool>>> ConditionalMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, CancellationToken, Task<bool>>>> AsyncConditionalMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target, string PropertyName), byte> IgnoredProperties { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), Type[]> ConstructorSelection { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), List<Action<object, object>>> BeforeMapActions { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), List<Action<object, object>>> AfterMapActions { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), Func<object, object>> CustomConstructors { get; } = new();
        internal ConcurrentDictionary<Type, object> TypeConverters { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target, string Property), object> ValueResolvers { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target, string Property), object> AsyncValueResolvers { get; } = new();

        public Func<string, string> NamingConvention { get; set; } = name => name;
        public bool ThrowOnConversionError { get; set; } = true;
        public int MaxDepth { get; set; } = 10;
        public MemberListType ValidateMemberList { get; set; } = MemberListType.Destination;
        public bool EnableDiagnostics { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
        public bool ValidateOnBuild { get; set; } = false;
        public int CacheExpirationMinutes { get; set; } = 30;
        public global::JMSAutoMapper.NullValueMappingPolicy NullValueMappingStrategy { get; set; } = global::JMSAutoMapper.NullValueMappingPolicy.Throw;
        public bool EnableStaticCache { get; set; } = true;

        public MapperConfiguration() { }

        public MapperConfiguration(Action<MapperConfiguration> configure)
        {
            configure?.Invoke(this);
        }

        /// <summary>
        /// Registra um novo mapeamento e valida se a configuração não está selada.
        /// </summary>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            ThrowIfSealed();
            return new MappingExpression<TSource, TDestination>(this);
        }

        /// <summary>
        /// Adiciona um perfil de configuração pelo tipo.
        /// </summary>
        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            var profile = new TProfile();
            AddProfile(profile);
        }

        /// <summary>
        /// Adiciona um perfil mesclando as configurações via extensões de dicionário.
        /// </summary>
        public void AddProfile(Profile profile)
        {
            ThrowIfSealed();
            
            CustomMappings.MergeNested(profile.Configuration.CustomMappings);
            AsyncCustomMappings.MergeNested(profile.Configuration.AsyncCustomMappings);
            CustomMappingExpressions.MergeNested(profile.Configuration.CustomMappingExpressions);
            PropertyMappings.MergeNested(profile.Configuration.PropertyMappings);
            ConditionalMappings.MergeNested(profile.Configuration.ConditionalMappings);
            
            foreach (var ignored in profile.Configuration.IgnoredProperties)
                IgnoredProperties.TryAdd(ignored.Key, 0);

            ConstructorSelection.Merge(profile.Configuration.ConstructorSelection);
            BeforeMapActions.MergeLists(profile.Configuration.BeforeMapActions);
            AfterMapActions.MergeLists(profile.Configuration.AfterMapActions);
            CustomConstructors.Merge(profile.Configuration.CustomConstructors);
        }

        public void AddProfilesFromAssembly(Assembly assembly)
        {
            var profileTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Profile)));

            foreach (var type in profileTypes)
            {
                AddProfile((Profile)Activator.CreateInstance(type)!);
            }
        }

        /// <summary>
        /// Ponto de entrada para validação de configuração.
        /// </summary>
        internal void AssertConfigurationIsValidInternal()
        {
            new ConfigurationValidator(this).Validate();
        }

        /// <summary>
        /// Cria o Mapper e sela a configuração para garantir imutabilidade e performance.
        /// </summary>
        public IMapper CreateMapper()
        {
            Seal();
            return new JMSMapper(this);
        }
    }
}
