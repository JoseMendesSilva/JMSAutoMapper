// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Core;
using JMSAutoMapper.Diagnostics;

namespace JMSAutoMapper.Abstractions
{
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


}
