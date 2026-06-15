# JMSAutoMapper

JMSAutoMapper is a high-performance, flexible, and easy-to-use object-to-object mapper for .NET. It simplifies the process of transferring data between different object models, reducing the need for boilerplate code and improving maintainability.

## Key Features

*   **High Performance:** Uses compiled expression trees to achieve fast and efficient mapping, avoiding the overhead of reflection-based approaches.
*   **Asynchronous Support:** True asynchronous mapping support via `MapAsync`, `IAsyncValueResolver` and async conditionals, powered by parallel execution plans.
*   **Flexible Configuration:** Provides a fluent API for configuring custom mappings, including property RazaoSocial mapping, custom value resolvers, and conditional mapping.
*   **Automatic Flattening:** Automatically maps nested source properties to flat destination properties based on naming conventions (e.g., `Source.Address.City` -> `Destination.AddressCity`).
*   **Mapping Inheritance:** Support for `IncludeBase`, allowing derived mappings to inherit configuration, conditions, and resolvers from base type maps.
*   **Bidirectional Mapping:** Supports automatic reverse mapping, allowing you to map objects in both directions with a single configuration.
*   **LINQ Projections:** Support for `IQueryable` projections (`ProjectTo`, `MapQueryable`) to generate efficient SQL queries with ORMs like Entity Framework.
*   **Intelligent Caching:** Built-in support for distributed caching (`IDistributedMapperCache`) and static caching for optimized performance.
*   **Circular Reference Handling:** Automatically handles circular references to prevent stack overflow exceptions.
*   **Extensive Collection Support:** High-performance mapping for Arrays, Lists, Sets, Dictionaries, and `System.Collections.Immutable` types using pre-compiled collection mappers.
*   **Value Type and Struct Support:** Can map value types (structs) and classes without parameterless constructors.
*   **Global Error Policies:** Configurable strategies for handling null values when mapping to non-nullable value types (Throw, Default, or Ignore).
*   **Constructor Injection:** Support for mapping to objects with parameterized constructors (`ConstructUsing`, `UseConstructor`).
*   **Diagnostics:** Built-in performance metrics and diagnostics tools.
*   **Dependency Injection Integration:** Provides an extension method for easy integration with `Microsoft.Extensions.DependencyInjection`.

## Technologies Used

*   **.NET 8+:** The project is built on the latest version of the .NET platform.
*   **Expression Trees:** The core of the mapper is built using expression trees, which are compiled into highly efficient delegates for maximum performance.
*   **System.Collections.Immutable:** Used for immutable collection support.
*   **Microsoft.Extensions.DependencyInjection.Abstractions:** For seamless integration with dependency injection containers.

## Getting Started

### Installation

To use JMSAutoMapper in your project, you can install it from NuGet:

```bash
Install-Package JMSAutoMapper
```

### Basic Usage

1.  **Create a Mapper Instance:**

    ```csharp
    var mapper = new JMSMapper();
    ```

2.  **Map an Object:**

    ```csharp
    var source = new Source { RazaoSocial = "Test", Age = 30 };
    var destination = mapper.Map<Destination>(source);
    ```

3.  **Map Asynchronously:**

    ```csharp
    var destination = await mapper.MapAsync<Destination>(source, cancellationToken);
    ```

4.  **Project IQueryable (EF Core):**

    ```csharp
    var dtos = mapper.ProjectTo<UserDto>(dbContext.Users).ToList();
    ```

### Configuration

JMSAutoMapper provides a flexible and fluent configuration API.

1.  **Create and Define Mappings (Fluent Style):**

    ```csharp
    var config = new MapperConfiguration(cfg => 
    {
        cfg.CreateMap<Source, Destination>();
        cfg.AddProfile<MyProfile>();
    });
    ```

2.  **Create a Mapper Instance:**

    ```csharp
    var mapper = config.CreateMapper();
    ```

#### Custom Property Mapping

If property RazaoSocials don't match, you can configure a custom mapping:

```csharp
config.CreateMap<Source, Destination>()
    .ForMember("DestRazaoSocial", "RazaoSocial");
```

#### Custom Value Resolvers

You can use a lambda expression to define a custom value resolver:
 
```csharp
config.CreateMap<Source, Destination>()
    .ForMember("FullRazaoSocial", src => $"{src.FirstRazaoSocial} {src.LastRazaoSocial}");
```

#### Conditional Mapping

Map a property only if a condition is met:

```csharp
config.CreateMap<Source, Destination>()
    .ForMember("Value", "Value", src => src.Value > 0);
```

#### Mapping Inheritance (IncludeBase)

You can share mapping configurations between base classes and derived classes to avoid redundancy:

```csharp
config.CreateMap<BaseEntity, BaseDto>()
    .ForMember(dest => dest.CreatedDate, src => src.CreatedAt);

config.CreateMap<UserEntity, UserDto>()
    .IncludeBase<BaseEntity, BaseDto>()
    .ForMember(dest => dest.UserRazaoSocial, src => src.Email);
```

#### Global Null Value Policies

Define how the mapper handles null source values when the destination is a non-nullable value type (like `int` or `decimal`). This can be configured at the `MapperConfiguration` level:

```csharp
config.NullValueMappingStrategy = NullValueMappingPolicy.Ignore; // Options: Throw (default), Default, Ignore
```

#### Bidirectional Mapping

Configure bidirectional mapping with a single call to `ReverseMap()`:

```csharp
config.CreateMap<Source, Destination>()
    .ReverseMap();
```

### Dependency Injection

JMSAutoMapper provides an extension method for easy integration with `Microsoft.Extensions.DependencyInjection`.

1.  **Add the Mapper to the Service Collection:**

    ```csharp
    services.AddJMSMapper(cfg =>
    {
        cfg.AddProfile<MyProfile>();
    }, options => {
        // Optional configuration
        options.EnableDiagnostics = true;
        options.EnableDistributedCache = true;
        options.ValidateOnBuild = true;
        options.MaxDepth = 10;
        options.NullValueMappingStrategy = NullValueMappingPolicy.Default;
    });
    ```

2.  **Inject the Mapper into Your Services:**

    ```csharp
    public class MyService
    {
        private readonly IMapper _mapper;

        public MyService(IMapper mapper)
        {
            _mapper = mapper;
        }
    }
    ```

## Advanced Configuration

### Naming Convention

You can specify a naming convention using a lambda expression. For example, to handle specific transformations:

```csharp
var config = new MapperConfiguration();
config.NamingConvention = RazaoSocial => RazaoSocial.Replace("_", "");
```

### Ignoring Properties

You can ignore properties during mapping.

```csharp
config.CreateMap<Source, Destination>()
    .Ignore("PropertyToIgnore");
```

### Using Profiles

Profiles allow you to organize your mapping configurations.

```csharp
public class MyProfile : Profile
{
    public MyProfile()
    {
        CreateMap<Source, Destination>();
    }
}

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MyProfile>();
});
```

## Advanced Usage Scenarios

### Mapping Nested Objects

JMSAutoMapper can automatically map nested objects, provided that mappings for the nested types are also configured.

```csharp
public class OrderSource
{
    public int OrderId { get; set; }
    public CustomerSource Customer { get; set; }
}

public class CustomerSource
{
    public int CustomerId { get; set; }
    public string RazaoSocial { get; set; }
}

public class OrderDestination
{
    public int OrderId { get; set; }
    public CustomerDestination Customer { get; set; }
}

public class CustomerDestination
{
    public int CustomerId { get; set; }
    public string FullRazaoSocial { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<CustomerSource, CustomerDestination>()
      .ForMember("FullRazaoSocial", src => src.RazaoSocial); // Custom mapping for nested object
config.CreateMap<OrderSource, OrderDestination>();

var mapper = new JMSMapper(config);

// Usage
var orderSource = new OrderSource
{
    OrderId = 1,
    Customer = new CustomerSource { CustomerId = 101, RazaoSocial = "John Doe" }
};

var orderDestination = mapper.Map<OrderDestination>(orderSource);
// orderDestination.Customer.FullRazaoSocial will be "John Doe"
```

### Mapping Collections

Mapping collections is straightforward. JMSAutoMapper handles `IEnumerable<T>`, `List<T>`, and arrays.

```csharp
public class ProductSource
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; }
}

public class ProductDestination
{
    public int ProductId { get; set; }
    public string ProductRazaoSocial { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<ProductSource, ProductDestination>()
      .ForMember("ProductId", "Id")
      .ForMember("ProductRazaoSocial", "RazaoSocial");

var mapper = new JMSMapper(config);

// Usage
var productSources = new List<ProductSource>
{
    new ProductSource { Id = 1, RazaoSocial = "Laptop" },
    new ProductSource { Id = 2, RazaoSocial = "Mouse" }
};

var productDestinations = mapper.Map<IEnumerable<ProductDestination>>(productSources);
// productDestinations will contain two ProductDestination objects
```

### Mapping from Different Source Types to the Same Destination Type

You can map multiple source types to a single destination type.

```csharp
public class UserProfileSource
{
    public string UserRazaoSocial { get; set; }
    public string EmailAddress { get; set; }
}

public class EmployeeSource
{
    public string EmployeeRazaoSocial { get; set; }
    public string WorkEmail { get; set; }
}

public class ContactDestination
{
    public string RazaoSocial { get; set; }
    public string Email { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<UserProfileSource, ContactDestination>()
      .ForMember("RazaoSocial", "UserRazaoSocial")
      .ForMember("Email", "EmailAddress");

config.CreateMap<EmployeeSource, ContactDestination>()
      .ForMember("RazaoSocial", "EmployeeRazaoSocial")
      .ForMember("Email", "WorkEmail");

var mapper = new JMSMapper(config);

// Usage
var userProfile = new UserProfileSource { UserRazaoSocial = "Alice", EmailAddress = "alice@example.com" };
var contactFromProfile = mapper.Map<ContactDestination>(userProfile);

var employee = new EmployeeSource { EmployeeRazaoSocial = "Bob", WorkEmail = "bob@company.com" };
var contactFromEmployee = mapper.Map<ContactDestination>(employee);
```

## Relational Mapping

JMSAutoMapper can also facilitate mapping between objects that represent relational data, such as mapping a database entity to a DTO (Data Transfer Object) and vice-versa, including handling relationships.

```csharp
// Example: Mapping a simple relational entity to a DTO
public class UserEntity
{
    public int Id { get; set; }
    public string FirstRazaoSocial { get; set; }
    public string LastRazaoSocial { get; set; }
    public AddressEntity Address { get; set; } // One-to-one relationship
}

public class AddressEntity
{
    public int Id { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public string FullRazaoSocial { get; set; }
    public string UserStreet { get; set; }
    public string UserCity { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<UserEntity, UserDto>()
      .ForMember("UserId", "Id")
      .ForMember("FullRazaoSocial", src => $"{src.FirstRazaoSocial} {src.LastRazaoSocial}")
      .ForMember("UserStreet", src => src.Address.Street) // Mapping nested property
      .ForMember("UserCity", src => src.Address.City);    // Mapping nested property

config.CreateMap<AddressEntity, UserDto>(); // You might also map AddressEntity directly if needed

var mapper = new JMSMapper(config);

// Usage
var userEntity = new UserEntity
{
    Id = 1,
    FirstRazaoSocial = "Jane",
    LastRazaoSocial = "Doe",
    Address = new AddressEntity { Id = 10, Street = "123 Main St", City = "Anytown" }
};

var userDto = mapper.Map<UserDto>(userEntity);
// userDto.UserId will be 1
// userDto.FullRazaoSocial will be "Jane Doe"
// userDto.UserStreet will be "123 Main St"
// userDto.UserCity will be "Anytown"
```

## Detailed API Reference

### `JMSMapper` Class

The `JMSMapper` class is the core of the mapping engine. It's responsible for performing the actual object-to-object mapping based on the configured mappings.

*   **Constructors:**
    *   `JMSMapper()`: Initializes a new instance of the `JMSMapper` with a default, empty configuration.
    *   `JMSMapper(MapperConfiguration configuration)`: Initializes a new instance of the `JMSMapper` with the specified `MapperConfiguration`.

*   **Methods:**
    *   `<TDestination> Map<TDestination>(object source)`: Maps a source object to a new instance of the specified destination type.
    *   `<TSource, TDestination> Map(TSource source, TDestination destination)`: Maps a source object to an existing destination object. This is useful for updating existing objects.
    *   `Task<TDestination> MapAsync<TDestination>(object source, CancellationToken token)`: Asynchronously maps a source object using parallel execution for complex nested properties and async resolvers.
    *   `IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)`: Projects an IQueryable to the destination type, translating the configuration into a LINQ expression tree.
    *   `void AssertConfigurationIsValid()`: Validates that all destination properties are covered by the current configuration.
    *   `DiagnosticInfo GetDiagnostics()`: Provides runtime metrics including cache hit rates and memory usage.

### `MapperConfiguration` Class

The `MapperConfiguration` class is used to define and manage the mapping configurations for your application.

*   **Constructors:**
    *   `MapperConfiguration()`: Initializes a new instance of the `MapperConfiguration`.
    *   `MapperConfiguration(Action<MapperConfiguration> configure)`: Initializes a new instance and immediately executes the provided configuration action.

*   **Methods:**
    *   `<TSource, TDestination> IMappingExpression<TSource, TDestination> CreateMap()`: Creates a new mapping configuration between the specified source and destination types. This method returns an `IMappingExpression` object, which allows for fluent configuration of the mapping.
    *   `void AddProfile<TProfile>()`: Registers a mapping profile.
    *   `void AddProfilesFromAssembly(Assembly assembly)`: Scans an assembly for `Profile` implementations.
    *   `IMapper CreateMapper()`: Instantiates an `IMapper` instance based on the configuration.
    *   `NullValueMappingPolicy NullValueMappingStrategy`: Configures the global policy for null values in value types (Throw, Default, Ignore).
    *   `Func<string, string> NamingConvention`: Global convention for automatic property RazaoSocial matching.
    *   `bool ThrowOnConversionError`: Toggles exception throwing on type conversion failures.
    *   `int MaxDepth`: Sets the global recursion limit for object mapping.

### `IMappingExpression<TSource, TDestination>` Interface

The `IMappingExpression` interface provides a fluent API for configuring individual mappings between a source and a destination type.

*   **Methods:**
    *   `IMappingExpression<TSource, TDestination> ForMember(string destinationMemberRazaoSocial, string sourceMemberRazaoSocial)`: Configures a custom mapping for a specific member (property) in the destination type, specifying the corresponding member RazaoSocial in the source type.
    *   `IMappingExpression<TSource, TDestination> ForMember<TSourceMember>(string destinationMemberRazaoSocial, Expression<Func<TSource, TSourceMember>> valueResolver)`: Configures a custom mapping for a specific member in the destination type using a custom value resolver (a lambda expression) to determine the value from the source object.
    *   `IMappingExpression<TSource, TDestination> ForMember<TSourceMember>(string destinationMemberRazaoSocial, string sourceMemberRazaoSocial, Expression<Func<TSource, bool>> condition)`: Configures a conditional mapping for a specific member. The mapping will only occur if the provided condition (a lambda expression) evaluates to `true`.
    *   `IMappingExpression<TSource, TDestination> ReverseMap()`: Configures a reverse mapping, allowing you to map from the destination type back to the source type with the same configuration.
    *   `IMappingExpression<TSource, TDestination> Ignore(string destinationMemberRazaoSocial)`: Ignores a destination member during mapping.

## Contributing

Contributions are welcome! Please see the [Guia de Contribuição](CONTRIBUTING.md) for more details.

## License

JMSAutoMapper is licensed under the MIT License.
