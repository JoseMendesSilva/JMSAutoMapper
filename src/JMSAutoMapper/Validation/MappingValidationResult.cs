using System;
using System.Collections.Generic;
using System.Linq;

namespace JMSAutoMapper.Validation
{
    /// <summary>
    /// Encapsula os resultados da validação de uma configuração do Mapper, 
    /// permitindo a distinção entre erros fatais e avisos informativos.
    /// </summary>
    public class MappingValidationResult
    {
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        /// <summary>
        /// Lista de erros críticos que impedem a inicialização segura do Mapper.
        /// Se houver itens aqui, IsValid retornará falso.
        /// </summary>
        public IReadOnlyList<string> Errors => _errors;

        /// <summary>
        /// Lista de avisos sobre configurações que podem ser otimizadas ou que podem causar 
        /// comportamentos inesperados, mas não impedem o funcionamento básico.
        /// </summary>
        public IReadOnlyList<string> Warnings => _warnings;

        /// <summary>
        /// Indica se a configuração passou na validação sem erros impeditivos.
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// Adiciona um erro crítico à coleção de resultados.
        /// </summary>
        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _errors.Add(message);
        }

        /// <summary>
        /// Adiciona um aviso informativo à coleção de resultados.
        /// </summary>
        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _warnings.Add(message);
        }

        /// <summary>
        /// Mescla mensagens pré-formatadas vindas de validadores internos (como o MissingMemberValidator).
        /// </summary>
        internal void MergeMessages(IEnumerable<string> messages)
        {
            foreach (var message in messages)
            {
                if (message.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
                    AddError(message.Substring(6).Trim());
                else if (message.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase))
                    AddWarning(message.Substring(8).Trim());
                else
                    AddError(message);
            }
        }
    }
}