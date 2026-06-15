// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"


// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Cache;

namespace JMSAutoMapper.Diagnostics
{
    /// <summary>
    /// Informações de diagnóstico do mapper.
    /// </summary>
    public class DiagnosticInfo
    {
        /// <summary>Número total de mapeamentos configurados.</summary>
        public int TotalMappings { get; set; }

        /// <summary>Número de mapeamentos em cache.</summary>
        public int CachedMappings { get; set; }

        /// <summary>Tempo médio de mapeamento (ms).</summary>
        public double AverageMapTimeMs { get; set; }

        /// <summary>Total de mapeamentos executados.</summary>
        public long TotalMapsExecuted { get; set; }

        /// <summary>Memória utilizada (bytes).</summary>
        public long MemoryUsedBytes { get; set; }

        /// <summary>Erros ocorridos.</summary>
        public int ErrorCount { get; set; }

        /// <summary>Últimos erros (máx 10).</summary>
        public List<string> RecentErrors { get; set; } = new();

        /// <summary>Mapeamentos lentos (&gt; 100ms) (máx 10).</summary>
        public List<string> SlowMappings { get; set; } = new();

        /// <summary>Estatísticas de cache.</summary>
        public CacheStatistics CacheStats { get; set; } = new();
    }

    
}
