using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace JMSAutoMapper.Reflection
{
    /// <summary>
    /// Armazena metadados de reflexão otimizados para um tipo.
    /// </summary>
    public class TypeMetadata
    {
        /// <summary>O tipo ao qual estes metadados se referem.</summary>
        public Type Type { get; }
        /// <summary>Lista de propriedades públicas que podem ser lidas.</summary>
        public PropertyMetadata[] PublicReadableProperties { get; }
        /// <summary>Lista de propriedades públicas que podem ser escritas.</summary>
        public PropertyMetadata[] PublicWritableProperties { get; }
        /// <summary>Dicionário para busca rápida de propriedades por nome.</summary>
        public IReadOnlyDictionary<string, PropertyMetadata> PropertiesByName { get; }

        public TypeMetadata(Type type, IEnumerable<PropertyMetadata> properties)
        {
            Type = type;
            var props = properties.ToArray();
            PublicReadableProperties = props.Where(p => p.CanRead).ToArray();
            PublicWritableProperties = props.Where(p => p.CanWrite).ToArray();
            PropertiesByName = props.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }
    }
}