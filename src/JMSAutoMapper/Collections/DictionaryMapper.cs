using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JMSAutoMapper.Collections
{
    internal static class DictionaryMapper
    {
        public static object Map(IDictionary source, Type targetType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            var args = targetType.GetGenericArguments();
            var keyType = args[0];
            var valType = args[1];
            var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valType))!;

            // Obtém o CompiledMappingPlan para o tipo de valor do item uma única vez
            var sourceValueType = CollectionTypeHelper.GetCollectionItemType(source.GetType()) ?? typeof(object); // Assumindo que a origem é IDictionary<SKey, SVal>
            var itemPlan = MapperPlanCache.GetOrAdd((sourceValueType, valType), k => new MappingPlanBuilder(((JMSMapper)mapper)._config).Build(k.Source, k.Target));
            var compiledItemMapper = new CompiledMappingPlan(itemPlan);

            foreach (DictionaryEntry entry in source)
            {
                // Chaves de dicionário geralmente são tipos simples, mas mapeamos o valor como tipo complexo
                var key = entry.Key; 
                var val = entry.Value != null 
                    ? compiledItemMapper.Map(entry.Value, (JMSMapper)mapper, mappedObjects) 
                    : null;

                if (val != null) result.Add(key, val);
            }
            return result;
        }
    }
}