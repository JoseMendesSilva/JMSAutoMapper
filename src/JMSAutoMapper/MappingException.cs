using System;
using System.Runtime.Serialization;

namespace JMSAutoMapper
{
    /// <summary>
    /// Exceção de mapeamento.
    /// Lançada quando ocorrem erros durante configuração ou execução.
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        /// <summary>Construtor padrão.</summary>
        public MappingException() { }

        /// <summary>Construtor com mensagem.</summary>
        public MappingException(string message) : base(message) { }

        /// <summary>Construtor com mensagem e inner exception.</summary>
        public MappingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>Construtor para serialização.</summary>
        [Obsolete("Formatter-based serialization is obsolete.", DiagnosticId = "SYSLIB0051")]
        protected MappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}