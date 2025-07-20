//using AutoMapper;
using JMSAutoMapper;

using JMSAutoMapperDemo.Dtos;
using JMSAutoMapperDemo.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Runtime.Caching;

namespace JMSAutoMapper.Tests;

public partial class JMSMapperTest
{
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
    }
