// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JMSAutoMapper.Cache
{
    /// <summary>
    /// Gerador de chaves para cache com suporte a diferentes estratégias.
    /// VERSÃO CORRIGIDA - Garante chaves consistentes para o mesmo objeto.
    /// </summary>
    public static class CacheKeyGenerator
    {
        private static readonly ConditionalWeakTable<object, string> _weakObjectKeys = new();
        private static readonly ConcurrentDictionary<Type, bool> _cacheableTypeCache = new();

        /// <summary>
        /// Determina se um tipo é cacheável estaticamente.
        /// </summary>
        public static bool IsTypeCacheable(Type type)
        {
            return _cacheableTypeCache.GetOrAdd(type, t =>
                t.GetCustomAttribute<CacheableAttribute>() != null ||
                t.IsValueType); // Tipos por valor são sempre cacheáveis
        }

        /// <summary>
        /// Obtém o tempo de expiração recomendado para um tipo.
        /// </summary>
        public static int GetExpirationForType(Type type)
        {
            var attr = type.GetCustomAttribute<CacheableAttribute>();
            return attr?.ExpirationMinutes ?? 30;
        }

        /// <summary>
        /// Gera uma chave de cache consistente baseada nos tipos e no objeto.
        /// Esta é a versão CORRIGIDA que garante a mesma chave para o mesmo objeto.
        /// </summary>
        public static string GenerateKey(Type sourceType, Type targetType, object source)
        {
            // Para tipos por valor (int, decimal, structs), usar o próprio valor como chave
            if (sourceType.IsValueType)
            {
                return $"map_val_{sourceType.FullName}_{targetType.FullName}_{source}";
            }

            // Para strings, usar o conteúdo como chave
            if (source is string str)
            {
                return $"map_str_{sourceType.FullName}_{targetType.FullName}_{str.GetHashCode()}";
            }

            // Para referências, usar ConditionalWeakTable para associar uma chave única ao objeto
            // sem impedir o Garbage Collector de coletá-lo
            var key = _weakObjectKeys.GetValue(source, obj =>
                $"map_ref_{sourceType.FullName}_{targetType.FullName}_{Guid.NewGuid():N}");

            return key;
        }

        /// <summary>
        /// Gera uma chave de cache estática baseada apenas nos tipos.
        /// Útil para objetos que são sempre iguais (configurações, enums, etc).
        /// </summary>
        public static string GenerateStaticKey(Type sourceType, Type targetType, object? source = null)
        {
            // Se temos um source e ele é um tipo simples ou valor, incluir na chave
            if (source != null)
            {
                if (sourceType.IsValueType || sourceType == typeof(string))
                {
                    return $"map_static_{sourceType.FullName}_{targetType.FullName}_{source}";
                }

                // Para objetos com ID, tentar usar uma propriedade de identificação
                var idProperty = sourceType.GetProperty("Id") ??
                                 sourceType.GetProperty("ID") ??
                                 sourceType.GetProperty("Codigo");

                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(source);
                    return $"map_static_{sourceType.FullName}_{targetType.FullName}_{idValue}";
                }
            }

            return $"map_static_{sourceType.FullName}_{targetType.FullName}";
        }

        /// <summary>
        /// Gera uma chave para cache de coleções baseada nos elementos.
        /// </summary>
        public static string GenerateCollectionKey(Type sourceType, Type targetType, IEnumerable source)
        {
            // Gerar hash baseado nos primeiros elementos para identificar a coleção
            const int maxElements = 5;
            var elements = source.Cast<object>().Take(maxElements).ToList();

            if (!elements.Any())
                return $"map_coll_empty_{sourceType.FullName}_{targetType.FullName}";

            var hash = string.Join("_", elements.Select(e => e?.GetHashCode() ?? 0));
            var count = (source as ICollection)?.Count ?? elements.Count;

            return $"map_coll_{sourceType.FullName}_{targetType.FullName}_{count}_{hash}";
        }
    }

}
