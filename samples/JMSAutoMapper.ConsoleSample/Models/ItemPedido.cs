//using JMSAutoMapper;

namespace JMSAutoMapper.ConsoleSample.Models;

public class ItemPedido
{
    public int ItemPedidoId { get; set; }
    public string? Name { get; set; }
    public int Quantidade { get; set; }
    public string Produto { get; set; }
    public decimal Preco { get; set; }
}