using System;
using System.Reflection;

namespace JMSAutoMapper.Reflection
{
    public class PropertyMetadata
    {
        public required string Name { get; init; }
        public required Type PropertyType { get; init; }
        public required PropertyInfo PropertyInfo { get; init; }
        public bool CanRead { get; init; }
        public bool CanWrite { get; init; }
        public Func<object, object?>? Getter { get; set; }
        public Action<object, object?>? Setter { get; set; }
    }
}