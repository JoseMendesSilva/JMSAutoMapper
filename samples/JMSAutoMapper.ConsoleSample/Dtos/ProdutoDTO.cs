namespace JMSAutoMapper.ConsoleSample.Dtos;

public class ProdutoDTO
{
    public int Codigo { get; set; }
    public string NomeProduto { get; set; }
    public string DescricaoResumida { get; set; }
    public string PrecoFormatado { get; set; }
    public string StatusEstoque { get; set; }
    public string Categoria { get; set; }
    public bool TemEstoque { get; set; }
}

