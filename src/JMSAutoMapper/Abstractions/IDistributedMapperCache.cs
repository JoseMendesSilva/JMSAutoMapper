using System;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Abstractions
{
    public interface IDistributedMapperCache
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}