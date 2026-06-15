namespace JMSAutoMapper.ConsoleSample.Models;

// =============================================
// EXEMPLO 1: MAPEAMENTO BÁSICO
// =============================================

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; }
}

