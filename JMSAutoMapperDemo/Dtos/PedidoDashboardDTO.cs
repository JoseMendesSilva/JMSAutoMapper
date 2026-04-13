public class PedidoDashboardDTO
{
    public string Identificador { get; set; }
    public string DataFormatada { get; set; }
    public string Cliente { get; set; }
    public string ValorTotal { get; set; }
    public string CorStatus { get; set; }
    public string IconePrioridade { get; set; }
    public int TotalItens { get; set; }
    public bool EhUrgente { get; set; }
    public bool PodeDespachar { get; set; }
}

