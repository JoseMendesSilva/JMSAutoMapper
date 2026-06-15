namespace JMSAutoMapper.ConsoleSample.Dtos;

public class DepartamentoDTO
{
    public string NomeDepartamento { get; set; }
    public string Sigla { get; set; }
    public int TotalFuncionarios { get; set; }
    public int FuncionariosAtivos { get; set; }
    public string OrcamentoFormatado { get; set; }
    public List<string> NomesFuncionarios { get; set; }
    public decimal MediaSalarial { get; set; }
}

