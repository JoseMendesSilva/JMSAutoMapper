using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;
using System.Diagnostics;

namespace JMSAutoMapper.Tests;

public class LoadTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public DateTime Created { get; set; }
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public string CreatedDate { get; set; } = default!;
    }

    [Fact(DisplayName = "Load Test - 100k Objects Mapping")]
    [Trait("Category", "Performance")]
    public void Map_100000Objects_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = Enumerable.Range(1, 100000)
            .Select(i => new TestEntity
            {
                Id = i,
                RazaoSocial = $"Item {i}",
                Created = DateTime.Now.AddSeconds(-i)
            }).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = mapper.Map<List<TestDto>>(sources);

        // Assert
        stopwatch.Stop();
        Assert.Equal(100000, results.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000,
            $"Mapping 100,000 objects took {stopwatch.ElapsedMilliseconds}ms");
    }


    [Fact(DisplayName = "Load Test - 1000k Objects Mapping")]
    [Trait("Category", "Performance")]
    public void Map_1000000Objects_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = Enumerable.Range(1, 1000000)
            .Select(i => new TestEntity
            {
                Id = i,
                RazaoSocial = $"Item {i}",
                Created = DateTime.Now.AddSeconds(-i)
            }).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = mapper.Map<List<TestDto>>(sources);

        // Assert
        stopwatch.Stop();
        Assert.Equal(1000000, results.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000,
            $"Mapping 1000,000 objects took {stopwatch.ElapsedMilliseconds}ms");
    }

}