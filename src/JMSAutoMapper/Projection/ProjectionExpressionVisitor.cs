using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JMSAutoMapper.Projection
{
    internal class ProjectionExpressionVisitor : ExpressionVisitor
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
                    bindings.Add(Expression.Bind(targetProperty, body!));
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

    internal class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter) { _oldParameter = oldParameter; _newParameter = newParameter; }
        protected override Expression VisitParameter(ParameterExpression node) => node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}