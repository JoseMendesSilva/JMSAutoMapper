using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.Tests;

public partial class UnitTests
{

    [Fact]
    public void MapDictionary_ShouldConvertKeyAndValue()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var source = new Dictionary<int, Source>
            {
                { 1, new Source { Id = 1, Name = "Test1" } },
                { 2, new Source { Id = 2, Name = "Test2" } }
            };

        // Act
        var result = mapper.MapDictionary<int, Destination>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[1].FullName);
    }

    [Fact]
    public void Map_ShouldMapIntToInt()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        int source = 123;

        // Act
        int result = mapper.Map<int>(source);

        // Assert
        Assert.Equal(source, result);
    }

    [Fact]
    public void Map_ShouldMapIntToString()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        int source = 123;

        // Act
        string result = mapper.Map<string>(source);

        // Assert
        Assert.Equal(source.ToString(), result);
    }

    [Fact]
    public void Map_ShouldMapStringToInt()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        string source = "789";

        // Act
        int result = mapper.Map<int>(source);

        // Assert
        Assert.Equal(int.Parse(source), result);
    }

    [Fact]
    public void Map_ShouldMapDateTimeToDateTime()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        DateTime source = DateTime.Now;

        // Act
        DateTime result = mapper.Map<DateTime>(source);

        // Assert
        Assert.Equal(source, result);
    }

    [Fact]
    public void Map_ShouldMapGuidToGuid()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        Guid source = Guid.NewGuid();

        // Act
        Guid result = mapper.Map<Guid>(source);

        // Assert
        Assert.Equal(source, result);
    }

    [Fact]
    public void Map_ShouldMapComplexObjectWithMatchingProperties()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var source = new SourceA { Id = 1, Name = "Test", Value = 123.45 };

        // Act
        var result = mapper.Map<DestinationA>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Value, result.Value);
    }

    [Fact]
    public void Map_ShouldRenamePropertyUsingForMember()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceB, DestinationB>()
              .ForMember("RenamedProperty", src => src.OriginalProperty);
        var mapper = new JMSMapper(config);
        var source = new SourceB { OriginalProperty = "Hello ForMember" };

        // Act
        var result = mapper.Map<DestinationB>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.OriginalProperty, result.RenamedProperty);
    }

    [Fact]
    public void Map_ShouldUseCustomValueResolverForMember()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceC, DestinationC>()
              .ForMember("TextValue", src => src.NumericValue.ToString());
        var mapper = new JMSMapper(config);
        var source = new SourceC { NumericValue = 123 };

        // Act
        var result = mapper.Map<DestinationC>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.NumericValue.ToString(), result.TextValue);
    }

    [Fact]
    public void Map_ShouldIgnoreProperty()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceD, DestinationD>()
              .Ignore(dest => dest.OtherProperty);
        var mapper = new JMSMapper(config);
        var source = new SourceD { Id = 1, Name = "Test", IgnoredProperty = "Should Be Ignored" };

        // Act
        var result = mapper.Map<DestinationD>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Null(result.OtherProperty); // OtherProperty should be null as it was ignored
    }

    [Fact]
    public void Map_ShouldApplyConditionalMappingWhenConditionIsTrue()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceE, DestinationE>()
              .ForMember("ConditionalProperty", src => src.Name, src => src.Condition);
        var mapper = new JMSMapper(config);
        var source = new SourceE { Id = 1, Name = "Conditional Test", Condition = true };

        // Act
        var result = mapper.Map<DestinationE>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Name, result.ConditionalProperty);
    }

    [Fact]
    public void Map_ShouldNotApplyConditionalMappingWhenConditionIsFalse()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceE, DestinationE>()
              .ForMember("ConditionalProperty", src => src.Name, src => src.Condition);
        var mapper = new JMSMapper(config);
        var source = new SourceE { Id = 1, Name = "Conditional Test", Condition = false };

        // Act
        var result = mapper.Map<DestinationE>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Null(result.ConditionalProperty); // Should be null or default if not mapped
    }

    [Fact]
    public void Map_ShouldMapNullableIntToInt()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        int? source = 123;

        // Act
        int result = mapper.Map<int>(source);

        // Assert
        Assert.Equal(source.Value, result);
    }

    [Fact]
    public void Map_ShouldMapIntToNullableInt()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        int source = 123;

        // Act
        int? result = mapper.Map<int?>(source);

        // Assert
        Assert.Equal(source, result);
    }

    [Fact]
    public void Map_ShouldMapStringToNullableString()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        string source = "test";

        // Act
        string? result = mapper.Map<string?>(source);

        // Assert
        Assert.Equal(source, result);
    }

    [Fact]
    public void Map_ShouldMapNullToNullableType()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        string? source = null;

        // Act
        string? result = mapper.Map<string?>(source);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Map_ShouldThrowExceptionWhenMappingNullToNonNullableValueType()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        object? source = null;

        // Act & Assert
        var exception = Record.Exception(() => mapper.Map<int>(source));
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
        Assert.Contains("Não é possível mapear uma origem nula para um tipo de valor não anulável 'Int32'.", exception.Message);
    }

    [Fact]
    public void MapIEnumerable_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "Test1" },
            new Source { Id = 2, Name = "Test2" }
        };

        // Act
        var destinations = mapper.MapIEnumerable<Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].Name, destinations[0].FullName);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
    }

    [Fact]
    public void MapIEnumerable_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new List<Source>();

        // Act
        var destinations = mapper.MapIEnumerable<Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapIEnumerable_ShouldHandleNullItemsInCollection()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new List<Source?>
        {
            new Source { Id = 1, Name = "Test1" },
            null,
            new Source { Id = 3, Name = "Test3" }
        };

        // Act
        var destinations = mapper.MapIEnumerable<Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count); // Null item should be skipped
        Assert.Equal(sources[0]!.Id, destinations[0].Id);
        Assert.Equal(sources[0]!.Name, destinations[0].FullName);
        Assert.Equal(sources[2]!.Id, destinations[1].Id);
        Assert.Equal(sources[2]!.Name, destinations[1].FullName);
    }

    [Fact]
    public void MapList_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "ListTest1" },
            new Source { Id = 2, Name = "ListTest2" }
        };

        // Act
        var destinations = mapper.MapList<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].Name, destinations[0].FullName);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
    }

    [Fact]
    public void MapList_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new List<Source>();

        // Act
        var destinations = mapper.MapList<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapArray_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new Source[]
        {
            new Source { Id = 1, Name = "ArrayTest1" },
            new Source { Id = 2, Name = "ArrayTest2" }
        };

        // Act
        var destinations = mapper.MapArray<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Length);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].Name, destinations[0].FullName);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
    }

    [Fact]
    public void MapArray_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new Source[0];

        // Act
        var destinations = mapper.MapArray<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapHashSet_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new HashSet<Source>
        {
            new Source { Id = 1, Name = "HashSetTest1" },
            new Source { Id = 2, Name = "HashSetTest2" }
        };

        // Act
        var destinations = mapper.MapHashSet<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(0).Id && d.FullName == sources.ElementAt(0).Name);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(1).Id && d.FullName == sources.ElementAt(1).Name);
    }

    [Fact]
    public void MapHashSet_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new HashSet<Source>();

        // Act
        var destinations = mapper.MapHashSet<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapDictionary_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new Dictionary<int, Source>
        {
            { 1, new Source { Id = 1, Name = "DictTest1" } },
            { 2, new Source { Id = 2, Name = "DictTest2" } }
        };

        // Act
        var destinations = mapper.MapDictionary<int, Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.Values.ElementAt(0).Id, destinations.Values.ElementAt(0).Id);
        Assert.Equal(sources.Values.ElementAt(0).Name, destinations.Values.ElementAt(0).FullName);
        Assert.Equal(sources.Values.ElementAt(1).Id, destinations.Values.ElementAt(1).Id);
        Assert.Equal(sources.Values.ElementAt(1).Name, destinations.Values.ElementAt(1).FullName);
    }

    [Fact]
    public void MapDictionary_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new Dictionary<int, Source>();

        // Act
        var destinations = mapper.MapDictionary<int, Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapDictionary_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapDictionary<int, Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapDictionary_ShouldHandleNullValuesInCollection()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new Dictionary<int, Source?>
        {
            { 1, new Source { Id = 1, Name = "DictTest1" } },
            { 2, null },
            { 3, new Source { Id = 3, Name = "DictTest3" } }
        };

        // Act
        var destinations = mapper.MapDictionary<int, Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count); // Null value should be skipped
        Assert.Equal(sources[1]!.Id, destinations[1].Id);
        Assert.Equal(sources[1]!.Name, destinations[1].FullName);
        Assert.Equal(sources[3]!.Id, destinations[3].Id);
        Assert.Equal(sources[3]!.Name, destinations[3].FullName);
    }

    [Fact]
    public void MapImmutableList_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = ImmutableList.Create(
            new Source { Id = 1, Name = "ImmutableListTest1" },
            new Source { Id = 2, Name = "ImmutableListTest2" }
        );

        // Act
        var destinations = mapper.MapImmutableList<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].Name, destinations[0].FullName);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
    }

    [Fact]
    public void MapImmutableList_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableList.Create<Source>();

        // Act
        var destinations = mapper.MapImmutableList<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableList_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapImmutableList<Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableDictionary_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = ImmutableDictionary.CreateRange(new Dictionary<int, Source>
        {
            { 1, new Source { Id = 1, Name = "ImmutableDictTest1" } },
            { 2, new Source { Id = 2, Name = "ImmutableDictTest2" } }
        });

        // Act
        var destinations = mapper.MapImmutableDictionary<int, Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
        Assert.Equal(sources[2].Id, destinations[2].Id);
        Assert.Equal(sources[2].Name, destinations[2].FullName);
    }

    [Fact]
    public void MapImmutableDictionary_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableDictionary.Create<int, Source>();

        // Act
        var destinations = mapper.MapImmutableDictionary<int, Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableDictionary_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapImmutableDictionary<int, Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableArray_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = ImmutableArray.Create(
            new Source { Id = 1, Name = "ImmutableArrayTest1" },
            new Source { Id = 2, Name = "ImmutableArrayTest2" }
        );

        // Act
        var destinations = mapper.MapImmutableArray<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Length);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].Name, destinations[0].FullName);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].Name, destinations[1].FullName);
    }

    [Fact]
    public void MapImmutableArray_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableArray.Create<Source>();

        // Act
        var destinations = mapper.MapImmutableArray<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableArray_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapImmutableArray<Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableQueue_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = ImmutableQueue.CreateRange(new List<Source>
        {
            new Source { Id = 1, Name = "ImmutableQueueTest1" },
            new Source { Id = 2, Name = "ImmutableQueueTest2" }
        });

        // Act
        var destinations = mapper.MapImmutableQueue<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count());
        Assert.Equal(sources.Peek().Id, destinations.Peek().Id);
        Assert.Equal(sources.Peek().Name, destinations.Peek().FullName);
    }

    [Fact]
    public void MapImmutableQueue_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableQueue.Create<Source>();

        // Act
        var destinations = mapper.MapImmutableQueue<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableQueue_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapImmutableQueue<Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableStack_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = ImmutableStack.CreateRange((IEnumerable<Source>)new List<Source>
            { new Source { Id = 1, Name = "ImmutableStackTest1" },
            new Source { Id = 2, Name = "ImmutableStackTest2" } }
        );

        var destinations = mapper.MapImmutableStack<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count());
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(0).Id && d.FullName == sources.ElementAt(0).Name);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(1).Id && d.FullName == sources.ElementAt(1).Name);
    }

    [Fact]
    public void MapImmutableStack_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableStack.Create<Source>();

        // Act
        var destinations = mapper.MapImmutableStack<Destination>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact]
    public void MapImmutableStack_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.MapImmutableStack<Destination>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }





    [Fact]
    public void MapQueryable_ShouldMapBasicQueryable()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "QueryableTest1" },
            new Source { Id = 2, Name = "QueryableTest2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).Name, destinations[0].FullName);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).Name, destinations[1].FullName);
    }

    [Fact]
    public void MapQueryable_ShouldMapNestedProperties()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceF, DestinationF>();
        config.CreateMap<NestedSourceF, NestedDestinationF>();
        var mapper = new JMSMapper(config);
        var sources = new List<SourceF>
        {
            new SourceF { Id = 1, Name = "Parent1", Nested = new NestedSourceF { Value = "NestedValue1" } },
            new SourceF { Id = 2, Name = "Parent2", Nested = new NestedSourceF { Value = "NestedValue2" } }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<SourceF, DestinationF>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).Name, destinations[0].Name);
        Assert.Equal(sources.ElementAt(0).Nested.Value, destinations[0].Nested.Value);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).Name, destinations[1].Name);
        Assert.Equal(sources.ElementAt(1).Nested.Value, destinations[1].Nested.Value);
    }

    [Fact]
    public void MapQueryable_ShouldRespectForMemberAndIgnore()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name + " Mapped")
              .Ignore(dest => dest.Age);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "QueryableForMemberIgnore1" },
            new Source { Id = 2, Name = "QueryableForMemberIgnore2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).Name + " Mapped", destinations[0].FullName);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).Name + " Mapped", destinations[1].FullName);
    }

    [Fact]
    public void MapQueryable_ShouldProjectPropertiesCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>();
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "Test1" },
            new Source { Id = 2, Name = "Test2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Null(destinations[0].FullName);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Null(destinations[1].FullName);
    }

    [Fact]
    public void AddJMSMapper_ShouldAddMapperToDIMain()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJMSMapper();
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        // Assert
        Assert.NotNull(mapper);
        Assert.IsType<JMSMapper>(mapper);
    }

    [Fact]
    public void AddJMSMapper_ShouldAddMapperToDIWithBasicConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJMSMapper(config =>
        {
            config.CreateMap<Source, Destination>();
        });
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        // Assert
        Assert.NotNull(mapper);
        Assert.IsType<JMSMapper>(mapper);
    }

    public class TestProfile : Profile
    {
        public override void Configure()
        {
            CreateMap<Source, Destination>()
                .ForMember("FullName", src => src.Name + " From Profile");
        }
    }

    public class TestProfile2 : Profile
    {
        public override void Configure()
        {
            CreateMap<SourceA, DestinationA>()
                .ForMember("Name", src => src.Name + " From Profile2");
        }
    }

    [Fact]
    public void AddJMSMapper_ShouldAddMapperToDIWithProfiles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJMSMapper(configureProfiles: profiles =>
        {
            profiles.AddProfile<TestProfile>();
        });
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        // Assert
        Assert.NotNull(mapper);
        Assert.IsType<JMSMapper>(mapper);

        var source = new Source { Id = 1, Name = "Test" };
        var destination = mapper.Map<Destination>(source);

        Assert.Equal("Test From Profile", destination.FullName);
    }

    [Fact]
    public void AddJMSMapper_ShouldHandleMultipleProfiles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJMSMapper(configureProfiles: profiles =>
        {
            profiles.AddProfile<TestProfile>();
            profiles.AddProfile<TestProfile2>();
        });
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        // Assert
        Assert.NotNull(mapper);
        Assert.IsType<JMSMapper>(mapper);

        var source1 = new Source { Id = 1, Name = "Test1" };
        var destination1 = mapper.Map<Destination>(source1);
        Assert.Equal("Test1 From Profile", destination1.FullName);

        var source2 = new SourceA { Id = 2, Name = "Test2", Value = 1.0 };
        var destination2 = mapper.Map<DestinationA>(source2);
        Assert.Equal("Test2 From Profile2", destination2.Name);
    }

    [Fact]
    public void Map_ShouldReverseMapCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullName", src => src.Name)
              .ReverseMap();
        var mapper = new JMSMapper(config);

        var destination = new Destination { Id = 10, FullName = "Reverse Test" };

        // Act
        var source = mapper.Map<Source>(destination);

        // Assert
        Assert.NotNull(source);
        Assert.Equal(destination.Id, source.Id);
        Assert.Equal(destination.FullName, source.Name);
    }

    public class SourceG
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DestinationG
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    [Fact]
    public void Map_ShouldApplyCustomNamingConvention()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.NamingConvention = name =>
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        };
        config.CreateMap<SourceG, DestinationG>();
        var mapper = new JMSMapper(config);

        var source = new SourceG { FirstName = "John", LastName = "Doe" };

        // Act
        var destination = mapper.Map<DestinationG>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(source.FirstName, destination.firstName);
        Assert.Equal(source.LastName, destination.lastName);
    }

    [Fact]
    public void Map_ShouldHandleFailedTypeConversion()
    {
        // Arrange
        var config = new MapperConfiguration { ThrowOnConversionError = true };
        var mapper = new JMSMapper(config);
        string source = "abc";

        // Act & Assert
        Assert.Throws<MappingException>(() => mapper.Map<int>(source));
    }

    [Fact]
    public void Map_ShouldHandleCircularReferences()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<PersonSource, PersonDestination>();
        config.CreateMap<ChildSource, ChildDestination>();
        var mapper = new JMSMapper(config);

        var personSource = new PersonSource { Id = 1, Name = "Parent" };
        var childSource = new ChildSource { Id = 2, Name = "Child", Parent = personSource };
        personSource.Child = childSource;

        // Act
        var personDestination = mapper.Map<PersonDestination>(personSource);

        // Assert
        Assert.NotNull(personDestination);
        Assert.Equal(personSource.Id, personDestination.Id);
        Assert.Equal(personSource.Name, personDestination.Name);
        Assert.NotNull(personDestination.Child);
        Assert.Equal(childSource.Id, personDestination.Child.Id);
        Assert.Equal(childSource.Name, personDestination.Child.Name);
        Assert.NotNull(personDestination.Child.Parent);
        Assert.Equal(personSource.Id, personDestination.Child.Parent.Id);
        Assert.Equal(personSource.Name, personDestination.Child.Parent.Name);
        Assert.Same(personDestination, personDestination.Child.Parent); // Verify same instance for circular reference
    }

    [Fact]
    public void Map_ShouldHandleUnmappedProperties()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceH, DestinationH>();
        var mapper = new JMSMapper(config);

        var source = new SourceH { Id = 1, Name = "Test Source" };

        // Act
        var destination = mapper.Map<DestinationH>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
        Assert.Null(destination.UnmappedProperty); // Should be null as it's not mapped
    }

    [Fact]
    public void Map_ShouldUseDefaultConstructorWhenAvailable()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceWithData, DestinationWithDefaultConstructor>();
        var mapper = new JMSMapper(config);

        var source = new SourceWithData { Value1 = 10, Value2 = "Hello" };

        // Act
        var destination = mapper.Map<DestinationWithDefaultConstructor>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(source.Value1, destination.Value1);
        Assert.Equal(source.Value2, destination.Value2);
    }

    //[Fact]
    //public void Map_ShouldUseParameterizedConstructorWhenAvailable()
    //{
    //    // Arrange
    //    var config = new MapperConfiguration();
    //    config.CreateMap<SourceWithData, DestinationWithParameterizedConstructor>();
    //    var mapper = new JMSMapper(config);

    //    var source = new SourceWithData { Value1 = 20, Value2 = "World" };

    //    // Act
    //    var destination = mapper.Map<DestinationWithParameterizedConstructor>(source);

    //    // Assert
    //    Assert.NotNull(destination);
    //    Assert.Equal(source.Value1, destination.Value1);
    //    Assert.Equal(source.Value2, destination.Value2);
    //}

    public class DestinationWithMultipleConstructors
    {
        public int Value1 { get; }
        public string Value2 { get; }
        public string ConstructorUsed { get; }

        public DestinationWithMultipleConstructors()
        {
            ConstructorUsed = "Default";
        }

        public DestinationWithMultipleConstructors(int value1)
        {
            Value1 = value1;
            ConstructorUsed = "Int";
        }

        public DestinationWithMultipleConstructors(int value1, string value2)
        {
            Value1 = value1;
            Value2 = value2;
            ConstructorUsed = "IntAndString";
        }
    }

    public class DestinationWithDifferentParameterName
    {
        public int Value1 { get; }
        public string Value2 { get; }

        public DestinationWithDifferentParameterName(int val1, string val2)
        {
            Value1 = val1;
            Value2 = val2;
        }
    }

    [Fact]
    public void Map_ShouldUseExplicitlySelectedConstructor()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceWithData, DestinationWithMultipleConstructors>()
            .UseConstructor(typeof(int), typeof(string));
        var mapper = new JMSMapper(config);

        var source = new SourceWithData { Value1 = 30, Value2 = "Explicit" };

        // Act
        var destination = mapper.Map<DestinationWithMultipleConstructors>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal("IntAndString", destination.ConstructorUsed);
        Assert.Equal(source.Value1, destination.Value1);
        Assert.Equal(source.Value2, destination.Value2);
    }

    //[Fact]
    //public void Map_ShouldUseParameterizedConstructorWithDifferentParameterName()
    //{
    //    // Arrange
    //    var config = new MapperConfiguration();
    //    config.CreateMap<SourceWithData, DestinationWithDifferentParameterName>()
    //        .UseConstructor(typeof(int), typeof(string));
    //    var mapper = new JMSMapper(config);

    //    var source = new SourceWithData { Value1 = 40, Value2 = "Different" };

    //    // Act
    //    var destination = mapper.Map<DestinationWithDifferentParameterName>(source);

    //    // Assert
    //    Assert.NotNull(destination);
    //    Assert.Equal(source.Value1, destination.Value1);
    //    Assert.Equal(source.Value2, destination.Value2);
    //}

    [Fact]
    public void Map_ShouldMapEnumToString()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceEnum, DestinationEnum>()
            .ForMember("EnumStringValue", src => src.EnumValue.ToString());
        var mapper = new JMSMapper(config);

        var source = new SourceEnum { EnumValue = MyEnum.Value1 };

        // Act
        var destination = mapper.Map<DestinationEnum>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(MyEnum.Value1, destination.EnumValue);
        Assert.Equal("Value1", destination.EnumStringValue);
    }

    [Fact]
    public void Map_ShouldMapStringtoEnum()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceEnum, DestinationEnum>();
        var mapper = new JMSMapper(config);

        var source = new SourceEnum { StringValue = "Value2" };

        // Act
        var destination = mapper.Map<DestinationEnum>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(MyEnum.Value2, destination.StringValue);
    }

    [Fact]
    public void Map_ShouldMapIntToEnum()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceEnum, DestinationEnum>();
        var mapper = new JMSMapper(config);

        var source = new SourceEnum { IntValue = 0 }; // MyEnum.Value1

        // Act
        var destination = mapper.Map<DestinationEnum>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(MyEnum.Value1, destination.IntValue);
    }

    [Fact]
    public void Map_ShouldMapEnumToEnum()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceEnum, DestinationEnum>();
        var mapper = new JMSMapper(config);

        var source = new SourceEnum { EnumValue = MyEnum.Value3 };

        // Act
        var destination = mapper.Map<DestinationEnum>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(MyEnum.Value3, destination.EnumValue);
    }

    [Fact]
    public void Map_ShouldHandleInvalidEnumConversion()
    {
        // Arrange
        var config = new MapperConfiguration { ThrowOnConversionError = true };
        config.CreateMap<SourceEnum, DestinationEnum>();
        var mapper = new JMSMapper(config);

        var source = new SourceEnum { StringValue = "InvalidValue" };

        // Act & Assert
        Assert.Throws<MappingException>(() => mapper.Map<DestinationEnum>(source));
    }

    [Fact]
    public void Map_ShouldPerformWellWithLargeVolumeOfData_Sync()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>();
        var mapper = new JMSMapper(config);

        var sources = new List<Source>();
        for (int i = 0; i < 10000; i++)
        {
            sources.Add(new Source { Id = i, Name = $"Test {i}" });
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var destinations = new List<Destination>();
        foreach (var source in sources)
        {
            destinations.Add(mapper.Map<Destination>(source));
        }

        stopwatch.Stop();

        // Assert
        Assert.Equal(sources.Count, destinations.Count);
        // You might want to add a time assertion here, e.g., Assert.True(stopwatch.ElapsedMilliseconds < 500);
        // For now, just ensure it runs without error.
        Console.WriteLine($"Sync mapping 10,000 items took: {stopwatch.ElapsedMilliseconds} ms");
    }


}