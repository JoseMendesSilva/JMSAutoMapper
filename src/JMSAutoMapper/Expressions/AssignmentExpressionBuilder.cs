using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Expressions
{
    /// <summary>
    /// Utilitário responsável por construir expressões de atribuição entre membros de origem e destino,
    /// integrando a lógica de conversão e proteção contra nulos.
    /// </summary>
    internal static class AssignmentExpressionBuilder
    {
        /// <summary>
        /// Constrói uma expressão de atribuição: Destino.Propriedade = Converter(Origem.Propriedade).
        /// </summary>
        /// <param name="target">Expressão que representa o objeto de destino.</param>
        /// <param name="targetMember">O membro (propriedade ou campo) de destino.</param>
        /// <param name="sourceAccess">Expressão de acesso ao valor de origem.</param>
        /// <param name="mapperInstance">Expressão da instância do MapperBase (para chamar ConvertValue).</param>
        /// <param name="convertValueMethod">MethodInfo do método ConvertValue.</param>
        /// <returns>Uma expressão que representa a operação de atribuição completa.</returns>
        public static Expression Build(
            Expression target,
            MemberInfo targetMember,
            Expression sourceAccess,
            Expression mapperInstance,
            MethodInfo convertValueMethod)
        {
            var targetMemberAccess = Expression.MakeMemberAccess(target, targetMember);
            var targetType = targetMemberAccess.Type;

            // Lógica de conversão: (TargetType)mapper.ConvertValue(sourceAccess, typeof(TargetType))
            var conversionLogic = Expression.Convert(
                Expression.Call(
                    Expression.Convert(mapperInstance, typeof(MapperBase)),
                    convertValueMethod,
                    Expression.Convert(sourceAccess, typeof(object)),
                    Expression.Constant(targetType)
                ),
                targetType
            );

            // Aplica proteção contra nulo se necessário usando o NullGuardExpressionBuilder
            var guardedLogic = NullGuardExpressionBuilder.Guard(sourceAccess, conversionLogic, targetType);

            // Retorna a atribuição final
            return Expression.Assign(targetMemberAccess, guardedLogic);
        }
    }
}