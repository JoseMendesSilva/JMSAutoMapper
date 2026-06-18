using Xunit;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;
using FluentAssertions;

namespace JMSAutoMapper.Tests
{
    public class SimpleMapTests
    {
        [Fact(DisplayName = "Simple Map - Int to String")]
        [Trait("Category", "Primitive Types")]
        public void Map_ShouldMapIntToString()
        {
            var mapper = new JMSMapper(new MapperConfiguration());
            int source = 123;
            string result = mapper.Map<string>(source);
            Assert.Equal("123", result);
        }

        [Fact(DisplayName = "Basic Map - Entity to DTO properties")]
        [Trait("Feature", "Mapping")]
        public void Map_UsuarioEntityToUsuarioDTO_ShouldMapBasicProperties()
        {
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            var mapper = config.CreateMapper();
            var entity = new UsuarioEntity { Id = 1, RazaoSocial = "João Silva" };
            var result = mapper.Map<UsuarioDTO>(entity);
            result.RazaoSocial.Should().Be(entity.RazaoSocial);
        }
    }
}
