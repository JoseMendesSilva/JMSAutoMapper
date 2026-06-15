using JMSAutoMapper.ConsoleSample.Models;

namespace JMSAutoMapper.ConsoleSample.Dtos
{
   public class EnderecoDto
    {
        public int Logradouro { get; set; }
        public string edtsdo { get; set; }
        public Cliente Cliente { get; set; }
        public string Rua { get; set; }
        public string Cidade { get; set; }
        public string CEP { get; set; }
    }
}
