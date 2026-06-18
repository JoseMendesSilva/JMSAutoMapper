using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JMSAutoMapper.Cache; // Adicionado para MapperPlanCache
using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Mapeador de coleções otimizado que utiliza planos de execução em cache.
    /// </summary>
    internal static class CompiledCollectionMapper
    {
        private static readonly ConcurrentDictionary<(Type SourceCollectionType, Type TargetCollectionType, Type SourceItemType, Type TargetItemType), Delegate> _collectionMappers = new();

        public static object? Map(IEnumerable? source, Type targetCollectionType, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            if (source == null)
            {
                // Retorna coleção vazia para origem nula
                if (targetCollectionType.IsArray) return Array.CreateInstance(itemType, 0);
                if (targetCollectionType.IsGenericType && targetCollectionType.GetGenericTypeDefinition() == typeof(List<>))
                    return Activator.CreateInstance(targetCollectionType);
                if (typeof(IEnumerable).IsAssignableFrom(targetCollectionType))
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                return null;
            }

            var sourceType = source.GetType();
            var sourceItemType = CollectionTypeHelper.GetCollectionItemType(sourceType) ?? typeof(object);
            var key = (SourceCollectionType: sourceType, TargetCollectionType: targetCollectionType, SourceItemType: sourceItemType, TargetItemType: itemType);
            
            var collectionMapper = _collectionMappers.GetOrAdd(key, k => BuildCollectionMapper(k.SourceCollectionType, k.TargetCollectionType, k.SourceItemType, k.TargetItemType, ((JMSMapper)mapper)._config));

            // Faz o cast para o tipo Func específico para evitar o overhead de DynamicInvoke
            var typedCollectionMapper = (Func<IEnumerable, JMSMapper, Dictionary<object, object>, object>)collectionMapper;
            return typedCollectionMapper(source, (JMSMapper)mapper, mappedObjects);
        }

        private static Delegate BuildCollectionMapper(Type sourceCollectionType, Type targetCollectionType, Type sourceItemType, Type targetItemType, MapperConfiguration config)
        {
            // Obtém o CompiledMappingPlan para o item uma única vez
            var itemPlan = MapperPlanCache.GetOrAdd((sourceItemType, targetItemType), k => new MappingPlanBuilder(config).Build(k.Source, k.Target));
            var compiledItemMapper = new CompiledMappingPlan(itemPlan);
            var compiledItemMapperConstant = Expression.Constant(compiledItemMapper);
            var compiledItemMapperMapMethod = typeof(CompiledMappingPlan).GetMethod(nameof(CompiledMappingPlan.Map))!;

            // Parâmetros para o delegate gerado
            var sourceParam = Expression.Parameter(typeof(IEnumerable), "source");
            var jmsMapperParam = Expression.Parameter(typeof(JMSMapper), "jmsMapper");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");

            // Obtém o método Add para List<targetItemType>
            var listType = typeof(List<>).MakeGenericType(targetItemType);
            var listAddMethod = listType.GetMethod("Add")!;

            // Variáveis para o corpo do delegate - Usamos o tipo concreto para a lista para evitar erro de despacho de método
            var mappedListVar = Expression.Variable(listType, "mappedList");
            var itemVar = Expression.Variable(typeof(object), "item");
            var mappedItemVar = Expression.Variable(typeof(object), "mappedItem");
            var enumeratorVar = Expression.Variable(typeof(IEnumerator), "enumerator");

            // Obtém GetEnumerator e MoveNext/Current para IEnumerable
            var getEnumeratorMethod = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator))!;
            var moveNextMethod = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!;
            var currentMethod = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current))!.GetGetMethod()!;

            // Define o Label para sair do loop
            var breakLabel = Expression.Label("LoopBreak");

            var expressions = new List<Expression>();

            // 1. Inicializa mappedList com capacidade
            var countVar = Expression.Variable(typeof(int), "count");
            var tryGetCount = Expression.IfThenElse(
                Expression.TypeIs(sourceParam, typeof(ICollection)),
                Expression.Assign(countVar, Expression.Property(Expression.Convert(sourceParam, typeof(ICollection)), nameof(ICollection.Count))),
                Expression.Assign(countVar, Expression.Constant(0))
            );
            expressions.Add(tryGetCount);

            var listConstructor = listType.GetConstructor(new[] { typeof(int) });
            expressions.Add(Expression.Assign(mappedListVar, Expression.New(listConstructor!, countVar)));

            // 2. Loop através da origem
            expressions.Add(Expression.Assign(enumeratorVar, Expression.Call(sourceParam, getEnumeratorMethod)));
            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Call(enumeratorVar, moveNextMethod),
                    Expression.Block(
                        Expression.Assign(itemVar, Expression.Property(enumeratorVar, currentMethod)),
                        Expression.IfThen(
                            Expression.NotEqual(itemVar, Expression.Constant(null)),
                            Expression.Block(
                                Expression.Assign(mappedItemVar, Expression.Call(compiledItemMapperConstant, compiledItemMapperMapMethod, itemVar, jmsMapperParam, mappedObjectsParam)),
                                Expression.IfThen(
                                    Expression.NotEqual(mappedItemVar, Expression.Constant(null)),
                                    Expression.Call(mappedListVar, listAddMethod, Expression.Convert(mappedItemVar, targetItemType))
                                )
                            )
                        )
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );
            expressions.Add(loop);

            expressions.Add(Expression.Convert(mappedListVar, typeof(object))); // Retorna a mappedList como object

            var body = Expression.Block(new[] { mappedListVar, itemVar, mappedItemVar, enumeratorVar, countVar }, expressions);

            return Expression.Lambda<Func<IEnumerable, JMSMapper, Dictionary<object, object>, object>>(body, sourceParam, jmsMapperParam, mappedObjectsParam).Compile();
        }
    }
}