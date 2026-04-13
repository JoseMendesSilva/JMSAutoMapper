using System.Diagnostics;

namespace JMSAutoMapper.Tests;

public class LoadTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }
    }

    [Fact]
    public void Map_100000Objects_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = Enumerable.Range(1, 100000)
            .Select(i => new TestEntity
            {
                Id = i,
                Name = $"Item {i}",
                Created = DateTime.Now.AddDays(-i)
            }).ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = mapper.MapList<TestDto>(sources);

        // Assert
        stopwatch.Stop();
        Assert.Equal(100000, results.Count);
        Assert.True(stopwatch.ElapsedMilliseconds < 10000,
            $"Mapping 100,000 objects took {stopwatch.ElapsedMilliseconds}ms");
    }

}