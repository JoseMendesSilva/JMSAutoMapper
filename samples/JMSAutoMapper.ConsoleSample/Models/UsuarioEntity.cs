namespace JMSAutoMapper.ConsoleSample.Models;

// ===== CLASSES DE TESTE =====
public class UsuarioEntity : EntidadeBase
{
    public string RazaoSocial { get; set; }
    public string Email { get; set; }
    public int Idade { get; set; }
    public decimal Salario { get; set; }
    public DateTime DataCriacao { get; set; }
    public EnderecoEntity Endereco { get; set; }
    public List<PedidoEntity> Pedidos { get; set; } = new();
}
