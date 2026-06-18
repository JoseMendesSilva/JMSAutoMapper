using System;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Extensão da classe MapperConfiguration responsável pelo controle de estado e selamento da configuração.
    /// </summary>
    public partial class MapperConfiguration
    {
        private volatile bool _isSealed;
        private readonly object _sealLock = new();

        /// <summary>
        /// Indica se a configuração já foi finalizada e selada, impedindo novas modificações.
        /// </summary>
        public bool IsSealed => _isSealed;

        /// <summary>
        /// Sela a configuração do Mapper. Uma vez selada, nenhuma nova regra de mapeamento pode ser registrada.
        /// Este método é chamado automaticamente na criação do primeiro Mapper.
        /// </summary>
        public void Seal()
        {
            if (_isSealed) return;

            lock (_sealLock)
            {
                if (_isSealed) return;

                // Se configurado para validar no build, executa a validação antes de selar
                if (ValidateOnBuild)
                {
                    AssertConfigurationIsValidInternal();
                }

                _isSealed = true;
            }
        }

        /// <summary>
        /// Verifica se a configuração está selada e lança uma exceção caso tente-se modificar algo.
        /// </summary>
        internal void ThrowIfSealed()
        {
            if (_isSealed)
            {
                throw new MappingException("A configuração do JMSAutoMapper não pode ser modificada após ter sido selada ou após a criação do Mapper.");
            }
        }
    }
}