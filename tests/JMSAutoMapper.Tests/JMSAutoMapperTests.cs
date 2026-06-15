﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using FluentAssertions;
using JMSAutoMapper.Core;
using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.ConsoleSample.Models;
using JMSAutoMapper.DependencyInjection; // Adicionado para MapperExtensions
using JMSAutoMapper.ConsoleSample;
using JMSAutoMapper.ConsoleSample.Dtos;

namespace JMSAutoMapper.Tests
{
    public class JMSAutoMapperTests
    {
      
        // ===== TESTES DE CONFIGURAÇÃO =====

        [Fact(DisplayName = "Config - Valid Mapping Registration")]
        [Trait("Feature", "Configuration")]
        public void Configuration_ShouldBeValid_WhenAllMappingsAreConfigured()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().NotThrow();
        }

        [Fact(DisplayName = "Config - Validation Error on Unmapped Properties")]
        [Trait("Feature", "Configuration")]
        public void Configuration_ShouldThrow_WhenUnmappedPropertiesExist()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ForMember("Id", "Id") // Apenas uma propriedade mapeada
                  .ValidateMemberList(MemberListType.Destination);

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().Throw<MappingException>()
                  .WithMessage("*Propriedades não mapeadas*");
        }

        [Fact(DisplayName = "Config - Inline Configuration in Constructor")]
        [Trait("Feature", "Configuration")]
        public void Constructor_WithAction_ShouldConfigureCorrectly()
        {
            // Arrange & Act
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UsuarioEntity, UsuarioDTO>();
                cfg.AddProfile<TestProfile>();
            });

            // Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().NotThrow();

            var mapper = config.CreateMapper();
            var entity = new UsuarioEntity { Id = 1, RazaoSocial = "Fluent Test" };
            var dto = mapper.Map<UsuarioDTO>(entity);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(entity.Id);
            dto.RazaoSocial.Should().Be(entity.RazaoSocial);
        }

        // ===== TESTES DE MAPEAMENTO BÁSICO =====

        [Fact(DisplayName = "Basic Map - Entity to DTO properties")]
        [Trait("Feature", "Mapping")]
        public void Map_UsuarioEntityToUsuarioDTO_ShouldMapBasicProperties()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = "João Silva",
                Email = "joao@email.com",
                Idade = 30,
                Salario = 5000.50m,
                DataCriacao = new DateTime(2023, 1, 1)
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.RazaoSocial.Should().Be(entity.RazaoSocial);
            result.Email.Should().Be(entity.Email);
            result.Idade.Should().Be(entity.Idade);
            result.Salario.Should().Be(entity.Salario);
            result.DataCriacao.Should().Be(entity.DataCriacao);
        }

        [Fact(DisplayName = "Numeric Map - Implicit Conversions")]
        [Trait("Feature", "Numeric Conversions")]
        public void Map_WithDifferentTypes_ShouldConvertImplicitly()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ProdutoEntity, ProdutoModel>();
            var mapper = config.CreateMapper();

            var entity = new ProdutoEntity
            {
                Codigo = 100,
                Descricao = "Notebook",
                Preco = 2500.75
            };

            // Act
            var result = mapper.Map<ProdutoModel>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Codigo.Should().Be(entity.Codigo);
            result.Descricao.Should().Be(entity.Descricao);
            result.Preco.Should().Be((decimal)entity.Preco);
        }

        // ===== TESTES DE COMPORTAMENTO COM DADOS INCONSISTENTES =====

        [Fact(DisplayName = "Safety - Null Source handling")]
        [Trait("Feature", "Safety")]
        public void Map_WithNullSource_ShouldReturnDefault()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            // Act
            var result = mapper.Map<UsuarioDTO>((UsuarioEntity)null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "Safety - Null Properties handling")]
        [Trait("Feature", "Safety")]
        public void Map_WithNullProperties_ShouldHandleGracefully()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = null!, // CS8625
                Email = "test@email.com",
                Endereco = null! // CS8625
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.RazaoSocial.Should().BeNull();
            result.Endereco.Should().BeNull();
        }

        // ===== TESTES DE MAPEAMENTO COMPLEXO =====

        [Fact(DisplayName = "Complex Map - Nested Objects")]
        [Trait("Feature", "Complex Mapping")]
        public void Map_WithNestedObjects_ShouldMapCorrectly()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>().ReverseMap();
            config.CreateMap<EnderecoEntity, EnderecoDto>().ReverseMap();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = "Maria",
                Endereco = new EnderecoEntity
                {
                    Rua = "Rua A",
                    Cidade = "São Paulo",
                    CEP = "01234-567"
                }
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Endereco.Should().NotBeNull();
            result.Endereco.Rua.Should().Be(entity.Endereco.Rua);
            result.Endereco.Cidade.Should().Be(entity.Endereco.Cidade);
            result.Endereco.CEP.Should().Be(entity.Endereco.CEP);
        }

        [Fact(DisplayName = "Complex Map - Collections Mapping")]
        [Trait("Feature", "Complex Mapping")]
        public void Map_WithCollections_ShouldMapAllItems()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<PedidoEntity, PedidoDto>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = "Carlos",
                Pedidos = new List<PedidoEntity>
                {
                    new PedidoEntity { Id = 1, Produto = "Livro", Valor = 29.99m },
                    new PedidoEntity { Id = 2, Produto = "Mouse", Valor = 89.90m }
                }
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Pedidos.Should().HaveCount(2);
            result.Pedidos[0].Id.Should().Be(1);
            result.Pedidos[0].Produto.Should().Be("Livro");
            result.Pedidos[1].Id.Should().Be(2);
            result.Pedidos[1].Produto.Should().Be("Mouse");
        }

        // ===== TESTES DE TRANSFORMAÇÃO CUSTOMIZADA =====

        [Fact(DisplayName = "Custom Map - String Concatenation")]
        [Trait("Feature", "Custom Logic")]
        public void Map_WithCustomMapping_ShouldUseCustomLogic()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDto>()
                  .ForMember("RazaoSocialCompleto", src => $"{src.PrimeiroRazaoSocial} {src.UltimoRazaoSocial}")
                  .ForMember("Status", src => src.Ativo ? "Ativo" : "Inativo");
            var mapper = config.CreateMapper();

            var entity = new ClienteEntity
            {
                PrimeiroRazaoSocial = "Ana",
                UltimoRazaoSocial = "Santos",
                Ativo = true
            };

            // Act
            var result = mapper.Map<ClienteDto>(entity);

            // Assert
            result.Should().NotBeNull();
            result.RazaoSocialCompleto.Should().Be("Ana Santos");
            result.Status.Should().Be("Ativo");
        }

        [Fact(DisplayName = "Advanced Map - Conditional Projection")]
        [Trait("Feature", "Advanced")]
        public void Map_WithConditionalMapping_ShouldApplyCondition()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDto>()
                  .ForMember("Status", src => "Premium", src => src.Ativo);
            var mapper = config.CreateMapper();

            var entityAtivo = new ClienteEntity { PrimeiroRazaoSocial = "A", UltimoRazaoSocial = "B", Ativo = true };
            var entityInativo = new ClienteEntity { PrimeiroRazaoSocial = "C", UltimoRazaoSocial = "D", Ativo = false };

            // Act
            var resultAtivo = mapper.Map<ClienteDto>(entityAtivo);
            var resultInativo = mapper.Map<ClienteDto>(entityInativo);

            // Assert
            resultAtivo.Status.Should().Be("Premium");
            resultInativo.Status.Should().BeNull(); // Não deve mapear quando condição é falsa
        }

        [Fact(DisplayName = "Advanced Map - External Value Resolver")]
        [Trait("Feature", "Advanced")]
        public void Map_WithValueResolver_ShouldUseResolver()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDto>()
                  .ForMember(dest => dest.Status, opt => opt.MapFrom<ConfiguracaoResolvedor>());
            var mapper = config.CreateMapper();

            var entity = new ClienteEntity { PrimeiroRazaoSocial = "Teste", UltimoRazaoSocial = "Silva", Ativo = false };

            // Act
            var result = mapper.Map<ClienteDto>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Inativo");
        }

        // ===== TESTES DE IGNORAR PROPRIEDADES =====

        [Fact(DisplayName = "Advanced Map - Explicit Property Ignore")]
        [Trait("Feature", "Mapping")]
        public void Map_WithIgnoredProperty_ShouldNotMapProperty()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .Ignore(dest => dest.RazaoSocialCompleto);
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                RazaoSocial = "João",
                Email = "joao@email.com"
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.RazaoSocialCompleto.Should().BeNull(); // Propriedade ignorada
            result.RazaoSocial.Should().Be("João"); // Outras propriedades mapeadas normalmente
        }

        // ===== TESTES DE CONSTRUTORES =====

        public class ClienteComConstrutor
        {
            public string RazaoSocial { get; }
            public int Idade { get; }

            public ClienteComConstrutor(string razaoSocial, int idade)
            {
                RazaoSocial = razaoSocial;
                Idade = idade;
            }
        }

        [Fact(DisplayName = "Advanced Map - Parameterized Constructor Injection")]
        [Trait("Feature", "Advanced")]
        public void Map_WithConstructorParameters_ShouldUseConstructor()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, ClienteComConstrutor>()
                  .UseConstructor(typeof(string), typeof(int))
                  .ForMember("RazaoSocial", "RazaoSocial")
                  .ForMember("Idade", "Idade");
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { RazaoSocial = "Carlos", Idade = 25 };

            // Act
            var result = mapper.Map<ClienteComConstrutor>(entity);

            // Assert
            result.Should().NotBeNull();
            result.RazaoSocial.Should().Be("Carlos");
            result.Idade.Should().Be(25);
        }

        // ===== TESTES DE BEFORE/AFTER MAP =====

        [Fact(DisplayName = "Lifecycle - Before and After Actions")]
        [Trait("Feature", "Lifecycle")]
        public void Map_WithBeforeAndAfterMap_ShouldExecuteActions()
        {
            // Arrange
            var beforeMapCalled = false;
            var afterMapCalled = false;

            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .BeforeMap((src, dest) => beforeMapCalled = true)
                  .AfterMap((src, dest) => afterMapCalled = true);
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { RazaoSocial = "Teste" };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            beforeMapCalled.Should().BeTrue();
            afterMapCalled.Should().BeTrue();
        }

        // ===== TESTES DE COLETIONS =====

        [Fact(DisplayName = "Collection Map - IEnumerable support")]
        [Trait("Feature", "Collections")]
        public void MapIEnumerable_ShouldMapAllItems()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, RazaoSocial = "A" },
                new UsuarioEntity { Id = 2, RazaoSocial = "B" },
                new UsuarioEntity { Id = 3, RazaoSocial = "C" }
            };

            // Act
            var result = mapper.Map<IEnumerable<UsuarioDTO>>(entities);

            // Assert
            result.Should().HaveCount(3);
            result.Select(x => x.Id).Should().Equal(1, 2, 3);
        }

        [Fact(DisplayName = "Collection Map - List support")]
        [Trait("Feature", "Collections")]
        public void MapList_ShouldReturnList()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, RazaoSocial = "A" },
                new UsuarioEntity { Id = 2, RazaoSocial = "B" }
            };

            // Act
            var result = mapper.Map<List<UsuarioDTO>>(entities);

            // Assert
            result.Should().BeOfType<List<UsuarioDTO>>();
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "Collection Map - Array support")]
        [Trait("Feature", "Collections")]
        public void MapArray_ShouldReturnArray()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, RazaoSocial = "A" },
                new UsuarioEntity { Id = 2, RazaoSocial = "B" }
            };

            // Act
            var result = mapper.Map<UsuarioDTO[]>(entities);

            // Assert
            result.Should().BeOfType<UsuarioDTO[]>();
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "Collection Map - Dictionary support")]
        [Trait("Feature", "Collections")]
        public void MapDictionary_ShouldMapKeyValuePairs()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var dictionary = new Dictionary<int, UsuarioEntity>
            {
                [1] = new UsuarioEntity { Id = 1, RazaoSocial = "Usuario 1" },
                [2] = new UsuarioEntity { Id = 2, RazaoSocial = "Usuario 2" }
            };

            // Act
            var result = mapper.Map<Dictionary<int, UsuarioDTO>>(dictionary);

            // Assert
            result.Should().HaveCount(2);
            result[1].RazaoSocial.Should().Be("Usuario 1");
            result[2].RazaoSocial.Should().Be("Usuario 2");
        }

        // ===== TESTES DE MAPEAMENTO COM DESTINO =====

        [Fact(DisplayName = "Advanced Map - Update Existing Object")]
        [Trait("Feature", "Mapping")]
        public void Map_WithDestination_ShouldUpdateExistingObject()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var source = new UsuarioEntity { Id = 1, RazaoSocial = "Novo RazaoSocial", Email = "novo@email.com" };
            var destination = new UsuarioDTO { Id = 1, RazaoSocial = "RazaoSocial Antigo", Email = "antigo@email.com" };

            // Act
            var result = mapper.Map(source, destination);

            // Assert
            result.Should().BeSameAs(destination);
            result.RazaoSocial.Should().Be("Novo RazaoSocial");
            result.Email.Should().Be("novo@email.com");
        }

        // ===== TESTES DE PERFIS =====

        public class TestProfile : Profile
        {
            public override void Configure()
            {
                CreateMap<UsuarioEntity, UsuarioDTO>();
            }
        }

        [Fact(DisplayName = "Config - Profile Integration")]
        [Trait("Feature", "Configuration")]
        public void AddProfile_ShouldRegisterMappingsFromProfile()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.AddProfile<TestProfile>();

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().NotThrow();
        }

        // ===== TESTES DE VALORES NULOS E SUBSTITUIÇÃO =====

        [Fact(DisplayName = "Advanced Map - Null Substitute")]
        [Trait("Feature", "Advanced")]
        public void Map_WithNullSubstitute_ShouldUseSubstituteValue()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ForMember(dest => dest.RazaoSocial, opt => opt.NullSubstitute("RazaoSocial Padrão"));
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { RazaoSocial = null! }; // CS8625

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.RazaoSocial.Should().Be("RazaoSocial Padrão");
        }

        // ===== TESTES DE REVERSE MAP =====

        [Fact(DisplayName = "Advanced Map - Bidirectional Mapping")]
        [Trait("Feature", "Advanced")]
        public void ReverseMap_ShouldCreateBidirectionalMapping()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ReverseMap();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { Id = 1, RazaoSocial = "Original" };
            var dto = new UsuarioDTO { Id = 2, RazaoSocial = "DTO" };

            // Act
            var entityFromDto = mapper.Map<UsuarioEntity>(dto);
            var dtoFromEntity = mapper.Map<UsuarioDTO>(entity);

            // Assert
            entityFromDto.Should().NotBeNull();
            entityFromDto.RazaoSocial.Should().Be("DTO");
            dtoFromEntity.Should().NotBeNull();
            dtoFromEntity.RazaoSocial.Should().Be("Original");
        }

        // ===== TESTES DE MAPEAMENTO DE TIPOS IMUTÁVEIS =====

        [Fact(DisplayName = "Collection Map - ImmutableList support")]
        [Trait("Feature", "Collections")]
        public void MapImmutableList_ShouldReturnImmutableList()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, RazaoSocial = "A" },
                new UsuarioEntity { Id = 2, RazaoSocial = "B" }
            };

            // Act
            var result = mapper.Map<ImmutableList<UsuarioDTO>>(entities);

            // Assert
            result.Should().BeOfType<System.Collections.Immutable.ImmutableList<UsuarioDTO>>();
            result.Should().HaveCount(2);
        }

        // ===== TESTES DE VALIDAÇÃO DE CONFIGURAÇÃO =====

        [Fact(DisplayName = "Safety - Global Configuration Validation")]
        [Trait("Feature", "Safety")]
        public void AssertConfigurationIsValid_ShouldValidateAllMappings()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<EnderecoEntity, EnderecoDto>();
            config.CreateMap<PedidoEntity, PedidoDto>();

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().NotThrow();
        }

        // ===== TESTES DE MAPEAMENTO DE ENUMS =====

        public enum StatusPedido
        {
            Pendente,
            Processando,
            Concluido
        }

        public class PedidoComStatusEntity
        {
            public StatusPedido Status { get; set; }
        }

        public class PedidoComStatusDTO
        {
            public StatusPedido Status { get; set; }
            public string StatusString { get; set; } = default!; // CS8618
        }

        [Fact(DisplayName = "Enum Map - Conversion and ToString support")]
        [Trait("Feature", "Enums")]
        public void Map_WithEnums_ShouldMapCorrectly()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<PedidoComStatusEntity, PedidoComStatusDTO>()
                  .ForMember("StatusString", src => src.Status.ToString());
            var mapper = config.CreateMapper();

            var entity = new PedidoComStatusEntity { Status = StatusPedido.Concluido };

            // Act
            var result = mapper.Map<PedidoComStatusDTO>(entity);

            // Assert
            result.Status.Should().Be(StatusPedido.Concluido);
            result.StatusString.Should().Be("Concluido");
        }

        // ===== TESTES DE PERFORMANCE (BÁSICOS) =====

        [Fact(DisplayName = "Load Test - Collection scalability")]
        [Trait("Feature", "Performance")]
        public void Map_MultipleItems_ShouldHandleLargeCollections()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = Enumerable.Range(1, 1000)
                .Select(i => new UsuarioEntity { Id = i, RazaoSocial = $"Usuario {i}" })
                .ToList();

            // Act
            var result = mapper.Map<List<UsuarioDTO>>(entities);

            // Assert
            result.Should().HaveCount(1000);
            result[999].RazaoSocial.Should().Be("Usuario 1000");
        }

        // ===== TESTES PARA O CENÁRIO DE COMPRA SOLICITADO =====

        [Fact(DisplayName = "Complex Map - Nested Collections")]
        [Trait("Feature", "Complex Mapping")]
        public void Map_CompraToDto_ShouldMapNestedCollectionsAndProperties()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<Fornecedor, FornecedorDto>();
                cfg.CreateMap<ItemCompra, ItemCompraDto>();
                cfg.CreateMap<ContasAPagar, ContasAPagarDto>();
                
                cfg.CreateMap<Compra, CompraDto>()
                   .ForMember(dest => dest.Fornecedor, opt => opt.MapFrom(src => src.Fornecedor))
                   .ForMember(dest => dest.ItensCompra, opt => opt.MapFrom(src => src.ItensCompra))
                   .ForMember(dest => dest.ContasAPagars, opt => opt.MapFrom(src => src.ContasAPagar))
                   .ReverseMap();
            });
            var mapper = config.CreateMapper();

            var compra = new Compra {
                Id = 1,
                Fornecedor = new Fornecedor { Nome = "Fornecedor ABC" },
                ItensCompra = new List<ItemCompra> { new ItemCompra { Produto = "Notebook" } },
                ContasAPagar = new List<ContasAPagar> { new ContasAPagar { Valor = 5000 } }
            };

            // Act
            var dto = mapper.Map<CompraDto>(compra);

            // Assert
            dto.Should().NotBeNull();
            dto.Fornecedor.Nome.Should().Be("Fornecedor ABC");
            dto.ItensCompra.Should().HaveCount(1);
            dto.ContasAPagars.Should().HaveCount(1);
            dto.ContasAPagars[0].Valor.Should().Be(5000);
        }

        [Fact(DisplayName = "Advanced Map - ReverseMap with Ignores")]
        [Trait("Feature", "Advanced")]
        public void Map_DtoToCompra_ShouldRespectIgnoreInReverseMap()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<Compra, CompraDto>()
                   .ForMember(dest => dest.ContasAPagars, opt => opt.MapFrom(src => src.ContasAPagar))
                   .ReverseMap()
                   .ForMember(dest => dest.Fornecedor, opt => opt.Ignore())
                   .ForMember(dest => dest.ItensCompra, opt => opt.Ignore())
                   .ForMember(dest => dest.ContasAPagar, opt => opt.Ignore());
            });
            var mapper = config.CreateMapper();

            var dto = new CompraDto {
                Id = 10,
                Fornecedor = new FornecedorDto { Nome = "Não deve mapear" }
            };

            // Act
            var entity = mapper.Map<Compra>(dto);

            // Assert
            entity.Id.Should().Be(10);
            entity.Fornecedor.Should().BeNull(); // Ignorado no ReverseMap
            entity.ItensCompra.Should().BeEmpty(); // Coleção nula na origem vira vazia/nula conforme política
        }
    }
}