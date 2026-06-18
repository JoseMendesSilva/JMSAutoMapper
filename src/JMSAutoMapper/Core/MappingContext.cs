using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JMSAutoMapper.Configuration;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Mantém o estado de uma operação de mapeamento em execução, 
    /// sendo responsável pelo cache de instâncias para evitar recursão infinita em referências circulares.
    /// </summary>
    public class MappingContext
    {
        private readonly Dictionary<object, object> _instanceCache;
        private readonly ConcurrentDictionary<object, object>? _asyncInstanceCache;
        private readonly MapperConfiguration _configuration;

        /// <summary>
        /// Configuração utilizada durante o ciclo de vida deste mapeamento.
        /// </summary>
        public MapperConfiguration Configuration => _configuration;

        /// <summary>
        /// Inicializa um contexto para mapeamentos síncronos.
        /// </summary>
        public MappingContext(MapperConfiguration configuration, Dictionary<object, object> instanceCache)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _instanceCache = instanceCache ?? throw new ArgumentNullException(nameof(instanceCache));
        }

        /// <summary>
        /// Inicializa um contexto para mapeamentos assíncronos.
        /// </summary>
        public MappingContext(MapperConfiguration configuration, ConcurrentDictionary<object, object> asyncInstanceCache)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _instanceCache = new Dictionary<object, object>(); // Não utilizado no modo assíncrono
            _asyncInstanceCache = asyncInstanceCache ?? throw new ArgumentNullException(nameof(asyncInstanceCache));
        }

        /// <summary>
        /// Tenta recuperar uma instância já mapeada do cache de instâncias.
        /// </summary>
        public bool TryGetMappedInstance(object source, out object? destination)
        {
            if (_asyncInstanceCache != null)
            {
                return _asyncInstanceCache.TryGetValue(source, out destination);
            }
            return _instanceCache.TryGetValue(source, out destination);
        }

        /// <summary>
        /// Registra uma nova instância mapeada no cache do contexto.
        /// </summary>
        public void RegisterInstance(object source, object destination)
        {
            if (_asyncInstanceCache != null)
            {
                _asyncInstanceCache.TryAdd(source, destination);
            }
            else
            {
                _instanceCache[source] = destination;
            }
        }
    }
}