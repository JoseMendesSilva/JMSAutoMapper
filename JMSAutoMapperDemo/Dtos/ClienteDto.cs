using JMSAutoMapperDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMSAutoMapperDemo.Dtos;

public class ClienteDto
{
    public int ClienteId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public List<Endereco> Enderecos { get; set; } = new List<Endereco>();
}
