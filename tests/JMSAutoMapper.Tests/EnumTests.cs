using Xunit;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;
using FluentAssertions;

namespace JMSAutoMapper.Tests
{
    public class EnumTests
    {
        [Fact(DisplayName = "Enum Map - Enum to String")]
        [Trait("Category", "Enums")]
        public void Map_ShouldMapEnumToString()
        {
            var config = new MapperConfiguration();
            config.CreateMap<SourceEnum, DestinationEnum>()
                .ForMember("EnumStringValue", src => src.EnumValue.ToString());
            var mapper = config.CreateMapper();

            var source = new SourceEnum { EnumValue = MyEnum.Value1 };
            var destination = mapper.Map<DestinationEnum>(source);

            destination.EnumStringValue.Should().Be("Value1");
        }

        [Fact(DisplayName = "Enum Map - String to Enum")]
        [Trait("Category", "Enums")]
        public void Map_ShouldMapStringtoEnum()
        {
            var config = new MapperConfiguration();
            config.CreateMap<SourceEnum, DestinationEnum>();
            var mapper = config.CreateMapper();

            var source = new SourceEnum { StringValue = "Value2" };
            var destination = mapper.Map<DestinationEnum>(source);

            destination.StringValue.Should().Be(MyEnum.Value2);
        }
    }
}
