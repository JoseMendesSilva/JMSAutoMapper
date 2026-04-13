using JMSAutoMapperDemo.Models;
// =============================================
// EXEMPLO 5: MAPEAMENTO COM EMPRESA
// =============================================

public class Empresa
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; }
    public string CNPJ { get; set; }
    public List<Funcionario> Funcionarios { get; set; }
    public Endereco Endereco { get; set; }
    public decimal FaturamentoAnual { get; set; }
}

