using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JMSAutoMapper.Internals;
namespace JMSAutoMapper.Expressions
{
    /// <summary>
    /// Utilitário responsável por construir expressões de "Flattening" (Achatamento).
    /// Esta técnica permite que propriedades aninhadas na origem sejam mapeadas automaticamente para 
    /// propriedades planas no destino (ex: Pedido.Cliente.Nome -> PedidoClienteNome).
    /// </summary>
    internal static class FlatteningExpressionBuilder
    {
        /// <summary>
        /// Tenta resolver uma expressão de acesso a membros navegando recursivamente na árvore de propriedades da origem.
        /// </summary>
        /// <param name="source">Expressão base (parâmetro de origem).</param>
        /// <param name="destinationMemberName">O nome da propriedade no objeto de destino.</param>
        /// <returns>A expressão de acesso resolvida ou null se não houver correspondência.</returns>
        public static Expression? Build(Expression source, string destinationMemberName)
        {
            if (source == null || string.IsNullOrWhiteSpace(destinationMemberName)) return null;
            return ResolveRecursive(source, destinationMemberName);
        }

        private static Expression? ResolveRecursive(Expression current, string remainingName)
        {
            var type = current.Type;
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 1. Tenta correspondência direta (exact match)
            var directMatch = properties.FirstOrDefault(p => string.Equals(p.Name, remainingName, StringComparison.OrdinalIgnoreCase));
            if (directMatch != null)
            {
                return Expression.Property(current, directMatch);
            }

            // 2. Tenta encontrar prefixos para navegação aninhada
            // Ordenamos por tamanho descendente para pegar o prefixo mais longo/específico primeiro
            foreach (var prop in properties.OrderByDescending(p => p.Name.Length))
            {
                if (remainingName.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase) && IsComplexType(prop.PropertyType))
                {
                    var nestedAccess = Expression.Property(current, prop);
                    var nextPart = remainingName.Substring(prop.Name.Length);

                    if (string.IsNullOrEmpty(nextPart)) continue; // Evita loop se o nome for igual ao prefixo

                    var result = ResolveRecursive(nestedAccess, nextPart);
                    if (result != null) return result;
                }
            }

            return null;
        }

        private static bool IsComplexType(Type type)
        {
            return type.IsComplex();
        }
    }
}