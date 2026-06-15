// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Abstractions;

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Classe base para perfis de configuração.
    /// Permite organizar mapeamentos em grupos lógicos.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    /// <code>
    /// public class UsuarioProfile : Profile
    /// {
    ///     public override void Configure()
    ///     {
    ///         CreateMap&lt;Usuario, UsuarioDto&gt;()
    ///             .ForMember(dto => dto.NomeCompleto, src => src.Nome + " " + src.Sobrenome);
    ///         
    ///         CreateMap&lt;Endereco, EnderecoDto&gt;()
    ///             .ReverseMap();
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public abstract class Profile
    {
        internal MapperConfiguration Configuration { get; private set; }

        /// <summary>Construtor padrão.</summary>
        public Profile()
        {
            Configuration = new MapperConfiguration();
            Configure();
        }

        /// <summary>Método de configuração a ser implementado.</summary>
        public virtual void Configure() { }

        /// <summary>Cria mapeamento.</summary>
        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => Configuration.CreateMap<TSource, TDestination>();
    }

    

}
