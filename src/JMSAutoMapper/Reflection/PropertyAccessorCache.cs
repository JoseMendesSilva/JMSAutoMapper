using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JMSAutoMapper.Reflection
{
    public static class PropertyAccessorCache
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyMetadata>> _cache = new();

        public static IReadOnlyList<PropertyMetadata> GetProperties(Type type)
        {
            return _cache.GetOrAdd(type, t =>
            {
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new PropertyMetadata
                    {
                        Name = p.Name,
                        PropertyType = p.PropertyType,
                        PropertyInfo = p,
                        CanRead = p.CanRead,
                        CanWrite = p.CanWrite
                    }).ToList();
            });
        }
    }
}