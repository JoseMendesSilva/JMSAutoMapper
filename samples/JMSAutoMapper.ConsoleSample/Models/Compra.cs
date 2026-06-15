namespace JMSAutoMapper.ConsoleSample.Models;

// ===== CLASSES PARA TESTE DE COMPRA (CENÁRIO COMPLEXO) =====
public class Compra
{
    public int Id { get; set; }
    public Fornecedor Fornecedor { get; set; }
    public List<ItemCompra> ItensCompra { get; set; } = new();
    public List<ContasAPagar> ContasAPagar { get; set; } = new();
}
