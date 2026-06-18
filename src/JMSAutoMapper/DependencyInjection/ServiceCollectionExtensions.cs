using System.Reflection;
using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;
using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.DependencyInjection
{
    /// <summary>
    /// Extensões para registrar o JMSAutoMapper no Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra o JMSAutoMapper no container de serviços.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<ProfileConfiguration>? configureProfiles = null,
            bool enableDistributedCache = false)
        {
            var config = new MapperConfiguration();
            configure?.Invoke(config);

            var profileConfig = new ProfileConfiguration(config);
            configureProfiles?.Invoke(profileConfig);

            if (enableDistributedCache)
            {
                services.AddSingleton<IDistributedMapperCache, InMemoryDistributedCache>();
            }

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                var distributedCache = enableDistributedCache
                    ? provider.GetService<IDistributedMapperCache>()
                    : null;

                var mapper = new JMSMapper(config, logger, distributedCache);

                if (config.ValidateOnBuild)
                {
                    mapper.AssertConfigurationIsValid();
                }

                return mapper;
            });

            return services;
        }

        /// <summary>
        /// Registra o JMSAutoMapper procurando perfis no assembly informado.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(this IServiceCollection services,
            Assembly assembly,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            var mapperOptions = new JMSMapperOptions();
            options?.Invoke(mapperOptions);

            if (!mapperOptions.AssembliesToScan.Contains(assembly))
            {
                mapperOptions.AssembliesToScan.Add(assembly);
            }

            return services.AddJMSAutoMapper(mapperOptions.AssembliesToScan.ToArray(), configure, options);
        }

        /// <summary>
        /// Registra o JMSAutoMapper procurando perfis nos assemblies informados.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper(this IServiceCollection services,
            Assembly[] assemblies,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            var mapperOptions = new JMSMapperOptions();
            options?.Invoke(mapperOptions);

            var config = new MapperConfiguration
            {
                EnableDiagnostics = mapperOptions.EnableDiagnostics,
                EnableDistributedCache = mapperOptions.EnableDistributedCache,
                CacheExpirationMinutes = mapperOptions.CacheExpirationMinutes,
                MaxDepth = mapperOptions.MaxDepth,
                ThrowOnConversionError = mapperOptions.ThrowOnConversionError,
                NullValueMappingStrategy = mapperOptions.NullValueMappingStrategy,
                ValidateOnBuild = mapperOptions.ValidateOnBuild,
                EnableStaticCache = mapperOptions.EnableStaticCache
            };

            if (mapperOptions.NamingConvention != null)
            {
                config.NamingConvention = mapperOptions.NamingConvention;
            }

            var allAssemblies = new HashSet<Assembly>(assemblies);
            foreach (var assembly in mapperOptions.AssembliesToScan)
            {
                allAssemblies.Add(assembly);
            }

            foreach (var assembly in allAssemblies)
            {
                config.AddProfilesFromAssembly(assembly);
            }

            configure?.Invoke(config);

            if (mapperOptions.EnableDistributedCache)
            {
                services.AddSingleton<IDistributedMapperCache, InMemoryDistributedCache>();
            }

            services.AddSingleton<IMapper>(provider =>
            {
                var logger = provider.GetService<Action<string, Exception>>();
                var distributedCache = mapperOptions.EnableDistributedCache
                    ? provider.GetService<IDistributedMapperCache>()
                    : null;

                var mapper = new JMSMapper(config, logger, distributedCache);

                if (config.ValidateOnBuild)
                {
                    mapper.AssertConfigurationIsValid();
                }

                return mapper;
            });

            return services;
        }

        /// <summary>
        /// Registra o JMSAutoMapper procurando perfis no assembly que contém o tipo informado.
        /// </summary>
        public static IServiceCollection AddJMSAutoMapper<T>(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSAutoMapper(typeof(T).Assembly, configure, options);
        }

        /// <summary>
        /// Alias compatível para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<ProfileConfiguration>? configureProfiles = null,
            bool enableDistributedCache = false)
        {
            return services.AddJMSAutoMapper(configure, configureProfiles, enableDistributedCache);
        }

        /// <summary>
        /// Alias compatível para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Assembly assembly,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSAutoMapper(assembly, configure, options);
        }

        /// <summary>
        /// Alias compatível para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
            Assembly[] assemblies,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSAutoMapper(assemblies, configure, options);
        }

        /// <summary>
        /// Alias compatível para AddJMSAutoMapper.
        /// </summary>
        public static IServiceCollection AddJMSMapper<T>(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSAutoMapper<T>(configure, options);
        }
    }
}
