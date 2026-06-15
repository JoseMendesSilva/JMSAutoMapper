// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Core;

namespace JMSAutoMapper.Abstractions
{
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


}
