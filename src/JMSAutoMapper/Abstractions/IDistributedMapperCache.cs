// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

namespace JMSAutoMapper.Abstractions
{
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

    

}
