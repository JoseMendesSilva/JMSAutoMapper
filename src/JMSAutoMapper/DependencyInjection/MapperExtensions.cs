using System.Reflection;
using JMSAutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.DependencyInjection
{
    /// <summary>
    /// Compatibilidade para chamadas estáticas antigas. Use ServiceCollectionExtensions.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Compatibilidade para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<ProfileConfiguration>? configureProfiles = null,
            bool enableDistributedCache = false)
        {
            return ServiceCollectionExtensions.AddJMSAutoMapper(services, configure, configureProfiles, enableDistributedCache);
        }

        /// <summary>
        /// Compatibilidade para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(IServiceCollection services,
            Assembly assembly,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSAutoMapper(services, assembly, configure, options);
        }

        /// <summary>
        /// Compatibilidade para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(IServiceCollection services,
            Assembly[] assemblies,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSAutoMapper(services, assemblies, configure, options);
        }

        /// <summary>
        /// Compatibilidade para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper<T>(IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSAutoMapper<T>(services, configure, options);
        }

        /// <summary>
        /// Compatibilidade para AddJMSMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<ProfileConfiguration>? configureProfiles = null,
            bool enableDistributedCache = false)
        {
            return ServiceCollectionExtensions.AddJMSMapper(services, configure, configureProfiles, enableDistributedCache);
        }

        /// <summary>
        /// Compatibilidade para AddJMSMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(IServiceCollection services,
            Assembly assembly,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSMapper(services, assembly, configure, options);
        }

        /// <summary>
        /// Compatibilidade para AddJMSMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(IServiceCollection services,
            Assembly[] assemblies,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSMapper(services, assemblies, configure, options);
        }

        /// <summary>
        /// Compatibilidade para AddJMSMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper<T>(IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return ServiceCollectionExtensions.AddJMSMapper<T>(services, configure, options);
        }
    }
}
