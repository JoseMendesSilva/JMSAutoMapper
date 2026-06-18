using System;
using System.Collections.Generic;

namespace JMSAutoMapper.Tests.Infrastructure
{
    public class Source { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public DateTime BirthDate { get; set; } public Source Parent { get; set; } = default!; public List<Source> Children { get; set; } = new(); }
    public class Destination { public int Id { get; set; } public string? FullRazaoSocial { get; set; } public int Age { get; set; } public Destination Parent { get; set; } = default!; public List<Destination> Children { get; set; } = new(); }
    
    public class SourceA { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public double Value { get; set; } }
    public class DestinationA { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public double Value { get; set; } }

    public class SourceB { public string OriginalProperty { get; set; } = default!; }
    public class DestinationB { public string ReRazaoSocialdProperty { get; set; } = default!; }

    public class SourceC { public int NumericValue { get; set; } }
    public class DestinationC { public string TextValue { get; set; } = default!; }

    public class SourceD { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string IgnoredProperty { get; set; } = default!; }
    public class DestinationD { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string OtherProperty { get; set; } = default!; }

    public class SourceE { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public bool Condition { get; set; } }
    public class DestinationE { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string ConditionalProperty { get; set; } = default!; }

    public class SourceF { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public NestedSourceF Nested { get; set; } = default!; }
    public class NestedSourceF { public string Value { get; set; } = default!; }
    public class DestinationF { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public NestedDestinationF Nested { get; set; } = default!; }
    public class NestedDestinationF { public string Value { get; set; } = default!; }

    public class PersonSource { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public ChildSource? Child { get; set; } }
    public class ChildSource { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public PersonSource? Parent { get; set; } }
    public class PersonDestination { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public ChildDestination? Child { get; set; } }
    public class ChildDestination { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public PersonDestination? Parent { get; set; } }

    public class SourceH { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; }
    public class DestinationH { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string UnmappedProperty { get; set; } = default!; }

    public class SourceWithData { public int Value1 { get; set; } public string Value2 { get; set; } = default!; }
    public class DestinationWithDefaultConstructor { public int Value1 { get; set; } public string Value2 { get; set; } = default!; }

    public enum MyEnum { Value1, Value2, Value3 }
    public class SourceEnum { public string StringValue { get; set; } = default!; public int IntValue { get; set; } public MyEnum EnumValue { get; set; } }
    public class DestinationEnum { public MyEnum StringValue { get; set; } public MyEnum IntValue { get; set; } public MyEnum EnumValue { get; set; } public string EnumStringValue { get; set; } = default!; }

    public class UsuarioEntity { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string Email { get; set; } = default!; public int Idade { get; set; } public decimal Salario { get; set; } public DateTime DataCriacao { get; set; } public EnderecoEntity Endereco { get; set; } = default!; public List<PedidoEntity> Pedidos { get; set; } = new(); }
    public class UsuarioDTO { public int Id { get; set; } public string RazaoSocial { get; set; } = default!; public string Email { get; set; } = default!; public int Idade { get; set; } public decimal Salario { get; set; } public DateTime DataCriacao { get; set; } public string RazaoSocialCompleto { get; set; } = default!; public EnderecoDto Endereco { get; set; } = default!; public List<PedidoDto> Pedidos { get; set; } = new(); }
    public class EnderecoEntity { public string Rua { get; set; } = default!; public string Cidade { get; set; } = default!; public string CEP { get; set; } = default!; }
    public class EnderecoDto { public string Rua { get; set; } = default!; public string Cidade { get; set; } = default!; public string CEP { get; set; } = default!; }
    public class PedidoEntity { public int Id { get; set; } public string Produto { get; set; } = default!; public decimal Valor { get; set; } }
    public class PedidoDto { public int Id { get; set; } public string Produto { get; set; } = default!; public decimal Valor { get; set; } }

    public class ProdutoEntity { public int Codigo { get; set; } public string Descricao { get; set; } = default!; public double Preco { get; set; } }
    public class ProdutoModel { public int Codigo { get; set; } public string Descricao { get; set; } = default!; public decimal Preco { get; set; } }

    public class ClienteEntity { public string PrimeiroRazaoSocial { get; set; } = default!; public string UltimoRazaoSocial { get; set; } = default!; public bool Ativo { get; set; } }
    public class ClienteDto { public string RazaoSocialCompleto { get; set; } = default!; public string Status { get; set; } = default!; }

    public class Fornecedor { public string Nome { get; set; } = default!; }
    public class FornecedorDto { public string Nome { get; set; } = default!; }
    public class ItemCompra { public string Produto { get; set; } = default!; }
    public class ItemCompraDto { public string Produto { get; set; } = default!; }
    public class ContasAPagar { public decimal Valor { get; set; } }
    public class ContasAPagarDto { public decimal Valor { get; set; } }
    public class Compra { public int Id { get; set; } public Fornecedor Fornecedor { get; set; } = default!; public List<ItemCompra> ItensCompra { get; set; } = new(); public List<ContasAPagar> ContasAPagar { get; set; } = new(); }
    public class CompraDto { public int Id { get; set; } public FornecedorDto Fornecedor { get; set; } = default!; public List<ItemCompraDto> ItensCompra { get; set; } = new(); public List<ContasAPagarDto> ContasAPagars { get; set; } = new(); }
}
