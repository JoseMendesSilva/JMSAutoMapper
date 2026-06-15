using JMSAutoMapper.ConsoleSample.Models;

namespace JMSAutoMapper.ConsoleSample.Dtos;

public class ClienteDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; }
    public int Age { get; set; }
    public List<Endereco> Enderecos { get; set; } = new List<Endereco>();
    public string Status { get; set; }
    public string RazaoSocialCompleto { get; set; }
}
