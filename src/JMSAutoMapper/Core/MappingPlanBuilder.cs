using JMSAutoMapper.Configuration;
using JMSAutoMapper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JMSAutoMapper.Core
{
    internal class MappingPlanBuilder
    {
        private readonly MapperConfiguration _config;

        public MappingPlanBuilder(MapperConfiguration config)
        {
            _config = config;
        }

        public MappingPlan Build(Type sourceType, Type targetType)
        {
            var sourceMetadata = PropertyAccessorCache.GetMetadata(sourceType);
            var targetMetadata = PropertyAccessorCache.GetMetadata(targetType);
            var propertyMaps = new List<PropertyMap>();

            foreach (var targetProp in targetMetadata.PublicWritableProperties)
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProp.Name)))
                    continue;

                var key = (sourceType, targetType);
                
                // 1. Mapeamento Customizado (Resolvers)
                if (_config.CustomMappings.TryGetValue(key, out var customMaps) &&
                    customMaps.TryGetValue(targetProp.Name, out var mappingFunc))
                {
                    propertyMaps.Add(new PropertyMap
                    {
                        DestinationName = targetProp.Name,
                        DestinationType = targetProp.PropertyType,
                        CustomResolver = (src, m) => mappingFunc(src, m),
                        Setter = targetProp.Setter
                    });
                    continue;
                }

                // 2. Mapeamento por Nome ou Convenção
                string sourcePropName = _config.PropertyMappings.TryGetValue(key, out var propMaps) && 
                                        propMaps.TryGetValue(targetProp.Name, out var mappedName) 
                                        ? mappedName : _config.NamingConvention(targetProp.Name);

                if (sourceMetadata.PropertiesByName.TryGetValue(sourcePropName, out var sourceProp))
                {
                    propertyMaps.Add(new PropertyMap
                    {
                        SourceName = sourceProp.Name,
                        DestinationName = targetProp.Name,
                        SourceType = sourceProp.PropertyType,
                        DestinationType = targetProp.PropertyType,
                        Getter = sourceProp.Getter,
                        Setter = targetProp.Setter,
                        Condition = _config.ConditionalMappings.TryGetValue(key, out var conds) && 
                                    conds.TryGetValue(targetProp.Name, out var cond) ? cond : null
                    });
                }
            }

            return new MappingPlan(sourceType, targetType, propertyMaps.ToArray(), 
                () => Activator.CreateInstance(targetType)!, 
                false, false, false, false);
        }
    }
}