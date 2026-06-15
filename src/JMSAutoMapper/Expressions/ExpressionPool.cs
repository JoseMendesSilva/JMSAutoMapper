// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Collections.Concurrent;

namespace JMSAutoMapper.Expressions
{
    /// <summary>
    /// Pool de expressões compiladas para reutilização.
    /// Otimiza performance evitando recompilação de expressões.
    /// </summary>
    public class ExpressionPool
    {
        private readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _compiledExpressions = new();

        /// <summary>Obtém ou adiciona expressão compilada ao pool.</summary>
        public Delegate GetOrAdd((Type Source, Type Target) key, Func<(Type, Type), Delegate> factory)
        {
            return _compiledExpressions.GetOrAdd(key, factory);
        }

        /// <summary>Tenta obter expressão do pool.</summary>
        public bool TryGet((Type Source, Type Target) key, out Delegate? @delegate)
        {
            return _compiledExpressions.TryGetValue(key, out @delegate);
        }

        /// <summary>Limpa todas as expressões do pool.</summary>
        public void Clear() => _compiledExpressions.Clear();
    }

    
}
