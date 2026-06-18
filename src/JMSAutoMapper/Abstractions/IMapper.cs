using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Abstractions
{
    public interface IMapper
    {
        T Map<T>(object? source);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        Task<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default);
        Task<T> MapAsync<T>(object? source, CancellationToken cancellationToken = default);
        IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source) where TSource : class where TDestination : class;
        object Map(object source, Type sourceType, Type destinationType);
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
        void AssertConfigurationIsValid();
        DiagnosticInfo GetDiagnostics();
    }
}