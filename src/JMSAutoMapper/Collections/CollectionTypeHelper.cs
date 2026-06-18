using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Utilitários para identificação e manipulação de tipos de coleção.
    /// </summary>
    internal static class CollectionTypeHelper
    {
        public static bool IsCollection(Type type) => 
            type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

        public static bool IsDictionary(Type type) => 
            typeof(IDictionary).IsAssignableFrom(type);

        public static Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            
            if (typeof(IDictionary).IsAssignableFrom(collectionType)) return null;

            if (collectionType.IsGenericType) return collectionType.GetGenericArguments().FirstOrDefault();

            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                var ienumerable = collectionType.GetInterface("IEnumerable`1");
                if (ienumerable != null) return ienumerable.GetGenericArguments().FirstOrDefault();
            }

            return null;
        }
    }
}