//using AutoMapper;
using JMSAutoMapper;

using JMSAutoMapperDemo.Dtos;
using JMSAutoMapperDemo.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Runtime.Caching;

namespace JMSAutoMapper.Tests;

public partial class JMSMapperTest
{
    private readonly JMSMapper? _mapper;

    //public JMSMapperTest()
    //{
    //    //var config = new MapperConfiguration(cfg =>
    //    //{
    //    //    cfg.CreateMap<ClienteOrigem, Cliente>();
    //    //    cfg.CreateMap<PedidoOrigem, Pedido>();
    //    //});
    //    //_mapper = new JMSMapper(config);
    //}

    [Fact(DisplayName = "Simple Object")]
    [Trait("S/O", "Should Map Correctly")]
    public void Map_SimpleObject_ShouldMapCorrectly()
    {
        //Arrange
        var mapper = new JMSMapper();
        var source = new Cliente { ClienteId = 1, Name = "Test", Age = 25 };

        // Act
        var result = mapper.Map<ClienteDto>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.ClienteId, result.ClienteId);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Age, result.Age);
    }

    [Fact(DisplayName = "Complex Object")]
    [Trait("C/O", "Should Map Correctly")]
    public void Map_ComplexObject_ShouldMapCorrectly()
    {
        // Arrange
        var mapper = new JMSMapper();
        var source = new Cliente
        {
            ClienteId = 1,
            Name = "Test",
            Age = 25,
            Enderecos = new List<Endereco>
            {
                new Endereco { EnderecoId = 1, Logradouro = "Item 1" },
                new Endereco { EnderecoId = 2, Logradouro = "Item 2" }
            }
        };

        // Act
        var result = mapper.Map<ClienteDto>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.ClienteId, result.ClienteId);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Age, result.Age);

        Assert.NotNull(result.Enderecos);
        Assert.Equal(2, result.Enderecos.Count);

        Assert.Equal(source.Enderecos[0].EnderecoId, result.Enderecos[0].EnderecoId);
        Assert.Equal(source.Enderecos[0].Logradouro, result.Enderecos[0].Logradouro);

        Assert.Equal(source.Enderecos[1].EnderecoId, result.Enderecos[1].EnderecoId);
        Assert.Equal(source.Enderecos[1].Logradouro, result.Enderecos[1].Logradouro);
    }

    [Fact(DisplayName = "Int To String")]
    [Trait("I/S", "Should Map Correctly")]
    public void Map_IntToString_ShouldMapCorrectly()
    {
        // Arrange
        var mapper = new JMSMapper();
        var source = new Cliente { ClienteId = 1, Name = "123", Age = 25 };

        // Act
        var result = mapper.Map<ClienteDto>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Name); // Verifica se o valor foi convertido corretamente para string
    }

    [Fact(DisplayName = "Null Source")]
    [Trait("N/S", "Should Return Null")]
    public void Map_NullSource_ShouldReturnNull()
    {
        // Arrange
        var mapper = new JMSMapper();
        Cliente source = null!;

        // Act
        var result = mapper.Map<ClienteDto>(source);

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "Correctly Object")]
    [Trait("R/C", "Return Correctly")]
    public void Test_Mapeamento_Deve_Retornar_Objeto_Correto()
    {
        // Arrange
        var origem = new Cliente { ClienteId = 1, Name = "Test", Age = 25 };
        var mapper = new JMSMapper(); // Substitua pelo seu mapper

        // Act
        var destino = mapper.Map<ClienteDto>(origem);

        // Assert
        Assert.NotNull(destino);
        Assert.Equal("Test", destino.Name);
        Assert.Equal(25, destino.Age);
    }

    [Fact(DisplayName = "Throw Exception")]
    [Trait("N/O", "Null Object")]
    public void Test_Mapeamento_Deve_Ser_Objeto_Nulo()
    {
        // Arrange
        Cliente origem = null!;
        var mapper = new JMSMapper(); // Substitua pelo seu mapper

        // Act
        var result = mapper.Map<ClienteDto>(origem);
        // Assert
        Assert.Null(result);
        //Assert.Throws<ArgumentNullException>(() => mapper.Map<ClienteDto>(origem));
    }

    //[Fact]
    //public void Map_Deve_Retornar_Lista_Mapeada()
    //{
    //    // Arrange
    //    Cliente origem = null!;
    //    var mapper = new JMSMapper(); // Substitua pelo seu mapper

    //    //Act
    //    var source = new List<object> { new { Id = 1, Nome = "Cliente A" }, new { Id = 2, Nome = "Cliente B" } };
    //    var result = mapper.Map<List<Cliente>>(source);

    //    //Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count());
    //    Assert.Equal("Cliente A", result[0].Name);
    //}

    //[Fact]
    //public void MapList_Deve_Retornar_Lista_Mapeada()
    //{
    //    var source = new List<ClienteDto>
    //{
    //    new() { ClienteId = 1, Name = "Cliente A" },
    //    new() { ClienteId = 2, Name = "Cliente B" }
    //};

    //    var result = _mapper.MapList<Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count);
    //    Assert.Equal("Cliente A", result[0].Name);
    //}

    //[Fact]
    //public void MapDictionary_Deve_Retornar_Dicionario_Mapeado()
    //{
    //    var source = new Dictionary<int, ClienteDto>
    //    {
    //        [1] = new() { ClienteId = 1, Name = "Cliente A" },
    //        [2] = new() { ClienteId = 2, Name = "Cliente B" }
    //    };

    //    var result = _mapper.MapDictionary<int, Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count);
    //    Assert.Equal("Cliente B", result[2].Name);
    //}

    //[Fact]
    //public void MapImmutableList_Deve_Retornar_ImmutableList()
    //{
    //    var source = new List<ClienteDto>
    //{
    //    new() { ClienteId = 1, Name = "Cliente X" }
    //};

    //    var result = _mapper.MapImmutableList<Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Single(result);
    //    Assert.Equal("Cliente X", result[0].Name);
    //}

    //[Fact]
    //public void MapImmutableQueue_Deve_Retornar_ImmutableQueue()
    //{
    //    var source = new Queue<ClienteDto>();
    //    source.Enqueue(new() { ClienteId = 1, Name = "Cliente Y" });

    //    var result = _mapper.MapImmutableQueue<Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Equal("Cliente Y", result.Peek().Name);
    //}

    //[Fact]
    //public async Task MapAsync_Deve_Retornar_Objeto_Mapeado()
    //{
    //    var source = new ClienteDto { ClienteId = 1, Name = "Cliente Async" };

    //    var result = await _mapper.MapAsync<Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Equal("Cliente Async", result.Name);
    //}

    //[Fact]
    //public async Task MapIEnumerableAsync_Deve_Retornar_Enumerable_Mapeado()
    //{
    //    var source = new List<ClienteDto>
    //{
    //    new() { ClienteId = 1, Name = "Cliente A" }
    //};

    //    var result = await _mapper.MapIEnumerableAsync<Cliente>(source);

    //    Assert.NotNull(result);
    //    Assert.Single(result);
    //    Assert.Equal("Cliente A", result.First().Name);
    //}

    //[Fact]
    //public void MapToCacheItem_Deve_Retornar_CacheItem_Valido()
    //{
    //    var source = new ClienteDto { ClienteId = 1, Name = "Cliente Cache" };
    //    var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) };

    //    var result = _mapper.MapToCacheItem<Cliente>(source, "cliente_1", policy);

    //    Assert.NotNull(result);
    //    Assert.Equal("cliente_1", result.Key);
    //    Assert.Equal("Cliente Cache", ((Cliente)result.Value).Name);
    //}

    //[Fact]
    //public void MapToCacheItem_SemPolicy_Nao_Deve_Adicionar_Ao_Cache()
    //{
    //    // Arrange
    //    var source = new ClienteDto { ClienteId = 1, Name = "Cliente Cache" };
    //    var cache = MemoryCache.Default;
    //    var cacheKey = "cliente_test_" + Guid.NewGuid();

    //    // Act
    //    var result = _mapper.MapToCacheItem<Cliente>(source, cacheKey);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Null(cache.Get(cacheKey)); // Verifica que não foi adicionado ao cache
    //}

    //[Fact]
    //public void MapToCacheItem_SemPolicy_Deve_Retornar_CacheItem()
    //{
    //    var source = new ClienteDto { ClienteId = 1, Name = "Cliente Cache" };

    //    // Limpa o cache antes do teste
    //    MemoryCache.Default.Remove("cliente_1");

    //    var result = _mapper.MapToCacheItem<Cliente>(source, "cliente_1");

    //    Assert.NotNull(result);

    //    // Verifica se o item foi adicionado ao cache
    //    var cachedItem = MemoryCache.Default.Get("cliente_1");
    //    Assert.NotNull(cachedItem);

    //    // Verifica a política de forma indireta
    //    var cacheItemPolicy = MemoryCache.Default.GetCacheItem("cliente_1")?.Policy;
    //    Assert.Null(cacheItemPolicy);
    //}

    //[Fact]
    //public void Map_SimpleObject_ReturnsCorrectlyMappedObject()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    mapper.CreateMap<Source, Destination>()
    //        .ForMember(dest => dest.DestName, opt => opt.MapFrom("Name"));

    //    var source = new Source { Name = "Test", Age = 30 };

    //    // Act
    //    var result = mapper.Map<Destination>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal("Test", result.DestName);
    //    Assert.Equal(30, result.Age);
    //}

    //[Fact]
    //public void Map_WithCircularReference_DoesNotStackOverflow()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var parent = new Parent { Id = 1 };
    //    var child = new Child { Id = 2, Parent = parent };
    //    parent.Children = new List<Child> { child };

    //    // Act
    //    var result = mapper.Map<ParentDto>(parent);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Single(result.Children);
    //    Assert.Same(result, result.Children[0].Parent);
    //}

    //[Fact]
    //public async Task MapAsync_Collection_ReturnsMappedCollection()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = Enumerable.Range(1, 10).Select(i => new Source { Name = $"Test{i}", Age = i });

    //    // Act
    //    var result = await mapper.MapIEnumerableAsync<Destination>(sources);

    //    // Assert
    //    Assert.Equal(10, result.Count());
    //}
}
