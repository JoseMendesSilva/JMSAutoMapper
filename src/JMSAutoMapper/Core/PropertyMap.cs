using JMSAutoMapper.Abstractions;
using System;

namespace JMSAutoMapper.Core
{
    internal enum PropertyMapKind
    {
        Simple,
        Nested,
        Collection,
        Custom
    }

    internal class PropertyMap
    {
        public string SourceName { get; init; } = string.Empty;
        public string DestinationName { get; init; } = string.Empty;
        public Type SourceType { get; init; } = default!;
        public Type DestinationType { get; init; } = default!;
        public Func<object, object?>? Getter { get; init; }
        public Action<object, object?>? Setter { get; init; }
        public Func<object, IMapper, object?>? CustomResolver { get; init; }
        public Func<object, bool>? Condition { get; init; }
        public bool Ignore { get; init; }
        public PropertyMapKind Kind { get; set; } = PropertyMapKind.Simple;
        public Func<object, IMapper, Dictionary<object, object>, object?>? NestedMapper { get; set; }
    }
}