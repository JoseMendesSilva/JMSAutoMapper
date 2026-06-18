#if false
using System;
using System.Globalization;
using JMSAutoMapper.Internals; // Add this for TypeExtensions

namespace JMSAutoMapper.Conversion
{
    /// <summary>
    /// Extensão da classe MapperBase responsável por conversões envolvendo tipos de data e hora.
    /// </summary>
    public abstract partial class MapperBase
    {
        /// <summary>
        /// Converte um valor para o tipo temporal especificado (DateTime, DateTimeOffset ou TimeSpan).
        /// </summary>
        protected object? ConvertToTemporalInternal(object value, Type targetType)
        {
            var underlyingTargetType = targetType.GetUnderlyingType();
            
            try
            {
                // Conversão para DateTime
                if (underlyingTargetType == typeof(DateTime))
                {
                    if (value is DateTime dt) return dt;
                    if (value is DateTimeOffset dto) return dto.DateTime;
                    if (value is string s) return DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    if (value is long ticks) return new DateTime(ticks);
                    
                    return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                }

                // Conversão para DateTimeOffset (Recomendado para evitar problemas de fuso horário)
                if (underlyingTargetType == typeof(DateTimeOffset))
                {
                    if (value is DateTimeOffset dto) return dto;
                    if (value is DateTime dt) return new DateTimeOffset(dt);
                    if (value is string s) return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    if (value is long ticks) return new DateTimeOffset(ticks, TimeSpan.Zero);

                    return DateTimeOffset.Parse(value.ToString()!, CultureInfo.InvariantCulture);
                }

                // Conversão para TimeSpan
                if (underlyingTargetType == typeof(TimeSpan))
                {
                    if (value is TimeSpan ts) return ts;
                    if (value is string s) return TimeSpan.Parse(s, CultureInfo.InvariantCulture);
                    if (value is long ticks) return new TimeSpan(ticks);
                    if (value is double days) return TimeSpan.FromDays(days);
                    
                    return TimeSpan.Parse(value.ToString()!, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                _diagnostics.RecordError(ex, $"TemporalConversion: {value.GetType().Name} to {targetType.Name}");
                
                if (_config.ThrowOnConversionError)
                    throw new MappingException($"Falha ao converter valor de '{value.GetType().Name}' para o tipo temporal '{targetType.Name}'", ex);
            }

            return null;
        }

        /// <summary>
        /// Verifica se o tipo alvo é um dos tipos temporais suportados.
        /// </summary>
        protected static bool IsTemporalType(Type type)
        {
            var underlyingType = type.GetUnderlyingType();
            return underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset) || underlyingType == typeof(TimeSpan);
        }
    }
}
#endif
