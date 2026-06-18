using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace JMSAutoMapper.Internals
{
    /// <summary>
    /// Fornece métodos utilitários para validação de argumentos e estados.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Lança um <see cref="ArgumentNullException"/> se o argumento fornecido for nulo.
        /// </summary>
        /// <typeparam name="T">O tipo do argumento.</typeparam>
        /// <param name="argument">O argumento a ser verificado.</param>
        /// <param name="paramName">O nome do parâmetro que causou a exceção.</param>
        /// <returns>O argumento não nulo.</returns>
        /// <exception cref="ArgumentNullException">Se <paramref name="argument"/> for nulo.</exception>
        public static T ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            return argument ?? throw new ArgumentNullException(paramName);
        }
    }
}