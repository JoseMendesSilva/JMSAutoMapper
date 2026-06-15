using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;
using FluentAssertions;

namespace JMSAutoMapper.Tests
{
    public class CollectionMapTests
    {
        [Fact(DisplayName = "List Map - Basic List")]
        [Trait("Category", "Collections")]
        public void MapList_ShouldMapCollectionCorrectly()
        {
            var config = new MapperConfiguration();
            config.CreateMap<Source, Destination>().ForMember("FullRazaoSocial", src => src.RazaoSocial);
            var mapper = new JMSMapper(config);
            var sources = new List<Source> { new Source { Id = 1, RazaoSocial = "Test1" } };
            var destinations = mapper.Map<List<Destination>>(sources);
            destinations.Should().HaveCount(1);
            destinations[0].FullRazaoSocial.Should().Be("Test1");
        }

        [Fact(DisplayName = "ImmutableArray Map - Null Source returns Empty")]
        [Trait("Category", "Immutable Collections")]
        public void MapImmutableArray_ShouldReturnEmptyWhenSourceIsNull()
        {
            var mapper = new JMSMapper(new MapperConfiguration());
            var destinations = mapper.Map<ImmutableArray<Destination>>(null);
            destinations.Should().BeEmpty();
        }
    }
}