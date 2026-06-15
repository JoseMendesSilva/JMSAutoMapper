// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Reflection;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Configuração de perfil para DI.
    /// </summary>
    public class ProfileConfiguration
    {
        private readonly MapperConfiguration _config;

        /// <summary>Construtor.</summary>
        public ProfileConfiguration(MapperConfiguration config) => _config = config;

        /// <summary>Adiciona perfil.</summary>
        public void AddProfile<TProfile>() where TProfile : Profile, new() => _config.AddProfile<TProfile>();

        /// <summary>Adiciona todos os perfis de um assembly.</summary>
        public void AddProfilesFromAssembly(Assembly assembly) => _config.AddProfilesFromAssembly(assembly);
    }

    

}
