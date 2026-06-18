using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JMSAutoMapper.Internals; // For TypeExtensions

namespace JMSAutoMapper.Validation
{
    /// <summary>
    /// Validador responsável por identificar propriedades não mapeadas entre tipos de origem e destino.
    /// </summary>
    internal class MissingMemberValidator
    {
        private readonly MapperConfiguration _configuration;

        public MissingMemberValidator(MapperConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Valida os mapeamentos configurados e retorna uma lista de mensagens de erro/aviso
        /// para propriedades de destino não mapeadas.
        /// </summary>
        /// <returns>Uma lista de strings contendo mensagens de erro ou aviso.</returns>
        public List<string> Validate()
        {
            var messages = new List<string>();

            var allTypeMaps = _configuration.CustomMappings.Keys
                .Concat(_configuration.PropertyMappings.Keys)
                .Concat(_configuration.CustomMappingExpressions.Keys)
                .Concat(_configuration.AsyncCustomMappings.Keys)
                .Distinct()
                .ToList();

            foreach (var typeMap in allTypeMaps)
            {
                var sourceType = typeMap.Source;
                var targetType = typeMap.Target;

                var destProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToList();

                var mappedProperties = new HashSet<string>();

                if (_configuration.CustomMappings.TryGetValue(typeMap, out var customMaps))
                    mappedProperties.UnionWith(customMaps.Keys);
                if (_configuration.PropertyMappings.TryGetValue(typeMap, out var propMaps))
                    mappedProperties.UnionWith(propMaps.Keys);
                if (_configuration.CustomMappingExpressions.TryGetValue(typeMap, out var exprMaps))
                    mappedProperties.UnionWith(exprMaps.Keys);
                if (_configuration.AsyncCustomMappings.TryGetValue(typeMap, out var asyncMaps))
                    mappedProperties.UnionWith(asyncMaps.Keys);

                var ignoredForType = _configuration.IgnoredProperties.Keys
                    .Where(ip => ip.Source == sourceType && ip.Target == targetType)
                    .Select(ip => ip.PropertyName);
                mappedProperties.UnionWith(ignoredForType);

                if (_configuration.CustomConstructors.ContainsKey(typeMap) || _configuration.ConstructorSelection.ContainsKey(typeMap))
                {
                    continue;
                }

                var unmapped = destProperties.Where(p => !mappedProperties.Contains(p.Name)).ToList();

                if (unmapped.Any())
                {
                    var message = $"Propriedades não mapeadas para {sourceType.Name} -> {targetType.Name}: {string.Join(", ", unmapped.Select(p => p.Name))}";
                    messages.Add(_configuration.ValidateMemberList == MemberListType.Destination ? $"ERROR: {message}" : $"WARNING: {message}");
                }
            }
            return messages;
        }
    }
}