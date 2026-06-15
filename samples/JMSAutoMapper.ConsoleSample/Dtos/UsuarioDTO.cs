namespace JMSAutoMapper.ConsoleSample.Dtos;

public class UsuarioDTO
{
    public int Codigo { get; set; }
    public string NomeCompleto { get; set; }
    public string EmailContato { get; set; }
    public string DataCriacaoFormatada { get; set; }
    public string Status { get; set; }

    public int Id { get; set; }
    public string RazaoSocial { get; set; }
    public string Email { get; set; }
    public int Idade { get; set; }
    public decimal Salario { get; set; }
    public DateTime DataCriacao { get; set; }
    public EnderecoDto Endereco { get; set; }
    public List<PedidoDto> Pedidos { get; set; } = new();
    public string RazaoSocialCompleto { get; set; }
}

