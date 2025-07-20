using System.Collections.Concurrent;

namespace JMSAutoMapper.Tests;

public class AdditionalTests
{
    //[Fact]
    //public void MapArray_ShouldReturnArray()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = new[] {
    //            new TestEntity { Id = 1 },
    //            new TestEntity { Id = 2 }
    //        };

    //    // Act
    //    var results = mapper.MapArray<TestDto>(sources);

    //    // Assert
    //    Assert.NotNull(results);
    //    Assert.Equal(2, results.Length);
    //    Assert.IsType<TestDto[]>(results);
    //}

    //[Fact]
    //public void MapHashSet_ShouldReturnHashSet()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = new[] {
    //            new TestEntity { Id = 1 },
    //            new TestEntity { Id = 2 }
    //        };

    //    // Act
    //    var results = mapper.MapHashSet<TestDto>(sources);

    //    // Assert
    //    Assert.NotNull(results);
    //    Assert.Equal(2, results.Count);
    //    Assert.IsType<HashSet<TestDto>>(results);
    //}

    //[Fact]
    //public void MapImmutableCollections_ShouldReturnCorrectTypes()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = new[] {
    //            new TestEntity { Id = 1 },
    //            new TestEntity { Id = 2 }
    //        };

    //    // Act
    //    var list = mapper.MapImmutableList<TestDto>(sources);
    //    var array = mapper.MapImmutableArray<TestDto>(sources);
    //    var queue = mapper.MapImmutableQueue<TestDto>(sources);
    //    var stack = mapper.MapImmutableStack<TestDto>(sources);

    //    // Assert
    //    Assert.NotNull(list);
    //    Assert.Equal(2, list.Count);

    //    Assert.NotNull(array);
    //    Assert.Equal(2, array.Length);

    //    Assert.NotNull(queue);
    //    Assert.Equal(2, queue.Count());

    //    Assert.NotNull(stack);
    //    Assert.Equal(2, stack.Count());
    //}

    //[Fact]
    //public void MapBlockingCollection_ShouldReturnBlockingCollection()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = new[] {
    //            new TestEntity { Id = 1 },
    //            new TestEntity { Id = 2 }
    //        };

    //    // Act
    //    var results = mapper.MapBlockingCollection<TestDto>(sources, boundedCapacity: 10);

    //    // Assert
    //    Assert.NotNull(results);
    //    Assert.Equal(2, results.Count);
    //    Assert.IsType<BlockingCollection<TestDto>>(results);
    //}

    //[Fact]
    //public void Map_WithCustomProfile_ShouldApplyMappings()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    mapper.AddProfile(new UserProfile());
    //    mapper.Initialize();

    //    var source = new Source
    //    {
    //        Id = 1,
    //        Name = "John",
    //        BirthDate = new DateTime(1990, 1, 1)
    //    };

    //    // Act
    //    var result = mapper.Map<Destination>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(source.Name, result.FullName);
    //    Assert.Equal(DateTime.Now.Year - 1990, result.Age);
    //}
}