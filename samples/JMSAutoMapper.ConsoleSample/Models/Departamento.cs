namespace JMSAutoMapper.ConsoleSample.Models;

// =============================================
// EXEMPLO 4: MAPEAMENTO COM COLEÇÕES
// =============================================

public class Departamento
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Sigla { get; set; }
    public List<Funcionario> Funcionarios { get; set; }
    public decimal Orcamento { get; set; }
    public DateTime DataCriacao { get; set; }
}

