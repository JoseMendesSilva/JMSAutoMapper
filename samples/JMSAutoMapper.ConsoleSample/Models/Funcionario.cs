namespace JMSAutoMapper.ConsoleSample.Models;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cargo { get; set; }
    public decimal Salario { get; set; }
    public DateTime DataAdmissao { get; set; }
    public bool Ativo { get; set; }
}

