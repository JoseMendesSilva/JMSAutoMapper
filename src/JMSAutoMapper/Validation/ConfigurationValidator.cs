// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Reflection;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;

namespace JMSAutoMapper.Validation
{
    /// <summary>
    /// Validador de configuração.
    /// Verifica erros comuns e problemas de performance.
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly MapperConfiguration _configuration;
        private readonly List<string> _warnings = new();
        private readonly List<string> _errors = new();

        /// <summary>Construtor.</summary>
        public ConfigurationValidator(MapperConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>Valida a configuração.</summary>
        public void Validate()
        {
            ValidateMappings();
            ValidateCircularReferences();
            ValidatePerformance();
            ValidateTypeCompatibility();
            ThrowConfigurationExceptionsIfAny();
        }

        private void ValidateMappings()
        {
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

                // Pega todas as propriedades do destino que podem ser escritas
                var destProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToList();

                // Propriedades que são mapeadas via configurações
                var mappedProperties = new HashSet<string>();

                // Mapeamentos customizados síncronos
                if (_configuration.CustomMappings.TryGetValue(typeMap, out var customMaps))
                    mappedProperties.UnionWith(customMaps.Keys);

                // Mapeamentos de propriedade por nome
                if (_configuration.PropertyMappings.TryGetValue(typeMap, out var propMaps))
                    mappedProperties.UnionWith(propMaps.Keys);

                // Mapeamentos por expressão
                if (_configuration.CustomMappingExpressions.TryGetValue(typeMap, out var exprMaps))
                    mappedProperties.UnionWith(exprMaps.Keys);

                // Mapeamentos assíncronos
                if (_configuration.AsyncCustomMappings.TryGetValue(typeMap, out var asyncMaps))
                    mappedProperties.UnionWith(asyncMaps.Keys);

                // Propriedades ignoradas
                var ignoredForType = _configuration.IgnoredProperties.Keys
                    .Where(ip => ip.Source == sourceType && ip.Target == targetType)
                    .Select(ip => ip.PropertyName);
                mappedProperties.UnionWith(ignoredForType);

                // Propriedades que são definidas por construtores personalizados
                if (_configuration.CustomConstructors.ContainsKey(typeMap) ||
                    _configuration.ConstructorSelection.ContainsKey(typeMap))
                {
                    // Se tem construtor personalizado, consideramos que todas as propriedades podem ser definidas lá
                    continue;
                }

                var unmapped = destProperties.Where(p => !mappedProperties.Contains(p.Name)).ToList();

                if (unmapped.Any())
                {
                    var message = $"Propriedades não mapeadas para {sourceType.Name} -> {targetType.Name}: {string.Join(", ", unmapped.Select(p => p.Name))}";

                    if (_configuration.ValidateMemberList == MemberListType.Destination)
                    {
                        _errors.Add(message);
                    }
                    else
                    {
                        _warnings.Add(message);
                    }
                }
            }
        }

        private void ValidateTypeCompatibility()
        {
            foreach (var typeMap in _configuration.PropertyMappings.Keys)
            {
                var sourceType = typeMap.Source;
                var targetType = typeMap.Target;

                if (_configuration.PropertyMappings.TryGetValue(typeMap, out var propMaps))
                {
                    foreach (var mapping in propMaps)
                    {
                        var sourceProp = sourceType.GetProperty(mapping.Value);
                        var targetProp = targetType.GetProperty(mapping.Key);

                        if (sourceProp == null)
                        {
                            _warnings.Add($"Propriedade de origem não encontrada: {sourceType.Name}.{mapping.Value}");
                            continue;
                        }

                        if (targetProp == null)
                        {
                            _warnings.Add($"Propriedade de destino não encontrada: {targetType.Name}.{mapping.Key}");
                            continue;
                        }

                        if (!AreTypesCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                        {
                            _warnings.Add($"Tipos incompatíveis: {sourceType.Name}.{mapping.Value} ({sourceProp.PropertyType.Name}) -> " +
                                         $"{targetType.Name}.{mapping.Key} ({targetProp.PropertyType.Name})");
                        }
                    }
                }
            }
        }

        private bool AreTypesCompatible(Type sourceType, Type targetType)
        {
            if (sourceType == targetType) return true;
            if (targetType.IsAssignableFrom(sourceType)) return true;

            var underlyingSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
            var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return underlyingSource == underlyingTarget;
        }

        private void ValidateCircularReferences()
        {
            var visited = new HashSet<(Type, Type)>();
            var stack = new Stack<(Type, Type)>();

            var allTypeMaps = _configuration.CustomMappings.Keys
                .Concat(_configuration.PropertyMappings.Keys)
                .Concat(_configuration.CustomMappingExpressions.Keys)
                .Distinct()
                .ToList();

            foreach (var typeMap in allTypeMaps)
            {
                if (HasCircularReference(typeMap, visited, stack))
                {
                    _warnings.Add($"Possível referência circular detectada envolvendo {typeMap.Source.Name} -> {typeMap.Target.Name}");
                }
            }
        }

        private bool HasCircularReference((Type Source, Type Target) current,
            HashSet<(Type, Type)> visited,
            Stack<(Type, Type)> stack)
        {
            if (stack.Contains(current)) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            stack.Push(current);

            var sourceProperties = current.Source.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProperty in sourceProperties)
            {
                if (IsNavigationProperty(sourceProperty))
                {
                    var elementType = GetCollectionItemType(sourceProperty.PropertyType);
                    if (elementType != null)
                    {
                        var elementTypePair = (elementType, current.Target);
                        if (HasCircularReference(elementTypePair, visited, stack))
                            return true;
                    }
                }
            }

            stack.Pop();
            return false;
        }

        private bool IsNavigationProperty(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                   (property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private Type? GetCollectionItemType(Type collectionType)
        {
            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments().FirstOrDefault();
            if (collectionType.IsArray)
                return collectionType.GetElementType();
            return null;
        }

        private void ValidatePerformance()
        {
            var totalMappings = _configuration.CustomMappings.Count +
                               _configuration.PropertyMappings.Count +
                               _configuration.CustomMappingExpressions.Count +
                               _configuration.AsyncCustomMappings.Count;

            var complexMappings = _configuration.CustomMappings.Values.Sum(cm => cm.Count) +
                                 _configuration.CustomMappingExpressions.Values.Sum(ce => ce.Count) +
                                 _configuration.AsyncCustomMappings.Values.Sum(am => am.Count);

            if (totalMappings > 0 && complexMappings > totalMappings * 0.3)
            {
                _warnings.Add("Alta complexidade detectada - considere otimizar resolvers personalizados");
            }

            if (_configuration.MaxDepth > 20)
            {
                _warnings.Add($"Profundidade máxima ({_configuration.MaxDepth}) muito alta - pode impactar performance");
            }
        }

        private void ThrowConfigurationExceptionsIfAny()
        {
            if (_errors.Any())
                throw new MappingException($"Erros de configuração:\n{string.Join("\n", _errors)}");

            if (_warnings.Any() && _configuration.EnableDiagnostics)
            {
                Console.WriteLine($"Avisos de configuração:\n{string.Join("\n", _warnings)}");
            }
        }
    }

    

}
