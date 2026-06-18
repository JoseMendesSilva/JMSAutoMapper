using System;
using System.Linq.Expressions;
using System.Reflection;
using JMSAutoMapper.Internals;

namespace JMSAutoMapper.Projection
{
    /// <summary>
    /// Utilitário especializado em construir árvores de expressão compatíveis com Entity Framework e outros provedores LINQ.
    /// Focado em gerar padrões de código que são facilmente traduzíveis para instruções SQL (CASE WHEN, CAST, etc).
    /// </summary>
    internal static class EfSafeExpressionBuilder
    {
        /// <summary>
        /// Cria uma expressão de verificação de nulo (Null Check) segura para IQueryable.
        /// Diferente do mapeamento de objetos em memória, projeções não podem invocar métodos de validação complexos,
        /// então utilizamos expressões condicionais simples que se traduzem em CASE WHEN no banco de dados.
        /// </summary>
        /// <param name="source">Expressão de acesso ao objeto pai.</param>
        /// <param name="member">O membro (propriedade/campo) a ser acessado.</param>
        /// <returns>Uma expressão condicional que previne NullReferenceException no provedor LINQ.</returns>
        public static Expression BuildNullSafeMemberAccess(Expression source, MemberInfo member)
        {
            var memberAccess = Expression.MakeMemberAccess(source, member);

            // Se o tipo base da expressão (source) não for anulável, o acesso é inerentemente seguro no SQL.
            if (!source.Type.IsNullable() && source.Type.IsValueType)
            {
                return memberAccess;
            }

            // Representa no SQL: CASE WHEN source IS NULL THEN NULL ELSE source.Member END
            return Expression.Condition(
                Expression.Equal(source, Expression.Constant(null, source.Type)),
                Expression.Default(memberAccess.Type),
                memberAccess
            );
        }

        /// <summary>
        /// Constrói uma conversão de tipo que provedores SQL conseguem traduzir (CAST).
        /// </summary>
        /// <param name="expression">A expressão original.</param>
        /// <param name="targetType">O tipo de destino desejado.</param>
        /// <returns>Uma expressão de conversão traduzível.</returns>
        public static Expression SafeConvert(Expression expression, Type targetType)
        {
            if (expression.Type == targetType) return expression;

            // O uso de Expression.Convert é amplamente suportado e traduzido para CAST no SQL.
            return Expression.Convert(expression, targetType);
        }
    }
}