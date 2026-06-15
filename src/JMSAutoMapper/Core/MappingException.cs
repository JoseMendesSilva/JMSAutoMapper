// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Runtime.Serialization;

namespace JMSAutoMapper.Core
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
        protected MappingException(SerializationInfo info, StreamingContext context) : base() { }
    }

    

}
