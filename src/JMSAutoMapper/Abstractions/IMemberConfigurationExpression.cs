// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using System.Linq.Expressions;

namespace JMSAutoMapper.Abstractions
{
    /// <summary>
    /// Interface para configuração de membros.
    /// Fornece opções avançadas para configurar uma propriedade específica.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    /// <typeparam name="TMember">Tipo do membro.</typeparam>
    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>Mapeia de expressão de origem.</summary>
        void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

        /// <summary>Mapeia de função com origem e destino.</summary>
        void MapFrom(Func<TSource, TDestination, TMember> resolver);

        /// <summary>Mapeia de value resolver síncrono (cria nova instância).</summary>
        void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new();

        /// <summary>Mapeia de value resolver assíncrono (cria nova instância).</summary>
        void MapFromAsync<TAsyncResolver>() where TAsyncResolver : IAsyncValueResolver<TSource, TDestination, TMember>, new();

        /// <summary>Mapeia de instância de value resolver síncrono.</summary>
        void MapFrom(IValueResolver<TSource, TDestination, TMember> resolver);

        /// <summary>Mapeia de instância de value resolver assíncrono.</summary>
        void MapFromAsync(IAsyncValueResolver<TSource, TDestination, TMember> resolver);

        /// <summary>Ignora o membro no mapeamento.</summary>
        void Ignore();

        /// <summary>Condição para mapeamento.</summary>
        void Condition(Func<TSource, bool> condition);

        /// <summary>Condição assíncrona para mapeamento.</summary>
        void ConditionAsync(Func<TSource, CancellationToken, Task<bool>> condition);

        /// <summary>Substitui valor nulo por valor especificado.</summary>
        void NullSubstitute(object value);

        /// <summary>Usa valor de destino existente (não substitui se já tiver valor).</summary>
        void UseDestinationValue();

        /// <summary>Converte valor usando função.</summary>
        void ConvertUsing(Func<TMember, object> converter);

        /// <summary>Mapeia de membro de origem por nome.</summary>
        void MapFromSourceMember(string sourceMemberName);
    }


}
