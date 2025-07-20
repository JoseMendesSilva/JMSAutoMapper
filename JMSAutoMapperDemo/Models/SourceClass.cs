//using JMSAutoMapper;

namespace JMSAutoMapperDemo.Models;


public class SourceClass
{
    public int SourceClassId { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
    public DateTime Data { get; set; }
    public eTeste EnumTeste { get; set; }
    public List<ItensPedido> ItensPedido { get; set; } = new List<ItensPedido>();
}