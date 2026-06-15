using System;

namespace JMSAutoMapper.Core
{
    internal class MappingPlan
    {
        public Type SourceType { get; }
        public Type DestinationType { get; }
        public PropertyMap[] Properties { get; }
        public Func<object>? DestinationFactory { get; }
        public bool HasCustomMappings { get; }
        public bool HasResolvers { get; }
        public bool HasConditions { get; }
        public bool HasAsyncMembers { get; }

        public MappingPlan(Type sourceType, Type destinationType, PropertyMap[] properties, 
            Func<object>? destinationFactory, bool hasCustomMappings, bool hasResolvers, 
            bool hasConditions, bool hasAsyncMembers)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            Properties = properties;
            DestinationFactory = destinationFactory;
            HasCustomMappings = hasCustomMappings;
            HasResolvers = hasResolvers;
            HasConditions = hasConditions;
            HasAsyncMembers = hasAsyncMembers;
        }
    }
}