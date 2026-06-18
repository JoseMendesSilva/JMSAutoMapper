using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JMSAutoMapper.Abstractions
{   
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);
        void MapFrom(Func<TSource, TDestination, TMember> resolver);
        void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new();
        void MapFromAsync<TAsyncResolver>() where TAsyncResolver : IAsyncValueResolver<TSource, TDestination, TMember>, new();
        void MapFrom(IValueResolver<TSource, TDestination, TMember> resolver);
        void MapFromAsync(IAsyncValueResolver<TSource, TDestination, TMember> resolver);
        void Ignore();
        void Condition(Func<TSource, bool> condition);
        void ConditionAsync(Func<TSource, CancellationToken, Task<bool>> condition);
        void NullSubstitute(object value);
        void UseDestinationValue();
        void ConvertUsing(Func<TMember, object> converter);
        void MapFromSourceMember(string sourceMemberName);
    }
}
