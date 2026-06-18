using JMSAutoMapper.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Coordenador principal para mapeamento de coleções.
    /// </summary>
    internal static class CollectionMapper
    {
        public static object? Map(IEnumerable? source, Type targetType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return null;

            if (CollectionTypeHelper.IsDictionary(targetType))
            {
                return DictionaryMapper.Map((IDictionary)source, targetType, mapper, mappedObjects);
            }

            var itemType = CollectionTypeHelper.GetCollectionItemType(targetType) ?? typeof(object);

            if (targetType.IsArray)
            {
                return ArrayMapper.Map(source, itemType, mapper, mappedObjects);
            }

            if (targetType.IsGenericType && targetType.Namespace?.StartsWith("System.Collections.Immutable") == true)
            {
                return ImmutableCollectionMapper.Map(source, targetType, itemType, mapper, mappedObjects);
            }

            // Fallback para Listas (List<T>, IList<T>, IEnumerable<T>)
            return ListMapper.Map(source, targetType, itemType, mapper, mappedObjects);
        }
    }
}