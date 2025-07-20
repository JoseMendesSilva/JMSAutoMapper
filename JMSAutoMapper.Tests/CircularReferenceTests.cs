namespace JMSAutoMapper.Tests;

public class CircularReferenceTests
    {
        //public class Employee
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //    public Employee Manager { get; set; }
        //    public List<Employee> Subordinates { get; set; } = new List<Employee>();
        //}

        //public class EmployeeDto
        //{
        //    public int Id { get; set; }
        //    public string FullName { get; set; }
        //    public EmployeeDto Manager { get; set; }
        //    public List<EmployeeDto> TeamMembers { get; set; } = new List<EmployeeDto>();
        //}

        //public class EmployeeProfile : JMSProfile
        //{
        //    public override void Configure(IJMSMapperConfiguration config)
        //    {
        //        config.CreateMap<Employee, EmployeeDto>()
        //            .ForMember(dest => dest.FullName, opt => opt.MapFrom("Name"))
        //            .ForMember(dest => dest.TeamMembers, opt => opt.MapFrom("Subordinates"));
        //    }
        //}

        //[Fact]
        //public void Map_EmployeeWithCircularReference_ShouldNotStackOverflow()
        //{
        //    // Arrange
        //    var mapper = new JMSMapper();
        //    mapper.AddProfile(new EmployeeProfile());
        //    mapper.Initialize();

        //    var ceo = new Employee { Id = 1, Name = "John CEO" };
        //    var manager = new Employee { Id = 2, Name = "Alice Manager", Manager = ceo };
        //    var employee = new Employee { Id = 3, Name = "Bob Employee", Manager = manager };

        //    ceo.Subordinates.Add(manager);
        //    manager.Subordinates.Add(employee);

        //    // Act
        //    var result = mapper.Map<EmployeeDto>(ceo);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(ceo.Name, result.FullName);
        //    Assert.Single(result.TeamMembers);
        //    Assert.Equal(manager.Name, result.TeamMembers[0].FullName);
        //    Assert.Single(result.TeamMembers[0].TeamMembers);
        //    Assert.Equal(employee.Name, result.TeamMembers[0].TeamMembers[0].FullName);

        //    // Verifica referências circulares
        //    Assert.Null(result.Manager); // CEO não tem manager
        //    Assert.Same(result, result.TeamMembers[0].Manager); // Manager do manager é o CEO
        //}
    }