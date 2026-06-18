using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JMSAutoMapper.Reflection;
using JMSAutoMapper.Core;

namespace JMSAutoMapper.Cache
{
    /// <summary>
    /// Serviço centralizado para gerenciamento de todos os caches internos do Mapper.
    /// </summary>
    internal class MapperCacheService
    {
        /// <summary>Cache de metadados de tipos (propriedades, acessores compilados).</summary>
        public ConcurrentDictionary<Type, TypeMetadata> TypeMetadata { get; } = new();

        /// <summary>Cache de planos de mapeamento compilados.</summary>
        public ConcurrentDictionary<(Type Source, Type Target), object> CompiledMappers { get; } = new();

        /// <summary>Cache de mapeadores assíncronos.</summary>
        public ConcurrentDictionary<(Type Source, Type Target), object> CompiledAsyncMappers { get; } = new();

        /// <summary>Cache de instâncias para objetos imutáveis ou estáticos.</summary>
        public ConcurrentDictionary<string, Task<object>> StaticCache { get; } = new();

        /// <summary>Sinalizadores para sincronização de criação de cache.</summary>
        public ConcurrentDictionary<string, SemaphoreSlim> CacheLocks { get; } = new();

        public void Clear()
        {
            TypeMetadata.Clear();
            CompiledMappers.Clear();
            CompiledAsyncMappers.Clear();
            StaticCache.Clear();
            CacheLocks.Clear();
        }
    }
}