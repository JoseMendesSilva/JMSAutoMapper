// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

namespace JMSAutoMapper.Configuration
{
    ///// <summary>Valida a configuração.</summary>
    //public static void AssertConfigurationIsValid(this MapperConfiguration config)
    //{
    //    var validator = new ConfigurationValidator(config);
    //    validator.Validate();
    //}
    /// <summary>
    /// Extensões para a configuração do mapeador.
    /// </summary>
    public static class MapperConfigurationExtensions
    {
        /// <summary>
        /// Valida se a configuração de mapeamento é válida.
        /// </summary>
        /// <param name="config">A configuração a ser validada.</param>
        public static void AssertConfigurationIsValid(this MapperConfiguration config) => config.AssertConfigurationIsValidInternal();
    }
}
