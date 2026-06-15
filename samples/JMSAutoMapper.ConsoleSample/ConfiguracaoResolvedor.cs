using JMSAutoMapper.Abstractions;
using JMSAutoMapper.ConsoleSample.Dtos;
using JMSAutoMapper.ConsoleSample.Models;
using JMSAutoMapper.Core;

namespace JMSAutoMapper.ConsoleSample;

public class ConfiguracaoResolvedor : IValueResolver<ClienteEntity, ClienteDto, string>
{
    public string Resolve(ClienteEntity source, ClienteDto destination, string destMember, ResolutionContext context)
    {
        return source.Ativo ? "Ativo" : "Inativo";
    }
}
