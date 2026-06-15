// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

namespace JMSAutoMapper.Cache
{
    /// <summary>
    /// Estatísticas de cache.
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>Número de hits no cache.</summary>
        public long CacheHits { get; set; }

        /// <summary>Número de misses no cache.</summary>
        public long CacheMisses { get; set; }

        /// <summary>Taxa de acerto do cache (0-1).</summary>
        public double HitRate => CacheHits + CacheMisses > 0
            ? (double)CacheHits / (CacheHits + CacheMisses)
            : 0;

        /// <summary>Tempo médio economizado pelo cache (ms).</summary>
        public double AverageTimeSavedMs { get; set; }
    }

}
