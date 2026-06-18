using System;
using System.Collections.Generic;

namespace JMSAutoMapper
{
    public class DiagnosticInfo
    {
        public int TotalMappings { get; set; }
        public int CachedMappings { get; set; }
        public double AverageMapTimeMs { get; set; }
        public long TotalMapsExecuted { get; set; }
        public long MemoryUsedBytes { get; set; }
        public int ErrorCount { get; set; }
        public List<string> RecentErrors { get; set; } = new();
        public List<string> SlowMappings { get; set; } = new();
        public CacheStatistics CacheStats { get; set; } = new();
    
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRate => CacheHits + CacheMisses > 0 
            ? (double)CacheHits / (CacheHits + CacheMisses) 
            : 0;
        public double AverageTimeSavedMs { get; set; }
    }
}