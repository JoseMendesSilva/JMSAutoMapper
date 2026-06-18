using System;
using System.Linq;
using JMSAutoMapper.Internals;
using JMSAutoMapper; // Add this for MappingException

namespace JMSAutoMapper.Validation
{
    /// <summary>
    /// Orquestrador central de validação de configuração.
    /// </summary>
    internal class ConfigurationValidator
    {
        private readonly MapperConfiguration _configuration;

        public ConfigurationValidator(MapperConfiguration configuration)
        {
            _configuration = Guard.ThrowIfNull(configuration);
        }

        /// <summary>
        /// Executa todas as regras de validação e lança exceção se houver erros críticos.
        /// </summary>
        public void Validate()
        {
            var result = new MappingValidationResult();

            // 1. Validação de Membros Ausentes
            var missingValidator = new MissingMemberValidator(_configuration);
            result.MergeMessages(missingValidator.Validate());

            // 2. Validação de Construtores
            var constructorValidator = new ConstructorValidator(_configuration);
            var constResult = constructorValidator.Validate();
            result.MergeMessages(constResult.Errors.Select(e => $"ERROR: {e}"));

            if (!result.IsValid)
            {
                throw new MappingException($"Erros de configuração detectados:\n{string.Join("\n", result.Errors)}");
            }
        }
    }
}