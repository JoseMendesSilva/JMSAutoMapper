using System.Collections.Concurrent;
using System.Diagnostics;

namespace JMSAutoMapper.Tests;

public class StressTests
{
    private class ComplexEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ComplexEntity Parent { get; set; }
        public List<ComplexEntity> Children { get; set; } = new List<ComplexEntity>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    private class ComplexDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public ComplexDto Parent { get; set; }
        public List<ComplexDto> Children { get; set; } = new List<ComplexDto>();
        public ConcurrentDictionary<string, object> Attributes { get; set; } = new ConcurrentDictionary<string, object>();
    }

    //[Fact]
    //public void Map_DeepObjectGraph_ShouldNotStackOverflow()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    mapper.SetMaxRecursionDepth(100);

    //    var root = new ComplexEntity { Id = 1, Name = "Root" };
    //    var current = root;
    //    for (int i = 2; i < 100; i++)
    //    {
    //        var child = new ComplexEntity { Id = i, Name = $"Child {i}", Parent = current };
    //        current.Children.Add(child);
    //        current = child;
    //    }

    //    // Act
    //    var result = mapper.Map<ComplexDto>(root);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(root.Id, result.Id);
    //}

    //[Fact]
    //public async Task MapIEnumerableAsync_UnderHighLoad_ShouldCompleteSuccessfully()
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
    //    var results = await mapper.MapIEnumerableAsync<TestDto>(sources, maxDegreeOfParallelism: 8);

    //    // Assert
    //    stopwatch.Stop();
    //    Assert.Equal(10000, results.Count());
    //    Assert.True(stopwatch.ElapsedMilliseconds < 2000,
    //        $"Mapping 10,000 objects in parallel took {stopwatch.ElapsedMilliseconds}ms");
    //}

    //[Fact]
    //public void Map_WithMultipleThreads_ShouldBeThreadSafe()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = Enumerable.Range(1, 1000)
    //        .Select(i => new TestEntity { Id = i, Name = $"Item {i}" }).ToList();

    //    var results = new ConcurrentBag<TestDto>();
    //    var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };

    //    // Act
    //    Parallel.ForEach(sources, options, item =>
    //    {
    //        var result = mapper.Map<TestDto>(item);
    //        results.Add(result);
    //    });

    //    // Assert
    //    Assert.Equal(1000, results.Count);
    //    Assert.Equal(1000, results.Select(r => r.Id).Distinct().Count());
    //}
}