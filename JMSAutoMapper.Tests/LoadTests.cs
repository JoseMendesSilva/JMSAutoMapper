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

    //[Fact]
    //public void Map_10000Objects_ShouldCompleteInReasonableTime()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = Enumerable.Range(1, 10000)
    //        .Select(i => new TestEntity
    //        {
    //            Id = i,
    //            Name = $"Item {i}",
    //            Created = DateTime.Now.AddDays(-i)
    //        }).ToList();

    //    var stopwatch = Stopwatch.StartNew();

    //    // Act
    //    var results = mapper.MapList<TestDto>(sources);

    //    // Assert
    //    stopwatch.Stop();
    //    Assert.Equal(10000, results.Count);
    //    Assert.True(stopwatch.ElapsedMilliseconds < 1000,
    //        $"Mapping 10,000 objects took {stopwatch.ElapsedMilliseconds}ms");
    //}

    //[Fact]
    //public void Map_ConcurrentDictionary_ShouldHandleLargeCollections()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var source = new Dictionary<int, TestEntity>();
    //    for (int i = 0; i < 5000; i++)
    //    {
    //        source[i] = new TestEntity { Id = i, Name = $"Item {i}" };
    //    }

    //    // Act
    //    var result = mapper.MapConcurrentDictionary<int, TestDto>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(5000, result.Count);
    //}
}