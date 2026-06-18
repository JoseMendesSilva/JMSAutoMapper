using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JMSAutoMapper; // Add this for MappingException

namespace JMSAutoMapper.Reflection
{
    /// <summary>
    /// Fábrica especializada na criação de instâncias de objetos utilizando construtores parametrizados.
    /// Utiliza Expression Trees para compilar delegados de criação, evitando o overhead de reflexão repetitiva 
    /// e ConstructorInfo.Invoke durante o mapeamento.
    /// </summary>
    internal static class ConstructorFactory
    {
        private static readonly ConcurrentDictionary<(Type TargetType, string ParamSignature), Func<object[], object>> _factoryCache = new();

        /// <summary>
        /// Cria uma nova instância do tipo especificado utilizando os argumentos fornecidos.
        /// </summary>
        /// <param name="type">O tipo de destino.</param>
        /// <param name="parameterTypes">Os tipos dos parâmetros do construtor desejado.</param>
        /// <param name="args">Os valores dos argumentos para o construtor.</param>
        /// <returns>Uma nova instância do objeto.</returns>
        public static object CreateInstance(Type type, Type[] parameterTypes, object[] args)
        {
            var signature = GetSignature(parameterTypes);
            var factory = _factoryCache.GetOrAdd((type, signature), _ => CreateFactoryDelegate(type, parameterTypes));
            return factory(args);
        }

        private static string GetSignature(Type[] parameterTypes)
        {
            return string.Join(";", parameterTypes.Select(t => t.AssemblyQualifiedName ?? t.FullName ?? t.Name));
        }

        private static Func<object[], object> CreateFactoryDelegate(Type type, Type[] parameterTypes)
        {
            var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameterTypes, null);
            
            if (constructor == null)
            {
                throw new MappingException($"Construtor não encontrado para o tipo {type.FullName} com a assinatura de parâmetros fornecida.");
            }

            var parametersParameter = Expression.Parameter(typeof(object[]), "args");
            var parameterExpressions = new Expression[parameterTypes.Length];

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var index = Expression.Constant(i);
                var accessor = Expression.ArrayIndex(parametersParameter, index);
                parameterExpressions[i] = Expression.Convert(accessor, parameterTypes[i]);
            }

            var newExpression = Expression.New(constructor, parameterExpressions);
            var castExpression = Expression.Convert(newExpression, typeof(object));
            
            var lambda = Expression.Lambda<Func<object[], object>>(castExpression, parametersParameter);
            return lambda.Compile();
        }
    }
}