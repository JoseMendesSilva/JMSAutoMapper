$basePath = Get-Location
$solutionName = "JMSAutoMapper"

Write-Host ">> Iniciando criacao da Solucao e Projetos JMSAutoMapper em: $basePath" -ForegroundColor Cyan

# 1. Criar a Solução
if (!(Test-Path "$basePath\$solutionName.sln")) {
    dotnet new sln -n $solutionName
    Write-Host "Solution: $solutionName.sln [OK]" -ForegroundColor Green
}

# 2. Criar os Projetos (.csproj) conforme a árvore v2
Write-Host "[MSG] Criando projetos .NET..." -ForegroundColor Cyan

dotnet new classlib -n JMSAutoMapper -o src/JMSAutoMapper --force
dotnet new xunit -n JMSAutoMapper.Tests -o tests/JMSAutoMapper.Tests --force
dotnet new console -n JMSAutoMapper.Benchmarks -o benchmarks/JMSAutoMapper.Benchmarks --force
dotnet new console -n JMSAutoMapper.ConsoleSample -o samples/JMSAutoMapper.ConsoleSample --force
dotnet new webapi -n JMSAutoMapper.WebApiSample -o samples/JMSAutoMapper.WebApiSample --force
dotnet new winforms -n JMSAutoMapper.WinFormsSample -o samples/JMSAutoMapper.WinFormsSample --force

# 3. Adicionar Projetos à Solução
Write-Host "[MSG] Adicionando projetos a solucao..." -ForegroundColor Cyan
$projects = Get-ChildItem -Path $basePath -Filter "*.csproj" -Recurse
foreach ($proj in $projects) {
    dotnet sln "$basePath\$solutionName.sln" add $proj.FullName
    Write-Host "Added: $($proj.Name) [OK]" -ForegroundColor Gray
}

# 4. Configurar Referências entre Projetos
Write-Host "[MSG] Configurando referencias..." -ForegroundColor Cyan
$coreProj = "src/JMSAutoMapper/JMSAutoMapper.csproj"
dotnet add "tests/JMSAutoMapper.Tests/JMSAutoMapper.Tests.csproj" reference $coreProj
dotnet add "benchmarks/JMSAutoMapper.Benchmarks/JMSAutoMapper.Benchmarks.csproj" reference $coreProj
dotnet add "samples/JMSAutoMapper.ConsoleSample/JMSAutoMapper.ConsoleSample.csproj" reference $coreProj
dotnet add "samples/JMSAutoMapper.WebApiSample/JMSAutoMapper.WebApiSample.csproj" reference $coreProj
dotnet add "samples/JMSAutoMapper.WinFormsSample/JMSAutoMapper.WinFormsSample.csproj" reference $coreProj

# 5. Definição das pastas e arquivos internos

# Definição das pastas
$folders = @(
    "src/JMSAutoMapper/Abstractions",
    "src/JMSAutoMapper/Configuration",
    "src/JMSAutoMapper/Core",
    "src/JMSAutoMapper/Conversion",
    "src/JMSAutoMapper/Collections",
    "src/JMSAutoMapper/Reflection",
    "src/JMSAutoMapper/Expressions",
    "src/JMSAutoMapper/Projection",
    "src/JMSAutoMapper/Cache",
    "src/JMSAutoMapper/Diagnostics",
    "src/JMSAutoMapper/Validation",
    "src/JMSAutoMapper/DependencyInjection",
    "src/JMSAutoMapper/Internals",
    "tests/JMSAutoMapper.Tests",
    "benchmarks/JMSAutoMapper.Benchmarks",
    "samples/JMSAutoMapper.ConsoleSample",
    "samples/JMSAutoMapper.WebApiSample",
    "samples/JMSAutoMapper.WinFormsSample"
)

# Definição dos arquivos (esqueletos vazios)
$files = @(
    "src/JMSAutoMapper/Abstractions/IMapper.cs",
    "src/JMSAutoMapper/Abstractions/IValueResolver.cs",
    "src/JMSAutoMapper/Abstractions/IAsyncValueResolver.cs",
    "src/JMSAutoMapper/Abstractions/ITypeConverter.cs",
    "src/JMSAutoMapper/Abstractions/IMappingExpression.cs",
    "src/JMSAutoMapper/Abstractions/IMemberConfigurationExpression.cs",
    "src/JMSAutoMapper/Abstractions/IDistributedMapperCache.cs",
    "src/JMSAutoMapper/Configuration/MapperConfiguration.cs",
    "src/JMSAutoMapper/Configuration/MapperConfigurationSeal.cs",
    "src/JMSAutoMapper/Configuration/Profile.cs",
    "src/JMSAutoMapper/Configuration/ProfileConfiguration.cs",
    "src/JMSAutoMapper/Configuration/MappingExpression.cs",
    "src/JMSAutoMapper/Configuration/MemberConfigurationExpression.cs",
    "src/JMSAutoMapper/Configuration/MemberListType.cs",
    "src/JMSAutoMapper/Configuration/NullValueMappingPolicy.cs",
    "src/JMSAutoMapper/Configuration/JMSMapperOptions.cs",
    "src/JMSAutoMapper/Core/JMSMapper.cs",
    "src/JMSAutoMapper/Core/MapperBase.cs",
    "src/JMSAutoMapper/Core/ResolutionContext.cs",
    "src/JMSAutoMapper/Core/MappingContext.cs",
    "src/JMSAutoMapper/Core/MappingPlan.cs",
    "src/JMSAutoMapper/Core/MappingPlanBuilder.cs",
    "src/JMSAutoMapper/Core/MappingException.cs",
    "src/JMSAutoMapper/Conversion/TypeConversionEngine.cs",
    "src/JMSAutoMapper/Conversion/NumericConversionTable.cs",
    "src/JMSAutoMapper/Conversion/EnumConverter.cs",
    "src/JMSAutoMapper/Conversion/NullableConverter.cs",
    "src/JMSAutoMapper/Conversion/DateTimeConverter.cs",
    "src/JMSAutoMapper/Conversion/StringConverter.cs",
    "src/JMSAutoMapper/Collections/CollectionMapper.cs",
    "src/JMSAutoMapper/Collections/ArrayMapper.cs",
    "src/JMSAutoMapper/Collections/ListMapper.cs",
    "src/JMSAutoMapper/Collections/DictionaryMapper.cs",
    "src/JMSAutoMapper/Collections/ImmutableCollectionMapper.cs",
    "src/JMSAutoMapper/Collections/CollectionTypeHelper.cs",
    "src/JMSAutoMapper/Reflection/TypeMetadata.cs",
    "src/JMSAutoMapper/Reflection/PropertyMetadata.cs",
    "src/JMSAutoMapper/Reflection/PropertyAccessorCache.cs",
    "src/JMSAutoMapper/Reflection/ConstructorFactory.cs",
    "src/JMSAutoMapper/Reflection/ObjectFactory.cs",
    "src/JMSAutoMapper/Reflection/ReferenceEqualityComparer.cs",
    "src/JMSAutoMapper/Expressions/ExpressionPool.cs",
    "src/JMSAutoMapper/Expressions/ExpressionCompiler.cs",
    "src/JMSAutoMapper/Expressions/AssignmentExpressionBuilder.cs",
    "src/JMSAutoMapper/Expressions/NullGuardExpressionBuilder.cs",
    "src/JMSAutoMapper/Expressions/FlatteningExpressionBuilder.cs",
    "src/JMSAutoMapper/Projection/QueryableMapper.cs",
    "src/JMSAutoMapper/Projection/ProjectionBuilder.cs",
    "src/JMSAutoMapper/Projection/ProjectionExpressionVisitor.cs",
    "src/JMSAutoMapper/Projection/EfSafeExpressionBuilder.cs",
    "src/JMSAutoMapper/Cache/CacheableAttribute.cs",
    "src/JMSAutoMapper/Cache/CacheKeyGenerator.cs",
    "src/JMSAutoMapper/Cache/InMemoryDistributedCache.cs",
    "src/JMSAutoMapper/Cache/MapperCacheService.cs",
    "src/JMSAutoMapper/Cache/CacheStatistics.cs",
    "src/JMSAutoMapper/Diagnostics/DiagnosticInfo.cs",
    "src/JMSAutoMapper/Diagnostics/DiagnosticCollector.cs",
    "src/JMSAutoMapper/Diagnostics/MappingDiagnosticEvent.cs",
    "src/JMSAutoMapper/Diagnostics/MappingPerformanceTracker.cs",
    "src/JMSAutoMapper/Validation/ConfigurationValidator.cs",
    "src/JMSAutoMapper/Validation/MappingValidationResult.cs",
    "src/JMSAutoMapper/Validation/MissingMemberValidator.cs",
    "src/JMSAutoMapper/Validation/ConstructorValidator.cs",
    "src/JMSAutoMapper/DependencyInjection/MapperExtensions.cs",
    "src/JMSAutoMapper/DependencyInjection/ServiceCollectionExtensions.cs",
    "src/JMSAutoMapper/Internals/TypeExtensions.cs",
    "src/JMSAutoMapper/Internals/ExpressionExtensions.cs",
    "src/JMSAutoMapper/Internals/DictionaryExtensions.cs",
    "src/JMSAutoMapper/Internals/Guard.cs",
    "tests/JMSAutoMapper.Tests/SimpleMapTests.cs",
    "tests/JMSAutoMapper.Tests/NullableTests.cs",
    "tests/JMSAutoMapper.Tests/CollectionMapTests.cs",
    "tests/JMSAutoMapper.Tests/NestedObjectTests.cs",
    "tests/JMSAutoMapper.Tests/CircularReferenceTests.cs",
    "tests/JMSAutoMapper.Tests/ReverseMapTests.cs",
    "tests/JMSAutoMapper.Tests/ProjectToTests.cs",
    "tests/JMSAutoMapper.Tests/AsyncResolverTests.cs",
    "tests/JMSAutoMapper.Tests/CacheTests.cs",
    "tests/JMSAutoMapper.Tests/ConfigurationValidationTests.cs",
    "tests/JMSAutoMapper.Tests/DependencyInjectionTests.cs",
    "benchmarks/JMSAutoMapper.Benchmarks/SimpleMapBenchmark.cs",
    "benchmarks/JMSAutoMapper.Benchmarks/CollectionMapBenchmark.cs",
    "benchmarks/JMSAutoMapper.Benchmarks/NestedMapBenchmark.cs",
    "benchmarks/JMSAutoMapper.Benchmarks/ProjectToBenchmark.cs"
)

# Criação das Pastas
foreach ($folder in $folders) {
    $path = Join-Path $basePath $folder
    if (!(Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        Write-Host "Folder: $folder [OK]" -ForegroundColor Green
    }
}

# Criação dos Arquivos
foreach ($file in $files) {
    $path = Join-Path $basePath $file
    if (!(Test-Path $path)) {
        New-Item -ItemType File -Path $path -Force | Out-Null
        Write-Host "File:   $file [OK]" -ForegroundColor Gray
    }
}

# Limpeza de arquivos padrão desnecessários (Class1.cs, UnitTest1.cs)
$cleanFiles = @(
    "src/JMSAutoMapper/Class1.cs",
    "tests/JMSAutoMapper.Tests/UnitTest1.cs"
)
foreach ($cFile in $cleanFiles) {
    $path = Join-Path $basePath $cFile
    if (Test-Path $path) { Remove-Item $path -Force }
}

Write-Host "`n*** Estrutura skeleton criada com sucesso! ***" -ForegroundColor Cyan