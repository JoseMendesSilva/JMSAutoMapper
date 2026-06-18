using System;

namespace JMSAutoMapper.Diagnostics
{
    /// <summary>
    /// Define as categorias de eventos capturáveis pelo motor de diagnóstico do JMSAutoMapper.
    /// </summary>
    public enum DiagnosticEventType
    {
        Information,
        Warning,
        Error,
        SlowMapping,
        CacheHit,
        CacheMiss
    }

    /// <summary>
    /// Estrutura de dados que encapsula os detalhes de um evento de diagnóstico ocorrido durante o ciclo de vida de um mapeamento.
    /// </summary>
    public class MappingDiagnosticEvent
    {
        /// <summary>Instante em que o evento foi gerado.</summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>O tipo/categoria do evento registrado.</summary>
        public DiagnosticEventType Type { get; }

        /// <summary>O nome do tipo de origem envolvido na operação.</summary>
        public string SourceType { get; }

        /// <summary>O nome do tipo de destino envolvido na operação.</summary>
        public string TargetType { get; }

        /// <summary>Mensagem descritiva do evento.</summary>
        public string Message { get; }

        /// <summary>Tempo total de execução em milissegundos, se aplicável.</summary>
        public long ElapsedMilliseconds { get; }

        /// <summary>A exceção capturada, caso o evento represente um erro.</summary>
        public Exception? Exception { get; }

        public MappingDiagnosticEvent(DiagnosticEventType type, string sourceType, string targetType, string message, long elapsedMilliseconds = 0, Exception? exception = null)
        {
            Type = type;
            SourceType = sourceType;
            TargetType = targetType;
            Message = message;
            ElapsedMilliseconds = elapsedMilliseconds;
            Exception = exception;
        }
    }
}