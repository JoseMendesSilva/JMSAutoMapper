//using JMSAutoMapper;

using JMSAutoMapperDemo.Models;

namespace JMSAutoMapperDemo.Dtos;

public class SourceClassDto
{
    public int SourceClassId { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
    public DateTime Data { get; set; }
    public eTeste EnumTeste { get; set; }
    public List<ItensPedidoDto> ItensPedido { get; set; } = new List<ItensPedidoDto>();
}
