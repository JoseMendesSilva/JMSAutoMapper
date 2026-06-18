using JMSAutoMapper.Core;

namespace JMSAutoMapper.Abstractions
{
    /// <summary>
    /// Interface para resolvers de valor customizados.
    /// </summary>
    public interface IValueResolver<in TSource, in TDestination, TDestMember>
    {
        TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
    }
}
