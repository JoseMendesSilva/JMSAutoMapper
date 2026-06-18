using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Reflection
{
    /// <summary>
    /// Fábrica especializada na criação de instâncias de objetos.
    /// Utiliza Expression Trees compiladas para obter performance próxima à criação direta via 'new',
    /// superando significativamente o uso de Activator.CreateInstance em loops de mapeamento.
    /// </summary>
    internal static class ObjectFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> _factoryCache = new();

        /// <summary>
        /// Cria uma nova instância do tipo especificado usando o construtor sem parâmetros.
        /// </summary>
        /// <param name="type">O tipo de objeto a ser criado.</param>
        /// <returns>Uma nova instância do tipo.</returns>
        public static object CreateInstance(Type type)
        {
            var factory = _factoryCache.GetOrAdd(type, CreateFactoryDelegate);
            return factory();
        }

        private static Func<object> CreateFactoryDelegate(Type type)
        {
            // Para tipos de valor (structs), usamos a lógica do Activator que lida corretamente com o estado inicial.
            if (type.IsValueType)
            {
                return () => Activator.CreateInstance(type)!;
            }

            // Tenta obter o construtor público sem parâmetros.
            var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            
            if (constructor == null)
            {
                // Fallback para Activator caso o construtor seja privado ou não exista (ex: tipos anônimos ou sem ctor default).
                return () => Activator.CreateInstance(type)!;
            }

            var newExpression = Expression.New(constructor);
            var castExpression = Expression.Convert(newExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(castExpression);

            return lambda.Compile();
        }
    }
}