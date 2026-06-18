using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;

namespace JMSAutoMapper.Tests
{
    public class ComplexMapTests
    {
        [Fact(DisplayName = "Complex Map - Nested Objects")]
        [Trait("Feature", "Complex Mapping")]
        public void Map_WithNestedObjects_ShouldMapCorrectly()
        {
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>().ReverseMap();
            config.CreateMap<EnderecoEntity, EnderecoDto>().ReverseMap();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = "Maria",
                Endereco = new EnderecoEntity { Rua = "Rua A", Cidade = "São Paulo", CEP = "01234-567" }
            };

            var result = mapper.Map<UsuarioDTO>(entity);

            result.Endereco.Should().NotBeNull();
            result.Endereco.Rua.Should().Be(entity.Endereco.Rua);
        }

        [Fact(DisplayName = "Complex Map - Nested Collections")]
        [Trait("Feature", "Complex Mapping")]
        public void Map_WithCollections_ShouldMapAllItems()
        {
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<PedidoEntity, PedidoDto>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Pedidos = new List<PedidoEntity> { new PedidoEntity { Id = 1, Produto = "Notebook" } }
            };

            var result = mapper.Map<UsuarioDTO>(entity);
            result.Pedidos.Should().HaveCount(1);
            result.Pedidos[0].Produto.Should().Be("Notebook");
        }

        [Fact(DisplayName = "Circular Reference - Object Graph Safety")]
        [Trait("Category", "Safety")]
        public void Map_ShouldHandleCircularReferences()
        {
            var config = new MapperConfiguration();
            config.CreateMap<PersonSource, PersonDestination>();
            config.CreateMap<ChildSource, ChildDestination>();
            var mapper = new JMSMapper(config);

            var personSource = new PersonSource { Id = 1, RazaoSocial = "Parent" };
            var childSource = new ChildSource { Id = 2, RazaoSocial = "Child", Parent = personSource };
            personSource.Child = childSource;

            var result = mapper.Map<PersonDestination>(personSource);
            result.Child!.Parent.Should().BeSameAs(result);
        }
    }
}
