using System;
using System.Linq.Expressions;
using JMSAutoMapper.Internals;

namespace JMSAutoMapper.Expressions
{
    /// <summary>
    /// Utilitário para construção de árvores de expressão que incluem verificações de segurança contra nulos (Null Guards).
    /// Essencial para garantir a robustez dos planos de mapeamento compilados.
    /// </summary>
    internal static class NullGuardExpressionBuilder
    {
        /// <summary>
        /// Envolve uma lógica de mapeamento em uma verificação de nulidade da origem.
        /// </summary>
        /// <param name="sourceAccess">A expressão de acesso ao membro de origem.</param>
        /// <param name="mappingLogic">A expressão que representa a lógica de conversão/mapeamento.</param>
        /// <param name="destinationType">O tipo de retorno esperado para o membro de destino.</param>
        /// <returns>Uma expressão condicional que retorna o valor padrão caso a origem seja nula.</returns>
        public static Expression Guard(Expression sourceAccess, Expression mappingLogic, Type destinationType)
        {
            if (!IsTypeNullable(sourceAccess.Type))
            {
                return Expression.Convert(mappingLogic, destinationType);
            }

            // Representa: (source == null) ? default(TDest) : (TDest)mappingLogic
            return Expression.Condition(
                Expression.Equal(sourceAccess, Expression.Constant(null, sourceAccess.Type)),
                Expression.Default(destinationType),
                Expression.Convert(mappingLogic, destinationType)
            );
        }

        /// <summary>
        /// Constrói uma expressão que aplica um valor substituto caso a origem seja nula.
        /// </summary>
        public static Expression ApplyNullSubstitute(Expression sourceAccess, object substituteValue, Type destinationType)
        {
            var substituteConstant = Expression.Constant(substituteValue, destinationType);

            return Expression.Condition(
                Expression.Equal(sourceAccess, Expression.Constant(null, sourceAccess.Type)),
                substituteConstant,
                Expression.Convert(sourceAccess, destinationType)
            );
        }

        private static bool IsTypeNullable(Type type)
        {
            return !type.IsValueType || type.IsNullable();
        }
    }
}