using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Collections;
using JMSAutoMapper.Internals;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Executor de alta performance que processa um MappingPlan.
    /// </summary>
    internal class CompiledMappingPlan
    {
        private readonly MappingPlan _plan;

        public CompiledMappingPlan(MappingPlan plan)
        {
            _plan = plan;
        }

        public object Map(object source, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            var destination = _plan.DestinationFactory?.Invoke() 
                ?? Activator.CreateInstance(_plan.DestinationType)!;

            foreach (var propMap in _plan.Properties)
            {
                if (propMap.Condition != null && !propMap.Condition(source)) continue;

                object? value = null;
                switch (propMap.Kind)
                {
                    case PropertyMapKind.Custom:
                        value = propMap.CustomResolver?.Invoke(source, mapper);
                        break;
                    case PropertyMapKind.Nested:
                        var rawValue = propMap.Getter?.Invoke(source);
                        if (rawValue != null)
                            value = propMap.NestedMapper?.Invoke(rawValue, mapper, mappedObjects);
                        break;
                    case PropertyMapKind.Collection:
                        var collSource = propMap.Getter?.Invoke(source) as IEnumerable;
                        if (collSource != null)
                        {
                            var itemType = propMap.DestinationType.GetCollectionItemType() ?? typeof(object);
                            value = CompiledCollectionMapper.Map(collSource, propMap.DestinationType, itemType, mapper, mappedObjects);
                        }
                        break;
                    default:
                        value = propMap.Getter?.Invoke(source);
                        break;
                }

                propMap.Setter?.Invoke(destination, value);
            }

            return destination;
        }
    }
}