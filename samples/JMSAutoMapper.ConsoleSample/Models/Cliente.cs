namespace JMSAutoMapper.ConsoleSample.Models;

public class Cliente : EntidadeBase
{
    public int Age { get; set; }
    public string RazaoSocial { get; set; }
    public string EmailPrincipal { get; set; }
    public Endereco Sede { get; set; }
    public List<Contrato> ContratosAtivos { get; set; } = new();
    public List<Endereco> Enderecos { get; set; } = new List<Endereco>();
    public Dictionary<string, string> Metadados { get; set; } = new();
}
