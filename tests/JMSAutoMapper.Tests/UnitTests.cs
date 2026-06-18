using System.Collections.Immutable;
using System.Diagnostics;
using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.Core;
using JMSAutoMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace JMSAutoMapper.Tests;

public partial class UnitTests
{

    [Fact(DisplayName = "Map Dictionary - Key and Value Conversion")]
    [Trait("Category", "Collections")]
    public void MapDictionary_ShouldConvertKeyAndValue()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var source = new Dictionary<int, Source>
            {
                { 1, new Source { Id = 1, RazaoSocial = "Test1" } },
                { 2, new Source { Id = 2, RazaoSocial = "Test2" } }
            };

        // Act
        var result = mapper.Map<Dictionary<int, Destination>>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "Simple Map - Int to Int")]
    [Trait("Category", "Primitive Types")]
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

    [Fact(DisplayName = "Simple Map - Int to String")]
    [Trait("Category", "Primitive Types")]
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

    [Fact(DisplayName = "Simple Map - String to Int")]
    [Trait("Category", "Primitive Types")]
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

    [Fact(DisplayName = "Simple Map - DateTime to DateTime")]
    [Trait("Category", "Primitive Types")]
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

    [Fact(DisplayName = "Simple Map - Guid to Guid")]
    [Trait("Category", "Primitive Types")]
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

    [Fact(DisplayName = "Complex Map - Matching Properties")]
    [Trait("Category", "Complex Objects")]
    public void Map_ShouldMapComplexObjectWithMatchingProperties()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var source = new SourceA { Id = 1, RazaoSocial = "Test", Value = 123.45 };

        // Act
        var result = mapper.Map<DestinationA>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.RazaoSocial, result.RazaoSocial);
        Assert.Equal(source.Value, result.Value);
    }

    [Fact(DisplayName = "ForMember - Rename Property")]
    [Trait("Category", "Configuration")]
    public void Map_ShouldReRazaoSocialPropertyUsingForMember()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceB, DestinationB>()
              .ForMember("ReRazaoSocialdProperty", src => src.OriginalProperty);
        var mapper = new JMSMapper(config);
        var source = new SourceB { OriginalProperty = "Hello ForMember" };

        // Act
        var result = mapper.Map<DestinationB>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.OriginalProperty, result.ReRazaoSocialdProperty);
    }

    [Fact(DisplayName = "Custom Resolver - ForMember Logic")]
    [Trait("Category", "Advanced Mapping")]
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

    [Fact(DisplayName = "Ignore Member - Should Not Map")]
    [Trait("Category", "Configuration")]
    public void Map_ShouldIgnoreProperty()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceD, DestinationD>()
              .Ignore(dest => dest.OtherProperty);
        var mapper = new JMSMapper(config);
        var source = new SourceD { Id = 1, RazaoSocial = "Test", IgnoredProperty = "Should Be Ignored" };

        // Act
        var result = mapper.Map<DestinationD>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.RazaoSocial, result.RazaoSocial);
        Assert.Null(result.OtherProperty); // OtherProperty should be null as it was ignored
    }

    [Fact(DisplayName = "Conditional Mapping - Condition is True")]
    [Trait("Category", "Advanced Mapping")]
    public void Map_ShouldApplyConditionalMappingWhenConditionIsTrue()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceE, DestinationE>()
              .ForMember("ConditionalProperty", src => src.RazaoSocial, src => src.Condition);
        var mapper = new JMSMapper(config);
        var source = new SourceE { Id = 1, RazaoSocial = "Conditional Test", Condition = true };

        // Act
        var result = mapper.Map<DestinationE>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.RazaoSocial, result.RazaoSocial);
        Assert.Equal(source.RazaoSocial, result.ConditionalProperty);
    }

    [Fact(DisplayName = "Conditional Mapping - Condition is False")]
    [Trait("Category", "Advanced Mapping")]
    public void Map_ShouldNotApplyConditionalMappingWhenConditionIsFalse()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceE, DestinationE>()
              .ForMember("ConditionalProperty", src => src.RazaoSocial, src => src.Condition);
        var mapper = new JMSMapper(config);
        var source = new SourceE { Id = 1, RazaoSocial = "Conditional Test", Condition = false };

        // Act
        var result = mapper.Map<DestinationE>(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.RazaoSocial, result.RazaoSocial);
        Assert.Null(result.ConditionalProperty); // Should be null or default if not mapped
    }

    [Fact(DisplayName = "Nullable Map - Nullable Int to Int")]
    [Trait("Category", "Nullable Types")]
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

    [Fact(DisplayName = "Nullable Map - Int to Nullable Int")]
    [Trait("Category", "Nullable Types")]
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

    [Fact(DisplayName = "Nullable Map - String to Nullable String")]
    [Trait("Category", "Nullable Types")]
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

    [Fact(DisplayName = "Nullable Map - Null Source to Nullable Type")]
    [Trait("Category", "Nullable Types")]
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

    [Fact(DisplayName = "Safety - Throw Exception on Null to ValueType")]
    [Trait("Category", "Safety")]
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

    [Fact(DisplayName = "IEnumerable Map - Basic List")]
    [Trait("Category", "Collections")]
    public void MapIEnumerable_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "Test1" },
            new Source { Id = 2, RazaoSocial = "Test2" }
        };

        // Act
        var destinations = mapper.Map<IEnumerable<Destination>>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "IEnumerable Map - Empty Source")]
    [Trait("Category", "Collections")]
    public void MapIEnumerable_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new List<Source>();

        // Act
        var destinations = mapper.Map<IEnumerable<Destination>>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "IEnumerable Map - Skip Null Items")]
    [Trait("Category", "Collections")]
    public void MapIEnumerable_ShouldHandleNullItemsInCollection()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new List<Source?>
        {
            new Source { Id = 1, RazaoSocial = "Test1" },
            null,
            new Source { Id = 3, RazaoSocial = "Test3" }
        };

        // Act
        var destinations = mapper.Map<IEnumerable<Destination>>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count); // Null item should be skipped
        Assert.Equal(sources[0]!.Id, destinations[0].Id);
        Assert.Equal(sources[0]!.RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[2]!.Id, destinations[1].Id);
        Assert.Equal(sources[2]!.RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "List Map - Basic List")]
    [Trait("Category", "Collections")]
    public void MapList_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "ListTest1" },
            new Source { Id = 2, RazaoSocial = "ListTest2" }
        };

        // Act
        var destinations = mapper.Map<List<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "List Map - Empty Source")]
    [Trait("Category", "Collections")]
    public void MapList_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new List<Source>();

        // Act
        var destinations = mapper.Map<List<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "Array Map - Basic Array")]
    [Trait("Category", "Collections")]
    public void MapArray_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new Source[]
        {
            new Source { Id = 1, RazaoSocial = "ArrayTest1" },
            new Source { Id = 2, RazaoSocial = "ArrayTest2" }
        };

        // Act
        var destinations = mapper.Map<Destination[]>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Length);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "Array Map - Empty Source")]
    [Trait("Category", "Collections")]
    public void MapArray_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new Source[0];

        // Act
        var destinations = mapper.Map<Destination[]>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "HashSet Map - Basic HashSet")]
    [Trait("Category", "Collections")]
    public void MapHashSet_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new HashSet<Source>
        {
            new Source { Id = 1, RazaoSocial = "HashSetTest1" },
            new Source { Id = 2, RazaoSocial = "HashSetTest2" }
        };

        // Act
        var destinations = mapper.Map<HashSet<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(0).Id && d.FullRazaoSocial == sources.ElementAt(0).RazaoSocial);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(1).Id && d.FullRazaoSocial == sources.ElementAt(1).RazaoSocial);
    }

    [Fact(DisplayName = "HashSet Map - Empty Source")]
    [Trait("Category", "Collections")]
    public void MapHashSet_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new HashSet<Source>();

        // Act
        var destinations = mapper.Map<HashSet<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "Dictionary Map - Basic Dictionary")]
    [Trait("Category", "Collections")]
    public void MapDictionary_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new Dictionary<int, Source>
        {
            { 1, new Source { Id = 1, RazaoSocial = "DictTest1" } },
            { 2, new Source { Id = 2, RazaoSocial = "DictTest2" } }
        };

        // Act
        var destinations = mapper.Map<Dictionary<int, Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.Values.ElementAt(0).Id, destinations.Values.ElementAt(0).Id);
        Assert.Equal(sources.Values.ElementAt(0).RazaoSocial, destinations.Values.ElementAt(0).FullRazaoSocial);
        Assert.Equal(sources.Values.ElementAt(1).Id, destinations.Values.ElementAt(1).Id);
        Assert.Equal(sources.Values.ElementAt(1).RazaoSocial, destinations.Values.ElementAt(1).FullRazaoSocial);
    }

    [Fact(DisplayName = "Dictionary Map - Empty Source")]
    [Trait("Category", "Collections")]
    public void MapDictionary_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = new Dictionary<int, Source>();

        // Act
        var destinations = mapper.Map<Dictionary<int, Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "Dictionary Map - Null Source returns Empty")]
    [Trait("Category", "Collections")]
    public void MapDictionary_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<Dictionary<int, Destination>>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "Dictionary Map - Skip Null Values")]
    [Trait("Category", "Collections")]
    public void MapDictionary_ShouldHandleNullValuesInCollection()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new Dictionary<int, Source?>
        {
            { 1, new Source { Id = 1, RazaoSocial = "DictTest1" } },
            { 2, null },
            { 3, new Source { Id = 3, RazaoSocial = "DictTest3" } }
        };

        // Act
        var destinations = mapper.Map<Dictionary<int, Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count); // Null value should be skipped
        Assert.Equal(sources[1]!.Id, destinations[1].Id);
        Assert.Equal(sources[1]!.RazaoSocial, destinations[1].FullRazaoSocial);
        Assert.Equal(sources[3]!.Id, destinations[3].Id);
        Assert.Equal(sources[3]!.RazaoSocial, destinations[3].FullRazaoSocial);
    }

    [Fact(DisplayName = "ImmutableList Map - Basic Mapping")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableList_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = ImmutableList.Create(
            new Source { Id = 1, RazaoSocial = "ImmutableListTest1" },
            new Source { Id = 2, RazaoSocial = "ImmutableListTest2" }
        );

        // Act
        var destinations = mapper.Map<ImmutableList<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "ImmutableList Map - Empty Source")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableList_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableList.Create<Source>();

        // Act
        var destinations = mapper.Map<ImmutableList<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableList Map - Null Source returns Empty")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableList_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<ImmutableList<Destination>>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableDictionary Map - Basic Mapping")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableDictionary_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = ImmutableDictionary.CreateRange(new Dictionary<int, Source>
        {
            { 1, new Source { Id = 1, RazaoSocial = "ImmutableDictTest1" } },
            { 2, new Source { Id = 2, RazaoSocial = "ImmutableDictTest2" } }
        });

        // Act
        var destinations = mapper.Map<ImmutableDictionary<int, Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
        Assert.Equal(sources[2].Id, destinations[2].Id);
        Assert.Equal(sources[2].RazaoSocial, destinations[2].FullRazaoSocial);
    }

    [Fact(DisplayName = "ImmutableDictionary Map - Empty Source")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableDictionary_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableDictionary.Create<int, Source>();

        // Act
        var destinations = mapper.Map<ImmutableDictionary<int, Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableDictionary Map - Null Source returns Empty")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableDictionary_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<ImmutableDictionary<int, Destination>>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableArray Map - Basic Mapping")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableArray_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = ImmutableArray.Create(
            new Source { Id = 1, RazaoSocial = "ImmutableArrayTest1" },
            new Source { Id = 2, RazaoSocial = "ImmutableArrayTest2" }
        );

        // Act
        var destinations = mapper.Map<ImmutableArray<Destination>>(sources);

        // Assert
        Assert.False(destinations.IsDefault);
        Assert.Equal(2, destinations.Length);
        Assert.Equal(sources[0].Id, destinations[0].Id);
        Assert.Equal(sources[0].RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources[1].Id, destinations[1].Id);
        Assert.Equal(sources[1].RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "ImmutableArray Map - Empty Source")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableArray_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableArray.Create<Source>();

        // Act
        var destinations = mapper.Map<ImmutableArray<Destination>>(sources);

        // Assert
        Assert.False(destinations.IsDefault);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableArray Map - Null Source returns Empty")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableArray_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<ImmutableArray<Destination>>(null);

        // Assert
        Assert.False(destinations.IsDefault);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableQueue Map - Basic Mapping")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableQueue_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = ImmutableQueue.CreateRange(new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "ImmutableQueueTest1" },
            new Source { Id = 2, RazaoSocial = "ImmutableQueueTest2" }
        });

        // Act
        var destinations = mapper.Map<ImmutableQueue<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count());
        Assert.Equal(sources.Peek().Id, destinations.Peek().Id);
        Assert.Equal(sources.Peek().RazaoSocial, destinations.Peek().FullRazaoSocial);
    }

    [Fact(DisplayName = "ImmutableQueue Map - Empty Source")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableQueue_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableQueue.Create<Source>();

        // Act
        var destinations = mapper.Map<ImmutableQueue<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableQueue Map - Null Source returns Empty")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableQueue_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<ImmutableQueue<Destination>>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableStack Map - Basic Mapping")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableStack_ShouldMapCollectionCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = ImmutableStack.CreateRange((IEnumerable<Source>)new List<Source>
            { new Source { Id = 1, RazaoSocial = "ImmutableStackTest1" },
            new Source { Id = 2, RazaoSocial = "ImmutableStackTest2" } }
        );

        var destinations = mapper.Map<ImmutableStack<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count());
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(0).Id && d.FullRazaoSocial == sources.ElementAt(0).RazaoSocial);
        Assert.Contains(destinations, d => d.Id == sources.ElementAt(1).Id && d.FullRazaoSocial == sources.ElementAt(1).RazaoSocial);
    }

    [Fact(DisplayName = "ImmutableStack Map - Empty Source")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableStack_ShouldReturnEmptyCollectionWhenSourceIsEmpty()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());
        var sources = ImmutableStack.Create<Source>();

        // Act
        var destinations = mapper.Map<ImmutableStack<Destination>>(sources);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }

    [Fact(DisplayName = "ImmutableStack Map - Null Source returns Empty")]
    [Trait("Category", "Immutable Collections")]
    public void MapImmutableStack_ShouldReturnEmptyWhenSourceIsNull()
    {
        // Arrange
        var mapper = new JMSMapper(new MapperConfiguration());

        // Act
        var destinations = mapper.Map<ImmutableStack<Destination>>(null);

        // Assert
        Assert.NotNull(destinations);
        Assert.Empty(destinations);
    }





    [Fact(DisplayName = "MapQueryable - Basic Support")]
    [Trait("Category", "Queryable")]
    public void MapQueryable_ShouldMapBasicQueryable()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "QueryableTest1" },
            new Source { Id = 2, RazaoSocial = "QueryableTest2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).RazaoSocial, destinations[0].FullRazaoSocial);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).RazaoSocial, destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "MapQueryable - Nested Properties")]
    [Trait("Category", "Queryable")]
    public void MapQueryable_ShouldMapNestedProperties()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceF, DestinationF>();
        config.CreateMap<NestedSourceF, NestedDestinationF>();
        var mapper = new JMSMapper(config);
        var sources = new List<SourceF>
        {
            new SourceF { Id = 1, RazaoSocial = "Parent1", Nested = new NestedSourceF { Value = "NestedValue1" } },
            new SourceF { Id = 2, RazaoSocial = "Parent2", Nested = new NestedSourceF { Value = "NestedValue2" } }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<SourceF, DestinationF>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).RazaoSocial, destinations[0].RazaoSocial);
        Assert.Equal(sources.ElementAt(0).Nested.Value, destinations[0].Nested.Value);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).RazaoSocial, destinations[1].RazaoSocial);
        Assert.Equal(sources.ElementAt(1).Nested.Value, destinations[1].Nested.Value);
    }

    [Fact(DisplayName = "MapQueryable - Support ForMember and Ignore")]
    [Trait("Category", "Queryable")]
    public void MapQueryable_ShouldRespectForMemberAndIgnore()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial + " Mapped")
              .Ignore(dest => dest.Age);
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "QueryableForMemberIgnore1" },
            new Source { Id = 2, RazaoSocial = "QueryableForMemberIgnore2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Equal(sources.ElementAt(0).RazaoSocial + " Mapped", destinations[0].FullRazaoSocial);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Equal(sources.ElementAt(1).RazaoSocial + " Mapped", destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "MapQueryable - Default Projection")]
    [Trait("Category", "Queryable")]
    public void MapQueryable_ShouldProjectPropertiesCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>();
        var mapper = new JMSMapper(config);
        var sources = new List<Source>
        {
            new Source { Id = 1, RazaoSocial = "Test1" },
            new Source { Id = 2, RazaoSocial = "Test2" }
        }.AsQueryable();

        // Act
        var destinations = mapper.MapQueryable<Source, Destination>(sources).ToList();

        // Assert
        Assert.NotNull(destinations);
        Assert.Equal(2, destinations.Count);
        Assert.Equal(sources.ElementAt(0).Id, destinations[0].Id);
        Assert.Null(destinations[0].FullRazaoSocial);
        Assert.Equal(sources.ElementAt(1).Id, destinations[1].Id);
        Assert.Null(destinations[1].FullRazaoSocial);
    }

    [Fact(DisplayName = "Dependency Injection - Register IMapper")]
    [Trait("Category", "Integration")]
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

    [Fact(DisplayName = "Dependency Injection - Register IMapper with recommended API")]
    [Trait("Category", "Integration")]
    public void AddJMSAutoMapper_ShouldAddMapperToDIMain()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJMSAutoMapper();
        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetService<IMapper>();

        // Assert
        Assert.NotNull(mapper);
        Assert.IsType<JMSMapper>(mapper);
    }

    [Fact(DisplayName = "Dependency Injection - Register with Configuration")]
    [Trait("Category", "Integration")]
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
                .ForMember("FullRazaoSocial", src => src.RazaoSocial + " From Profile");
        }
    }

    public class TestProfile2 : Profile
    {
        public override void Configure()
        {
            CreateMap<SourceA, DestinationA>()
                .ForMember("RazaoSocial", src => src.RazaoSocial + " From Profile2");
        }
    }

    [Fact(DisplayName = "Dependency Injection - Register with Profiles")]
    [Trait("Category", "Integration")]
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

        var source = new Source { Id = 1, RazaoSocial = "Test" };
        var destination = mapper.Map<Destination>(source);

        Assert.Equal("Test From Profile", destination.FullRazaoSocial);
    }

    [Fact(DisplayName = "Dependency Injection - Handle Multiple Profiles")]
    [Trait("Category", "Integration")]
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

        var source1 = new Source { Id = 1, RazaoSocial = "Test1" };
        var destination1 = mapper.Map<Destination>(source1);
        Assert.Equal("Test1 From Profile", destination1.FullRazaoSocial);

        var source2 = new SourceA { Id = 2, RazaoSocial = "Test2", Value = 1.0 };
        var destination2 = mapper.Map<DestinationA>(source2);
        Assert.Equal("Test2 From Profile2", destination2.RazaoSocial);
    }

    [Fact(DisplayName = "Reverse Map - Bidirectional Support")]
    [Trait("Category", "Configuration")]
    public void Map_ShouldReverseMapCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>()
              .ForMember("FullRazaoSocial", src => src.RazaoSocial)
              .ReverseMap();
        var mapper = new JMSMapper(config);

        var destination = new Destination { Id = 10, FullRazaoSocial = "Reverse Test" };

        // Act
        var source = mapper.Map<Source>(destination);

        // Assert
        Assert.NotNull(source);
        Assert.Equal(destination.Id, source.Id);
        Assert.Equal(destination.FullRazaoSocial, source.RazaoSocial);
    }

    public class SourceG
    {
        public string FirstRazaoSocial { get; set; } = default!;
        public string LastRazaoSocial { get; set; } = default!;
    }

    public class DestinationG
    {
        public string firstRazaoSocial { get; set; } = default!;
        public string lastRazaoSocial { get; set; } = default!;
    }

    [Fact(DisplayName = "Naming Convention - Custom Strategy")]
    [Trait("Category", "Configuration")]
    public void Map_ShouldApplyCustomNamingConvention()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.NamingConvention = RazaoSocial =>
        {
            if (string.IsNullOrEmpty(RazaoSocial)) return RazaoSocial;
            return char.ToLowerInvariant(RazaoSocial[0]) + RazaoSocial.Substring(1);
        };
        config.CreateMap<SourceG, DestinationG>();
        var mapper = new JMSMapper(config);

        var source = new SourceG { FirstRazaoSocial = "John", LastRazaoSocial = "Doe" };

        // Act
        var destination = mapper.Map<DestinationG>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(source.FirstRazaoSocial, destination.firstRazaoSocial);
        Assert.Equal(source.LastRazaoSocial, destination.lastRazaoSocial);
    }

    [Fact(DisplayName = "Safety - Handle Conversion Errors")]
    [Trait("Category", "Safety")]
    public void Map_ShouldHandleFailedTypeConversion()
    {
        // Arrange
        var config = new MapperConfiguration { ThrowOnConversionError = true };
        var mapper = new JMSMapper(config);
        string source = "abc";

        // Act & Assert
        Assert.Throws<MappingException>(() => mapper.Map<int>(source));
    }

    [Fact(DisplayName = "Circular Reference - Object Graph Safety")]
    [Trait("Category", "Safety")]
    public void Map_ShouldHandleCircularReferences()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<PersonSource, PersonDestination>();
        config.CreateMap<ChildSource, ChildDestination>();
        var mapper = new JMSMapper(config);

        var personSource = new PersonSource { Id = 1, RazaoSocial = "Parent" };
        var childSource = new ChildSource { Id = 2, RazaoSocial = "Child", Parent = personSource };
        personSource.Child = childSource;

        // Act
        var personDestination = mapper.Map<PersonDestination>(personSource);

        // Assert
        Assert.NotNull(personDestination);
        Assert.Equal(personSource.Id, personDestination.Id);
        Assert.Equal(personSource.RazaoSocial, personDestination.RazaoSocial);
        Assert.NotNull(personDestination.Child);
        Assert.Equal(childSource.Id, personDestination.Child.Id);
        Assert.Equal(childSource.RazaoSocial, personDestination.Child.RazaoSocial);
        Assert.NotNull(personDestination.Child.Parent);
        Assert.Equal(personSource.Id, personDestination.Child.Parent.Id);
        Assert.Equal(personSource.RazaoSocial, personDestination.Child.Parent.RazaoSocial);
        Assert.Same(personDestination, personDestination.Child.Parent); // Verify same instance for circular reference
    }

    [Fact(DisplayName = "Complex Map - Handle Unmapped properties")]
    [Trait("Category", "Complex Objects")]
    public void Map_ShouldHandleUnmappedProperties()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<SourceH, DestinationH>();
        var mapper = new JMSMapper(config);

        var source = new SourceH { Id = 1, RazaoSocial = "Test Source" };

        // Act
        var destination = mapper.Map<DestinationH>(source);

        // Assert
        Assert.NotNull(destination);
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.RazaoSocial, destination.RazaoSocial);
        Assert.Null(destination.UnmappedProperty); // Should be null as it's not mapped
    }

    [Fact(DisplayName = "Constructor Selection - Use Default")]
    [Trait("Category", "Advanced Mapping")]
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


    public class DestinationWithMultipleConstructors
    {
        public int Value1 { get; }
        public string Value2 { get; } = default!;
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

    public class DestinationWithDifferentParameterRazaoSocial
    {
        public int Value1 { get; }
        public string Value2 { get; }

        public DestinationWithDifferentParameterRazaoSocial(int val1, string val2)
        {
            Value1 = val1;
            Value2 = val2;
        }
    }

    [Fact(DisplayName = "Constructor Selection - Explicit Selection")]
    [Trait("Category", "Advanced Mapping")]
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

    [Fact(DisplayName = "Enum Map - Enum to String")]
    [Trait("Category", "Enums")]
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

    [Fact(DisplayName = "Enum Map - String to Enum")]
    [Trait("Category", "Enums")]
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

    [Fact(DisplayName = "Enum Map - Int to Enum")]
    [Trait("Category", "Enums")]
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

    [Fact(DisplayName = "Enum Map - Enum to Enum")]
    [Trait("Category", "Enums")]
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

    [Fact(DisplayName = "Enum Map - Safety check")]
    [Trait("Category", "Enums")]
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

    [Fact(DisplayName = "Load Test - Sync High Volume")]
    [Trait("Category", "Performance")]
    public void Map_ShouldPerformWellWithLargeVolumeOfData_Sync()
    {
        // Arrange
        var config = new MapperConfiguration();
        config.CreateMap<Source, Destination>();
        var mapper = new JMSMapper(config);

        var sources = new List<Source>();
        for (int i = 0; i < 10000; i++)
        {
            sources.Add(new Source { Id = i, RazaoSocial = $"Test {i}" });
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
