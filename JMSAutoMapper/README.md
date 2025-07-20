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

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue on GitHub.

## License

JMSAutoMapper is licensed under the MIT License.