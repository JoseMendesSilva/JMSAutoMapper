using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JMSAutoMapper.Abstractions;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Mapeador de coleções otimizado que utiliza planos de execução em cache.
    /// </summary>
    internal static class CompiledCollectionMapper
    {
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _collectionMappers = new();

        public static object? Map(IEnumerable? source, Type targetCollectionType, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return null;

            var sourceType = source.GetType();
            var key = (sourceType, targetCollectionType);

            var collectionMapper = _collectionMappers.GetOrAdd(key, k => BuildCollectionMapper(k.Item1, k.Item2, itemType));

            return collectionMapper.DynamicInvoke(source, mapper, mappedObjects);
        }

        private static Delegate BuildCollectionMapper(Type sourceType, Type targetType, Type itemType)
        {
            return new Func<IEnumerable, IMapper, Dictionary<object, object>, object>((src, m, cache) => 
            {
                var listType = typeof(List<>).MakeGenericType(itemType);
                var mappedList = (IList)Activator.CreateInstance(listType)!;
                foreach (var item in src)
                {
                    if (item == null) continue;
                    var mappedItem = m.Map(item, item.GetType(), itemType);
                    if (mappedItem != null) mappedList.Add(mappedItem);
                }
                return mappedList;
            });
        }
    }
}