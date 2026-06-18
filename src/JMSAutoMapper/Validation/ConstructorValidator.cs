using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JMSAutoMapper.Internals; // For Guard.ThrowIfNull
using JMSAutoMapper; // For MapperConfiguration

namespace JMSAutoMapper.Validation
{
    /// <summary>
    /// Validador responsável por verificar a validade dos construtores personalizados
    /// e selecionados para os mapeamentos.
    /// </summary>
    internal class ConstructorValidator
    {
        private readonly MapperConfiguration _configuration;

        public ConstructorValidator(MapperConfiguration configuration)
        {
            _configuration = Guard.ThrowIfNull(configuration);
        }

        /// <summary>
        /// Valida os construtores configurados e retorna os resultados da validação.
        /// </summary>
        /// <returns>Um objeto MappingValidationResult contendo erros e avisos.</returns>
        public MappingValidationResult Validate()
        {
            var result = new MappingValidationResult();

            // Validar construtores selecionados via UseConstructor
            foreach (var entry in _configuration.ConstructorSelection)
            {
                var typeMap = entry.Key;
                var parameterTypes = entry.Value;
                var targetType = typeMap.Target;

                var constructor = targetType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameterTypes, null);

                if (constructor == null)
                {
                    result.AddError($"ERROR: Nenhum construtor público encontrado para o tipo de destino '{targetType.Name}' com a assinatura de parâmetros: ({string.Join(", ", parameterTypes.Select(t => t.Name))}).");
                }
            }

            // Validar construtores personalizados via ConstructUsing
            // A lógica de ConstructUsing já garante que o delegate é válido em tempo de configuração.
            // No entanto, podemos adicionar uma verificação básica se o delegate é nulo, embora seja improvável.
            foreach (var entry in _configuration.CustomConstructors)
            {
                var typeMap = entry.Key;
                var customConstructorDelegate = entry.Value;

                if (customConstructorDelegate == null)
                {
                    result.AddError($"ERROR: O construtor personalizado para '{typeMap.Source.Name}' -> '{typeMap.Target.Name}' foi configurado como nulo.");
                }
            }

            return result;
        }
    }
}