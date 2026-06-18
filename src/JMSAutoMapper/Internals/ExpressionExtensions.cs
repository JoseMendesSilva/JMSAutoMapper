using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Internals
{
    /// <summary>
    /// Extensões para facilitar o trabalho com Expression Trees dentro do motor de mapeamento.
    /// </summary>
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Extrai o MemberInfo de uma expressão lambda (ex: x => x.Nome).
        /// Lida automaticamente com conversões implícitas para object.
        /// </summary>
        public static MemberInfo GetMemberInfo(this LambdaExpression expression)
        {
            var body = expression.Body;

            // Remove conversões (boxing/unboxing) comuns em expressões do tipo Expression<Func<T, object>>
            if (body is UnaryExpression unary && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                body = unary.Operand;
            }

            if (body is MemberExpression member)
            {
                return member.Member;
            }

            if (body is MethodCallExpression methodCall && methodCall.Method.Name.StartsWith("get_"))
            {
                // Fallback para métodos de acesso gerados pelo compilador em certos cenários
                return methodCall.Method;
            }

            throw new ArgumentException($"A expressão '{expression}' não é um acesso a membro (propriedade ou campo) válido.");
        }

        /// <summary>
        /// Obtém o nome do membro referenciado na expressão.
        /// </summary>
        public static string GetMemberName(this LambdaExpression expression)
        {
            return expression.GetMemberInfo().Name;
        }
    }
}