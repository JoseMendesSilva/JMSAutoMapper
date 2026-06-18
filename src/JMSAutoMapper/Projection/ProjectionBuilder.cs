using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JMSAutoMapper.Internals;
using JMSAutoMapper.Expressions;

namespace JMSAutoMapper.Projection
{
    /// <summary>
    /// Implementação do Visitor responsável por transformar o grafo de objetos em uma Expression Tree de projeção.
    /// Utilizado principalmente para suporte a IQueryable (Entity Framework).
    /// </summary>
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

        /// <summary>
        /// Constrói a expressão de projeção (MemberInit) para o tipo alvo.
        /// </summary>
        public Expression BuildProjectionExpression()
        {
            // Proteção contra recursão infinita em projeções
            if (_visited.Contains((_sourceType, _targetType)))
            {
                return Expression.Default(_targetType);
            }

            _visited.Add((_sourceType, _targetType));

            var bindings = new List<MemberBinding>();
            var targetProperties = _targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            foreach (var targetProperty in targetProperties)
            {
                if (_config.IgnoredProperties.ContainsKey((_sourceType, _targetType, targetProperty.Name)))
                    continue;

                // 1. Mapeamento customizado por expressão
                if (_config.CustomMappingExpressions.TryGetValue((_sourceType, _targetType), out var customMaps) &&
                    customMaps.TryGetValue(targetProperty.Name, out var lambda))
                {
                    var body = new ParameterReplacer(lambda.Parameters[0], (ParameterExpression)_sourceExpression)
                        .Visit(lambda.Body);
                    bindings.Add(Expression.Bind(targetProperty, body));
                    continue;
                }

                // 2. Resolução de membro (Flattening ou Propriedade Direta)
                var sourceMemberAccess = ResolveSourceMember(targetProperty.Name);

                if (sourceMemberAccess != null)
                {
                    if (targetProperty.PropertyType.IsComplex() && sourceMemberAccess.Type != targetProperty.PropertyType)
                    {
                        // Projeção aninhada recursiva
                        var nestedVisitor = new ProjectionExpressionVisitor(
                            _config,
                            sourceMemberAccess.Type,
                            targetProperty.PropertyType,
                            sourceMemberAccess,
                            _mapper,
                            new HashSet<(Type, Type)>(_visited));

                        var nestedInit = nestedVisitor.BuildProjectionExpression();
                        
                        // Proteção contra nulos traduzível para SQL (CASE WHEN)
                        var safeAccess = Expression.Condition(
                            Expression.Equal(sourceMemberAccess, Expression.Constant(null, sourceMemberAccess.Type)),
                            Expression.Default(targetProperty.PropertyType),
                            nestedInit);

                        bindings.Add(Expression.Bind(targetProperty, safeAccess));
                    }
                    else
                    {
                        // Atribuição direta com conversão segura (CAST no SQL)
                        var converted = EfSafeExpressionBuilder.SafeConvert(sourceMemberAccess, targetProperty.PropertyType);
                        bindings.Add(Expression.Bind(targetProperty, converted));
                    }
                }
            }

            return Expression.MemberInit(Expression.New(_targetType), bindings);
        }

        private Expression? ResolveSourceMember(string targetPropertyName)
        {
            // Tenta mapeamento explícito de propriedade
            if (_config.PropertyMappings.TryGetValue((_sourceType, _targetType), out var mappings) &&
                mappings.TryGetValue(targetPropertyName, out var sourceName))
            {
                var prop = _sourceType.GetProperty(sourceName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null) return Expression.Property(_sourceExpression, prop);
            }

            // Tenta Flattening automático (ex: ClienteNome -> Cliente.Nome)
            var flattened = FlatteningExpressionBuilder.Build(_sourceExpression, targetPropertyName);
            if (flattened != null) return flattened;

            // Tenta correspondência direta por convenção
            var directPropName = _config.NamingConvention(targetPropertyName);
            var directProp = _sourceType.GetProperty(directPropName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (directProp != null) return Expression.Property(_sourceExpression, directProp);

            return null;
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