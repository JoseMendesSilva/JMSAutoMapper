namespace JMSAutoMapper.ConsoleSample.Dtos;

public class CompraDto
{
    public int Id { get; set; }
    public FornecedorDto Fornecedor { get; set; }
    public List<ItemCompraDto> ItensCompra { get; set; } = new();
    public List<ContasAPagarDto> ContasAPagars { get; set; } = new(); // Nome diferente
}
