using System;
using System.Linq.Expressions;

namespace JMSAutoMapper.Abstractions
{
    public interface IMappingExpression<TSource, TDestination>
    {
        IMappingExpression<TSource, TDestination> ForMember<TMember>(string destinationProperty, Expression<Func<TSource, TMember>> mappingExpression, Func<TSource, bool>? condition = null);
        IMappingExpression<TSource, TDestination> ForMember(string destinationProperty, string sourceProperty, Func<TSource, bool>? condition = null);
        IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options);
        IMappingExpression<TDestination, TSource> ReverseMap();
        IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember);
        IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes);
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeAction);
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterAction);
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor);
        IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>();
        IMappingExpression<TSource, TDestination> ValidateMemberList(MemberListType memberList = MemberListType.Destination);
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);
    }
}