#if false
using System;
using System.Globalization;
using JMSAutoMapper.Internals; // Add this for TypeExtensions

namespace JMSAutoMapper.Conversion
{
    /// <summary>
    /// Extensão da classe MapperBase especializada em conversões de e para tipos string.
    /// </summary>
    public abstract partial class MapperBase
    {
        /// <summary>
        /// Converte um valor para string aplicando formatações de cultura invariante 
        /// para garantir consistência em logs, chaves de cache e transporte de dados.
        /// </summary>
        /// <param name="value">O objeto a ser convertido.</param>
        /// <returns>String formatada ou null.</returns>
        protected string? ConvertToStringInternal(object? value)
        {
            if (value == null) return null;
            if (value is string s) return s;

            // Tratamento para Enums: retorna o nome da constante
            if (value.GetType().IsEnum) return value.ToString();

            // Tratamento para Data e Hora: Formato ISO 8601 (o mais seguro para mapeamento e persistência)
            if (value is DateTime dt) return dt.ToString("o", CultureInfo.InvariantCulture);
            if (value is DateTimeOffset dto) return dto.ToString("o", CultureInfo.InvariantCulture);

            // Suporte para IFormattable (Double, Decimal, float, etc)
            // Garante que decimais usem '.' independentemente da cultura do servidor/cliente
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        /// <summary>
        /// Converte uma string para o tipo de destino especificado (Parsing robusto).
        /// </summary>
        protected object? ConvertFromStringInternal(string? value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (targetType == typeof(string)) return value;
                // Para tipos de valor não anuláveis, retorna a instância padrão (ex: 0 para int)
                if (Nullable.GetUnderlyingType(targetType) != null || !targetType.IsValueType) return null;
                return Activator.CreateInstance(targetType);
            }

            var underlyingType = targetType.GetUnderlyingType();

            try
            {
                // Conversões de tipos comuns a partir de string com parsing seguro
                if (underlyingType == typeof(Guid)) return Guid.Parse(value);
                if (underlyingType == typeof(bool)) return bool.Parse(value);
                
                if (underlyingType == typeof(DateTime)) 
                    return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                
                if (underlyingType == typeof(DateTimeOffset)) 
                    return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                
                if (underlyingType == typeof(TimeSpan)) return TimeSpan.Parse(value, CultureInfo.InvariantCulture);

                if (underlyingType.IsEnum) return Enum.Parse(underlyingType, value, true);

                // Fallback para tipos numéricos e outros que implementam IConvertible
                return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _diagnostics.RecordError(ex, $"StringConversion: '{value}' to {targetType.Name}");
                if (_config.ThrowOnConversionError) throw new MappingException($"Falha ao converter string '{value}' para o tipo {targetType.Name}", ex);
                return null;
            }
        }
    }
}
#endif
