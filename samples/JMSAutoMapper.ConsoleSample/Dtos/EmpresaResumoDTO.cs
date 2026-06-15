namespace JMSAutoMapper.ConsoleSample.Dtos;

public class EmpresaResumoDTO
{
    public string NomeEmpresa { get; set; }
    public string CNPJFormatado { get; set; }
    public int TotalFuncionarios { get; set; }
    public string Localizacao { get; set; }
    public string FaturamentoFormatado { get; set; }
    public List<string> Departamentos { get; set; }
    public decimal MediaSalarial { get; set; }
}

