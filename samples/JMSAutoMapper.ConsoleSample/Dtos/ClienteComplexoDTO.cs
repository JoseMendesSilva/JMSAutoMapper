namespace JMSAutoMapper.ConsoleSample.Dtos;

public class ClienteComplexoDTO
{
    public int Codigo { get; set; }
    public string NomeEmpresa { get; set; }
    public string Contato { get; set; }
    // Flattening (Achatamento)
    public string EnderecoCompleto { get; set; }
    public string CidadeSede { get; set; }
    // Deep Mapping
    public List<ContratoDTO> ResumoContratos { get; set; }
    // Custom Logic
    public string ScoreFinanceiro { get; set; }
    public decimal ValorTotalContratos { get; set; }
    public string UltimaAtualizacao { get; set; }
}

