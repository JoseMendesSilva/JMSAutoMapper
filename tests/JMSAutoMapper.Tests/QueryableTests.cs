using Xunit;
using System.Linq;
using System.Collections.Generic;
using JMSAutoMapper.Core;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Tests.Infrastructure;
using FluentAssertions;

namespace JMSAutoMapper.Tests
{
    public class QueryableTests
    {
        [Fact(DisplayName = "MapQueryable - Basic Support")]
        [Trait("Category", "Queryable")]
        public void MapQueryable_ShouldMapBasicQueryable()
        {
            var config = new MapperConfiguration();
            config.CreateMap<Source, Destination>()
                  .ForMember("FullRazaoSocial", src => src.RazaoSocial);
            var mapper = config.CreateMapper();
            
            var sources = new List<Source>
            {
                new Source { Id = 1, RazaoSocial = "QueryableTest1" },
                new Source { Id = 2, RazaoSocial = "QueryableTest2" }
            }.AsQueryable();

            var result = mapper.MapQueryable<Source, Destination>(sources).ToList();

            result.Should().HaveCount(2);
            result[0].FullRazaoSocial.Should().Be("QueryableTest1");
        }
    }
}
