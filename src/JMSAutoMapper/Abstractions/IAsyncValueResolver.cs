// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Core;

namespace JMSAutoMapper.Abstractions
{
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

}
