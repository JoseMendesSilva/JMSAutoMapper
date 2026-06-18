#if false
using System;

using JMSAutoMapper.Internals;
namespace JMSAutoMapper.Conversion
{
    /// <summary>
    /// Extensão da classe MapperBase responsável pelo tratamento de tipos Nullable.
    /// Extensão da classe MapperBase responsável pelo tratamento de tipos Nullable.
    /// </summary>
    public abstract partial class MapperBase
    {
        /// <summary>
        /// Determina se o tipo fornecido é um Nullable&lt;T&gt;.
        /// </summary>
        /// <param name="type">O tipo a ser verificado.</param>
        /// <returns>True se for Nullable, caso contrário False.</returns>
        protected static bool IsNullableType(Type type) // Renamed to avoid conflict with extension method
        {
            return type.IsNullable(); // Using extension method
        }

        /// <summary>
        /// Extrai o tipo interno de um Nullable, ou retorna o próprio tipo se não for Nullable.
        /// </summary>
        /// <param name="type">O tipo de entrada.</param>
        /// <returns>O tipo subjacente.</returns>
        protected static Type GetUnderlyingType(Type type) // Renamed to avoid conflict with extension method
        {
            return type.GetUnderlyingType(); // Using extension method
        }

        /// <summary>
        /// Resolve o valor padrão para um tipo de destino quando o valor de origem é nulo.
        /// Garante que tipos de valor não-anuláveis recebam seu valor padrão (ex: 0, false) 
        /// enquanto tipos de referência e Nullables permanecem nulos.
        /// </summary>
        /// <param name="targetType">O tipo para o qual se está mapeando.</param>
        /// <returns>Null se o destino permitir, ou o valor padrão do tipo de valor.</returns>
        protected object? ResolveNullValue(Type targetType)
        {
            // Se o destino é uma classe ou Nullable<T>, o nulo é um valor de destino válido.
            if (!targetType.IsValueType || targetType.IsNullable()) // Using extension method
            {
                return null;
            }

            // Para tipos de valor puros (int, DateTime, etc), retornamos a instância padrão (0, MinValue).
            return Activator.CreateInstance(targetType);
        }
    }
}
#endif
