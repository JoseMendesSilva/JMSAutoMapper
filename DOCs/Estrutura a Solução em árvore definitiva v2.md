JMSAutoMapper.sln
│
├── src
│   └── JMSAutoMapper
│       │
│       ├── JMSAutoMapper.csproj
│       │
│       ├── Abstractions
│       │   ├── IMapper.cs
│       │   ├── IValueResolver.cs
│       │   ├── IAsyncValueResolver.cs
│       │   ├── ITypeConverter.cs
│       │   ├── IMappingExpression.cs
│       │   ├── IMemberConfigurationExpression.cs
│       │   └── IDistributedMapperCache.cs
│       │
│       ├── Configuration
│       │   ├── MapperConfiguration.cs
│       │   ├── MapperConfigurationSeal.cs
│       │   ├── Profile.cs
│       │   ├── ProfileConfiguration.cs
│       │   ├── MappingExpression.cs
│       │   ├── MemberConfigurationExpression.cs
│       │   ├── MemberListType.cs
│       │   ├── NullValueMappingPolicy.cs
│       │   └── JMSMapperOptions.cs
│       │
│       ├── Core
│       │   ├── JMSMapper.cs
│       │   ├── MapperBase.cs
│       │   ├── ResolutionContext.cs
│       │   ├── MappingContext.cs
│       │   ├── MappingPlan.cs
│       │   ├── MappingPlanBuilder.cs
│       │   └── MappingException.cs
│       │
│       ├── Conversion
│       │   ├── TypeConversionEngine.cs
│       │   ├── NumericConversionTable.cs
│       │   ├── EnumConverter.cs
│       │   ├── NullableConverter.cs
│       │   ├── DateTimeConverter.cs
│       │   └── StringConverter.cs
│       │
│       ├── Collections
│       │   ├── CollectionMapper.cs
│       │   ├── ArrayMapper.cs
│       │   ├── ListMapper.cs
│       │   ├── DictionaryMapper.cs
│       │   ├── ImmutableCollectionMapper.cs
│       │   └── CollectionTypeHelper.cs
│       │
│       ├── Reflection
│       │   ├── TypeMetadata.cs
│       │   ├── PropertyMetadata.cs
│       │   ├── PropertyAccessorCache.cs
│       │   ├── ConstructorFactory.cs
│       │   ├── ObjectFactory.cs
│       │   └── ReferenceEqualityComparer.cs
│       │
│       ├── Expressions
│       │   ├── ExpressionPool.cs
│       │   ├── ExpressionCompiler.cs
│       │   ├── AssignmentExpressionBuilder.cs
│       │   ├── NullGuardExpressionBuilder.cs
│       │   └── FlatteningExpressionBuilder.cs
│       │
│       ├── Projection
│       │   ├── QueryableMapper.cs
│       │   ├── ProjectionBuilder.cs
│       │   ├── ProjectionExpressionVisitor.cs
│       │   └── EfSafeExpressionBuilder.cs
│       │
│       ├── Cache
│       │   ├── CacheableAttribute.cs
│       │   ├── CacheKeyGenerator.cs
│       │   ├── InMemoryDistributedCache.cs
│       │   ├── MapperCacheService.cs
│       │   └── CacheStatistics.cs
│       │
│       ├── Diagnostics
│       │   ├── DiagnosticInfo.cs
│       │   ├── DiagnosticCollector.cs
│       │   ├── MappingDiagnosticEvent.cs
│       │   └── MappingPerformanceTracker.cs
│       │
│       ├── Validation
│       │   ├── ConfigurationValidator.cs
│       │   ├── MappingValidationResult.cs
│       │   ├── MissingMemberValidator.cs
│       │   └── ConstructorValidator.cs
│       │
│       ├── DependencyInjection
│       │   ├── MapperExtensions.cs
│       │   └── ServiceCollectionExtensions.cs
│       │
│       └── Internals
│           ├── TypeExtensions.cs
│           ├── ExpressionExtensions.cs
│           ├── DictionaryExtensions.cs
│           └── Guard.cs
│
├── tests
│   └── JMSAutoMapper.Tests
│       ├── SimpleMapTests.cs
│       ├── NullableTests.cs
│       ├── CollectionMapTests.cs
│       ├── NestedObjectTests.cs
│       ├── CircularReferenceTests.cs
│       ├── ReverseMapTests.cs
│       ├── ProjectToTests.cs
│       ├── AsyncResolverTests.cs
│       ├── CacheTests.cs
│       ├── ConfigurationValidationTests.cs
│       └── DependencyInjectionTests.cs
│
├── benchmarks
│   └── JMSAutoMapper.Benchmarks
│       ├── SimpleMapBenchmark.cs
│       ├── CollectionMapBenchmark.cs
│       ├── NestedMapBenchmark.cs
│       └── ProjectToBenchmark.cs
│
└── samples
    ├── JMSAutoMapper.ConsoleSample
    ├── JMSAutoMapper.WebApiSample
    └── JMSAutoMapper.WinFormsSample
	