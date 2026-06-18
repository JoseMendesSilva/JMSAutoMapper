using System.Threading;
using System.Threading.Tasks;
using JMSAutoMapper.Core;

namespace JMSAutoMapper.Abstractions
{
    /// <summary>
    /// Interface para resolvers de valor assíncronos.
    /// </summary>
    public interface IAsyncValueResolver<in TSource, in TDestination, TDestMember>
    {
        Task<TDestMember> ResolveAsync(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context, CancellationToken cancellationToken);
    }
}
