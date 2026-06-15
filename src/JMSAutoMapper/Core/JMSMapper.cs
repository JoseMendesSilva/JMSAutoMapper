// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Cache;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Core
{

    

    /// <summary>
    /// Classe base para value resolvers.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public abstract class ValueResolver<TSource, TDestination, TMember> : IValueResolver<TSource, TDestination, TMember>
    {
        /// <summary>Resolve o valor do membro.</summary>
        public abstract TMember Resolve(TSource source, TDestination destination, TMember destMember, ResolutionContext context);
    }

    /// <summary>
    /// Classe base para value resolvers assíncronos.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public abstract class AsyncValueResolver<TSource, TDestination, TMember> : IAsyncValueResolver<TSource, TDestination, TMember>
    {
        /// <summary>Resolve o valor do membro assincronamente.</summary>
        public abstract Task<TMember> ResolveAsync(TSource source, TDestination destination, TMember destMember, ResolutionContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementação principal do mapeador.
    /// </summary>
    public class JMSMapper : MapperBase
    {
        /// <summary>Construtor.</summary>
        public JMSMapper(MapperConfiguration config, Action<string, Exception>? logger = null, IDistributedMapperCache? distributedCache = null)
            : base(config, logger, distributedCache) { }

        /// <inheritdoc/>
        public override IQueryable<TDestination> MapQueryable<TSource, TDestination>(IQueryable<TSource> source)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "source");
            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.Visit();
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        /// <inheritdoc/>
        public override IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
        {
            var sourceType = source.ElementType;
            var targetType = typeof(TDestination);
            var parameter = Expression.Parameter(sourceType, "x");
            var visitor = new ProjectionExpressionVisitor(_config, sourceType, targetType, parameter, this, new HashSet<(Type, Type)>());
            var projectionBody = visitor.Visit();
            var lambda = Expression.Lambda(projectionBody, parameter);

            return source.Provider.CreateQuery<TDestination>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        /// <inheritdoc/>
        protected override T MapObject<T>(object source, Dictionary<object, object> mappedObjects)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    throw new ArgumentNullException(nameof(source), $"Não é possível mapear origem nula para tipo de valor não anulável '{typeof(T).Name}'.");
                return default!;
            }

            if (mappedObjects.TryGetValue(source, out var existing) && existing is T cached)
                return cached;

            var sourceType = source.GetType();
            var targetType = typeof(T);

            // Interceptação de Dicionários
            if (IsDictionary(targetType))
            {
                var resultDict = MapDictionaryHelper(source as IDictionary, targetType, mappedObjects);
                if (resultDict != null) mappedObjects[source] = resultDict;
                return (T)resultDict!;
            }

            // Interceptação de coleções lineares
            if (IsLinearCollection(targetType))
            {
                var resultCollection = MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);
                if (resultCollection != null) mappedObjects[source] = resultCollection;
                return (T)resultCollection!;
            }

            var key = (sourceType, targetType);

            var mapper = GetOrCreateMapper(key, () => BuildMapperDelegate(sourceType, targetType));

            var result = ((Func<object, Dictionary<object, object>, object>)mapper)(source, mappedObjects);
            return (T)result;
        }

        /// <inheritdoc/>
        protected override async Task<T> MapObjectAsync<T>(object source, ConcurrentDictionary<object, object> mappedObjects, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    throw new ArgumentNullException(nameof(source));
                return default!;
            }

            if (mappedObjects.TryGetValue(source, out var existing) && existing is T cached)
                return cached;

            var sourceType = source.GetType();
            var targetType = typeof(T);
            var key = (sourceType, targetType);

            if (IsDictionary(targetType))
            {
                var resultDict = await MapDictionaryAsyncHelper(source as IDictionary, targetType, mappedObjects, cancellationToken).ConfigureAwait(false);
                if (resultDict != null) mappedObjects.TryAdd(source, resultDict);
                return (T)resultDict!;
            }

            if (IsLinearCollection(targetType))
            {
                var resultCollection = await MapCollectionAsyncHelper(source as IEnumerable, targetType, mappedObjects, cancellationToken).ConfigureAwait(false);
                if (resultCollection != null) mappedObjects.TryAdd(source, resultCollection);
                return (T)resultCollection!;
            }

            var mapper = GetOrCreateAsyncMapper(key, () => BuildAsyncMapperDelegate(sourceType, targetType));
            var result = await ((Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>)mapper)(source, mappedObjects, cancellationToken).ConfigureAwait(false);
            
            return (T)result;
        }

        private object GetOrCreateAsyncMapper((Type Source, Type Target) key, Func<Delegate> factory)
        {
            return _compiledAsyncMappers.GetOrAdd(key, _ => factory());
        }

        private Func<object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>> GetMapComplexTypeAsyncDelegate(Type targetType)
        {
            return _mapComplexTypeAsyncDelegates.GetOrAdd(targetType, type =>
            {
                var method = typeof(JMSMapper).GetMethod(nameof(MapObjectAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(type);

                if (genericMethod == null)
                    throw new InvalidOperationException($"Não foi possível criar delegate assíncrono para o tipo {type.Name}");

                var mapperParam = Expression.Parameter(typeof(JMSMapper), "mapper");
                var sourceParam = Expression.Parameter(typeof(object), "source");
                var cacheParam = Expression.Parameter(typeof(ConcurrentDictionary<object, object>), "cache");
                var tokenParam = Expression.Parameter(typeof(CancellationToken), "token");

                var call = Expression.Call(mapperParam, genericMethod, sourceParam, cacheParam, tokenParam);
                var helperMethod = typeof(JMSMapper).GetMethod(nameof(TaskConvertHelper), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(type);
                var convertCall = Expression.Call(helperMethod, call);

                var lambda = Expression.Lambda<Func<JMSMapper, object, ConcurrentDictionary<object, object>, CancellationToken, Task<object>>>(
                    convertCall, mapperParam, sourceParam, cacheParam, tokenParam);

                var compiled = lambda.Compile();
                return (source, cache, token) => compiled(this, source, cache, token);
            });
        }

        private static async Task<object> TaskConvertHelper<T>(Task<T> task) => (await task.ConfigureAwait(false))!;

        private static Task SetPropertyFromTask(Task<object> task, object target, Action<object, object> setter)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted) throw t.Exception!.InnerException!;
                setter(target, t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private static async Task<object> ExecuteAsyncMappingPlan(object result, List<Task> tasks, List<Action<object, object>>? afterActions, object source)
        {
            if (tasks.Count > 0)
                await Task.WhenAll(tasks).ConfigureAwait(false);

            if (afterActions != null)
                foreach (var action in afterActions) action(source, result);

            return result;
        }

        private object? MapDictionaryHelper(IDictionary? sourceDict, Type targetDictType, Dictionary<object, object> mappedObjects)
        {
            if (sourceDict == null) return Activator.CreateInstance(targetDictType);
            var args = targetDictType.GetGenericArguments();
            var keyType = args[0];
            var valType = args[1];
            var resultDict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valType))!;
            
            foreach (DictionaryEntry entry in sourceDict)
            {
                var key = ConvertValue(entry.Key, keyType);
                var value = MapComplexType(valType, entry.Value!, mappedObjects);
                if (key != null && value != null) resultDict[key] = value;
            }

            return FinalizeCollection(resultDict, typeof(KeyValuePair<,>).MakeGenericType(keyType, valType), targetDictType);
        }

        private async Task<object?> MapDictionaryAsyncHelper(IDictionary? sourceDict, Type targetDictType, ConcurrentDictionary<object, object> mappedObjects, CancellationToken token)
        {
            if (sourceDict == null) return Activator.CreateInstance(targetDictType);
            var args = targetDictType.GetGenericArguments();
            var keyType = args[0];
            var valType = args[1];
            var resultDict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valType))!;

            var mapDelegate = GetMapComplexTypeAsyncDelegate(valType);
            foreach (DictionaryEntry entry in sourceDict)
            {
                var key = ConvertValue(entry.Key, keyType);
                var value = await mapDelegate(entry.Value!, mappedObjects, token).ConfigureAwait(false);
                if (key != null && value != null) resultDict[key] = value!;
            }

            return FinalizeCollection(resultDict, typeof(KeyValuePair<,>).MakeGenericType(keyType, valType), targetDictType);
        }

        private object? FinalizeCollection(object list, Type itemType, Type targetType)
        {
            if (targetType.IsArray)
            {
                var listInterface = (IList)list;
                var array = Array.CreateInstance(itemType, listInterface.Count);
                listInterface.CopyTo(array, 0);
                return array;
            }
            if (targetType.IsAssignableFrom(list.GetType())) return list;
            if (targetType.IsGenericType)
            {
                var def = targetType.GetGenericTypeDefinition();
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

                if (def == typeof(HashSet<>)) return Activator.CreateInstance(targetType, list);

                // Usamos GetMethods + First para evitar AmbiguousMatchException especificando o parâmetro IEnumerable
                if (def == typeof(ImmutableList<>)) return typeof(ImmutableList).GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable")).MakeGenericMethod(itemType).Invoke(null, new[] { list });
                if (def == typeof(ImmutableArray<>)) return typeof(ImmutableArray).GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable")).MakeGenericMethod(itemType).Invoke(null, new[] { list });
                if (def == typeof(ImmutableQueue<>)) return typeof(ImmutableQueue).GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable")).MakeGenericMethod(itemType).Invoke(null, new[] { list });
                if (def == typeof(ImmutableStack<>)) return typeof(ImmutableStack).GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable")).MakeGenericMethod(itemType).Invoke(null, new[] { list });
                
                if (def == typeof(ImmutableDictionary<,>)) 
                {
                    var args = targetType.GetGenericArguments();
                    return typeof(ImmutableDictionary).GetMethods().First(m => m.Name == "CreateRange" && m.GetParameters().Length == 1).MakeGenericMethod(args[0], args[1]).Invoke(null, new[] { list });
                }
            }
            return list;
        }

        private Delegate BuildAsyncMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = Expression.Parameter(typeof(ConcurrentDictionary<object, object>), "mappedObjects");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");
            var tasksVar = Expression.Variable(typeof(List<Task>), "tasks");

            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);
            var key = (sourceType, targetType);

            var expressions = new List<Expression>
            {
                Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType)),
                Expression.Assign(tasksVar, Expression.New(typeof(List<Task>)))
            };

            // 1. Instanciação
            if (_config.CustomConstructors.TryGetValue(key, out var customConstructor))
                expressions.Add(Expression.Assign(resultVar, Expression.Convert(Expression.Invoke(Expression.Constant(customConstructor), sourceParam), targetType)));
            else
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));

            // 2. Registro no cache de instâncias (Circular Reference)
            expressions.Add(Expression.Call(mappedObjectsParam,
                typeof(ConcurrentDictionary<object, object>).GetMethod("TryAdd")!,
                sourceParam, Expression.Convert(resultVar, typeof(object))));

            // 3. BeforeMap
            if (_config.BeforeMapActions.TryGetValue(key, out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var setPropertyFromTaskMethod = typeof(JMSMapper).GetMethod(nameof(SetPropertyFromTask), BindingFlags.Static | BindingFlags.NonPublic)!;
            var mapCollectionAsyncHelperMethod = typeof(JMSMapper).GetMethod(nameof(MapCollectionAsyncHelper), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var baseInstance = Expression.Convert(Expression.Constant(this), typeof(MapperBase));
            var thisInstance = Expression.Constant(this);

            // Mapeamento das propriedades
            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                Expression propertyMappingExpression = null;

                // Mapeamento Assíncrono Customizado
                if (_config.AsyncCustomMappings.TryGetValue(key, out var asyncMappings) && asyncMappings.TryGetValue(targetProperty.Name, out var asyncFunc))
                {
                    var targetExp = Expression.Parameter(typeof(object), "t");
                    var valueExp = Expression.Parameter(typeof(object), "v");
                    var setter = Expression.Lambda<Action<object, object>>(
                        Expression.Assign(
                            Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                            Expression.Convert(Expression.Call(baseInstance, convertValueMethod, valueExp, Expression.Constant(targetProperty.PropertyType)), targetProperty.PropertyType)
                        ), targetExp, valueExp).Compile();

                    var taskCall = Expression.Invoke(Expression.Constant(asyncFunc), sourceParam, thisInstance, cancellationTokenParam);
                    propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                        Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                }
                // Mapeamento por Complexidade / Coleção / Síncrono
                else
                {
                    Expression sourceValueAccess = null!;
                    if (_config.CustomMappings.TryGetValue(key, out var syncMappings) && syncMappings.TryGetValue(targetProperty.Name, out var syncFunc))
                    {
                        sourceValueAccess = Expression.Invoke(Expression.Constant(syncFunc), sourceParam, thisInstance);
                    }
                    else
                    {
                        var sourcePropName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                        var sourceProp = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropName, StringComparison.OrdinalIgnoreCase));
                        if (sourceProp != null && sourceProp.CanRead)
                            sourceValueAccess = Expression.Property(sourceVar, sourceProp);
                        else
                            sourceValueAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name)!;
                    }

                    if (sourceValueAccess != null)
                    {
                        if (IsCollection(targetProperty.PropertyType))
                        {
                            var targetExp = Expression.Parameter(typeof(object), "t");
                            var valueExp = Expression.Parameter(typeof(object), "v");
                            var setter = Expression.Lambda<Action<object, object>>(
                                Expression.Assign(
                                    Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                                    Expression.Convert(valueExp, targetProperty.PropertyType)
                                ), targetExp, valueExp).Compile();

                            var taskCall = Expression.Call(thisInstance, mapCollectionAsyncHelperMethod,
                                Expression.Convert(sourceValueAccess, typeof(IEnumerable)),
                                Expression.Constant(targetProperty.PropertyType),
                                mappedObjectsParam, cancellationTokenParam);

                            propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                                Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                        }
                        else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                        {
                            var mapAsyncDelegate = GetMapComplexTypeAsyncDelegate(targetProperty.PropertyType);

                            var targetExp = Expression.Parameter(typeof(object), "t");
                            var valueExp = Expression.Parameter(typeof(object), "v");
                            var setter = Expression.Lambda<Action<object, object>>(
                                Expression.Assign(
                                    Expression.Property(Expression.Convert(targetExp, targetType), targetProperty),
                                    Expression.Convert(valueExp, targetProperty.PropertyType)
                                ), targetExp, valueExp).Compile();

                            var taskCall = Expression.Invoke(Expression.Constant(mapAsyncDelegate),
                                Expression.Convert(sourceValueAccess, typeof(object)),
                                mappedObjectsParam, cancellationTokenParam);

                            propertyMappingExpression = Expression.Call(tasksVar, typeof(List<Task>).GetMethod("Add")!,
                                Expression.Call(setPropertyFromTaskMethod, taskCall, Expression.Convert(resultVar, typeof(object)), Expression.Constant(setter)));
                        }
                        else
                        {
                            // Lógica de Política de Erro para Tipos de Valor (Simples)
                            var isNonNullableValueTypeDest = targetProperty.PropertyType.IsValueType && Nullable.GetUnderlyingType(targetProperty.PropertyType) == null;
                            var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourceValueAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                            var assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));

                            // Determine if the source can be null
                            var sourceCanBeNull = sourceValueAccess != null && (!sourceValueAccess.Type.IsValueType || Nullable.GetUnderlyingType(sourceValueAccess.Type) != null);
                            Expression? sourceIsNull = (sourceCanBeNull && sourceValueAccess != null) ? Expression.Equal(sourceValueAccess, Expression.Constant(null, sourceValueAccess.Type)) : null;

                            if (isNonNullableValueTypeDest)
                            {
                                if (sourceCanBeNull)
                                {
                                    if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Throw)
                                    {
                                        propertyMappingExpression = Expression.IfThenElse(
                                            sourceIsNull ?? Expression.Constant(false),
                                            Expression.Throw(Expression.New(typeof(MappingException).GetConstructor(new[] { typeof(string) })!,
                                                Expression.Constant($"Falha ao mapear '{targetProperty.Name}': Valor de origem é nulo para um tipo de valor não anulável '{targetProperty.PropertyType.Name}'. Para mudar este comportamento, altere NullValueMappingStrategy."))),
                                            assignment);
                                    }
                                    else if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Ignore)
                                    {
                                        propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull!), assignment);
                                    }
                                    else // Default
                                    {
                                        propertyMappingExpression = assignment;
                                    }
                                }
                                else // Source cannot be null (non-nullable value type), so just assign
                                {
                                    propertyMappingExpression = assignment;
                                }
                            }
                            else // Target is nullable value type or reference type
                            {
                                if (sourceCanBeNull)
                                {
                                    propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull!), assignment);
                                }
                                else // Source cannot be null, just assign
                                {
                                    propertyMappingExpression = assignment;
                                }
                            }

                            if (propertyMappingExpression != null)
                            {
                                if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                                {
                                    var conditionCall = Expression.Invoke(Expression.Constant(conditionFunc), sourceParam);
                                    expressions.Add(Expression.IfThen(conditionCall, propertyMappingExpression));
                                }
                                else
                                {
                                    expressions.Add(propertyMappingExpression);
                                }
                            }
                        }
                    }
                }

                // Adicionar propertyMappingExpression à lista de expressões se não for nula
                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionCall = Expression.Invoke(Expression.Constant(conditionFunc), sourceParam);
                        expressions.Add(Expression.IfThen(conditionCall, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            // 4. Executar Plano e retornar Task
            // 4. Executar Plano e retornar Task (fora do loop foreach)
            var executeMethod = typeof(JMSMapper).GetMethod(nameof(ExecuteAsyncMappingPlan), BindingFlags.Static | BindingFlags.NonPublic)!;
            _config.AfterMapActions.TryGetValue(key, out var afterActions);

            expressions.Add(Expression.Call(executeMethod,
                Expression.Convert(resultVar, typeof(object)),
                tasksVar,
                Expression.Constant(afterActions, typeof(List<Action<object, object>>)),
                sourceParam));

            var body = Expression.Block(new[] { sourceVar, resultVar, tasksVar }, expressions);
            return Expression.Lambda<
                Func<object, ConcurrentDictionary<object, object>, CancellationToken,
                Task<object>>>(body, sourceParam, mappedObjectsParam, cancellationTokenParam).Compile();
        }

        private async Task<object?> MapCollectionAsyncHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, ConcurrentDictionary<object, object> mappedObjects, CancellationToken token)
        {
            if (targetCollectionType == null) return null;
            var itemType = GetCollectionItemType(targetCollectionType);
            if (itemType == null) return null;

            // Se o itemType for um tipo simples, mapeia os itens sincronicamente usando ConvertValue
            if (IsSimpleType(itemType))
            {
                var simpleListType = typeof(List<>).MakeGenericType(itemType!);
                var simpleMappedList = (IList)Activator.CreateInstance(simpleListType)!;

                if (sourceEnumerable == null)
                {
                    if (targetCollectionType.IsArray) return Array.CreateInstance(itemType!, 0);
                    if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;
                    try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
                }

                foreach (var item in sourceEnumerable)
                {
                    if (item == null) continue;
                    var convertedItem = ConvertValue(item, itemType);
                    if (convertedItem != null) simpleMappedList.Add(convertedItem);
                }

                if (targetCollectionType.IsArray)
                {
                    var array = Array.CreateInstance(itemType, simpleMappedList.Count);
                    simpleMappedList.CopyTo(array, 0);
                    return array;
                }

                if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;

                var simpleConstructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType!) });
                if (simpleConstructor != null) return simpleConstructor.Invoke(new object[] { simpleMappedList });

                return simpleMappedList;
            }

            var listType = typeof(List<>).MakeGenericType(itemType!);
            var mappedList = (IList)Activator.CreateInstance(listType)!;
            
            if (sourceEnumerable == null)
            {
                if (targetCollectionType.IsArray) return Array.CreateInstance(itemType, 0);
                if (targetCollectionType.IsAssignableFrom(listType)) return mappedList;
                try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
            }

            var items = sourceEnumerable.Cast<object>().ToList();
            var mapDelegate = GetMapComplexTypeAsyncDelegate(itemType);

            var tasks = items.Select(item => mapDelegate(item, mappedObjects, token)).ToList();
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var result in results)
            {
                if (result != null) mappedList.Add(result);
            }

            return FinalizeCollection(mappedList, itemType, targetCollectionType);
        }

        /// <inheritdoc/>
        protected override TDestination MapObjectWithDestination<TSource, TDestination>(TSource source, TDestination destination, Dictionary<object, object> mappedObjects)
        {
            if (source == null) return destination;
            if (mappedObjects.TryGetValue(source!, out var existing) && existing is TDestination cached) return cached;

            var sourceType = typeof(TSource);
            var targetType = typeof(TDestination);
            var key = (sourceType, targetType);

            var mapper = GetOrCreateMapperWithDestination(key, () => BuildMapperDelegateWithDestination(sourceType, targetType));

            var result = ((Func<object, object, Dictionary<object, object>, object>)mapper)(source!, destination!, mappedObjects);
            return (TDestination)result;
        }

        private object GetOrCreateMapper((Type Source, Type Target) key, Func<Delegate> factory)
        {
            if (_expressionPool.TryGet(key, out var cached))
                return cached!;

            return _compiledMappers.GetOrAdd(key, _ => factory());
        }

        private object GetOrCreateMapperWithDestination((Type Source, Type Target) key, Func<Delegate> factory)
        {
            if (_expressionPool.TryGet(key, out var cached))
                return cached!;

            return _compiledMappersWithDestination.GetOrAdd(key, _ => factory());
        }

        /// <summary>
        /// Obtém delegate para mapeamento de tipo complexo sem reflexão.
        /// </summary>
        private Func<object, Dictionary<object, object>, object> GetMapComplexTypeDelegate(Type targetType)
        {
            return _mapComplexTypeDelegates.GetOrAdd(targetType, type =>
            {
                // Criamos um delegate que chama MapObject<T> sem usar reflexão toda vez
                var method = typeof(JMSMapper).GetMethod(nameof(MapObject), BindingFlags.NonPublic | BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(type);

                if (genericMethod == null)
                    throw new InvalidOperationException($"Não foi possível criar delegate para o tipo {type.Name}");

                // Compila um delegate que chama o método genérico
                return (source, mappedObjects) => genericMethod.Invoke(this, new[] { source, mappedObjects })!;
            });
        }

        private Delegate BuildMapperDelegate(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");
            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");
            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var expressions = new List<Expression>
                {
                    Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType))
                };

            // Construção do objeto resultado
            if (_config.CustomConstructors.TryGetValue((sourceType, targetType), out var customConstructor))
            {
                expressions.Add(Expression.Assign(resultVar, Expression.Convert(
                    Expression.Invoke(Expression.Constant(customConstructor), Expression.Convert(sourceVar, typeof(object))),
                    targetType)));
            }
            else if (_config.ConstructorSelection.TryGetValue((sourceType, targetType), out var parameterTypes))
            {
                var bestConstructor = targetType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameterTypes, null);
                if (bestConstructor != null)
                {
                    var constructorParameters = bestConstructor.GetParameters();
                    var arguments = new Expression[constructorParameters.Length];

                    for (int i = 0; i < constructorParameters.Length; i++)
                    {
                        var param = constructorParameters[i];
                        var sourceProperty = sourceProperties.FirstOrDefault(sp =>
                            string.Equals(sp.Name, param.Name, StringComparison.OrdinalIgnoreCase));

                        arguments[i] = sourceProperty != null
                            ? Expression.Convert(Expression.Property(sourceVar, sourceProperty), param.ParameterType)
                            : Expression.Default(param.ParameterType);
                    }

                    expressions.Add(Expression.Assign(resultVar, Expression.New(bestConstructor, arguments)));
                }
                else
                {
                    expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
                }
            }
            else if (targetType.IsValueType)
            {
                expressions.Add(Expression.Assign(resultVar, Expression.New(targetType)));
            }
            else
            {
                expressions.Add(Expression.Assign(resultVar,
                    Expression.Convert(
                        Expression.Call(
                            typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(Type) })!,
                            Expression.Constant(targetType)),
                        targetType)));
            }

            // Cache da instância
            expressions.Add(Expression.Assign(
                Expression.Property(mappedObjectsParam, "Item", sourceParam),
                Expression.Convert(resultVar, typeof(object))));

            // Ações BeforeMap
            if (_config.BeforeMapActions.TryGetValue((sourceType, targetType), out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;

            var mapObjectMethod = typeof(JMSMapper).GetMethod(nameof(MapComplexType), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            // Mapeamento das propriedades
            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                var key = (sourceType, targetType);
                Expression? propertyMappingExpression = null;

                if (_config.CustomMappings.TryGetValue(key, out var mappings) &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = Expression.Invoke(Expression.Constant(mappingFunc), Expression.Convert(sourceVar, typeof(object)), thisInstance);

                    Expression assignment;
                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(valueExpression, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(valueExpression, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, valueExpression, Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    propertyMappingExpression = assignment;
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                    var sourceProperty = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null && sourceProperty.CanRead)
                        sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    else
                        sourcePropertyAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name);

                    if (sourcePropertyAccess == null)
                        continue;

                    Expression assignment;
                    var isNonNullableValueTypeDest = targetProperty.PropertyType.IsValueType && Nullable.GetUnderlyingType(targetProperty.PropertyType) == null;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(sourcePropertyAccess, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourcePropertyAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    // Aplicar Política de Nulos
                    if (!sourcePropertyAccess.Type.IsValueType || Nullable.GetUnderlyingType(sourcePropertyAccess.Type) != null)
                    {
                        var sourceIsNull = Expression.Equal(sourcePropertyAccess, Expression.Constant(null, sourcePropertyAccess.Type));
                        if (isNonNullableValueTypeDest)
                        {
                            if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Throw)
                            {
                                propertyMappingExpression = Expression.IfThenElse(sourceIsNull, Expression.Throw(Expression.New(typeof(MappingException).GetConstructor(new[] { typeof(string) })!, Expression.Constant($"Falha ao mapear '{targetProperty.Name}': Valor de origem é nulo para um tipo de valor não anulável '{targetProperty.PropertyType.Name}'. Para mudar este comportamento, altere NullValueMappingStrategy."))), assignment);
                            }
                            else if (_config.NullValueMappingStrategy == NullValueMappingPolicy.Ignore)
                            {
                                propertyMappingExpression = Expression.IfThen(Expression.Not(sourceIsNull), assignment);
                            }
                            else // Default
                            {
                                propertyMappingExpression = assignment;
                            }
                        }
                        else
                        {
                            propertyMappingExpression = IsCollection(targetProperty.PropertyType) ? assignment : Expression.IfThen(Expression.Not(sourceIsNull!), assignment); // CS8600
                        }
                    }
                    else
                    {
                        propertyMappingExpression = assignment;
                    }
                }

                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) && conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionExpression = Expression.Invoke(Expression.Constant(conditionFunc), Expression.Convert(sourceVar, typeof(object)));
                        expressions.Add(Expression.IfThen(conditionExpression, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            // Ações AfterMap
            if (_config.AfterMapActions.TryGetValue((sourceType, targetType), out var afterActions))
            {
                foreach (var action in afterActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            expressions.Add(Expression.Convert(resultVar, typeof(object)));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, Dictionary<object, object>, object>>(body, sourceParam, mappedObjectsParam);

            return lambda.Compile();
        }

        private Delegate BuildMapperDelegateWithDestination(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "sourceObj");
            var destParam = Expression.Parameter(typeof(object), "destObj");
            var mappedObjectsParam = Expression.Parameter(typeof(Dictionary<object, object>), "mappedObjects");
            var sourceVar = Expression.Variable(sourceType, "source");
            var resultVar = Expression.Variable(targetType, "result");
            var sourceProperties = GetProperties(sourceType);
            var targetProperties = GetProperties(targetType);

            var expressions = new List<Expression>
                {
                    Expression.Assign(sourceVar, Expression.Convert(sourceParam, sourceType)),
                    Expression.Assign(resultVar, Expression.Condition(
                        Expression.NotEqual(destParam, Expression.Constant(null)),
                        Expression.Convert(destParam, targetType),
                        Expression.New(targetType)
                    ))
                };

            expressions.Add(Expression.Assign(
                Expression.Property(mappedObjectsParam, "Item", sourceParam),
                Expression.Convert(resultVar, typeof(object))));

            if (_config.BeforeMapActions.TryGetValue((sourceType, targetType), out var beforeActions))
            {
                foreach (var action in beforeActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            var convertValueMethod = typeof(MapperBase).GetMethod("ConvertValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var mapCollectionHelperMethod = typeof(JMSMapper).GetMethod("MapCollectionHelper", BindingFlags.Instance | BindingFlags.NonPublic)!;

            // Obter delegate para MapComplexType
            var mapComplexTypeDelegate = GetMapComplexTypeDelegate(targetType);
            var mapComplexTypeConstant = Expression.Constant(mapComplexTypeDelegate);
            var mapObjectMethod = typeof(JMSMapper).GetMethod(nameof(MapComplexType), BindingFlags.Instance | BindingFlags.NonPublic)!;

            var thisInstance = Expression.Constant(this);
            var baseInstance = Expression.Convert(thisInstance, typeof(MapperBase));

            foreach (var targetProperty in targetProperties.Where(p => p.CanWrite))
            {
                if (_config.IgnoredProperties.ContainsKey((sourceType, targetType, targetProperty.Name)))
                    continue;

                var key = (sourceType, targetType);
                Expression? propertyMappingExpression = null;

                if (_config.CustomMappings.TryGetValue(key, out var mappings) &&
                    mappings.TryGetValue(targetProperty.Name, out var mappingFunc))
                {
                    var valueExpression = Expression.Invoke(Expression.Constant(mappingFunc), Expression.Convert(sourceVar, typeof(object)), thisInstance);

                    Expression assignment;
                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(valueExpression, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(valueExpression, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, valueExpression, Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    propertyMappingExpression = assignment;
                }
                else
                {
                    var sourcePropertyName = GetMappedPropertyName(sourceType, targetType, targetProperty.Name, _config.PropertyMappings);
                    var sourceProperty = sourceProperties.FirstOrDefault(p => string.Equals(p.Name, sourcePropertyName, StringComparison.OrdinalIgnoreCase));

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null && sourceProperty.CanRead)
                        sourcePropertyAccess = Expression.Property(sourceVar, sourceProperty);
                    else
                        sourcePropertyAccess = GetFlattenedSourceMember(sourceVar, targetProperty.Name);

                    if (sourcePropertyAccess == null)
                        continue;

                    Expression assignment;

                    if (IsCollection(targetProperty.PropertyType))
                    {
                        var mapCollectionHelperCall = Expression.Call(thisInstance, mapCollectionHelperMethod, Expression.Convert(sourcePropertyAccess, typeof(IEnumerable)), Expression.Constant(targetProperty.PropertyType), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapCollectionHelperCall, targetProperty.PropertyType));
                    }
                    else if (IsComplexType(targetProperty.PropertyType) && !IsSimpleType(targetProperty.PropertyType))
                    {
                        var mapObjectCall = Expression.Call(thisInstance, mapObjectMethod, Expression.Constant(targetProperty.PropertyType), Expression.Convert(sourcePropertyAccess, typeof(object)), mappedObjectsParam);
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(mapObjectCall, targetProperty.PropertyType));
                    }
                    else
                    {
                        var convertedValue = Expression.Call(baseInstance, convertValueMethod, Expression.Convert(sourcePropertyAccess, typeof(object)), Expression.Constant(targetProperty.PropertyType));
                        assignment = Expression.Assign(Expression.Property(resultVar, targetProperty), Expression.Convert(convertedValue, targetProperty.PropertyType));
                    }

                    if (!sourceProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(sourceProperty.PropertyType) != null)
                    {
                        propertyMappingExpression = Expression.IfThen(
                            Expression.NotEqual(sourcePropertyAccess, Expression.Constant(null, sourceProperty.PropertyType!)), // CS8602
                            assignment);
                    }
                    else
                    {
                        propertyMappingExpression = assignment;
                    }
                }

                if (propertyMappingExpression != null)
                {
                    if (_config.ConditionalMappings.TryGetValue(key, out var conditions) &&
                        conditions.TryGetValue(targetProperty.Name, out var conditionFunc))
                    {
                        var conditionExpression = Expression.Invoke(
                            Expression.Constant(conditionFunc),
                            Expression.Convert(sourceVar, typeof(object)));

                        expressions.Add(Expression.IfThen(conditionExpression, propertyMappingExpression));
                    }
                    else
                    {
                        expressions.Add(propertyMappingExpression);
                    }
                }
            }

            if (_config.AfterMapActions.TryGetValue((sourceType, targetType), out var afterActions))
            {
                foreach (var action in afterActions)
                {
                    expressions.Add(Expression.Invoke(
                        Expression.Constant(action),
                        Expression.Convert(sourceVar, typeof(object)),
                        Expression.Convert(resultVar, typeof(object))));
                }
            }

            expressions.Add(Expression.Convert(resultVar, typeof(object)));

            var body = Expression.Block(new[] { sourceVar, resultVar }, expressions);
            var lambda = Expression.Lambda<Func<object, object, Dictionary<object, object>, object>>(
                body, sourceParam, destParam, mappedObjectsParam);

            return lambda.Compile();
        }

        /// <summary>
        /// Método auxiliar para mapear tipos complexos usando delegate cacheado.
        /// </summary>
        private object? MapComplexType(Type targetType, object source, Dictionary<object, object> mappedObjects)
        {
            if (targetType == null) return null;
            if (source == null) return null;

            // Se o targetType for um tipo simples, converte-o diretamente.
            // Isso evita tentar construir um mapeador de objeto complexo para tipos simples como string.
            if (IsSimpleType(targetType))
            {
                return ConvertValue(source, targetType);
            }
            if (mappedObjects.TryGetValue(source, out var existing) && targetType.IsInstanceOfType(existing)) return existing;

            var sourceType = source.GetType();
            if (IsCollection(sourceType) && IsCollection(targetType))
                return MapCollectionHelper(source as IEnumerable, targetType, mappedObjects);

            var mapDelegate = GetMapComplexTypeDelegate(targetType);
            return mapDelegate(source, mappedObjects);
        }

        private object? MapCollectionHelper(IEnumerable? sourceEnumerable, Type targetCollectionType, Dictionary<object, object> mappedObjects)
        {
            if (targetCollectionType == null) return null;
            var itemType = GetCollectionItemType(targetCollectionType);
            if (itemType == null) return null; // CS8602

            // Se o itemType for um tipo simples, mapeia os itens sincronicamente usando ConvertValue
            if (IsSimpleType(itemType))
            {
                var simpleListType = typeof(List<>).MakeGenericType(itemType!);
                var simpleMappedList = (IList)Activator.CreateInstance(simpleListType)!;

                if (sourceEnumerable == null)
                {
                    if (targetCollectionType.IsArray) return Array.CreateInstance(itemType!, 0);
                    if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;
                    try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
                }

                foreach (var item in sourceEnumerable)
                {
                    if (item == null) continue;
                    var convertedItem = ConvertValue(item, itemType);
                    if (convertedItem != null) simpleMappedList.Add(convertedItem);
                }

                if (targetCollectionType.IsArray)
                {
                    var array = Array.CreateInstance(itemType!, simpleMappedList.Count);
                    simpleMappedList.CopyTo(array, 0);
                    return array;
                }

                if (targetCollectionType.IsAssignableFrom(simpleListType)) return simpleMappedList;

                var simpleConstructor = targetCollectionType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(itemType!) });
                if (simpleConstructor != null) return simpleConstructor.Invoke(new object[] { simpleMappedList });

                return simpleMappedList;
            }

            var listType = typeof(List<>).MakeGenericType(itemType!);
            var mappedList = (IList)Activator.CreateInstance(listType)!;

            if (sourceEnumerable == null)
            {
                if (targetCollectionType.IsArray) return Array.CreateInstance(itemType, 0);
                if (targetCollectionType.IsAssignableFrom(listType)) return mappedList;
                try { return Activator.CreateInstance(targetCollectionType); } catch { return null; }
            }

            foreach (var item in sourceEnumerable)
            {
                if (item == null) continue;

                var mappedItem = MapComplexType(itemType, item, mappedObjects);
                if (mappedItem != null)
                    mappedList.Add(mappedItem);
            }

            return FinalizeCollection(mappedList, itemType, targetCollectionType);
        }

        private bool IsCollection(Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        private bool IsDictionary(Type type) => typeof(IDictionary).IsAssignableFrom(type);
        private bool IsLinearCollection(Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && !IsDictionary(type);

        private bool IsComplexType(Type type)
        {
            return !(type == typeof(string) || IsSimpleType(type) || IsCollection(type));
        }

        private string GetMappedPropertyName(Type sourceType, Type targetType, string targetPropertyName,
            ConcurrentDictionary<(Type, Type), ConcurrentDictionary<string, string>> propertyMappings)
        {
            // Busca recursiva na hierarquia de tipos para suportar herança de mapeamento
            for (var s = sourceType; s != null && s != typeof(object); s = s.BaseType)
            {
                for (var t = targetType; t != null && t != typeof(object); t = t.BaseType)
                {
                    if (propertyMappings.TryGetValue((s, t), out var mappings) &&
                        mappings.TryGetValue(targetPropertyName, out var sourcePropertyName))
                    {
                        return sourcePropertyName;
                    }
                }
            }
            return _config.NamingConvention(targetPropertyName);
        }

        private class ProjectionExpressionVisitor : ExpressionVisitor
        {
            private readonly MapperConfiguration _config;
            private readonly Type _sourceType;
            private readonly Type _targetType;
            private readonly Expression _sourceExpression;
            private readonly JMSMapper _mapper;
            private readonly HashSet<(Type, Type)> _visited;

            public ProjectionExpressionVisitor(
                MapperConfiguration config,
                Type sourceType,
                Type targetType,
                Expression sourceExpression,
                JMSMapper mapper,
                HashSet<(Type, Type)> visited)
            {
                _config = config;
                _sourceType = sourceType;
                _targetType = targetType;
                _sourceExpression = sourceExpression;
                _mapper = mapper;
                _visited = visited;
            }

            public Expression Visit()
            {
                if (_visited.Contains((_sourceType, _targetType)))
                    return Expression.Default(_targetType);

                _visited.Add((_sourceType, _targetType));
                var bindings = new List<MemberBinding>();
                var targetProperties = _targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite);

                foreach (var targetProperty in targetProperties)
                {
                    if (_config.IgnoredProperties.ContainsKey((_sourceType, _targetType, targetProperty.Name)))
                        continue;

                    if (_config.CustomMappingExpressions.TryGetValue((_sourceType, _targetType), out var customMaps) &&
                        customMaps.TryGetValue(targetProperty.Name, out var lambda) && lambda != null)
                    {
                        var body = new ParameterReplacer(lambda.Parameters[0], (ParameterExpression)_sourceExpression)
                            .Visit(lambda.Body!);
                        bindings.Add(Expression.Bind(targetProperty, body));
                        continue;
                    }

                    var sourcePropertyName = _mapper.GetMappedPropertyName(
                        _sourceType, _targetType, targetProperty.Name, _config.PropertyMappings);

                    var sourceProperty = _sourceType.GetProperty(
                        sourcePropertyName,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    Expression? sourcePropertyAccess = null;
                    if (sourceProperty != null)
                        sourcePropertyAccess = Expression.Property(_sourceExpression, sourceProperty);
                    else
                        sourcePropertyAccess = _mapper.GetFlattenedSourceMember(_sourceExpression, targetProperty.Name);

                    if (sourcePropertyAccess != null)
                    {
                        if (sourcePropertyAccess != null && _mapper!.IsComplexType(targetProperty.PropertyType) &&
                            sourcePropertyAccess.Type != targetProperty.PropertyType)
                        {
                            var nestedVisitor = new ProjectionExpressionVisitor(
                                _config,
                                sourcePropertyAccess.Type,
                                targetProperty.PropertyType,
                                sourcePropertyAccess,
                                _mapper,
                                _visited);

                            var nestedInit = nestedVisitor.Visit();
                            var nullCheck = Expression.Equal(
                                sourcePropertyAccess,
                                Expression.Constant(null, sourceProperty.PropertyType));

                            var conditional = Expression.Condition(
                                nullCheck,
                                Expression.Default(targetProperty.PropertyType),
                                nestedInit);

                            bindings.Add(Expression.Bind(targetProperty, conditional));
                        }
                        else if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            bindings.Add(Expression.Bind(targetProperty, sourcePropertyAccess));
                        }
                        else
                        {
                            try
                            {
                                var converted = Expression.Convert(sourcePropertyAccess, targetProperty.PropertyType);
                                bindings.Add(Expression.Bind(targetProperty, converted));
                            }
                            catch (InvalidOperationException) { }
                        }
                    }
                }

                return Expression.MemberInit(Expression.New(_targetType), bindings);
            }
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node) =>
                node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    
}
