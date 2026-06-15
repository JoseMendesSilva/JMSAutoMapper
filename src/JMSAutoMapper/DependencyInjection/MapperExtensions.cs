// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Reflection;
using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Cache;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;
using JMSAutoMapper.DependencyInjection;
using JMSAutoMapper.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.DependencyInjection
{
    

    
    

    /// <summary>
    /// Extensões para DI (Microsoft.Extensions.DependencyInjection).
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Adiciona o mapper ao container DI.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="configureProfiles">Ação para configurar perfis.</param>
        /// <param name="enableDistributedCache">Habilitar cache distribuído.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
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
        /// Adiciona o mapper ao container DI com scanning de assembly.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assembly">Assembly para scan de perfis.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
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

            return services.AddJMSMapper(mapperOptions.AssembliesToScan.ToArray(), configure, options);
        }

        /// <summary>
        /// Adiciona o mapper ao container DI com scanning de múltiplos assemblies.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assemblies">Assemblies para scan de perfis.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper(this IServiceCollection services,
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

            // Scan assemblies para perfis
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
        /// Adiciona o mapper ao container DI com scanning do assembly da tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo para obter o assembly.</typeparam>
        /// <param name="services">Service collection.</param>
        /// <param name="configure">Ação para configurar o mapper.</param>
        /// <param name="options">Opções adicionais.</param>
        public static IServiceCollection AddJMSMapper<T>(this IServiceCollection services,
            Action<MapperConfiguration>? configure = null,
            Action<JMSMapperOptions>? options = null)
        {
            return services.AddJMSMapper(typeof(T).Assembly, configure, options);
        }

 
    }

    

}
