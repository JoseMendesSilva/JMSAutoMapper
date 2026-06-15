// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Reflection;

namespace JMSAutoMapper.Configuration
{
    

    /// <summary>
    /// Opções para configuração do mapper via DI.
    /// </summary>
    public class JMSMapperOptions
    {
        /// <summary>Habilitar diagnóstico.</summary>
        public bool EnableDiagnostics { get; set; } = true;

        /// <summary>Habilitar cache distribuído.</summary>
        public bool EnableDistributedCache { get; set; } = false;

        /// <summary>Habilitar cache estático para tipos marcados com [Cacheable].</summary>
        public bool EnableStaticCache { get; set; } = true;

        /// <summary>Tempo de expiração do cache (minutos).</summary>
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>Profundidade máxima de mapeamento.</summary>
        public int MaxDepth { get; set; } = 10;

        /// <summary>Se deve lançar exceção em erro de conversão.</summary>
        public bool ThrowOnConversionError { get; set; } = true;

        /// <summary>Política global para valores nulos em tipos de valor no destino.</summary>
        public NullValueMappingPolicy NullValueMappingStrategy { get; set; } = NullValueMappingPolicy.Throw;

        /// <summary>Convenção de nomenclatura.</summary>
        public Func<string, string>? NamingConvention { get; set; }

        /// <summary>Validar configuração após construção.</summary>
        public bool ValidateOnBuild { get; set; } = false;

        /// <summary>Assemblies para scan de perfis.</summary>
        public List<Assembly> AssembliesToScan { get; set; } = new();
    }

    

}
