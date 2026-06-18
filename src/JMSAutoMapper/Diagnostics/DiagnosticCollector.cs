using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JMSAutoMapper.Diagnostics
{
    /// <summary>
    /// Coletor central de diagnósticos que consolida métricas de performance, 
    /// erros e estatísticas de cache utilizando eventos estruturados.
    /// </summary>
    public class DiagnosticCollector
    {
        private readonly MapperConfiguration _config;
        private readonly ConcurrentQueue<MappingDiagnosticEvent> _eventHistory = new();
        private const int MaxHistoryItems = 100;

        private long _totalMapsExecuted;
        private long _totalMapTimeMs;
        private long _cacheHits;
        private long _cacheMisses;
        private long _totalTimeSavedByCache;

        public DiagnosticCollector(MapperConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Registra a execução de um mapeamento e identifica lentidões.
        /// </summary>
        public void RecordMap(string sourceType, string targetType, long elapsedMs)
        {
            if (!_config.EnableDiagnostics) return;

            Interlocked.Increment(ref _totalMapsExecuted);
            Interlocked.Add(ref _totalMapTimeMs, elapsedMs);

            if (elapsedMs > 100)
            {
                AddEvent(new MappingDiagnosticEvent(
                    DiagnosticEventType.SlowMapping,
                    sourceType,
                    targetType,
                    $"Mapeamento lento detectado: {elapsedMs}ms",
                    elapsedMs));
            }
        }

        /// <summary>
        /// Registra um acerto (hit) no cache e calcula o tempo economizado.
        /// </summary>
        public void RecordCacheHit(string sourceType, string targetType, long timeSavedMs)
        {
            if (!_config.EnableDiagnostics) return;

            Interlocked.Increment(ref _cacheHits);
            Interlocked.Add(ref _totalTimeSavedByCache, timeSavedMs);

            AddEvent(new MappingDiagnosticEvent(
                DiagnosticEventType.CacheHit, sourceType, targetType, 
                $"Valor recuperado do cache. Economia: {timeSavedMs}ms", timeSavedMs));
        }

        /// <summary>
        /// Registra uma falha (miss) no cache.
        /// </summary>
        public void RecordCacheMiss(string sourceType, string targetType)
        {
            if (!_config.EnableDiagnostics) return;
            Interlocked.Increment(ref _cacheMisses);
            AddEvent(new MappingDiagnosticEvent(DiagnosticEventType.CacheMiss, sourceType, targetType, "Cache miss."));
        }

        /// <summary>
        /// Registra um erro ocorrido durante o mapeamento com contexto completo.
        /// </summary>
        public void RecordError(Exception ex, string context, string sourceType = "N/A", string targetType = "N/A")
        {
            if (!_config.EnableDiagnostics) return;
            AddEvent(new MappingDiagnosticEvent(DiagnosticEventType.Error, sourceType, targetType, $"{context}: {ex.Message}", 0, ex));
        }

        private void AddEvent(MappingDiagnosticEvent diagEvent)
        {
            _eventHistory.Enqueue(diagEvent);
            while (_eventHistory.Count > MaxHistoryItems) _eventHistory.TryDequeue(out _);
        }

        /// <summary>
        /// Gera o objeto DiagnosticInfo consolidado para exportação ou visualização.
        /// </summary>
        public DiagnosticInfo GetInfo(long memoryUsedBytes)
        {
            var events = _eventHistory.ToList();
            var totalMaps = Interlocked.Read(ref _totalMapsExecuted);
            var totalHits = Interlocked.Read(ref _cacheHits);

            return new DiagnosticInfo
            {
                TotalMapsExecuted = totalMaps,
                AverageMapTimeMs = totalMaps > 0 ? (double)Interlocked.Read(ref _totalMapTimeMs) / totalMaps : 0,
                MemoryUsedBytes = memoryUsedBytes,
                ErrorCount = events.Count(e => e.Type == DiagnosticEventType.Error),
                RecentErrors = events.Where(e => e.Type == DiagnosticEventType.Error).OrderByDescending(e => e.Timestamp).Take(10).Select(e => e.Message).ToList(),
                SlowMappings = events.Where(e => e.Type == DiagnosticEventType.SlowMapping).OrderByDescending(e => e.Timestamp).Take(10).Select(e => $"{e.SourceType}->{e.TargetType}: {e.ElapsedMilliseconds}ms").ToList(),
                CacheStats = new CacheStatistics {
                    CacheHits = totalHits,
                    CacheMisses = Interlocked.Read(ref _cacheMisses),
                    AverageTimeSavedMs = totalHits > 0 ? (double)Interlocked.Read(ref _totalTimeSavedByCache) / totalHits : 0
                }
            };
        }
    }
}