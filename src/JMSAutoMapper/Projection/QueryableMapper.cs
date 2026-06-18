using System;
using System.Collections.Generic;
#if false
using System.Linq;
using System.Linq.Expressions;
using JMSAutoMapper.Projection; // Add this using directive

namespace JMSAutoMapper
{
    /// <summary>
    /// Extensão da classe JMSMapper responsável por implementar a lógica de projeção para IQueryable.
    /// Esta parte da classe permite a integração com provedores de dados (LINQ Providers),
    /// traduzindo mapeamentos de objetos para expressões que o banco de dados consegue processar.
    /// </summary>
    public partial class JMSMapper
    {
        /// <inheritdoc/>
        public override IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "source");

            // Utilizamos o ProjectionExpressionVisitor para percorrer a configuração do Mapper
            // e construir uma Expression Tree compatível com o provedor de consulta.
            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.BuildProjectionExpression();
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        /// <inheritdoc/>
        public override IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
        {
            var sourceType = source.ElementType;
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "x");

            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.BuildProjectionExpression();
            var lambda = Expression.Lambda(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }
    }
}
#endif
