// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

using JMSAutoMapper.Configuration;
using System.Linq.Expressions;

namespace JMSAutoMapper.Abstractions
{
    /// <summary>
    /// Interface para expressões de mapeamento.
    /// Fornece uma API fluente para configurar mapeamentos.
    /// </summary>
    /// <typeparam name="TSource">Tipo da origem.</typeparam>
    /// <typeparam name="TDestination">Tipo do destino.</typeparam>
    public interface IMappingExpression<TSource, TDestination>
    {
        /// <summary>Configura mapeamento com expressão lambda.</summary>
        /// <typeparam name="TMember">Tipo do membro.</typeparam>
        /// <param name="destinationProperty">Nome da propriedade destino.</param>
        /// <param name="mappingExpression">Expressão para obter o valor da origem.</param>
        /// <param name="condition">Condição opcional para aplicar o mapeamento.</param>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(string destinationProperty, Expression<Func<TSource, TMember>> mappingExpression, Func<TSource, bool>? condition = null);

        /// <summary>Configura mapeamento por nome de propriedade.</summary>
        /// <param name="destinationProperty">Nome da propriedade destino.</param>
        /// <param name="sourceProperty">Nome da propriedade origem.</param>
        /// <param name="condition">Condição opcional para aplicar o mapeamento.</param>
        IMappingExpression<TSource, TDestination> ForMember(string destinationProperty, string sourceProperty, Func<TSource, bool>? condition = null);

        /// <summary>Configura mapeamento com opções avançadas.</summary>
        /// <typeparam name="TMember">Tipo do membro destino.</typeparam>
        /// <param name="destinationMember">Expressão apontando para a propriedade destino.</param>
        /// <param name="options">Ação de configuração do membro.</param>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options);

        /// <summary>Cria mapeamento reverso (TDestination para TSource).</summary>
        IMappingExpression<TDestination, TSource> ReverseMap();

        /// <summary>Ignora propriedade no mapeamento.</summary>
        /// <typeparam name="TMember">Tipo do membro.</typeparam>
        /// <param name="destinationMember">Expressão apontando para a propriedade a ser ignorada.</param>
        IMappingExpression<TSource, TDestination> Ignore<TMember>(Expression<Func<TDestination, TMember>> destinationMember);

        /// <summary>Especifica construtor a ser usado para criar o objeto destino.</summary>
        /// <param name="parameterTypes">Tipos dos parâmetros do construtor.</param>
        IMappingExpression<TSource, TDestination> UseConstructor(params Type[] parameterTypes);

        /// <summary>Ação executada antes do mapeamento.</summary>
        /// <param name="beforeAction">Ação que recebe origem e destino.</param>
        IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeAction);

        /// <summary>Ação executada após o mapeamento.</summary>
        /// <param name="afterAction">Ação que recebe origem e destino.</param>
        IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterAction);

        /// <summary>Construtor personalizado para criar o objeto destino.</summary>
        /// <param name="constructor">Função que recebe a origem e retorna o destino.</param>
        IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor);

        /// <summary>Inclui mapeamento base.</summary>
        /// <typeparam name="TSourceBase">Tipo base da origem.</typeparam>
        /// <typeparam name="TDestinationBase">Tipo base do destino.</typeparam>
        IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>();

        /// <summary>Configura validação de membros.</summary>
        /// <param name="memberList">Tipo de lista a ser validada.</param>
        IMappingExpression<TSource, TDestination> ValidateMemberList(MemberListType memberList = MemberListType.Destination);

        /// <summary>Configura profundidade máxima de mapeamento.</summary>
        /// <param name="depth">Profundidade máxima.</param>
        IMappingExpression<TSource, TDestination> MaxDepth(int depth);
    }


}
