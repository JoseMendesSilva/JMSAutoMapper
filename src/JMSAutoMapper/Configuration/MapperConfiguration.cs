// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Core;
using JMSAutoMapper.Validation;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Classe principal de configuração do mapper.
    /// Armazena todos os mapeamentos e opções de configuração.
    /// </summary>
    /// <remarks>
    /// Exemplo de configuração:
    /// <code>
    /// var config = new MapperConfiguration();
    /// config.CreateMap&lt;Usuario, UsuarioDto&gt;()
    ///     .ForMember(dto => dto.NomeCompleto, opt => opt.MapFrom(src => src.Nome + " " + src.Sobrenome))
    ///     .ForMember(dto => dto.Idade, "IdadeUsuario")
    ///     .ReverseMap();
    ///     
    /// config.AddProfile&lt;MeuPerfil&gt;();
    /// config.AddProfilesFromAssembly(typeof(Program).Assembly);
    /// </code>
    /// </remarks>
    public class MapperConfiguration
    {
        // Dicionários de configuração - usando ConcurrentDictionary para thread safety
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, IMapper, object>>> CustomMappings { get; } = new();
        internal ConcurrentDictionary<(Type Source, Type Target), ConcurrentDictionary<string, Func<object, IMapper, CancellationToken, Task<object>>>> AsyncCustomMappings { get; } = new();
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

        /// <summary>Inicializa uma nova instância de MapperConfiguration.</summary>
        public MapperConfiguration() { }

        /// <summary>Inicializa uma nova instância de MapperConfiguration com uma ação de configuração.</summary>
        /// <param name="configure">Ação para configurar o mapeador.</param>
        public MapperConfiguration(Action<MapperConfiguration> configure)
        {
            configure(this);
        }

        /// <summary>Convenção de nomenclatura para mapeamento automático.</summary>
        public Func<string, string> NamingConvention { get; set; } = name => name;

        /// <summary>Se deve lançar exceção em erro de conversão.</summary>
        public bool ThrowOnConversionError { get; set; } = true;

        /// <summary>Política global para valores nulos em tipos de valor no destino.</summary>
        public NullValueMappingPolicy NullValueMappingStrategy { get; set; } = NullValueMappingPolicy.Default;

        /// <summary>Se deve criar mapeamentos de tipos ausentes automaticamente.</summary>
        public bool CreateMissingTypeMaps { get; set; } = false;

        /// <summary>Profundidade máxima de mapeamento (previne loops infinitos).</summary>
        public int MaxDepth { get; set; } = 10;

        /// <summary>Tipo de lista de membros para validação.</summary>
        public MemberListType ValidateMemberList { get; set; } = MemberListType.Destination;

        /// <summary>Tempo de expiração do cache (minutos).</summary>
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>Habilitar diagnóstico.</summary>
        public bool EnableDiagnostics { get; set; } = true;

        /// <summary>Habilitar cache distribuído.</summary>
        public bool EnableDistributedCache { get; set; } = false;

        /// <summary>Habilitar cache estático para tipos marcados com [Cacheable].</summary>
        public bool EnableStaticCache { get; set; } = true;

        /// <summary>Validação automática após configuração.</summary>
        public bool ValidateOnBuild { get; set; } = false;

        /// <summary>
        /// Adiciona um perfil de configuração.
        /// </summary>
        /// <typeparam name="TProfile">Tipo do perfil.</typeparam>
        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            var profile = new TProfile();
            AddProfile(profile);
        }

        /// <summary>
        /// Adiciona um perfil de configuração.
        /// </summary>
        /// <param name="profile">Instância do perfil.</param>
        public void AddProfile(Profile profile)
        {
            MergeDictionaries(profile.Configuration.CustomMappings, CustomMappings);
            MergeDictionaries(profile.Configuration.AsyncCustomMappings, AsyncCustomMappings);
            MergeDictionaries(profile.Configuration.CustomMappingExpressions, CustomMappingExpressions);
            MergeDictionaries(profile.Configuration.PropertyMappings, PropertyMappings);
            MergeDictionaries(profile.Configuration.ConditionalMappings, ConditionalMappings);
            MergeDictionaries(profile.Configuration.AsyncConditionalMappings, AsyncConditionalMappings);

            foreach (var ignored in profile.Configuration.IgnoredProperties)
                IgnoredProperties.TryAdd(ignored.Key, 0);

            MergeDictionaries(profile.Configuration.ConstructorSelection, ConstructorSelection);
            MergeDictionaries(profile.Configuration.BeforeMapActions, BeforeMapActions);
            MergeDictionaries(profile.Configuration.AfterMapActions, AfterMapActions);
            MergeDictionaries(profile.Configuration.CustomConstructors, CustomConstructors);
            MergeDictionaries(profile.Configuration.TypeConverters, TypeConverters);
            MergeDictionaries(profile.Configuration.ValueResolvers, ValueResolvers);
            MergeDictionaries(profile.Configuration.AsyncValueResolvers, AsyncValueResolvers);
        }

        private void MergeDictionaries<TKey, TValue>(ConcurrentDictionary<TKey, TValue> source, ConcurrentDictionary<TKey, TValue> target) where TKey : notnull
        {
            foreach (var kvp in source)
            {
                target.AddOrUpdate(kvp.Key, kvp.Value, (_, __) => kvp.Value);
            }
        }

        private void MergeDictionaries<TKey, TSubKey, TValue>(
            ConcurrentDictionary<TKey, ConcurrentDictionary<TSubKey, TValue>> source,
            ConcurrentDictionary<TKey, ConcurrentDictionary<TSubKey, TValue>> target) where TKey : notnull where TSubKey : notnull
        {
            foreach (var kvp in source)
            {
                var targetDict = target.GetOrAdd(kvp.Key, _ => new ConcurrentDictionary<TSubKey, TValue>());
                foreach (var subKvp in kvp.Value)
                {
                    targetDict.AddOrUpdate(subKvp.Key, subKvp.Value, (_, __) => subKvp.Value);
                }
            }
        }

        private void MergeDictionaries<TKey, TValue>(ConcurrentDictionary<TKey, List<TValue>> source, ConcurrentDictionary<TKey, List<TValue>> target) where TKey : notnull
        {
            foreach (var kvp in source)
            {
                var targetList = target.GetOrAdd(kvp.Key, _ => new List<TValue>());
                lock (targetList)
                {
                    targetList.AddRange(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Adiciona todos os perfis de um assembly.
        /// </summary>
        /// <param name="assembly">Assembly contendo os perfis.</param>
        public void AddProfilesFromAssembly(Assembly assembly)
        {
            var profileTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Profile)))
                .ToList();

            foreach (var profileType in profileTypes)
            {
                var profile = (Profile)Activator.CreateInstance(profileType)!;
                AddProfile(profile);
            }
        }

        /// <summary>
        /// Cria expressão de mapeamento.
        /// </summary>
        /// <typeparam name="TSource">Tipo da origem.</typeparam>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => new MappingExpression<TSource, TDestination>(this);

        /// <summary>
        /// Valida configuração.
        /// </summary>
        internal void AssertConfigurationIsValidInternal()
        {
            var validator = new ConfigurationValidator(this);
            validator.Validate();
        }

        /// <summary>
        /// Cria instância do mapper.
        /// </summary>
        public IMapper CreateMapper() => new JMSMapper(this);
    }
}
