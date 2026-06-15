using Xunit;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using FluentAssertions;
using JMSAutoMapper.Abstractions;

namespace JMSAutoMapper.Tests
{
    public class NullableTests
    {
        private readonly IMapper _mapper = new JMSMapper(new MapperConfiguration());

        [Fact(DisplayName = "Nullable Map - Nullable Int to Int")]
        [Trait("Category", "Nullable Types")]
        public void Map_ShouldMapNullableIntToInt()
        {
            int? source = 123;
            int result = _mapper.Map<int>(source);
            result.Should().Be(source.Value);
        }

        [Fact(DisplayName = "Nullable Map - Int to Nullable Int")]
        [Trait("Category", "Nullable Types")]
        public void Map_ShouldMapIntToNullableInt()
        {
            int source = 123;
            int? result = _mapper.Map<int?>(source);
            result.Should().Be(source);
        }

        [Fact(DisplayName = "Nullable Map - Null Source to Nullable Type")]
        [Trait("Category", "Nullable Types")]
        public void Map_ShouldMapNullToNullableType()
        {
            string? source = null;
            string? result = _mapper.Map<string?>(source);
            result.Should().BeNull();
        }

        [Fact(DisplayName = "Safety - Throw Exception on Null to ValueType")]
        [Trait("Category", "Safety")]
        public void Map_ShouldThrowExceptionWhenMappingNullToNonNullableValueType()
        {
            object? source = null;
            var act = () => _mapper.Map<int>(source);
            act.Should().Throw<System.ArgumentNullException>().And.Message.Should().Contain("não anulável");
        }
    }
}