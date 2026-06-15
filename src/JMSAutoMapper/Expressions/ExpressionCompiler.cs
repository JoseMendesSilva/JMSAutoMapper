using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Expressions
{
    /// <summary>
    /// Utilitário para compilar expressões lambda para acesso rápido a membros.
    /// </summary>
    internal static class ExpressionCompiler
    {
        /// <summary>Cria um delegate de leitura para uma propriedade.</summary>
        public static Func<object, object?> CreateGetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var convertInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
            var property = Expression.Property(convertInstance, propertyInfo);
            var convertProperty = Expression.Convert(property, typeof(object));
            
            return Expression.Lambda<Func<object, object?>>(convertProperty, instance).Compile();
        }

        /// <summary>Cria um delegate de escrita para uma propriedade.</summary>
        public static Action<object, object?> CreateSetter(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite) return null!;

            var instance = Expression.Parameter(typeof(object), "i");
            var value = Expression.Parameter(typeof(object), "v");
            
            var convertInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
            var convertValue = Expression.Convert(value, propertyInfo.PropertyType);
            
            var property = Expression.Property(convertInstance, propertyInfo);
            var assign = Expression.Assign(property, convertValue);
            
            // Para ValueTypes, a atribuição via object requer cuidado, 
            // mas como o Mapper trabalha com instâncias em memória, o cast funciona para classes.
            return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
        }
    }
}
