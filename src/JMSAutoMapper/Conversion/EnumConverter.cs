#if false
using System;

namespace JMSAutoMapper.Conversion
{
    /// <summary>
    /// Extensão da classe MapperBase responsável por conversões de e para tipos Enum.
    /// </summary>
    public abstract partial class MapperBase
    {
        /// <summary>
        /// Converte um valor para o tipo Enum especificado.
        /// </summary>
        /// <param name="value">O valor a ser convertido.</param>
        /// <param name="enumType">O tipo Enum de destino.</param>
        /// <returns>O valor convertido para o tipo Enum.</returns>
        /// <exception cref="InvalidOperationException">Lançada se a conversão não for possível.</exception>
        protected object ConvertToEnum(object value, Type enumType)
        {
            if (value.GetType().IsEnum) return Enum.ToObject(enumType, (int)value);
            if (value is string stringValue) return Enum.Parse(enumType, stringValue, true);
            
            if (value is int || value is short || value is byte || value is long || 
                value is uint || value is ushort || value is sbyte || value is ulong)
                return Enum.ToObject(enumType, value);
            
            if (value is decimal || value is double || value is float)
                return Enum.ToObject(enumType, Convert.ToInt32(value));

            throw new InvalidOperationException($"Não é possível converter {value.GetType().Name} para {enumType.Name}");
        }
    }
}
#endif
