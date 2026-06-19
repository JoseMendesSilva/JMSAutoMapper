// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Projection;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.Core
{
#if false
    #region Interfaces Principais
    /// <summary>
    /// Interface principal do mapeador objeto-objeto.
    /// Fornece métodos para mapeamento síncrono e assíncrono entre tipos.
    /// </summary>
    /// <remarks>
    /// Exemplo de uso:
    /// <code>
    /// IMapper mapper = config.CreateMapper();
    /// var destino = mapper.Map&lt;Destino&gt;(origem);
    /// var listaDestino = mapper.Map&lt;List&lt;Destino&gt;&gt;(listaOrigem);
    /// </code>
    /// </remarks>
    public interface IMapper
    {
        /// <summary>Mapeia um objeto de origem para o tipo destino especificado.</summary>
        /// <typeparam name="T">Tipo do objeto destino.</typeparam>
        /// <param name="source">Objeto de origem (pode ser null).</param>
        /// <returns>Objeto mapeado do tipo T.</returns>
        /// <exception cref="ArgumentNullException">Se source for null e T for value type não anulável.</exception>
        /// <exception cref="MappingException">Se ocorrer erro durante o mapeamento.</exception>
        T Map<T>(object? source);

        /// <summary>Mapeia de TSource para TDestination.</summary>
        /// <typeparam name="TSource">Tipo da origem.</typeparam>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        /// <param name="source">Objeto de origem.</param>
        /// <returns>Objeto destino mapeado.</returns>
        TDestination Map<TSource, TDestination>(TSource source);

        /// <summary>Mapeia para uma instância de destino existente.</summary>
        /// <typeparam name="TSource">Tipo da origem.</typeparam>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        /// <param name="source">Objeto de origem.</param>
        /// <param name="destination">Instância destino existente (será atualizada).</param>
        /// <returns>A mesma instância de destination, com os valores mapeados.</returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

        /// <summary>Mapeia com tipos especificados em runtime.</summary>
        /// <param name="source">Objeto de origem.</param>
        /// <param name="sourceType">Tipo da origem.</param>
        /// <param name="destinationType">Tipo do destino.</param>
        /// <returns>Objeto destino mapeado.</returns>
        object Map(object source, Type sourceType, Type destinationType);

        /// <summary>Mapeia assincronamente.</summary>
        /// <typeparam name="T">Tipo do objeto destino.</typeparam>
        /// <param name="source">Objeto de origem.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Task com o objeto mapeado.</returns>
        Task<T> MapAsync<T>(object? source, CancellationToken cancellationToken = default);

        /// <summary>Mapeia assincronamente de TSource para TDestination.</summary>
        /// <typeparam name="TSource">Tipo da origem.</typeparam>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        /// <param name="source">Objeto de origem.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Task com o objeto destino mapeado.</returns>
        Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);

        /// <summary>Mapeia IQueryable para projeções.</summary>
        /// <typeparam name="TSource">Tipo da origem.</typeparam>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        /// <param name="source">Queryable de origem.</param>
        /// <returns>Queryable com a projeção aplicada.</returns>
        /// <remarks>
        /// Útil para uso com Entity Framework, pois a projeção é traduzida para SQL.
        /// </remarks>
        IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;

        /// <summary>Projeta IQueryable para o tipo destino.</summary>
        /// <typeparam name="TDestination">Tipo do destino.</typeparam>
        /// <param name="source">Queryable de origem.</param>
        /// <returns>Queryable com a projeção aplicada.</returns>
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);

        /// <summary>Valida se a configuração é válida.</summary>
        /// <exception cref="MappingException">Se houver erros de configuração.</exception>
        void AssertConfigurationIsValid();

        /// <summary>Obtém diagnósticos do mapper.</summary>
        /// <returns>Informações de diagnóstico.</returns>
        DiagnosticInfo GetDiagnostics();
    }

    /// <summary>
    /// Interface para value resolvers síncronos.
    /// Permite lógica personalizada para resolver valores de propriedades.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TDestMember">Tipo do membro destino.</typeparam>
    /// <remarks>
    /// Exemplo:
    /// <code>
    /// public class NomeCompletoResolver : IValueResolver&lt;Usuario, UsuarioDto, string&gt;
    /// {
    ///     public string Resolve(Usuario source, UsuarioDto destination, string destMember, ResolutionContext context)
    ///     {
    ///         return $"{source.Nome} {source.Sobrenome}";
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public interface IValueResolver<TSource, TDestination, TDestMember>
    {
        /// <summary>Resolve o valor para o membro destino.</summary>
        TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
    }

    /// <summary>
    /// Interface para value resolvers assíncronos.
    /// Útil para resoluções que envolvem operações de I/O.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TDestMember">Tipo do membro destino.</typeparam>
    public interface IAsyncValueResolver<TSource, TDestination, TDestMember>
    {
        /// <summary>Resolve o valor para o membro destino assincronamente.</summary>
        Task<TDestMember> ResolveAsync(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Interface para conversores de tipo.
    /// Permite conversão completa entre dois tipos.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    public interface ITypeConverter<TSource, TDestination>
    {
        /// <summary>Converte de TSource para TDestination.</summary>
        TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
    }

    /// <summary>
    /// Interface para expressões de mapeamento.
    /// Fornece uma API fluente para configurar mapeamentos.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    public interface IMappingExpression<TSource, TDestination>
    {
        /// <summary>Configura mapeamento com expressão lambda.</summary>
        /// <typeparam name="TMember">Tipo do membro.</typeparam>
        /// <param name="destinationProperty">Nome da propriedade destino.</param>
        /// <param name="mappingExpression">Expressão para obter o valor da origem.</param>
        /// <param name="condition">Condição opcional para aplicar o mapeamento.</param>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(string destinationProperty, Expression<Func<TSource, TMember>> mappingExpression, Func<TSource, bool>? condition = null);

        /// <summary>Configura mapeamento por nome de propriedade.</summary>
        /// <param name="destinationProperty">Nome da propriedade destino.</param>
        /// <param name="sourceProperty">Nome da propriedade origem.</param>
        /// <param name="condition">Condição opcional para aplicar o mapeamento.</param>
        IMappingExpression<TSource, TDestination> ForMember(string destinationProperty, string sourceProperty, Func<TSource, bool>? condition = null);

        /// <summary>Configura mapeamento com opções avançadas.</summary>
        /// <typeparam name="TMember">Tipo do membro destino.</typeparam>
        /// <param name="destinationMember">Expressão apontando para a propriedade destino.</param>
        /// <param name="options">Ação de configuração do membro.</param>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options);

        /// <summary>Cria mapeamento reverso (TDestination para TSource).</summary>
        IMappingExpression<TDestination, TSource> ReverseMap();

        /// <summary>Ignora propriedade no mapeamento.</summary>
        /// <typeparam name="TMember">Tipo do membro.</typeparam>
        /// <param name="destinationMember">Expressão apontando para a propriedade a ser ignorada.</param>
        IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember);

        /// <summary>Especifica construtor a ser usado para criar o objeto destino.</summary>
        /// <param name="parameterTypes">Tipos dos parâmetros do construtor.</param>
        IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes);

        /// <summary>Ação executada antes do mapeamento.</summary>
        /// <param name="beforeAction">Ação que recebe origem e destino.</param>
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeAction);

        /// <summary>Ação executada após o mapeamento.</summary>
        /// <param name="afterAction">Ação que recebe origem e destino.</param>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterAction);

        /// <summary>Construtor personalizado para criar o objeto destino.</summary>
        /// <param name="constructor">Função que recebe a origem e retorna o destino.</param>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor);

        /// <summary>Inclui mapeamento base.</summary>
        /// <typeparam name="TSourceBase">Tipo base da origem.</typeparam>
        /// <typeparam name="TDestinationBase">Tipo base do destino.</typeparam>
        IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>();

        /// <summary>Configura validação de membros.</summary>
        /// <param name="memberList">Tipo de lista a ser validada.</param>
        IMappingExpression<TSource, TDestination> ValidateMemberList(MemberListType memberList = MemberListType.Destination);

        /// <summary>Configura profundidade máxima de mapeamento.</summary>
        /// <param name="depth">Profundidade máxima.</param>
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);
    }

    /// <summary>
    /// Interface para configuração de membros.
    /// Fornece opções avançadas para configurar uma propriedade específica.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>Mapeia de expressão de origem.</summary>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>Mapeia de função com origem e destino.</summary>
        void MapFrom(Func<TSource, TDestination, TMember> resolver);

        /// <summary>Mapeia de value resolver síncrono (cria nova instância).</summary>
        void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new();

        /// <summary>Mapeia de value resolver assíncrono (cria nova instância).</summary>
        void MapFromAsync<TAsyncResolver>() where TAsyncResolver : IAsyncValueResolver<TSource, TDestination, TMember>, new();

        /// <summary>Mapeia de instância de value resolver síncrono.</summary>
        void MapFrom(IValueResolver<TSource, TDestination, TMember> resolver);

        /// <summary>Mapeia de instância de value resolver assíncrono.</summary>
        void MapFromAsync(IAsyncValueResolver<TSource, TDestination, TMember> resolver);

        /// <summary>Ignora o membro no mapeamento.</summary>
        void Ignore();

        /// <summary>Condição para mapeamento.</summary>
        void Condition(Func<TSource, bool> condition);

        /// <summary>Condição assíncrona para mapeamento.</summary>
        void ConditionAsync(Func<TSource, CancellationToken, Task<bool>> condition);

        /// <summary>Substitui valor nulo por valor especificado.</summary>
        void NullSubstitute(object value);

        /// <summary>Usa valor de destino existente (não substitui se já tiver valor).</summary>
        void UseDestinationValue();

        /// <summary>Converte valor usando função.</summary>
        void ConvertUsing(Func<TMember, object> converter);

        /// <summary>Mapeia de membro de origem por nome.</summary>
        void MapFromSourceMember(string sourceMemberName);
    }

    #endregion

    #region Enums e Classes Auxiliares

    /// <summary>
    /// Tipo de lista de membros para validação.
    /// Define quais membros devem ser validados.
    /// </summary>
    public enum MemberListType { Destination, Source }

    /// <summary>
    /// Define a estratégia global para quando um valor de origem nulo é mapeado 
    /// para um tipo de valor não anulável (int, decimal, etc) no destino.
    /// </summary>
    public enum NullValueMappingPolicy { Throw, Default, Ignore }

    /// <summary>
    /// Classe base para value resolvers.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public abstract class ValueResolver<TSource, TDestination, TMember> : IValueResolver<TSource, TDestination, TMember>
    {
        /// <summary>Resolve o valor do membro.</summary>
        public abstract TMember Resolve(TSource source, TDestination destination, TMember destMember, ResolutionContext context);
    }

    /// <summary>
    /// Classe base para value resolvers assíncronos.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public abstract class AsyncValueResolver<TSource, TDestination, TMember> : IAsyncValueResolver<TSource, TDestination, TMember>
    {
        /// <summary>Resolve o valor do membro assincronamente.</summary>
        public abstract Task<TMember> ResolveAsync(TSource source, TDestination destination, TMember destMember, ResolutionContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Contexto de resolução para mapeamento.
    /// Fornece cache de instâncias durante o processo de mapeamento.
    /// </summary>
    public class ResolutionContext
    {
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<object, object> _instanceCache = new();
        private readonly ConcurrentDictionary<object, Task<object>> _asyncInstanceCache = new();

        /// <summary>Inicializa nova instância do ResolutionContext.</summary>
        public ResolutionContext(IMapper mapper) => _mapper = mapper;

        /// <summary>Mapeia objeto com cache de instâncias (previne loops infinitos).</summary>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) return default!;
            if (_instanceCache.TryGetValue(source, out var cached)) return (TDestination)cached;

            var result = _mapper.Map<TSource, TDestination>(source);
            if (result != null)
            {
                _instanceCache[source] = result!;
            }
            return result;
        }

        /// <summary>Mapeia objeto assincronamente com cache de instâncias.</summary>
        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
        {
            if (source == null) return default!;

            if (_asyncInstanceCache.TryGetValue(source, out var cachedTask))
            {
                var cachedResult = await cachedTask.ConfigureAwait(false);
                return (TDestination)cachedResult;
            }

            var task = _mapper.MapAsync<TSource, TDestination>(source, cancellationToken);

            // Converte Task<TDestination> para Task<object>
            var taskObject = task.ContinueWith(t => (object)t.Result!, cancellationToken);

            _asyncInstanceCache[source] = taskObject;

            var result = await task.ConfigureAwait(false);
            return result;
        }
    }

    /// <summary>
    /// Pool de expressões compiladas para reutilização.
    /// Otimiza performance evitando recompilação de expressões.
    /// </summary>
    public class ExpressionPool
    {
        private readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _compiledExpressions = new();

        /// <summary>Obtém ou adiciona expressão compilada ao pool.</summary>
        public Delegate GetOrAdd((Type Source, Type Target) key, Func<(Type, Type), Delegate> factory)
        {
            return _compiledExpressions.GetOrAdd(key, factory);
        }

        /// <summary>Tenta obter expressão do pool.</summary>
        public bool TryGet((Type Source, Type Target) key, out Delegate? @delegate)
        {
            return _compiledExpressions.TryGetValue(key, out @delegate);
        }

        /// <summary>Limpa todas as expressões do pool.</summary>
        public void Clear() => _compiledExpressions.Clear();
    }

    /// <summary>
    /// Cache distribuído para mapeamentos.
    /// Permite armazenar resultados de mapeamentos em cache externo.
    /// </summary>
    public interface IDistributedMapperCache
    {
        /// <summary>Obtém valor do cache.</summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>Armazena valor no cache.</summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>Remove valor do cache.</summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementação em memória do cache distribuído.
    /// Útil para testes e ambientes single-server.
    /// </summary>
    public class InMemoryDistributedCache : IDistributedMapperCache
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

        private class CacheItem
        {
            public object? Value { get; set; }
            public DateTime Expiration { get; set; }
        }

        /// <summary>Obtém valor do cache.</summary>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out var item) && item.Expiration > DateTime.UtcNow)
            {
                return Task.FromResult((T?)item.Value);
            }

            _cache.TryRemove(key, out _);
            return Task.FromResult(default(T));
        }

        /// <summary>Armazena valor no cache.</summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var item = new CacheItem
            {
                Value = value,
                Expiration = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(10))
            };

            _cache[key] = item;
            return Task.CompletedTask;
        }

        /// <summary>Remove valor do cache.</summary>
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Atributo para marcar classes que podem ser cacheadas estaticamente.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// Tempo de expiração em minutos (opcional).
        /// </summary>
        public int ExpirationMinutes { get; set; } = 30;
    }

    /// <summary>
    /// Gerador de chaves para cache com suporte a diferentes estratégias.
    /// VERSÃO CORRIGIDA - Garante chaves consistentes para o mesmo objeto.
    /// </summary>
    public static class CacheKeyGenerator
    {
        private static readonly ConditionalWeakTable<object, string> _weakObjectKeys = new();
        private static readonly ConcurrentDictionary<Type, bool> _cacheableTypeCache = new();

        /// <summary>
        /// Determina se um tipo é cacheável estaticamente.
        /// </summary>
        public static bool IsTypeCacheable(Type type)
        {
            return _cacheableTypeCache.GetOrAdd(type, t =>
                t.GetCustomAttribute<CacheableAttribute>() != null ||
                t.IsValueType); // Tipos por valor são sempre cacheáveis
        }

        /// <summary>
        /// Obtém o tempo de expiração recomendado para um tipo.
        /// </summary>
        public static int GetExpirationForType(Type type)
        {
            var attr = type.GetCustomAttribute<CacheableAttribute>();
            return attr?.ExpirationMinutes ?? 30;
        }

        /// <summary>
        /// Gera uma chave de cache consistente baseada nos tipos e no objeto.
        /// Esta é a versão CORRIGIDA que garante a mesma chave para o mesmo objeto.
        /// </summary>
        public static string GenerateKey(Type sourceType, Type targetType, object source)
        {
            // Para tipos por valor (int, decimal, structs), usar o próprio valor como chave
            if (sourceType.IsValueType)
            {
                return $"map_val_{sourceType.FullName}_{targetType.FullName}_{source}";
            }

            // Para strings, usar o conteúdo como chave
            if (source is string str)
            {
                return $"map_str_{sourceType.FullName}_{targetType.FullName}_{str.GetHashCode()}";
            }

            // Para referências, usar ConditionalWeakTable para associar uma chave única ao objeto
            // sem impedir o Garbage Collector de coletá-lo
            var key = _weakObjectKeys.GetValue(source, obj =>
                $"map_ref_{sourceType.FullName}_{targetType.FullName}_{Guid.NewGuid():N}");

            return key;
        }

        /// <summary>
        /// Gera uma chave de cache estática baseada apenas nos tipos.
        /// Útil para objetos que são sempre iguais (configurações, enums, etc).
        /// </summary>
        public static string GenerateStaticKey(Type sourceType, Type targetType, object? source = null)
        {
            // Se temos um source e ele é um tipo simples ou valor, incluir na chave
            if (source != null)
            {
                if (sourceType.IsValueType || sourceType == typeof(string))
                {
                    return $"map_static_{sourceType.FullName}_{targetType.FullName}_{source}";
                }

                // Para objetos com ID, tentar usar uma propriedade de identificação
                var idProperty = sourceType.GetProperty("Id") ??
                                 sourceType.GetProperty("ID") ??
                                 sourceType.GetProperty("Codigo");

                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(source);
                    return $"map_static_{sourceType.FullName}_{targetType.FullName}_{idValue}";
                }
            }

            return $"map_static_{sourceType.FullName}_{targetType.FullName}";
        }

        /// <summary>
        /// Gera uma chave para cache de coleções baseada nos elementos.
        /// </summary>
        public static string GenerateCollectionKey(Type sourceType, Type targetType, IEnumerable source)
        {
            // Gerar hash baseado nos primeiros elementos para identificar a coleção
            const int maxElements = 5;
            var elements = source.Cast<object>().Take(maxElements).ToList();

            if (!elements.Any())
                return $"map_coll_empty_{sourceType.FullName}_{targetType.FullName}";

            var hash = string.Join("_", elements.Select(e => e?.GetHashCode() ?? 0));
            var count = (source as ICollection)?.Count ?? elements.Count;

            return $"map_coll_{sourceType.FullName}_{targetType.FullName}_{count}_{hash}";
        }
    }

    /// <summary>
    /// Informações de diagnóstico do mapper.
    /// </summary>
    public class DiagnosticInfo
    {
        /// <summary>Número total de mapeamentos configurados.</summary>
        public int TotalMappings { get; set; }

        /// <summary>Número de mapeamentos em cache.</summary>
        public int CachedMappings { get; set; }

        /// <summary>Tempo médio de mapeamento (ms).</summary>
        public double AverageMapTimeMs { get; set; }

        /// <summary>Total de mapeamentos executados.</summary>
        public long TotalMapsExecuted { get; set; }

        /// <summary>Memória utilizada (bytes).</summary>
        public long MemoryUsedBytes { get; set; }

        /// <summary>Erros ocorridos.</summary>
        public int ErrorCount { get; set; }

        /// <summary>Últimos erros (máx 10).</summary>
        public List<string> RecentErrors { get; set; } = new();

        /// <summary>Mapeamentos lentos (&gt; 100ms) (máx 10).</summary>
        public List<string> SlowMappings { get; set; } = new();

        /// <summary>Estatísticas de cache.</summary>
        public CacheStatistics CacheStats { get; set; } = new();
    }

    /// <summary>
    /// Estatísticas de cache.
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>Número de hits no cache.</summary>
        public long CacheHits { get; set; }

        /// <summary>Número de misses no cache.</summary>
        public long CacheMisses { get; set; }

        /// <summary>Taxa de acerto do cache (0-1).</summary>
        public double HitRate => CacheHits + CacheMisses > 0
            ? (double)CacheHits / (CacheHits + CacheMisses)
            : 0;

        /// <summary>Tempo médio economizado pelo cache (ms).</summary>
        public double AverageTimeSavedMs { get; set; }
    }

    /// <summary>
    /// Exceção de mapeamento.
    /// Lançada quando ocorrem erros durante configuração ou execução.
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        /// <summary>Construtor padrão.</summary>
        public MappingException() { }

        /// <summary>Construtor com mensagem.</summary>
        public MappingException(string message) : base(message) { }

        /// <summary>Construtor com mensagem e inner exception.</summary>
        public MappingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>Construtor para serialização.</summary>
        protected MappingException(SerializationInfo info, StreamingContext context) : base() { }
    }

    /// <summary>
    /// Comparador de igualdade por referência.
    /// Usado para cache de objetos em dicionários.
    /// </summary>
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        private ReferenceEqualityComparer() { }

        /// <summary>Instância singleton.</summary>
        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        /// <inheritdoc/>
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    #endregion

    #region Configuração Principal

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

    /// <summary>
    /// Classe base para perfis de configuração.
    /// Permite organizar mapeamentos em grupos lógicos.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// <code>
    /// public class UsuarioProfile : Profile
    /// {
    ///     public override void Configure()
    ///     {
    ///         CreateMap&lt;Usuario, UsuarioDto&gt;()
    ///             .ForMember(dto => dto.NomeCompleto, src => src.Nome + " " + src.Sobrenome);
    ///         
    ///         CreateMap&lt;Endereco, EnderecoDto&gt;()
    ///             .ReverseMap();
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public abstract class Profile
    {
        internal MapperConfiguration Configuration { get; private set; }

        /// <summary>Construtor padrão.</summary>
        public Profile()
        {
            Configuration = new MapperConfiguration();
            Configure();
        }

        /// <summary>Método de configuração a ser implementado.</summary>
        public virtual void Configure() { }

        /// <summary>Cria mapeamento.</summary>
        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => Configuration.CreateMap<TSource, TDestination>();
    }

    /// <summary>
    /// Configuração de perfil para DI.
    /// </summary>
    public class ProfileConfiguration
    {
        private readonly MapperConfiguration _config;

        /// <summary>Construtor.</summary>
        public ProfileConfiguration(MapperConfiguration config) => _config = config;

        /// <summary>Adiciona perfil.</summary>
        public void AddProfile<TProfile>() where TProfile : Profile, new() => _config.AddProfile<TProfile>();

        /// <summary>Adiciona todos os perfis de um assembly.</summary>
        public void AddProfilesFromAssembly(Assembly assembly) => _config.AddProfilesFromAssembly(assembly);
    }

    #endregion

    #region Expressões de Mapeamento

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

    #endregion

    #endif

    #region Implementação do Mapeador

    /// <summary>
    /// Classe base abstrata para mapeadores.
    /// Contém a lógica comum de mapeamento.
    /// </summary>
    public abstract class MapperBase : IMapper
    {
        protected internal readonly MapperConfiguration _config;
        protected readonly Action<string, Exception>? _logger;
        protected readonly ExpressionPool _expressionPool;
        protected readonly IDistributedMapperCache? _distributedCache;
        protected readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledMappers = new();
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledMappersWithDestination = new();
        protected readonly ConcurrentDictionary<Type, Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>> _mapComplexTypeAsyncDelegates = new();
        protected readonly ConcurrentDictionary<(Type Source, Type Target), object> _compiledAsyncMappers = new();
        protected readonly ConcurrentDictionary<Type, Func<object, Dictionary<object, object>, object>> _mapComplexTypeDelegates = new();
        protected readonly ConcurrentDictionary<Type, Func<object, object>?> _collectionMapperCache = new();
        protected readonly DiagnosticCollector _diagnostics;

        // Caches para diferentes estratégias
        protected readonly ConcurrentDictionary<string, Task<object>> _staticCache = new();
        protected readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

        private static readonly ConcurrentDictionary<(Type Source, Type Target), Func<object, object>> _conversionTable = new();

        static MapperBase()
        {
            // Inicialização da Lookup Table de Conversões Numéricas
            _conversionTable.TryAdd((typeof(double), typeof(decimal)), v => Convert.ToDecimal((double)v));
            _conversionTable.TryAdd((typeof(decimal), typeof(double)), v => Convert.ToDouble((decimal)v));
            _conversionTable.TryAdd((typeof(float), typeof(decimal)), v => Convert.ToDecimal((float)v));
            _conversionTable.TryAdd((typeof(decimal), typeof(float)), v => Convert.ToSingle((decimal)v));
            _conversionTable.TryAdd((typeof(double), typeof(float)), v => Convert.ToSingle((double)v));
            _conversionTable.TryAdd((typeof(float), typeof(double)), v => Convert.ToDouble((float)v));

            // Inteiros para Decimal
            Type dec = typeof(decimal);
            _conversionTable.TryAdd((typeof(int), dec), v => Convert.ToDecimal((int)v));
            _conversionTable.TryAdd((typeof(long), dec), v => Convert.ToDecimal((long)v));
            _conversionTable.TryAdd((typeof(short), dec), v => Convert.ToDecimal((short)v));
            _conversionTable.TryAdd((typeof(byte), dec), v => Convert.ToDecimal((byte)v));

            // Decimal para Inteiros
            _conversionTable.TryAdd((dec, typeof(int)), v => Convert.ToInt32((decimal)v));
            _conversionTable.TryAdd((dec, typeof(long)), v => Convert.ToInt64((decimal)v));
            _conversionTable.TryAdd((dec, typeof(short)), v => Convert.ToInt16((decimal)v));
            _conversionTable.TryAdd((dec, typeof(byte)), v => Convert.ToByte((decimal)v));
        }

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

            public DiagnosticCollector(MapperConfiguration config) => _config = config;

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

            public void RecordCacheHit(long timeSavedMs)
            {
                Interlocked.Increment(ref _cacheHits);
                Interlocked.Add(ref _totalTimeSavedByCache, timeSavedMs);
            }

            public void RecordCacheMiss()
            {
                Interlocked.Increment(ref _cacheMisses);
            }

            public void RecordError(Exception ex, string context)
            {
                if (!_config.EnableDiagnostics) return;

                _recentErrors.Enqueue($"{context}: {ex.Message}");
                while (_recentErrors.Count > 10) _recentErrors.TryDequeue(out _);
            }

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
                var collectionMapper = GetCollectionMapper(targetType);

                if (source == null)
                {
                    if (collectionMapper != null)
                        return (T)collectionMapper(null!)!;

                    if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                        throw new ArgumentNullException(nameof(source), $"Não é possível mapear uma origem nula para um tipo de valor não anulável '{typeof(T).Name}'.");
                    return default!;
                }

                // 1. Tentar Mapeador de Coleção Pré-compilado (Alta Performance)
                if (collectionMapper != null) return (T)collectionMapper(source!)!;

                var sourceType = source.GetType();

                if (targetType.IsAssignableFrom(sourceType) || IsSimpleType(targetType))
                    return (T)ConvertValue(source, targetType)!;

                var mappedObjects = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
                return MapObject<T>(source, mappedObjects);
            }
            finally
            {
                stopwatch.Stop();
                _diagnostics.RecordMap(typeof(T).Name, source?.GetType().Name ?? "null", stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Obtém ou cria um delegate compilado para mapear coleções para o tipo de destino.
        /// </summary>
        protected virtual Func<object, object>? GetCollectionMapper(Type targetType)
        {
            if (targetType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(targetType))
                return null;

            return _collectionMapperCache.GetOrAdd(targetType, type =>
            {
                var itemType = GetCollectionItemType(type);
                if (itemType == null) return null;

                MethodInfo? method = null;
                var flags = BindingFlags.NonPublic | BindingFlags.Instance;

                if (type.IsArray)
                    method = typeof(MapperBase).GetMethod(nameof(MapArray), flags)?.MakeGenericMethod(itemType);
                else if (type.IsGenericType)
                {
                    var def = type.GetGenericTypeDefinition();
                    if (def == typeof(List<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapList), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(IEnumerable<>) || def == typeof(ICollection<>) || def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapIEnumerable), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(HashSet<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapHashSet), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(ImmutableList<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapImmutableList), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(ImmutableArray<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapImmutableArray), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(ImmutableQueue<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapImmutableQueue), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(ImmutableStack<>))
                        method = typeof(MapperBase).GetMethod(nameof(MapImmutableStack), flags)?.MakeGenericMethod(itemType);
                    else if (def == typeof(Dictionary<,>) || def == typeof(IDictionary<,>))
                    {
                        var args = type.GetGenericArguments();
                        method = typeof(MapperBase).GetMethod(nameof(MapDictionary), flags)?.MakeGenericMethod(args[0], args[1]);
                    }
                    else if (def == typeof(ImmutableDictionary<,>))
                    {
                        var args = type.GetGenericArguments();
                        method = typeof(MapperBase).GetMethod(nameof(MapImmutableDictionary), flags)?.MakeGenericMethod(args[0], args[1]);
                    }
                }

                if (method == null) return null;

                var sourceParam = Expression.Parameter(typeof(object), "source");
                var call = Expression.Call(Expression.Constant(this), method, sourceParam);
                var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)), sourceParam);
                return lambda.Compile();
            });
        }

        protected Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
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

                // Sem cache, fazer mapeamento normal
                _diagnostics.RecordCacheMiss();
                return await PerformMappingAsync<T>(source, targetType, sourceType, cancellationToken).ConfigureAwait(false);
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

                // CONVERSÕES NUMÉRICAS COMPLETAS
                // double -> decimal
                if (underlyingType == typeof(decimal) && underlyingValueType == typeof(double))
                {
                    return Convert.ToDecimal((double)value);
                }

                // decimal -> double
                if (underlyingType == typeof(double) && underlyingValueType == typeof(decimal))
                {
                    return Convert.ToDouble((decimal)value);
                }

                // float -> decimal
                if (underlyingType == typeof(decimal) && underlyingValueType == typeof(float))
                {
                    return Convert.ToDecimal((float)value);
                }

                // decimal -> float
                if (underlyingType == typeof(float) && underlyingValueType == typeof(decimal))
                {
                    return Convert.ToSingle((decimal)value);
                }

                // int -> decimal
                if (underlyingType == typeof(decimal) && (
                    underlyingValueType == typeof(int) ||
                    underlyingValueType == typeof(long) ||
                    underlyingValueType == typeof(short) ||
                    underlyingValueType == typeof(byte)))
                {
                    return Convert.ToDecimal(value);
                }

                // decimal -> int
                if ((underlyingType == typeof(int) ||
                     underlyingType == typeof(long) ||
                     underlyingType == typeof(short) ||
                     underlyingType == typeof(byte)) &&
                    underlyingValueType == typeof(decimal))
                {
                    return Convert.ChangeType(Convert.ToInt32((decimal)value), underlyingType);
                }

                // double -> float
                if (underlyingType == typeof(float) && underlyingValueType == typeof(double))
                {
                    return Convert.ToSingle((double)value);
                }

                // float -> double
                if (underlyingType == typeof(double) && underlyingValueType == typeof(float))
                {
                    return Convert.ToDouble((float)value);
                }

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
                    throw new global::JMSAutoMapper.MappingException($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);

                return null;
            }
        }

        private object ConvertToEnum(object value, Type enumType)
        {
            if (value.GetType().IsEnum) return Enum.ToObject(enumType, (int)value);
            if (value is string stringValue) return Enum.Parse(enumType, stringValue, true);
            if (value is int || value is short || value is byte || value is long || value is uint || value is ushort || value is sbyte || value is ulong)
                return Enum.ToObject(enumType, value);
            if (value is decimal || value is double || value is float)
                return Enum.ToObject(enumType, Convert.ToInt32(value));

            throw new InvalidOperationException($"Não é possível converter {value.GetType().Name} para {enumType.Name}");
        }

        /// <summary>Obtém propriedades com cache.</summary>
        protected PropertyInfo[] GetProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
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
        protected Expression? GetFlattenedSourceMember(Expression sourceExpr, string targetPropertyName)
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
                throw new global::JMSAutoMapper.MappingException($"Origem deve ser uma coleção para mapear para {typeof(IEnumerable<T>).Name}.");

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
                throw new global::JMSAutoMapper.MappingException($"Origem deve ser uma coleção para mapear para {typeof(List<T>).Name}.");

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
                throw new global::JMSAutoMapper.MappingException($"Origem deve ser um dicionário para mapear para {typeof(Dictionary<TKey, TValue>).Name}.");

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

    #endregion

    /// <summary>
    /// Classe base para value resolvers.
    /// </summary>
    public abstract class ValueResolver<TSource, TDestination, TMember> : IValueResolver<TSource, TDestination, TMember>
    {
        public abstract TMember Resolve(TSource source, TDestination destination, TMember destMember, ResolutionContext context);
    }

    /// <summary>
    /// Classe base para value resolvers assíncronos.
    /// </summary>
    public abstract class AsyncValueResolver<TSource, TDestination, TMember> : IAsyncValueResolver<TSource, TDestination, TMember>
    {
        public abstract Task<TMember> ResolveAsync(TSource source, TDestination destination, TMember destMember, ResolutionContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementação principal do mapeador.
    /// </summary>
    public class JMSMapper : MapperBase
    {
        /// <summary>Construtor.</summary>
        public JMSMapper(MapperConfiguration config, Action<string, Exception>? logger = null, IDistributedMapperCache? distributedCache = null)
            : base(config, logger, distributedCache) { }

        /// <inheritdoc/>
        public override IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "source");
            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.Visit();
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        /// <inheritdoc/>
        public override IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
        {
            var sourceType = source.ElementType;
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "x");
            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.Visit();
            var lambda = Expression.Lambda(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        /// <inheritdoc/>
        protected override T MapObject<T>(object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    throw new ArgumentNullException(nameof(source), $"Não é possível mapear origem nula para tipo de valor não anulável '{typeof(T).Name}'.");
                return default!;
            }

            if (mappedObjects.TryGetValue(source, out var existing) && existing is T cached)
                return cached;

            var sourceType = source.GetType();
            var targetType = typeof(T);

            // Interceptação de coleções no fluxo síncrono para evitar erro de construtor em interfaces (IEnumerable, etc)
            if (IsCollection(targetType))
            {
                var resultCollection = MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);
                if (resultCollection != null) mappedObjects[source] = resultCollection;
                return (T)resultCollection!;
            }

            var key = (sourceType, targetType);

            var mapper = GetOrCreateMapper(key, () => BuildMapperDelegate(sourceType, targetType));

            var result = ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
            return (T)result;
        }

        /// <inheritdoc/>
        protected override async Task<T> MapObjectAsync<T>(object source, ConcurrentDictionary<object, object> mappedObjects, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    throw new ArgumentNullException(nameof(source));
                return default!;
            }

            if (mappedObjects.TryGetValue(source, out var existing) && existing is T cached)
                return cached;

            var sourceType = source.GetType();
            var targetType = typeof(T);
            var key = (sourceType, targetType);

            // Interceptação de coleções no fluxo assíncrono para evitar erro de construtor (Expression.New)
            if (IsCollection(targetType))
            {
                var resultCollection = await MapCollectionAsyncHelper(source as IEnumerable, targetType, mappedObjects, cancellationToken).ConfigureAwait(false);
                if (resultCollection != null) mappedObjects.TryAdd(source, resultCollection);
                return (T)resultCollection!;
            }

            var mapper = GetOrCreateAsyncMapper(key, () => BuildAsyncMapperDelegate(sourceType, targetType));
            var result = await ((Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>)mapper)(source, mappedObjects, cancellationToken).ConfigureAwait(false);
            
            return (T)result;
        }

        private object GetOrCreateAsyncMapper((Type Source, Type Target) key, Func<Delegate> factory)
        {
            return _compiledAsyncMappers.GetOrAdd(key, _ => factory());
        }

        private Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>> GetMapComplexTypeAsyncDelegate(Type targetType)
        {
            return _mapComplexTypeAsyncDelegates.GetOrAdd(targetType, type =>
            {
                var method = typeof(JMSMapper).GetMethod(nameof(MapObjectAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(type);

                if (genericMethod == null)
                    throw new InvalidOperationException($"Não foi possível criar delegate assíncrono para o tipo {type.Name}");

                var mapperParam = Expression.Parameter(typeof(JMSMapper), "mapper");
                var sourceParam = Expression.Parameter(typeof(object), "source");
                var cacheParam = Expression.Parameter(typeof(ConcurrentDictionary<object, object>), "cache");
                var tokenParam = Expression.Parameter(typeof(CancellationToken), "token");

                var call = Expression.Call(mapperParam, genericMethod, sourceParam, cacheParam, tokenParam);
                var helperMethod = typeof(JMSMapper).GetMethod(nameof(TaskConvertHelper), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(type);
                var convertCall = Expression.Call(helperMethod, call);

                var lambda = Expression.Lambda<Func<JMSMapper, object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>>(
                    convertCall, mapperParam, sourceParam, cacheParam, tokenParam);

                var compiled = lambda.Compile();
                return (source, cache, token) => compiled(this, source, cache, token);
            });
        }

        private static async Task<object> TaskConvertHelper<T>(Task<T> task) => (await task.ConfigureAwait(false))!;

        private static Task SetPropertyFromTask(Task<object> task, object target, Action<object, object> setter)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted) throw t.Exception!.InnerException!;
                setter(target, t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private static async Task<object> ExecuteAsyncMappingPlan(object result, List<Task> tasks, List<Action<object, object>>? afterActions, object source)
        {
            if (tasks.Count > 0)
                await Task.WhenAll(tasks).ConfigureAwait(false);

            if (afterActions != null)
                foreach (var action in afterActions) action(source, result);

            return result;
        }

        private Delegate BuildAsyncMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = Expression.Parameter(typeof(ConcurrentDictionary<object, object>), "mappedObjects");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");
            var tasksVar = Expression.Variable(typeof(List<Task>), "tasks");

            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);
            var key = (sourceType, targetType);

            var expressions = new List<Expression>
            {
                Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType)),
                Expression.Assign(tasksVar, Expression.New(typeof(List<Task>)))
            };

            // 1. Instanciação
            if (_config.CustomConstructors.TryGetValue(key, out var customConstructor))
                expressions.Add(Expression.Assign(resultVar, Expression.Convert(Expression.Invoke(Expression.Constant(customConstructor), sourceParam), targetType)));
            else
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));

            // 2. Registro no cache de instâncias (Circular Reference)
            expressions.Add(Expression.Call(mappedObjectsParam,
                typeof(ConcurrentDictionary<object, object>).GetMethod("TryAdd")!,
                sourceParam, Expression.Convert(resultVar, typeof(object))));

            // 3. BeforeMap
            if (_config.BeforeMapActions.TryGetValue(key, out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var setPropertyFromTaskMethod = typeof(JMSMapper).GetMethod(nameof(SetPropertyFromTask), BindingFlags.Static | BindingFlags.NonPublic)!;
            var mapCollectionAsyncHelperMethod = typeof(JMSMapper).GetMethod(nameof(MapCollectionAsyncHelper), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var baseInstance = Expression.Convert(Expression.Constant(this), typeof(MapperBase));
            var thisInstance = Expression.Constant(this);

            // Mapeamento das propriedades
            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                Expression propertyMappingExpression = null;

                // Mapeamento Assíncrono Customizado
                if (_config.AsyncCustomMappings.TryGetValue(key, out var asyncMappings) && asyncMappings.TryGetValue(targetProperty.Name, out var asyncFunc))
                {
                    var targetExp = Expression.Parameter(typeof(object), "t");
                    var valueExp = Expression.Parameter(typeof(object), "v");
                    var setter = Expression.Lambda<Action<object, object>>(
                        Expression.Assign(
                            Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                            Expression.Convert(Expression.Call(baseInstance, convertValueMethod, valueExp, Expression.Constant(targetProperty.PropertyType)), targetProperty.PropertyType)
                        ), targetExp, valueExp).Compile();

                    var taskCall = Expression.Invoke(Expression.Constant(asyncFunc), sourceParam, cancellationTokenParam);
                    propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                        Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                }
                // Mapeamento por Complexidade / Coleção / Síncrono
                else
                {
                    Expression sourceValueAccess = null!;
                    if (_config.CustomMappings.TryGetValue(key, out var syncMappings) && syncMappings.TryGetValue(targetProperty.Name, out var syncFunc))
                    {
                        sourceValueAccess = Expression.Invoke(Expression.Constant(syncFunc), sourceParam);
                    }
                    else
                    {
                        var sourcePropName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                        var sourceProp = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropName, StringComparison.OrdinalIgnoreCase));
                        if (sourceProp != null && sourceProp.CanRead)
                            sourceValueAccess = Expression.Property(sourceVar, sourceProp);
                        else
                            sourceValueAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name)!;
                    }

                    if (sourceValueAccess != null)
                    {
                        if (IsCollection(targetProperty.PropertyType))
                        {
                            var targetExp = Expression.Parameter(typeof(object), "t");
                            var valueExp = Expression.Parameter(typeof(object), "v");
                            var setter = Expression.Lambda<Action<object, object>>(
                                Expression.Assign(
                                    Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                                    Expression.Convert(valueExp, targetProperty.PropertyType)
                                ), targetExp, valueExp).Compile();

                            var taskCall = Expression.Call(thisInstance, mapCollectionAsyncHelperMethod,
                                Expression.Convert(sourceValueAccess, typeof(IEnumerable)),
                                Expression.Constant(targetProperty.PropertyType),
                                mappedObjectsParam, cancellationTokenParam);

                            propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                                Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                        }
                        else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                        {
                            var mapAsyncDelegate = GetMapComplexTypeAsyncDelegate(targetProperty.PropertyType);

                            var targetExp = Expression.Parameter(typeof(object), "t");
                            var valueExp = Expression.Parameter(typeof(object), "v");
                            var setter = Expression.Lambda<Action<object, object>>(
                                Expression.Assign(
                                    Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                                    Expression.Convert(valueExp, targetProperty.PropertyType)
                                ), targetExp, valueExp).Compile();

                            var taskCall = Expression.Invoke(Expression.Constant(mapAsyncDelegate),
                                Expression.Convert(sourceValueAccess, typeof(object)),
                                mappedObjectsParam, cancellationTokenParam);

                            propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                                Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                        }
                        else
                        {
                            // Lógica de Política de Erro para Tipos de Valor (Simples)
                            var isNonNullableValueTypeDest = targetProperty.PropertyType.IsValueType && Nullable.GetUnderlyingType(targetProperty.PropertyType) == null;
                            var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourceValueAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                            var assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));

                            // Determine if the source can be null
                            var sourceCanBeNull = !sourceValueAccess.Type.IsValueType || Nullable.GetUnderlyingType(sourceValueAccess.Type) != null;
                            var sourceIsNull = sourceCanBeNull ? Expression.Equal(sourceValueAccess, Expression.Constant(null, sourceValueAccess.Type)) : null;

                            if (isNonNullableValueTypeDest)
                            {
                                if (sourceCanBeNull)
                                {
                                    if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Throw)
                                    {
                                        propertyMappingExpression = Expression.IfThenElse(
                                            sourceIsNull!,
                                            Expression.Throw(Expression.New(typeof(global::JMSAutoMapper.MappingException).GetConstructor(new[] { typeof(string) })!,
                                                Expression.Constant($"Falha ao mapear '{targetProperty.Name}': Valor de origem é nulo para um tipo de valor não anulável '{targetProperty.PropertyType.Name}'. Para mudar este comportamento, altere NullValueMappingStrategy."))),
                                            assignment);
                                    }
                                    else if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Ignore)
                                    {
                                        propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull!), assignment);
                                    }
                                    else // Default
                                    {
                                        propertyMappingExpression = assignment;
                                    }
                                }
                                else // Source cannot be null (non-nullable value type), so just assign
                                {
                                    propertyMappingExpression = assignment;
                                }
                            }
                            else // Target is nullable value type or reference type
                            {
                                if (sourceCanBeNull)
                                {
                                    propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull!), assignment);
                                }
                                else // Source cannot be null, just assign
                                {
                                    propertyMappingExpression = assignment;
                                }
                            }

                            if (propertyMappingExpression != null)
                            {
                                if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                                {
                                    var conditionCall = Expression.Invoke(Expression.Constant(conditionFunc), sourceParam);
                                    expressions.Add(Expression.IfThen(conditionCall, propertyMappingExpression));
                                }
                                else
                                {
                                    expressions.Add(propertyMappingExpression);
                                }
                            }
                        }
                    }
                }

                // Adicionar propertyMappingExpression à lista de expressões se não for nula
                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionCall = Expression.Invoke(Expression.Constant(conditionFunc), sourceParam);
                        expressions.Add(Expression.IfThen(conditionCall, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            // 4. Executar Plano e retornar Task
            // 4. Executar Plano e retornar Task (fora do loop foreach)
            var executeMethod = typeof(JMSMapper).GetMethod(nameof(ExecuteAsyncMappingPlan), BindingFlags.Static | BindingFlags.NonPublic)!;
            _config.AfterMapActions.TryGetValue(key, out var afterActions);

            expressions.Add(Expression.Call(executeMethod,
                Expression.Convert(resultVar, typeof(object)),
                tasksVar,
                Expression.Constant(afterActions, typeof(List<Action<object, object>>)),
                sourceParam));

            var body = Expression.Block(new[] { sourceVar, resultVar, tasksVar }, expressions);
            return Expression.Lambda<
                Func<object, ConcurrentDictionary<object, object>, CancellationToken,
                Task<object>>>(body, sourceParam, mappedObjectsParam, cancellationTokenParam).Compile();
        }

        private async Task<object?> MapCollectionAsyncHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, ConcurrentDictionary<object, object> mappedObjects, CancellationToken token)
        {
            if (targetCollectionType == null) return null;
            var itemType = GetCollectionItemType(targetCollectionType);
            if (itemType == null) return null;

            // Se o itemType for um tipo simples, mapeia os itens sincronicamente usando ConvertValue
            if (IsSimpleType(itemType))
            {
                var simpleListType = typeof(List<>).MakeGenericType(itemType!);
                var simpleMappedList = (IList)Activator.CreateInstance(simpleListType)!;

                if (sourceEnumerable == null)
                {
                    if (targetCollectionType.IsArray) return Array.CreateInstance(itemType!, 0);
                    if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;
                    try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
                }

                foreach (var item in sourceEnumerable)
                {
                    if (item == null) continue;
                    var convertedItem = ConvertValue(item, itemType);
                    if (convertedItem != null) simpleMappedList.Add(convertedItem);
                }

                if (targetCollectionType.IsArray)
                {
                    var array = Array.CreateInstance(itemType, simpleMappedList.Count);
                    simpleMappedList.CopyTo(array, 0);
                    return array;
                }

                if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;

                var simpleConstructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType!) });
                if (simpleConstructor != null) return simpleConstructor.Invoke(new object[] { simpleMappedList });

                return simpleMappedList;
            }

            var listType = typeof(List<>).MakeGenericType(itemType!);
            var mappedList = (IList)Activator.CreateInstance(listType)!;

            if (sourceEnumerable == null)
            {
                if (targetCollectionType.IsArray) return Array.CreateInstance(itemType, 0);
                if (targetCollectionType.IsAssignableFrom(listType)) return mappedList;
                try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
            }

            var items = sourceEnumerable.Cast<object>().ToList();
            var mapDelegate = GetMapComplexTypeAsyncDelegate(itemType!); // CS8602

            var tasks = items.Select(item => mapDelegate(item, mappedObjects, token)).ToList();
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var result in results)
            {
                if (result != null) mappedList.Add(result);
            }

            if (targetCollectionType.IsArray)
            { // CS8602
                var array = Array.CreateInstance(itemType, mappedList.Count);
                mappedList.CopyTo(array, 0);
                return array;
            }

            if (targetCollectionType.IsAssignableFrom(listType)) return mappedList;

            var constructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType) });
            if (constructor != null) return constructor.Invoke(new object[] { mappedList });
            
            return mappedList;
        }

        /// <inheritdoc/>
        protected override TDestination MapObjectWithDestination<TSource, TDestination>(TSource source, TDestination destination, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return destination;
            if (mappedObjects.TryGetValue(source!, out var existing) && existing is TDestination cached) return cached;

            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);
            var key = (sourceType, targetType);

            var mapper = GetOrCreateMapperWithDestination(key, () => BuildMapperDelegateWithDestination(sourceType, targetType));

            var result = ((Func<object, object, Dictionary<object, object>, object>)mapper)(source!, destination!, mappedObjects);
            return (TDestination)result;
        }

        private object GetOrCreateMapper((Type Source, Type Target) key, Func<Delegate> factory)
        {
            if (_expressionPool.TryGet(key, out var cached))
                return cached!;

            return _compiledMappers.GetOrAdd(key, _ => factory());
        }

        private object GetOrCreateMapperWithDestination((Type Source, Type Target) key, Func<Delegate> factory)
        {
            if (_expressionPool.TryGet(key, out var cached))
                return cached!;

            return _compiledMappersWithDestination.GetOrAdd(key, _ => factory());
        }

        /// <summary>
        /// Obtém delegate para mapeamento de tipo complexo sem reflexão.
        /// </summary>
        private Func<object, Dictionary<object, object>, object> GetMapComplexTypeDelegate(Type targetType)
        {
            return _mapComplexTypeDelegates.GetOrAdd(targetType, type =>
            {
                // Criamos um delegate que chama MapObject<T> sem usar reflexão toda vez
                var method = typeof(JMSMapper).GetMethod(nameof(MapObject), BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(type);

                if (genericMethod == null)
                    throw new InvalidOperationException($"Não foi possível criar delegate para o tipo {type.Name}");

                // Compila um delegate que chama o método genérico
                return (source, mappedObjects) => genericMethod.Invoke(this, new[] { source, mappedObjects })!;
            });
        }

        private Delegate BuildMapperDelegate(Type sourceType, Type targetType)
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

            // Construção do objeto resultado
            if (_config.CustomConstructors.TryGetValue((sourceType, targetType), out var customConstructor))
            {
                expressions.Add(Expression.Assign(resultVar, Expression.Convert(
                    Expression.Invoke(Expression.Constant(customConstructor), Expression.Convert(sourceVar, typeof(object))),
                    targetType)));
            }
            else if (_config.ConstructorSelection.TryGetValue((sourceType, targetType), out var parameterTypes))
            {
                var bestConstructor = targetType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameterTypes, null);
                if (bestConstructor != null)
                {
                    var constructorParameters = bestConstructor.GetParameters();
                    var arguments = new Expression[constructorParameters.Length];

                    for (int i = 0; i < constructorParameters.Length; i++)
                    {
                        var param = constructorParameters[i];
                        var sourceProperty = sourceProperties.FirstOrDefault(sp =>
                            string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                        arguments[i] = sourceProperty != null
                            ? Expression.Convert(Expression.Property(sourceVar, sourceProperty), param.ParameterType)
                            : Expression.Default(param.ParameterType);
                    }

                    expressions.Add(Expression.Assign(resultVar, Expression.New(bestConstructor, arguments)));
                }
                else
                {
                    expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
                }
            }
            else if (targetType.IsValueType)
            {
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
            }
            else
            {
                expressions.Add(Expression.Assign(resultVar,
                    Expression.Convert(
                        Expression.Call(
                            typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(Type) })!,
                            Expression.Constant(targetType)),
                        targetType)));
            }

            // Cache da instância
            expressions.Add(Expression.Assign(
                Expression.Property(mappedObjectsParam, "Item", sourceParam),
                Expression.Convert(resultVar, typeof(object))));

            // Ações BeforeMap
            if (_config.BeforeMapActions.TryGetValue((sourceType, targetType), out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;

            var mapObjectMethod = typeof(JMSMapper).GetMethod(nameof(MapComplexType), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            // Mapeamento das propriedades
            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                var key = (sourceType, targetType);
                Expression? propertyMappingExpression = null;

                if (_config.CustomMappings.TryGetValue(key, out var mappings) &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = Expression.Invoke(Expression.Constant(mappingFunc), Expression.Convert(sourceVar, typeof(object)));

                    Expression assignment;
                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(valueExpression, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(valueExpression, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, valueExpression, Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    propertyMappingExpression = assignment;
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                    var sourceProperty = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null && sourceProperty.CanRead)
                        sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    else
                        sourcePropertyAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name);

                    if (sourcePropertyAccess == null)
                        continue;

                    Expression assignment;
                    var isNonNullableValueTypeDest = targetProperty.PropertyType.IsValueType && Nullable.GetUnderlyingType(targetProperty.PropertyType) == null;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(sourcePropertyAccess, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourcePropertyAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    // Aplicar Política de Nulos
                    if (!sourcePropertyAccess.Type.IsValueType || Nullable.GetUnderlyingType(sourcePropertyAccess.Type) != null)
                    {
                        var sourceIsNull = Expression.Equal(sourcePropertyAccess, Expression.Constant(null, sourcePropertyAccess.Type));
                        if (isNonNullableValueTypeDest)
                        {
                            if (targetProperty.PropertyType.IsEnum)
                            {
                                propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull), assignment);
                            }
                            else if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Throw)
                            {
                                propertyMappingExpression = Expression.IfThenElse(sourceIsNull, Expression.Throw(Expression.New(typeof(global::JMSAutoMapper.MappingException).GetConstructor(new[] { typeof(string) })!, Expression.Constant($"Falha ao mapear '{targetProperty.Name}': Valor de origem é nulo para um tipo de valor não anulável '{targetProperty.PropertyType.Name}'. Para mudar este comportamento, altere NullValueMappingStrategy."))), assignment);
                            }
                            else if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Ignore)
                            {
                                propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull), assignment);
                            }
                            else // Default
                            {
                                propertyMappingExpression = assignment;
                            }
                        }
                        else
                        {
                            propertyMappingExpression = IsCollection(targetProperty.PropertyType) ? assignment : Expression.IfThen(Expression.Not(sourceIsNull!), assignment); // CS8600
                        }
                    }
                    else
                    {
                        propertyMappingExpression = assignment;
                    }
                }

                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
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

            // Ações AfterMap
            if (_config.AfterMapActions.TryGetValue((sourceType, targetType), out var afterActions))
            {
                foreach (var action in afterActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            expressions.Add(Expression.Convert(resultVar, typeof(object)));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, Dictionary<object, object>, object>>(body, sourceParam, mappedObjectsParam);

            return lambda.Compile();
        }

        private Delegate BuildMapperDelegateWithDestination(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var destParam = Expression.Parameter(typeof(object), "destObj");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");
            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");
            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var expressions = new List<Expression>
                {
                    Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType)),
                    Expression.Assign(resultVar, Expression.Condition(
                        Expression.NotEqual(destParam, Expression.Constant(null)),
                        Expression.Convert(destParam, targetType),
                        Expression.New(targetType)
                    ))
                };

            expressions.Add(Expression.Assign(
                Expression.Property(mappedObjectsParam, "Item", sourceParam),
                Expression.Convert(resultVar, typeof(object))));

            if (_config.BeforeMapActions.TryGetValue((sourceType, targetType), out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Obter delegate para MapComplexType
            var mapComplexTypeDelegate = GetMapComplexTypeDelegate(targetType);
            var mapComplexTypeConstant = Expression.Constant(mapComplexTypeDelegate);
            var mapObjectMethod = typeof(JMSMapper).GetMethod(nameof(MapComplexType), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                var key = (sourceType, targetType);
                Expression? propertyMappingExpression = null;

                if (_config.CustomMappings.TryGetValue(key, out var mappings) &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = Expression.Invoke(Expression.Constant(mappingFunc), Expression.Convert(sourceVar, typeof(object)));

                    Expression assignment;
                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(valueExpression, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(valueExpression, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, valueExpression, Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    propertyMappingExpression = assignment;
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                    var sourceProperty = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null && sourceProperty.CanRead)
                        sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    else
                        sourcePropertyAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name);

                    if (sourcePropertyAccess == null)
                        continue;

                    Expression assignment;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(sourcePropertyAccess, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourcePropertyAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    if (!sourceProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(sourceProperty.PropertyType) != null)
                    {
                        propertyMappingExpression = Expression.IfThen(
                            Expression.NotEqual(sourcePropertyAccess, Expression.Constant(null, sourceProperty.PropertyType!)), // CS8602
                            assignment);
                    }
                    else
                    {
                        propertyMappingExpression = assignment;
                    }
                }

                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) &&
                        conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionExpression = Expression.Invoke(
                            Expression.Constant(conditionFunc),
                            Expression.Convert(sourceVar, typeof(object)));

                        expressions.Add(Expression.IfThen(conditionExpression, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            if (_config.AfterMapActions.TryGetValue((sourceType, targetType), out var afterActions))
            {
                foreach (var action in afterActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            expressions.Add(Expression.Convert(resultVar, typeof(object)));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, object, Dictionary<object, object>, object>>(
                body, sourceParam, destParam, mappedObjectsParam);

            return lambda.Compile();
        }

        /// <summary>
        /// Método auxiliar para mapear tipos complexos usando delegate cacheado.
        /// </summary>
        private object? MapComplexType(Type targetType, object source, Dictionary<object, object> mappedObjects)
        {
            if (targetType == null) return null;
            if (source == null) return null;

            // Se o targetType for um tipo simples, converte-o diretamente.
            // Isso evita tentar construir um mapeador de objeto complexo para tipos simples como string.
            if (IsSimpleType(targetType))
            {
                return ConvertValue(source, targetType);
            }
            if (mappedObjects.TryGetValue(source, out var existing) && targetType.IsInstanceOfType(existing)) return existing;

            var sourceType = source.GetType();
            if (IsCollection(sourceType) && IsCollection(targetType))
                return MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);

            var mapDelegate = GetMapComplexTypeDelegate(targetType);
            return mapDelegate(source, mappedObjects);
        }

        private object? MapCollectionHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, Dictionary<object, object> mappedObjects)
        {
            if (targetCollectionType == null) return null;
            var itemType = GetCollectionItemType(targetCollectionType);
            if (itemType == null) return null; // CS8602

            // Se o itemType for um tipo simples, mapeia os itens sincronicamente usando ConvertValue
            if (IsSimpleType(itemType))
            {
                var simpleListType = typeof(List<>).MakeGenericType(itemType!);
                var simpleMappedList = (IList)Activator.CreateInstance(simpleListType)!;

                if (sourceEnumerable == null)
                {
                    if (targetCollectionType.IsArray) return Array.CreateInstance(itemType!, 0);
                    if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;
                    try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
                }

                foreach (var item in sourceEnumerable)
                {
                    if (item == null) continue;
                    var convertedItem = ConvertValue(item, itemType);
                    if (convertedItem != null) simpleMappedList.Add(convertedItem);
                }

                if (targetCollectionType.IsArray)
                {
                    var array = Array.CreateInstance(itemType!, simpleMappedList.Count);
                    simpleMappedList.CopyTo(array, 0);
                    return array;
                }

                if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;

                var simpleConstructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType!) });
                if (simpleConstructor != null) return simpleConstructor.Invoke(new object[] { simpleMappedList });

                return simpleMappedList;
            }

            var listType = typeof(List<>).MakeGenericType(itemType!);
            var mappedList = (IList)Activator.CreateInstance(listType)!;

            if (sourceEnumerable == null)
            {
                if (targetCollectionType.IsArray) return Array.CreateInstance(itemType, 0);
                if (targetCollectionType.IsAssignableFrom(listType)) return mappedList;
                try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
            }

            foreach (var item in sourceEnumerable)
            {
                if (item == null) continue;

                var mappedItem = MapComplexType(itemType, item, mappedObjects);
                if (mappedItem != null)
                    mappedList.Add(mappedItem);
            }

            if (targetCollectionType.IsArray)
            {
                var array = Array.CreateInstance(itemType, mappedList.Count);
                mappedList.CopyTo(array, 0);
                return array;
            }

            if (targetCollectionType.IsAssignableFrom(listType))
                return mappedList;

            var constructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType) });
            if (constructor != null)
                return constructor.Invoke(new object[] { mappedList });

            return mappedList;
        }

        private bool IsCollection(Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

        private bool IsComplexType(Type type)
        {
            return !(type == typeof(string) || IsSimpleType(type) || IsCollection(type));
        }

        private string GetMappedPropertyName(Type sourceType, Type targetType, string targetPropertyName,
            ConcurrentDictionary<(Type, Type), ConcurrentDictionary<string, string>> propertyMappings)
        {
            // Busca recursiva na hierarquia de tipos para suportar herança de mapeamento
            for (var s = sourceType; s != null && s != typeof(object); s = s.BaseType)
            {
                for (var t = targetType; t != null && t != typeof(object); t = t.BaseType)
                {
                    if (propertyMappings.TryGetValue((s, t), out var mappings) &&
                        mappings.TryGetValue(targetPropertyName, out var sourcePropertyName))
                    {
                        return sourcePropertyName;
                    }
                }
            }
            return _config.NamingConvention(targetPropertyName);
        }

        private class ProjectionExpressionVisitor : ExpressionVisitor
        {
            private readonly MapperConfiguration _config;
            private readonly Type _sourceType;
            private readonly Type _targetType;
            private readonly Expression _sourceExpression;
            private readonly JMSMapper _mapper;
            private readonly HashSet<(Type, Type)> _visited;

            public ProjectionExpressionVisitor(
                MapperConfiguration config,
                Type sourceType,
                Type targetType,
                Expression sourceExpression,
                JMSMapper mapper,
                HashSet<(Type, Type)> visited)
            {
                _config = config;
                _sourceType = sourceType;
                _targetType = targetType;
                _sourceExpression = sourceExpression;
                _mapper = mapper;
                _visited = visited;
            }

            public Expression Visit()
            {
                if (_visited.Contains((_sourceType, _targetType)))
                    return Expression.Default(_targetType);

                _visited.Add((_sourceType, _targetType));
                var bindings = new List<MemberBinding>();
                var targetProperties = _targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite);

                foreach (var targetProperty in targetProperties)
                {
                    if (_config.IgnoredProperties.ContainsKey((_sourceType, _targetType, targetProperty.Name)))
                        continue;

                    if (_config.CustomMappingExpressions.TryGetValue((_sourceType, _targetType), out var customMaps) &&
                        customMaps.TryGetValue(targetProperty.Name, out var lambda))
                    {
                        var body = new ParameterReplacer(lambda.Parameters[0], (ParameterExpression)_sourceExpression)
                            .Visit(lambda.Body);
                        bindings.Add(Expression.Bind(targetProperty, body));
                        continue;
                    }

                    var sourcePropertyName = _mapper.GetMappedPropertyName(
                        _sourceType, _targetType, targetProperty.Name, _config.PropertyMappings);

                    var sourceProperty = _sourceType.GetProperty(
                        sourcePropertyName,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null)
                        sourcePropertyAccess = Expression.Property(_sourceExpression, sourceProperty);
                    else
                        sourcePropertyAccess = _mapper.GetFlattenedSourceMember(_sourceExpression, targetProperty.Name);

                    if (sourcePropertyAccess != null)
                    {
                        if (_mapper.IsComplexType(targetProperty.PropertyType) &&
                            sourcePropertyAccess.Type != targetProperty.PropertyType)
                        {
                            var nestedVisitor = new ProjectionExpressionVisitor(
                                _config,
                                sourceProperty.PropertyType,
                                targetProperty.PropertyType,
                                sourcePropertyAccess,
                                _mapper,
                                _visited);

                            var nestedInit = nestedVisitor.Visit();
                            var nullCheck = Expression.Equal(
                                sourcePropertyAccess,
                                Expression.Constant(null, sourceProperty.PropertyType));

                            var conditional = Expression.Condition(
                                nullCheck,
                                Expression.Default(targetProperty.PropertyType),
                                nestedInit);

                            bindings.Add(Expression.Bind(targetProperty, conditional));
                        }
                        else if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            bindings.Add(Expression.Bind(targetProperty, sourcePropertyAccess));
                        }
                        else
                        {
                            try
                            {
                                var converted = Expression.Convert(sourcePropertyAccess, targetProperty.PropertyType);
                                bindings.Add(Expression.Bind(targetProperty, converted));
                            }
                            catch (InvalidOperationException) { }
                        }
                    }
                }

                return Expression.MemberInit(Expression.New(_targetType), bindings);
            }
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node) =>
                node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    #if false

    #region Validador de Configuração

    /// <summary>
    /// Validador de configuração.
    /// Verifica erros comuns e problemas de performance.
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly MapperConfiguration _configuration;
        private readonly List<string> _warnings = new();
        private readonly List<string> _errors = new();

        /// <summary>Construtor.</summary>
        public ConfigurationValidator(MapperConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>Valida a configuração.</summary>
        public void Validate()
        {
            ValidateMappings();
            ValidateCircularReferences();
            ValidatePerformance();
            ValidateTypeCompatibility();
            ThrowConfigurationExceptionsIfAny();
        }

        private void ValidateMappings()
        {
            var allTypeMaps = _configuration.CustomMappings.Keys
                .Concat(_configuration.PropertyMappings.Keys)
                .Concat(_configuration.CustomMappingExpressions.Keys)
                .Concat(_configuration.AsyncCustomMappings.Keys)
                .Distinct()
                .ToList();

            foreach (var typeMap in allTypeMaps)
            {
                var sourceType = typeMap.Source;
                var targetType = typeMap.Target;

                // Pega todas as propriedades do destino que podem ser escritas
                var destProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToList();

                // Propriedades que são mapeadas via configurações
                var mappedProperties = new HashSet<string>();

                // Mapeamentos customizados síncronos
                if (_configuration.CustomMappings.TryGetValue(typeMap, out var customMaps))
                    mappedProperties.UnionWith(customMaps.Keys);

                // Mapeamentos de propriedade por nome
                if (_configuration.PropertyMappings.TryGetValue(typeMap, out var propMaps))
                    mappedProperties.UnionWith(propMaps.Keys);

                // Mapeamentos por expressão
                if (_configuration.CustomMappingExpressions.TryGetValue(typeMap, out var exprMaps))
                    mappedProperties.UnionWith(exprMaps.Keys);

                // Mapeamentos assíncronos
                if (_configuration.AsyncCustomMappings.TryGetValue(typeMap, out var asyncMaps))
                    mappedProperties.UnionWith(asyncMaps.Keys);

                // Propriedades ignoradas
                var ignoredForType = _configuration.IgnoredProperties.Keys
                    .Where(ip => ip.Source == sourceType && ip.Target == targetType)
                    .Select(ip => ip.PropertyName);
                mappedProperties.UnionWith(ignoredForType);

                // Propriedades que são definidas por construtores personalizados
                if (_configuration.CustomConstructors.ContainsKey(typeMap) ||
                    _configuration.ConstructorSelection.ContainsKey(typeMap))
                {
                    // Se tem construtor personalizado, consideramos que todas as propriedades podem ser definidas lá
                    continue;
                }

                var unmapped = destProperties.Where(p => !mappedProperties.Contains(p.Name)).ToList();

                if (unmapped.Any())
                {
                    var message = $"Propriedades não mapeadas para {sourceType.Name} -> {targetType.Name}: {string.Join(", ", unmapped.Select(p => p.Name))}";

                    if (_configuration.ValidateMemberList == MemberListType.Destination)
                    {
                        _errors.Add(message);
                    }
                    else
                    {
                        _warnings.Add(message);
                    }
                }
            }
        }

        private void ValidateTypeCompatibility()
        {
            foreach (var typeMap in _configuration.PropertyMappings.Keys)
            {
                var sourceType = typeMap.Source;
                var targetType = typeMap.Target;

                if (_configuration.PropertyMappings.TryGetValue(typeMap, out var propMaps))
                {
                    foreach (var mapping in propMaps)
                    {
                        var sourceProp = sourceType.GetProperty(mapping.Value);
                        var targetProp = targetType.GetProperty(mapping.Key);

                        if (sourceProp == null)
                        {
                            _warnings.Add($"Propriedade de origem não encontrada: {sourceType.Name}.{mapping.Value}");
                            continue;
                        }

                        if (targetProp == null)
                        {
                            _warnings.Add($"Propriedade de destino não encontrada: {targetType.Name}.{mapping.Key}");
                            continue;
                        }

                        if (!AreTypesCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                        {
                            _warnings.Add($"Tipos incompatíveis: {sourceType.Name}.{mapping.Value} ({sourceProp.PropertyType.Name}) -> " +
                                         $"{targetType.Name}.{mapping.Key} ({targetProp.PropertyType.Name})");
                        }
                    }
                }
            }
        }

        private bool AreTypesCompatible(Type sourceType, Type targetType)
        {
            if (sourceType == targetType) return true;
            if (targetType.IsAssignableFrom(sourceType)) return true;

            var underlyingSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
            var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return underlyingSource == underlyingTarget;
        }

        private void ValidateCircularReferences()
        {
            var visited = new HashSet<(Type, Type)>();
            var stack = new Stack<(Type, Type)>();

            var allTypeMaps = _configuration.CustomMappings.Keys
                .Concat(_configuration.PropertyMappings.Keys)
                .Concat(_configuration.CustomMappingExpressions.Keys)
                .Distinct()
                .ToList();

            foreach (var typeMap in allTypeMaps)
            {
                if (HasCircularReference(typeMap, visited, stack))
                {
                    _warnings.Add($"Possível referência circular detectada envolvendo {typeMap.Source.Name} -> {typeMap.Target.Name}");
                }
            }
        }

        private bool HasCircularReference((Type Source, Type Target) current,
            HashSet<(Type, Type)> visited,
            Stack<(Type, Type)> stack)
        {
            if (stack.Contains(current)) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            stack.Push(current);

            var sourceProperties = current.Source.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProperty in sourceProperties)
            {
                if (IsNavigationProperty(sourceProperty))
                {
                    var elementType = GetCollectionItemType(sourceProperty.PropertyType);
                    if (elementType != null)
                    {
                        var elementTypePair = (elementType, current.Target);
                        if (HasCircularReference(elementTypePair, visited, stack))
                            return true;
                    }
                }
            }

            stack.Pop();
            return false;
        }

        private bool IsNavigationProperty(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                   (property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments().FirstOrDefault();
            if (collectionType.IsArray)
                return collectionType.GetElementType();
            return null;
        }

        private void ValidatePerformance()
        {
            var totalMappings = _configuration.CustomMappings.Count +
                               _configuration.PropertyMappings.Count +
                               _configuration.CustomMappingExpressions.Count +
                               _configuration.AsyncCustomMappings.Count;

            var complexMappings = _configuration.CustomMappings.Values.Sum(cm => cm.Count) +
                                 _configuration.CustomMappingExpressions.Values.Sum(ce => ce.Count) +
                                 _configuration.AsyncCustomMappings.Values.Sum(am => am.Count);

            if (totalMappings > 0 && complexMappings > totalMappings * 0.3)
            {
                _warnings.Add("Alta complexidade detectada - considere otimizar resolvers personalizados");
            }

            if (_configuration.MaxDepth > 20)
            {
                _warnings.Add($"Profundidade máxima ({_configuration.MaxDepth}) muito alta - pode impactar performance");
            }
        }

        private void ThrowConfigurationExceptionsIfAny()
        {
            if (_errors.Any())
                throw new global::JMSAutoMapper.MappingException($"Erros de configuração:\n{string.Join("\n", _errors)}");

            if (_warnings.Any() && _configuration.EnableDiagnostics)
            {
                Console.WriteLine($"Avisos de configuração:\n{string.Join("\n", _warnings)}");
            }
        }
    }

    #endregion

    #region Integração com DI

    /// <summary>
    /// Opções para configuração do mapper via DI.
    /// </summary>
    public class JMSMapperOptions
    {
        /// <summary>Habilitar diagnóstico.</summary>
        public bool EnableDiagnostics { get; set; } = true;

        /// <summary>Habilitar cache distribuído.</summary>
        public bool EnableDistributedCache { get; set; } = false;

        /// <summary>Habilitar cache estático para tipos marcados com [Cacheable].</summary>
        public bool EnableStaticCache { get; set; } = true;

        /// <summary>Tempo de expiração do cache (minutos).</summary>
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>Profundidade máxima de mapeamento.</summary>
        public int MaxDepth { get; set; } = 10;

        /// <summary>Se deve lançar exceção em erro de conversão.</summary>
        public bool ThrowOnConversionError { get; set; } = true;

        /// <summary>Política global para valores nulos em tipos de valor no destino.</summary>
        public NullValueMappingPolicy NullValueMappingStrategy { get; set; } = NullValueMappingPolicy.Throw;

        /// <summary>Convenção de nomenclatura.</summary>
        public Func<string, string>? NamingConvention { get; set; }

        /// <summary>Validar configuração após construção.</summary>
        public bool ValidateOnBuild { get; set; } = false;

        /// <summary>Assemblies para scan de perfis.</summary>
        public List<Assembly> AssembliesToScan { get; set; } = new();
    }

    /// <summary>
    /// Extensões para DI (Microsoft.Extensions.DependencyInjection).
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Adiciona o mapper ao container DI.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="configureProfiles">Ação para configurar perfis.</param>
        /// <param name="enableDistributedCache">Habilitar cache distribuído.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<ProfileConfiguration>? configureProfiles = null,
            bool enableDistributedCache = false)
        {
            var config = new MapperConfiguration();
            configure?.Invoke(config);

            var profileConfig = new ProfileConfiguration(config);
            configureProfiles?.Invoke(profileConfig);

            if (enableDistributedCache)
            {
                services.AddSingleton<IDistributedMapperCache, InMemoryDistributedCache>();
            }

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                var distributedCache = enableDistributedCache
                    ? provider.GetService<IDistributedMapperCache>()
                    : null;

                var mapper = new JMSMapper(config, logger, distributedCache);

                if (config.ValidateOnBuild)
                {
                    mapper.AssertConfigurationIsValid();
                }

                return mapper;
            });

            return services;
        }

        /// <summary>
        /// Adiciona o mapper ao container DI com scanning de assembly.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assembly">Assembly para scan de perfis.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Assembly assembly,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            var mapperOptions = new JMSMapperOptions();
            options?.Invoke(mapperOptions);

            if (!mapperOptions.AssembliesToScan.Contains(assembly))
            {
                mapperOptions.AssembliesToScan.Add(assembly);
            }

            return services.AddJMSMapper(mapperOptions.AssembliesToScan.ToArray(), configure, options);
        }

        /// <summary>
        /// Adiciona o mapper ao container DI com scanning de múltiplos assemblies.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assemblies">Assemblies para scan de perfis.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Assembly[] assemblies,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            var mapperOptions = new JMSMapperOptions();
            options?.Invoke(mapperOptions);

            var config = new MapperConfiguration
            {
                EnableDiagnostics = mapperOptions.EnableDiagnostics,
                EnableDistributedCache = mapperOptions.EnableDistributedCache,
                CacheExpirationMinutes = mapperOptions.CacheExpirationMinutes,
                MaxDepth = mapperOptions.MaxDepth,
                ThrowOnConversionError = mapperOptions.ThrowOnConversionError,
                NullValueMappingStrategy = mapperOptions.NullValueMappingStrategy,
                ValidateOnBuild = mapperOptions.ValidateOnBuild,
                EnableStaticCache = mapperOptions.EnableStaticCache
            };

            if (mapperOptions.NamingConvention != null)
            {
                config.NamingConvention = mapperOptions.NamingConvention;
            }

            // Scan assemblies para perfis
            var allAssemblies = new HashSet<Assembly>(assemblies);
            foreach (var assembly in mapperOptions.AssembliesToScan)
            {
                allAssemblies.Add(assembly);
            }

            foreach (var assembly in allAssemblies)
            {
                config.AddProfilesFromAssembly(assembly);
            }

            configure?.Invoke(config);

            if (mapperOptions.EnableDistributedCache)
            {
                services.AddSingleton<IDistributedMapperCache, InMemoryDistributedCache>();
            }

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                var distributedCache = mapperOptions.EnableDistributedCache
                    ? provider.GetService<IDistributedMapperCache>()
                    : null;

                var mapper = new JMSMapper(config, logger, distributedCache);

                if (config.ValidateOnBuild)
                {
                    mapper.AssertConfigurationIsValid();
                }

                return mapper;
            });

            return services;
        }

        /// <summary>
        /// Adiciona o mapper ao container DI com scanning do assembly da tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo para obter o assembly.</typeparam>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper<T>(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSMapper(typeof(T).Assembly, configure, options);
        }

        /// <summary>Valida a configuração.</summary>
        public static void AssertConfigurationIsValid(this MapperConfiguration config)
        {
            var validator = new ConfigurationValidator(config);
            validator.Validate();
        }
    }

    #endregion

    #endif

}
