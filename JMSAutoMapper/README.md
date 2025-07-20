# JMSAutoMapper

JMSAutoMapper is a high-performance, flexible, and easy-to-use object-to-object mapper for .NET. It simplifies the process of transferring data between different object models, reducing the need for boilerplate code and improving maintainability.

## Key Features

*   **High Performance:** Uses compiled expression trees to achieve fast and efficient mapping, avoiding the overhead of reflection-based approaches.
*   **Flexible Configuration:** Provides a fluent API for configuring custom mappings, including property name mapping, custom value resolvers, and conditional mapping.
*   **Bidirectional Mapping:** Supports automatic reverse mapping, allowing you to map objects in both directions with a single configuration.
*   **Circular Reference Handling:** Automatically handles circular references to prevent stack overflow exceptions.
*   **Nested Collection Mapping:** Correctly maps nested collections of any depth.
*   **Value Type and Struct Support:** Can map value types (structs) and classes without parameterless constructors.
*   **Dependency Injection Integration:** Provides an extension method for easy integration with `Microsoft.Extensions.DependencyInjection`.

## Technologies Used

*   **.NET 8:** The project is built on the latest version of the .NET platform.
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
    var source = new Source { Name = "Test", Age = 30 };
    var destination = mapper.Map<Destination>(source);
    ```

### Configuration

JMSAutoMapper provides a flexible configuration API to handle complex mapping scenarios.

1.  **Create a Mapper Configuration:**

    ```csharp
    var config = new MapperConfiguration();
    ```

2.  **Define Mappings:**

    ```csharp
    config.CreateMap<Source, Destination>();
    ```

3.  **Create a Mapper with the Configuration:**

    ```csharp
    var mapper = new JMSMapper(config);
    ```

#### Custom Property Mapping

If property names don't match, you can configure a custom mapping:

```csharp
config.CreateMap<Source, Destination>()
    .ForMember("DestName", "Name");
```

#### Custom Value Resolvers

You can use a lambda expression to define a custom value resolver:

```csharp
config.CreateMap<Source, Destination>()
    .ForMember("FullName", src => $"{src.FirstName} {src.LastName}");
```

#### Conditional Mapping

Map a property only if a condition is met:

```csharp
config.CreateMap<Source, Destination>()
    .ForMember("Value", "Value", src => src.Value > 0);
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
    services.AddJMSMapper(config =>
    {
        config.CreateMap<Source, Destination>();
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
    public string Name { get; set; }
}

public class OrderDestination
{
    public int OrderId { get; set; }
    public CustomerDestination Customer { get; set; }
}

public class CustomerDestination
{
    public int CustomerId { get; set; }
    public string FullName { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<CustomerSource, CustomerDestination>()
      .ForMember("FullName", src => src.Name); // Custom mapping for nested object
config.CreateMap<OrderSource, OrderDestination>();

var mapper = new JMSMapper(config);

// Usage
var orderSource = new OrderSource
{
    OrderId = 1,
    Customer = new CustomerSource { CustomerId = 101, Name = "John Doe" }
};

var orderDestination = mapper.Map<OrderDestination>(orderSource);
// orderDestination.Customer.FullName will be "John Doe"
```

### Mapping Collections

Mapping collections is straightforward. JMSAutoMapper handles `IEnumerable<T>`, `List<T>`, and arrays.

```csharp
public class ProductSource
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class ProductDestination
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<ProductSource, ProductDestination>()
      .ForMember("ProductId", "Id")
      .ForMember("ProductName", "Name");

var mapper = new JMSMapper(config);

// Usage
var productSources = new List<ProductSource>
{
    new ProductSource { Id = 1, Name = "Laptop" },
    new ProductSource { Id = 2, Name = "Mouse" }
};

var productDestinations = mapper.Map<IEnumerable<ProductDestination>>(productSources);
// productDestinations will contain two ProductDestination objects
```

### Mapping from Different Source Types to the Same Destination Type

You can map multiple source types to a single destination type.

```csharp
public class UserProfileSource
{
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
}

public class EmployeeSource
{
    public string EmployeeName { get; set; }
    public string WorkEmail { get; set; }
}

public class ContactDestination
{
    public string Name { get; set; }
    public string Email { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<UserProfileSource, ContactDestination>()
      .ForMember("Name", "UserName")
      .ForMember("Email", "EmailAddress");

config.CreateMap<EmployeeSource, ContactDestination>()
      .ForMember("Name", "EmployeeName")
      .ForMember("Email", "WorkEmail");

var mapper = new JMSMapper(config);

// Usage
var userProfile = new UserProfileSource { UserName = "Alice", EmailAddress = "alice@example.com" };
var contactFromProfile = mapper.Map<ContactDestination>(userProfile);

var employee = new EmployeeSource { EmployeeName = "Bob", WorkEmail = "bob@company.com" };
var contactFromEmployee = mapper.Map<ContactDestination>(employee);
```

## Relational Mapping

JMSAutoMapper can also facilitate mapping between objects that represent relational data, such as mapping a database entity to a DTO (Data Transfer Object) and vice-versa, including handling relationships.

```csharp
// Example: Mapping a simple relational entity to a DTO
public class UserEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
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
    public string FullName { get; set; }
    public string UserStreet { get; set; }
    public string UserCity { get; set; }
}

// Configuration
var config = new MapperConfiguration();
config.CreateMap<UserEntity, UserDto>()
      .ForMember("UserId", "Id")
      .ForMember("FullName", src => $"{src.FirstName} {src.LastName}")
      .ForMember("UserStreet", src => src.Address.Street) // Mapping nested property
      .ForMember("UserCity", src => src.Address.City);    // Mapping nested property

config.CreateMap<AddressEntity, UserDto>(); // You might also map AddressEntity directly if needed

var mapper = new JMSMapper(config);

// Usage
var userEntity = new UserEntity
{
    Id = 1,
    FirstName = "Jane",
    LastName = "Doe",
    Address = new AddressEntity { Id = 10, Street = "123 Main St", City = "Anytown" }
};

var userDto = mapper.Map<UserDto>(userEntity);
// userDto.UserId will be 1
// userDto.FullName will be "Jane Doe"
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

### `MapperConfiguration` Class

The `MapperConfiguration` class is used to define and manage the mapping configurations for your application.

*   **Constructors:**
    *   `MapperConfiguration()`: Initializes a new instance of the `MapperConfiguration`.

*   **Methods:**
    *   `<TSource, TDestination> IMappingExpression<TSource, TDestination> CreateMap()`: Creates a new mapping configuration between the specified source and destination types. This method returns an `IMappingExpression` object, which allows for fluent configuration of the mapping.

### `IMappingExpression<TSource, TDestination>` Interface

The `IMappingExpression` interface provides a fluent API for configuring individual mappings between a source and a destination type.

*   **Methods:**
    *   `IMappingExpression<TSource, TDestination> ForMember(string destinationMemberName, string sourceMemberName)`: Configures a custom mapping for a specific member (property) in the destination type, specifying the corresponding member name in the source type.
    *   `IMappingExpression<TSource, TDestination> ForMember<TSourceMember>(string destinationMemberName, Expression<Func<TSource, TSourceMember>> valueResolver)`: Configures a custom mapping for a specific member in the destination type using a custom value resolver (a lambda expression) to determine the value from the source object.
    *   `IMappingExpression<TSource, TDestination> ForMember<TSourceMember>(string destinationMemberName, string sourceMemberName, Expression<Func<TSource, bool>> condition)`: Configures a conditional mapping for a specific member. The mapping will only occur if the provided condition (a lambda expression) evaluates to `true`.
    *   `IMappingExpression<TSource, TDestination> ReverseMap()`: Configures a reverse mapping, allowing you to map from the destination type back to the source type with the same configuration.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue on GitHub.

## License

JMSAutoMapper is licensed under the MIT License.