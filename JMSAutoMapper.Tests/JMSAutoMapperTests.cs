using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using FluentAssertions;

namespace JMSAutoMapper.Tests
{
    public class JMSAutoMapperTests
    {
        // ===== CLASSES DE TESTE =====
        public class UsuarioEntity
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public int Idade { get; set; }
            public decimal Salario { get; set; }
            public DateTime DataCriacao { get; set; }
            public EnderecoEntity Endereco { get; set; }
            public List<PedidoEntity> Pedidos { get; set; } = new();
        }

        public class UsuarioDTO
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public int Idade { get; set; }
            public decimal Salario { get; set; }
            public DateTime DataCriacao { get; set; }
            public EnderecoDTO Endereco { get; set; }
            public List<PedidoDTO> Pedidos { get; set; } = new();
            public string NomeCompleto { get; set; }
        }

        public class EnderecoEntity
        {
            public string Rua { get; set; }
            public string Cidade { get; set; }
            public string CEP { get; set; }
        }

        public class EnderecoDTO
        {
            public string Rua { get; set; }
            public string Cidade { get; set; }
            public string CEP { get; set; }
        }

        public class PedidoEntity
        {
            public int Id { get; set; }
            public string Produto { get; set; }
            public decimal Valor { get; set; }
        }

        public class PedidoDTO
        {
            public int Id { get; set; }
            public string Produto { get; set; }
            public decimal Valor { get; set; }
        }

        public class ProdutoEntity
        {
            public int Codigo { get; set; }
            public string Descricao { get; set; }
            public double Preco { get; set; }
        }

        public class ProdutoModel
        {
            public int Codigo { get; set; }
            public string Descricao { get; set; }
            public decimal Preco { get; set; } // Tipo diferente
        }

        public class ClienteEntity
        {
            public string PrimeiroNome { get; set; }
            public string UltimoNome { get; set; }
            public bool Ativo { get; set; }
        }

        public class ClienteDTO
        {
            public string NomeCompleto { get; set; }
            public string Status { get; set; }
        }

        public class ConfiguracaoResolvedor : IValueResolver<ClienteEntity, ClienteDTO, string>
        {
            public string Resolve(ClienteEntity source, ClienteDTO destination, string destMember, ResolutionContext context)
            {
                return source.Ativo ? "Ativo" : "Inativo";
            }
        }

        // ===== TESTES DE CONFIGURAÇÃO =====

        [Fact]
        public void Configuration_ShouldBeValid_WhenAllMappingsAreConfigured()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().NotThrow();
        }

        [Fact]
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
                  .WithMessage("*Unmapped properties*");
        }

        // ===== TESTES DE MAPEAMENTO BÁSICO =====

        [Fact]
        public void Map_UsuarioEntityToUsuarioDTO_ShouldMapBasicProperties()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                Nome = "João Silva",
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
            result.Nome.Should().Be(entity.Nome);
            result.Email.Should().Be(entity.Email);
            result.Idade.Should().Be(entity.Idade);
            result.Salario.Should().Be(entity.Salario);
            result.DataCriacao.Should().Be(entity.DataCriacao);
        }

        [Fact]
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

        [Fact]
        public void Map_WithNullSource_ShouldReturnDefault()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            // Act
            var result = mapper.Map<UsuarioDTO>((UsuarioEntity)null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Map_WithNullProperties_ShouldHandleGracefully()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                Nome = null,
                Email = "test@email.com",
                Endereco = null
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().BeNull();
            result.Endereco.Should().BeNull();
        }

        // ===== TESTES DE MAPEAMENTO COMPLEXO =====

        [Fact]
        public void Map_WithNestedObjects_ShouldMapCorrectly()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<EnderecoEntity, EnderecoDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                Nome = "Maria",
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

        [Fact]
        public void Map_WithCollections_ShouldMapAllItems()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<PedidoEntity, PedidoDTO>();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                Nome = "Carlos",
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

        [Fact]
        public void Map_WithCustomMapping_ShouldUseCustomLogic()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDTO>()
                  .ForMember("NomeCompleto", src => $"{src.PrimeiroNome} {src.UltimoNome}")
                  .ForMember("Status", src => src.Ativo ? "Ativo" : "Inativo");
            var mapper = config.CreateMapper();

            var entity = new ClienteEntity
            {
                PrimeiroNome = "Ana",
                UltimoNome = "Santos",
                Ativo = true
            };

            // Act
            var result = mapper.Map<ClienteDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.NomeCompleto.Should().Be("Ana Santos");
            result.Status.Should().Be("Ativo");
        }

        [Fact]
        public void Map_WithConditionalMapping_ShouldApplyCondition()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDTO>()
                  .ForMember("Status", src => "Premium", src => src.Ativo);
            var mapper = config.CreateMapper();

            var entityAtivo = new ClienteEntity { PrimeiroNome = "A", UltimoNome = "B", Ativo = true };
            var entityInativo = new ClienteEntity { PrimeiroNome = "C", UltimoNome = "D", Ativo = false };

            // Act
            var resultAtivo = mapper.Map<ClienteDTO>(entityAtivo);
            var resultInativo = mapper.Map<ClienteDTO>(entityInativo);

            // Assert
            resultAtivo.Status.Should().Be("Premium");
            resultInativo.Status.Should().BeNull(); // Não deve mapear quando condição é falsa
        }

        [Fact]
        public void Map_WithValueResolver_ShouldUseResolver()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<ClienteEntity, ClienteDTO>()
                  .ForMember(dest => dest.Status, opt => opt.MapFrom<ConfiguracaoResolvedor>());
            var mapper = config.CreateMapper();

            var entity = new ClienteEntity { PrimeiroNome = "Teste", UltimoNome = "Silva", Ativo = false };

            // Act
            var result = mapper.Map<ClienteDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Inativo");
        }

        // ===== TESTES DE IGNORAR PROPRIEDADES =====

        [Fact]
        public void Map_WithIgnoredProperty_ShouldNotMapProperty()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .Ignore(dest => dest.NomeCompleto);
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity
            {
                Id = 1,
                Nome = "João",
                Email = "joao@email.com"
            };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Should().NotBeNull();
            result.NomeCompleto.Should().BeNull(); // Propriedade ignorada
            result.Nome.Should().Be("João"); // Outras propriedades mapeadas normalmente
        }

        // ===== TESTES DE CONSTRUTORES =====

        public class ClienteComConstrutor
        {
            public string Nome { get; }
            public int Idade { get; }

            public ClienteComConstrutor(string nome, int idade)
            {
                Nome = nome;
                Idade = idade;
            }
        }

        [Fact]
        public void Map_WithConstructorParameters_ShouldUseConstructor()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, ClienteComConstrutor>()
                  .UseConstructor(typeof(string), typeof(int))
                  .ForMember("Nome", "Nome")
                  .ForMember("Idade", "Idade");
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { Nome = "Carlos", Idade = 25 };

            // Act
            var result = mapper.Map<ClienteComConstrutor>(entity);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be("Carlos");
            result.Idade.Should().Be(25);
        }

        // ===== TESTES DE BEFORE/AFTER MAP =====

        [Fact]
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

            var entity = new UsuarioEntity { Nome = "Teste" };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            beforeMapCalled.Should().BeTrue();
            afterMapCalled.Should().BeTrue();
        }

        // ===== TESTES DE COLETIONS =====

        [Fact]
        public void MapIEnumerable_ShouldMapAllItems()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, Nome = "A" },
                new UsuarioEntity { Id = 2, Nome = "B" },
                new UsuarioEntity { Id = 3, Nome = "C" }
            };

            // Act
            var result = mapper.MapIEnumerable<UsuarioDTO>(entities);

            // Assert
            result.Should().HaveCount(3);
            result.Select(x => x.Id).Should().Equal(1, 2, 3);
        }

        [Fact]
        public void MapList_ShouldReturnList()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, Nome = "A" },
                new UsuarioEntity { Id = 2, Nome = "B" }
            };

            // Act
            var result = mapper.MapList<UsuarioDTO>(entities);

            // Assert
            result.Should().BeOfType<List<UsuarioDTO>>();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void MapArray_ShouldReturnArray()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, Nome = "A" },
                new UsuarioEntity { Id = 2, Nome = "B" }
            };

            // Act
            var result = mapper.MapArray<UsuarioDTO>(entities);

            // Assert
            result.Should().BeOfType<UsuarioDTO[]>();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void MapDictionary_ShouldMapKeyValuePairs()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var dictionary = new Dictionary<int, UsuarioEntity>
            {
                [1] = new UsuarioEntity { Id = 1, Nome = "Usuario 1" },
                [2] = new UsuarioEntity { Id = 2, Nome = "Usuario 2" }
            };

            // Act
            var result = mapper.MapDictionary<int, UsuarioDTO>(dictionary);

            // Assert
            result.Should().HaveCount(2);
            result[1].Nome.Should().Be("Usuario 1");
            result[2].Nome.Should().Be("Usuario 2");
        }

        // ===== TESTES DE MAPEAMENTO COM DESTINO =====

        [Fact]
        public void Map_WithDestination_ShouldUpdateExistingObject()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var source = new UsuarioEntity { Id = 1, Nome = "Novo Nome", Email = "novo@email.com" };
            var destination = new UsuarioDTO { Id = 1, Nome = "Nome Antigo", Email = "antigo@email.com" };

            // Act
            var result = mapper.Map(source, destination);

            // Assert
            result.Should().BeSameAs(destination);
            result.Nome.Should().Be("Novo Nome");
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

        [Fact]
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

        [Fact]
        public void Map_WithNullSubstitute_ShouldUseSubstituteValue()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ForMember(dest => dest.Nome, opt => opt.NullSubstitute("Nome Padrão"));
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { Nome = null };

            // Act
            var result = mapper.Map<UsuarioDTO>(entity);

            // Assert
            result.Nome.Should().Be("Nome Padrão");
        }

        // ===== TESTES DE REVERSE MAP =====

        [Fact]
        public void ReverseMap_ShouldCreateBidirectionalMapping()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ReverseMap();
            var mapper = config.CreateMapper();

            var entity = new UsuarioEntity { Id = 1, Nome = "Original" };
            var dto = new UsuarioDTO { Id = 2, Nome = "DTO" };

            // Act
            var entityFromDto = mapper.Map<UsuarioEntity>(dto);
            var dtoFromEntity = mapper.Map<UsuarioDTO>(entity);

            // Assert
            entityFromDto.Should().NotBeNull();
            entityFromDto.Nome.Should().Be("DTO");
            dtoFromEntity.Should().NotBeNull();
            dtoFromEntity.Nome.Should().Be("Original");
        }

        // ===== TESTES DE MAPEAMENTO DE TIPOS IMUTÁVEIS =====

        [Fact]
        public void MapImmutableList_ShouldReturnImmutableList()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = new List<UsuarioEntity>
            {
                new UsuarioEntity { Id = 1, Nome = "A" },
                new UsuarioEntity { Id = 2, Nome = "B" }
            };

            // Act
            var result = mapper.MapImmutableList<UsuarioDTO>(entities);

            // Assert
            result.Should().BeOfType<System.Collections.Immutable.ImmutableList<UsuarioDTO>>();
            result.Should().HaveCount(2);
        }

        // ===== TESTES DE VALIDAÇÃO DE CONFIGURAÇÃO =====

        [Fact]
        public void AssertConfigurationIsValid_ShouldValidateAllMappings()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.CreateMap<EnderecoEntity, EnderecoDTO>();
            config.CreateMap<PedidoEntity, PedidoDTO>();

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
            public string StatusString { get; set; }
        }

        [Fact]
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

        [Fact]
        public void Map_MultipleItems_ShouldHandleLargeCollections()
        {
            // Arrange
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();

            var entities = Enumerable.Range(1, 1000)
                .Select(i => new UsuarioEntity { Id = i, Nome = $"Usuario {i}" })
                .ToList();

            // Act
            var result = mapper.MapList<UsuarioDTO>(entities);

            // Assert
            result.Should().HaveCount(1000);
            result[999].Nome.Should().Be("Usuario 1000");
        }
    }
}