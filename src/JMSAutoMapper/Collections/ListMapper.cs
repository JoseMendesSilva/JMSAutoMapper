using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JMSAutoMapper.Collections
{
    internal static class ListMapper
    {
        public static IList MapToList(IEnumerable source, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            // Delega para CompiledCollectionMapper para mapeamento otimizado de itens
            // Isso retornará um IList (especificamente List<TItem>)
            var sourceItemType = CollectionTypeHelper.GetCollectionItemType(source.GetType()) ?? typeof(object);
            return (IList)CompiledCollectionMapper.Map(source, typeof(List<>).MakeGenericType(itemType), itemType, mapper, mappedObjects)!;
        }

        public static object Map(IEnumerable source, Type targetType, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            var list = MapToList(source, itemType, mapper, mappedObjects);
            
            if (targetType.IsAssignableFrom(list.GetType())) return list;
            
            var constructor = targetType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType) });
            if (constructor != null) return constructor.Invoke(new object[] { list });

            return list;
        }
    }
}