using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JMSAutoMapper.Abstractions;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Mapeador especializado para coleções imutáveis (.NET Immutable Collections).
    /// </summary>
    internal static class ImmutableCollectionMapper
    {
        public static object Map(IEnumerable source, Type targetType, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            // Delega para CompiledCollectionMapper para mapeamento otimizado de itens
            var sourceItemType = CollectionTypeHelper.GetCollectionItemType(source.GetType()) ?? typeof(object);
            var list = (IList)CompiledCollectionMapper.Map(source, typeof(List<>).MakeGenericType(itemType), itemType, mapper, mappedObjects)!;
            var def = targetType.GetGenericTypeDefinition();

            if (def == typeof(ImmutableList<>)) return CreateImmutable("ImmutableList", itemType, list);
            if (def == typeof(ImmutableArray<>)) return CreateImmutable("ImmutableArray", itemType, list);
            if (def == typeof(ImmutableQueue<>)) return CreateImmutable("ImmutableQueue", itemType, list);
            if (def == typeof(ImmutableStack<>)) return CreateImmutable("ImmutableStack", itemType, list);

            return list;
        }

        private static object CreateImmutable(string typeName, Type itemType, object list) =>
            typeof(ImmutableList).Assembly.GetType($"System.Collections.Immutable.{typeName}")!
                .GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1).MakeGenericMethod(itemType).Invoke(null, new[] { list })!;
    }
}