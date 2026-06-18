using Xunit;
using FluentAssertions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;
using JMSAutoMapper.DependencyInjection; // Adicionado para MapperExtensions
using JMSAutoMapper.Core;

namespace JMSAutoMapper.Tests
{
    public class ConfigurationTests
    {
        [Fact(DisplayName = "Config - Valid Mapping Registration")]
        [Trait("Feature", "Configuration")]
        public void Configuration_ShouldBeValid_WhenAllMappingsAreConfigured()
        {
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>();
            config.Invoking(c => c.AssertConfigurationIsValid()).Should().NotThrow();
        }

        [Fact(DisplayName = "Config - Validation Error on Unmapped Properties")]
        [Trait("Feature", "Configuration")]
        public void Configuration_ShouldThrow_WhenUnmappedPropertiesExist()
        {
            var config = new MapperConfiguration();
            config.CreateMap<UsuarioEntity, UsuarioDTO>()
                  .ForMember("Id", "Id")
                  .ValidateMemberList(MemberListType.Destination);

            config.Invoking(c => c.AssertConfigurationIsValid())
                  .Should().Throw<MappingException>();
        }

        [Fact(DisplayName = "Reverse Map - Bidirectional Support")]
        [Trait("Category", "Configuration")]
        public void Map_ShouldReverseMapCorrectly()
        {
            var config = new MapperConfiguration();
            config.CreateMap<Source, Destination>()
                  .ForMember("FullRazaoSocial", src => src.RazaoSocial)
                  .ReverseMap();
            var mapper = config.CreateMapper();

            var destination = new Destination { Id = 10, FullRazaoSocial = "Reverse Test" };
            var source = mapper.Map<Source>(destination);

            source.Id.Should().Be(10);
            source.RazaoSocial.Should().Be("Reverse Test");
        }
    }
}
