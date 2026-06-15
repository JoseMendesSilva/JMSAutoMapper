using JMSAutoMapper.Abstractions;
using System;

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

        public object Map(object source, IMapper mapper)
        {
            var destination = _plan.DestinationFactory?.Invoke() 
                ?? Activator.CreateInstance(_plan.DestinationType)!;

            foreach (var propMap in _plan.Properties)
            {
                if (propMap.Condition != null && !propMap.Condition(source)) continue;

                var value = propMap.CustomResolver != null 
                    ? propMap.CustomResolver(source, mapper) 
                    : propMap.Getter?.Invoke(source);

                propMap.Setter?.Invoke(destination, value);
            }

            return destination;
        }
    }
}