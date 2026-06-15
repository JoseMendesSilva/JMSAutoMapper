
// =============================================
// EXEMPLO 3: MAPEAMENTO COM CONDIÇÕES
// =============================================

using JMSAutoMapper.ConsoleSample.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Disponivel { get; set; }
    public CategoriaProduto Categoria { get; set; }
}

