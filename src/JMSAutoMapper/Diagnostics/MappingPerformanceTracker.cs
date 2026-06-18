using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JMSAutoMapper.Diagnostics
{
    /// <summary>
    /// Componente especializado em rastrear e processar métricas de tempo e performance de mapeamento.
    /// </summary>
    public class MappingPerformanceTracker
    {
        private readonly MapperConfiguration _config;
        private long _totalMapsExecuted;
        private long _totalMapTimeMs;
        private readonly ConcurrentQueue<string> _slowMappings = new();

        /// <summary>
        /// Inicializa uma nova instância do rastreador com base na configuração do mapper.
        /// </summary>
        /// <param name="config">A configuração que dita se o diagnóstico está ativo.</param>
        public MappingPerformanceTracker(MapperConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Registra a duração de um mapeamento e identifica casos de baixa performance.
        /// </summary>
        /// <param name="sourceType">Nome do tipo de origem.</param>
        /// <param name="targetType">Nome do tipo de destino.</param>
        /// <param name="elapsedMs">Tempo gasto na operação.</param>
        public void RecordMap(string sourceType, string targetType, long elapsedMs)
        {
            if (!_config.EnableDiagnostics) return;

            Interlocked.Increment(ref _totalMapsExecuted);
            Interlocked.Add(ref _totalMapTimeMs, elapsedMs);

            // Mapeamentos que levam mais de 100ms são registrados como "lentos" para análise posterior.
            if (elapsedMs > 100)
            {
                _slowMappings.Enqueue($"{sourceType} -> {targetType}: {elapsedMs}ms às {DateTime.Now:HH:mm:ss}");
                
                // Mantém apenas os 10 registros mais recentes para evitar consumo excessivo de memória.
                while (_slowMappings.Count > 10) _slowMappings.TryDequeue(out _);
            }
        }

        public long TotalMapsExecuted => Interlocked.Read(ref _totalMapsExecuted);

        public long TotalMapTimeMs => Interlocked.Read(ref _totalMapTimeMs);

        public double AverageMapTimeMs => TotalMapsExecuted > 0 
            ? (double)TotalMapTimeMs / TotalMapsExecuted 
            : 0;

        public List<string> GetSlowMappings() => _slowMappings.ToList();
    }
}