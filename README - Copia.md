# JMSAutoMapper

Biblioteca .NET para mapeamento objeto-objeto simples, flexível e extensível.

## Objetivos

- Configuração simples
- API previsível
- Alta extensibilidade
- Compatibilidade com .NET moderno
- Compatibilidade com .NET Framework
- Open Source Apache-2.0

## Instalação

```bash
dotnet add package JMSAutoMapper
```

## Exemplo Básico

```csharp
var config = new MapperConfiguration();

config.CreateMap<Customer, CustomerDto>();

var mapper = new JMSMapper(config);

var dto = mapper.Map<CustomerDto>(customer);
```

## Recursos

- Simple Mapping
- Nested Objects
- Collections
- ReverseMap
- IncludeBase
- Value Resolvers
- Async Resolvers
- Custom Type Converters
- Projection
- Dependency Injection
- Diagnostics
- Validation

## Benchmarks

Consulte:

docs/PERFORMANCE.md

## Documentação

- CONFIGURATION.md
- PROJECTION.md
- CACHE.md
- MIGRATION.md

## Licença

Apache License 2.0

## Autor

José Mendes da Silva

JMS Platform Services