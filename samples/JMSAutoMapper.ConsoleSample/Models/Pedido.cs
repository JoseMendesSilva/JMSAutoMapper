//using JMSAutoMapper;

namespace JMSAutoMapper.ConsoleSample.Models;

    public class Pedido
    {
        public int Id { get; set; }
        public string? Produto { get; set; }
    public string Numero { get; set; }
    public DateTime Data { get; set; }
    public string ClienteNome { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; }
    public int Prioridade { get; set; }
    public List<ItemPedido> Itens { get; set; }
}
