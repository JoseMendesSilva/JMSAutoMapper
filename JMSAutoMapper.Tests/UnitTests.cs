namespace JMSAutoMapper.Tests;

public partial class UnitTests
{

    //// Exemplo de implementação corrigida
    //public class UserProfile : JMSProfile
    //{
    //    public override void Configure(IJMSMapperConfiguration config)
    //    {
    //        config.CreateMap<Source, Destination>()
    //            .ForMember(dest => dest.FullName, opt => opt.MapFrom("Name"))
    //            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.BirthDate)));
    //    }

    //    private int CalculateAge(DateTime birthDate)
    //    {
    //        var today = DateTime.Today;
    //        var age = today.Year - birthDate.Year;
    //        if (birthDate.Date > today.AddYears(-age)) age--;
    //        return age;
    //    }
    //}

    //    //public class UserProfile : JMSProfile
    //    //{
    //    //    public override void Configure()
    //    //    {
    //    //        CreateMap<Source, Destination>()
    //    //            .ForMember(dest => dest.FullName, opt => opt.MapFrom("Name"))
    //    //            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.BirthDate)));
    //    //    }

    //    //    private int CalculateAge(DateTime birthDate)
    //    //    {
    //    //        var today = DateTime.Today;
    //    //        var age = today.Year - birthDate.Year;
    //    //        if (birthDate.Date > today.AddYears(-age)) age--;
    //    //        return age;
    //    //    }
    //    //}

    //    [Fact]
    //public void Map_SimpleObject_ShouldMapCorrectly()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    mapper.AddProfile(new UserProfile());
    //    mapper.Initialize();

    //    var source = new Source { Id = 1, Name = "Test", BirthDate = DateTime.Now.AddYears(-30) };

    //    // Act
    //    var result = mapper.Map<Destination>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(source.Id, result.Id);
    //    Assert.Equal(source.Name, result.FullName);
    //    Assert.Equal(30, result.Age);
    //}

    //[Fact]
    //public void Map_WithCircularReference_ShouldNotStackOverflow()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    mapper.SetMaxRecursionDepth(10);

    //    var parent = new Source { Id = 1 };
    //    var child = new Source { Id = 2, Parent = parent };
    //    parent.Children.Add(child);

    //    // Act
    //    var result = mapper.Map<Destination>(parent);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Single(result.Children);
    //    Assert.Equal(parent.Id, result.Id);
    //    Assert.Equal(child.Id, result.Children[0].Id);

    //    // Verifica se a referência circular foi mantida corretamente
    //    Assert.Same(result, result.Children[0].Parent);
    //}
    //[Fact]
    //public void MapIEnumerable_ShouldMapAllItems()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var sources = Enumerable.Range(1, 5)
    //        .Select(i => new Source { Id = i, Name = $"Test{i}" });

    //    // Act
    //    var results = mapper.MapIEnumerable<Destination>(sources);

    //    // Assert
    //    Assert.Equal(5, results.Count());
    //    Assert.All(results, r => Assert.NotNull(r));
    //}

    //[Fact]
    //public async Task MapAsync_ShouldReturnMappedObject()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var source = new Source { Id = 1, Name = "Test" };

    //    // Act
    //    var result = await mapper.MapAsync<Destination>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(source.Id, result.Id);
    //}

    //[Fact]
    //public void MapDictionary_ShouldConvertKeyAndValue()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var source = new Dictionary<int, Source>
    //        {
    //            { 1, new Source { Id = 1, Name = "Test1" } },
    //            { 2, new Source { Id = 2, Name = "Test2" } }
    //        };

    //    // Act
    //    var result = mapper.MapDictionary<int, Destination>(source);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count);
    //    Assert.Equal("Test1", result[1].FullName);
    //}

    //[Fact]
    //public void MapToCacheItem_ShouldReturnCacheItemWithMappedValue()
    //{
    //    // Arrange
    //    var mapper = new JMSMapper();
    //    var source = new Source { Id = 1, Name = "Test" };

    //    // Act
    //    var result = mapper.MapToCacheItem<Destination>(source, "test_key");

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal("test_key", result.Key);
    //    Assert.NotNull(result.Value);
    //    //Assert.Equal(1, result.Value);
    //}
}