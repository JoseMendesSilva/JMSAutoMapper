#if false
using System;
using JMSAutoMapper.Internals;

namespace JMSAutoMapper.Conversion
{
    /// <summary>
    /// Extensão da classe MapperBase contendo o motor de conversão de tipos.
    /// Extensão da classe MapperBase contendo o motor de conversão de tipos.
    /// </summary>
    public abstract partial class MapperBase
    {
        /// <summary>
        /// Converte um valor para o tipo de destino especificado, lidando com conversões numéricas complexas e tipos anuláveis.
        /// </summary>
        /// <param name="value">O valor a ser convertido.</param>
        /// <param name="targetType">O tipo para o qual o valor deve ser convertido.</param>
        /// <returns>O valor convertido ou o valor padrão do tipo se a conversão falhar (dependendo da configuração).</returns>
        protected object? ConvertValue(object? value, Type targetType)
        {
            try
            {
                if (value == null) 
                    return ResolveNullValue(targetType);

                var underlyingType = targetType.GetUnderlyingType();
                var valueType = value.GetType();
                var underlyingValueType = valueType.GetUnderlyingType();

                if (underlyingType == underlyingValueType) return value;

                // Conversões Numéricas de Alta Precisão (suporte solicitado na versão 100% funcional)
                if (underlyingType == typeof(decimal))
                {
                    if (underlyingValueType == typeof(double)) return Convert.ToDecimal((double)value);
                    if (underlyingValueType == typeof(float)) return Convert.ToDecimal((float)value);
                    if (underlyingValueType == typeof(int) || underlyingValueType == typeof(long) || 
                        underlyingValueType == typeof(short) || underlyingValueType == typeof(byte))
                        return Convert.ToDecimal(value);
                }

                if (underlyingValueType == typeof(decimal))
                {
                    if (underlyingType == typeof(double)) return Convert.ToDouble((decimal)value);
                    if (underlyingType == typeof(float)) return Convert.ToSingle((decimal)value);
                    if (underlyingType == typeof(int) || underlyingType == typeof(long) || 
                        underlyingType == typeof(short) || underlyingType == typeof(byte))
                        return Convert.ChangeType(Convert.ToInt32((decimal)value), underlyingType);
                }

                if (underlyingType == typeof(double) && underlyingValueType == typeof(float)) return Convert.ToDouble((float)value);
                if (underlyingType == typeof(float) && underlyingValueType == typeof(double)) return Convert.ToSingle((double)value);

                // Conversão para String
                if (underlyingType == typeof(string)) return value.ToString()!;

                // Conversão de Enums
                if (underlyingType.IsEnum) return ConvertToEnum(value, underlyingType);
                if (underlyingValueType.IsEnum && underlyingType == typeof(string)) return value.ToString()!;

                // Conversão de Datas e Tempos (DateTime, DateTimeOffset, TimeSpan)
                if (IsTemporalType(underlyingType))
                {
                    return ConvertToTemporalInternal(value, underlyingType);
                }

                // Tentativa de conversão genérica
                try
                {
                    return Convert.ChangeType(value, underlyingType);
                }
                catch (InvalidCastException)
                {
                    var stringValue = value.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        return Convert.ChangeType(stringValue, underlyingType);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);
                _diagnostics.RecordError(ex, $"ConvertValue {value?.GetType().Name}->{targetType.Name}");

                if (_config.ThrowOnConversionError)
                    throw new MappingException($"Erro ao converter valor de '{value?.GetType().Name}' para '{targetType.Name}'", ex);

                return null;
            }
        }

        // O método IsSimpleType será movido para TypeExtensions.cs e usado como extensão.
        // Este método será removido ou adaptado para usar a extensão.
        protected bool IsSimpleTypeInternal(Type type)
        {
            return type.IsSimple();
        }
    }
}
#endif
