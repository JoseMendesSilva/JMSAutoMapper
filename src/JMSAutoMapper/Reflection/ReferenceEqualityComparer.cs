// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Runtime.CompilerServices;

namespace JMSAutoMapper.Reflection
{
    /// <summary>
    /// Comparador de igualdade por referência.
    /// Usado para cache de objetos em dicionários.
    /// </summary>
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        private ReferenceEqualityComparer() { }

        /// <summary>Instância singleton.</summary>
        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        /// <inheritdoc/>
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    
}
