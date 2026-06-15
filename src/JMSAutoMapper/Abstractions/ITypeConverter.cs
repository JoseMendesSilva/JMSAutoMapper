// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Core;

namespace JMSAutoMapper.Abstractions
{
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


}
