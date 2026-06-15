using JMSAutoMapper.Core;
using System;
using System.Collections.Concurrent;

namespace JMSAutoMapper.Cache
{
    internal static class MapperPlanCache
    {
        private static readonly ConcurrentDictionary<(Type Source, Type Target), MappingPlan> _cache = new();

        public static MappingPlan GetOrAdd((Type Source, Type Target) key, Func<(Type Source, Type Target), MappingPlan> factory)
        {
            return _cache.GetOrAdd(key, factory);
        }
    }
}