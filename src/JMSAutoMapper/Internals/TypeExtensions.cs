using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace JMSAutoMapper.Internals
{
    /// <summary>
    /// Métodos de extensão para a classe Type, fornecendo utilitários para inspeção de tipos.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determina se o tipo fornecido é um Nullable&lt;T&gt;.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Extrai o tipo interno de um Nullable, ou retorna o próprio tipo se não for Nullable.
        /// </summary>
        public static Type GetUnderlyingType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Verifica se um tipo é considerado "simples" para fins de mapeamento direto (primitivos, enums, strings, etc).
        /// </summary>
        public static bool IsSimple(this Type type)
        {
            var underlyingType = type.GetUnderlyingType();
            return underlyingType.IsPrimitive ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(Guid) ||
                   underlyingType == typeof(TimeSpan) ||
                   underlyingType == typeof(DateTimeOffset);
        }

        /// <summary>
        /// Verifica se o tipo é uma coleção (implementa IEnumerable, mas não é string).
        /// </summary>
        public static bool IsCollection(this Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Obtém o tipo dos elementos de uma coleção.
        /// </summary>
        public static Type? GetCollectionItemType(this Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType)
            {
                var genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length == 1) return genericArgs[0]; // List<T>, IEnumerable<T>
                if (genericArgs.Length == 2 && typeof(IDictionary).IsAssignableFrom(collectionType)) return genericArgs[1]; // Dictionary<TKey, TValue>
            }

            // Fallback para interfaces não genéricas ou tipos base
            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                var ienumerable = collectionType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (ienumerable != null) return ienumerable.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// Verifica se o tipo é um tipo complexo (não é simples e não é coleção).
        /// </summary>
        public static bool IsComplex(this Type type)
        {
            return !type.IsSimple() && !type.IsCollection();
        }

        /// <summary>
        /// Verifica se uma propriedade é uma propriedade de navegação (coleção de outros objetos).
        /// </summary>
        public static bool IsNavigationProperty(this PropertyInfo property)
        {
            return property.PropertyType.IsCollection() && property.PropertyType.GetCollectionItemType()?.IsComplex() == true;
        }
    }
}