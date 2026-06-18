using System.Reflection;
using JMSAutoMapper;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Classe auxiliar para configuração fluente de perfis no registro DI.
    /// </summary>
    public class ProfileConfiguration
    {
        private readonly MapperConfiguration _config;
        public ProfileConfiguration(MapperConfiguration config) => _config = config;

        public void AddProfile<TProfile>() where TProfile : Profile, new() => _config.AddProfile(new TProfile());
        public void AddProfilesFromAssembly(Assembly assembly) => _config.AddProfilesFromAssembly(assembly);
    }
}