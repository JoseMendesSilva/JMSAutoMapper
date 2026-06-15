# JMSAutoMapper — Checklist de Implementação V2 e Otimização

## Objetivo

Este checklist orienta a implementação dos arquivos restantes da nova estrutura do **JMSAutoMapper**, mantendo a biblioteca estável, testável, versionada e preparada para publicação futura no NuGet.

Estado atual confirmado:

```text
✅ Arquitetura V2 criada
✅ Código antigo distribuído na nova estrutura
✅ Build com zero erros
✅ Build com zero warnings
✅ 120/120 testes passando
✅ BenchmarkDotNet configurado
✅ Licença Apache-2.0 definida
✅ Versionamento incremental iniciado
```

A partir deste ponto, o foco NÃO é adicionar novas funcionalidades.

O foco é:

```text
1. Preservar comportamento atual
2. Melhorar performance
3. Reduzir alocação de memória
4. Melhorar previsibilidade
5. Preparar documentação e NuGet
```

---

# Regra principal

Antes de qualquer alteração:

```bash
dotnet test
dotnet build -c Release
dotnet run -c Release --project benchmarks/JMSAutoMapper.Benchmark
```

A cada etapa:

```text
✅ Todos os testes devem continuar passando
✅ Nenhuma API pública deve ser removida
✅ Nenhum comportamento existente deve mudar sem teste
✅ Nenhuma otimização deve quebrar compatibilidade
```

---

# Política de versionamento

Usar versionamento incremental explícito, sem wildcard.

Não usar:

```xml
<AssemblyVersion>1.0.17.*</AssemblyVersion>
```

Usar:

```xml
<Version>0.0.1.0</Version>
<AssemblyVersion>0.1.0.0</AssemblyVersion>
<FileVersion>0.1.0.0</FileVersion>
<InformationalVersion>0.1.0.0</InformationalVersion>
```

## Marcos sugeridos

```text
0.0.1.0  Estrutura V2 estabilizada com 120/120 testes passando
0.1.0.0  Correções finais de comportamento e regressão
0.2.0.0  PropertyAccessorCache implementado
0.3.0.0  CompiledGetter e CompiledSetter implementados
0.4.0.0  MappingPlanBuilder implementado
0.5.0.0  CompiledMappingPlan implementado
0.6.0.0  CompiledCollectionMapper implementado
0.7.0.0  ObjectPoolProvider implementado
0.8.0.0  Benchmarks otimizados e documentados
0.9.0.0  Release candidate interna
1.0.0.0  Primeira versão pública estável
```

---

# Baseline atual de benchmark

Resultados atuais conhecidos:

```text
JmsMapper_SimpleMap       ~325 ns   | 464 B
AutoMapper_SimpleMap       ~77 ns   | 32 B

JmsMapper_CollectionMap    ~35 us   | 47.767 B
AutoMapper_CollectionMap   ~1.68 us | 5.392 B

JmsMapper_ComplexMap       ~672 ns  | 816 B
AutoMapper_ComplexMap       ~88 ns  | 64 B
```

## Metas pós-otimização

```text
SimpleMap      <= 150 ns
ComplexMap     <= 300 ns
CollectionMap  <= 10 us
Allocated       reduzir pelo menos 60%
```

---

# Sprint 0 — Congelamento da base atual

## Objetivo

Criar um ponto seguro antes das otimizações.

## Tarefas

- [ ] Confirmar que a branch atual está limpa.
- [ ] Rodar `dotnet test`.
- [ ] Rodar `dotnet build -c Release`.
- [ ] Rodar benchmark completo.
- [ ] Salvar relatório BenchmarkDotNet em `docs/benchmarks/baseline-v0.0.1.0.md`.
- [ ] Criar `CHANGELOG.md`.
- [ ] Criar tag Git:

```bash
git tag v0.0.1.0
git push origin v0.0.1.0
```

## Critério de aceite

```text
120/120 testes passando
Benchmark baseline salvo
Tag criada
```

---

# Sprint 1 — PropertyAccessorCache

## Arquivos principais

```text
src/JMSAutoMapper/Reflection/PropertyAccessorCache.cs
src/JMSAutoMapper/Reflection/TypeMetadata.cs
src/JMSAutoMapper/Reflection/PropertyMetadata.cs
```

## Objetivo

Evitar chamadas repetidas a:

```csharp
type.GetProperties()
property.GetValue()
property.SetValue()
```

## Implementação esperada

Criar cache por tipo:

```csharp
ConcurrentDictionary<Type, TypeMetadata>
```

`TypeMetadata` deve conter:

```text
Type Type
PropertyMetadata[] PublicReadableProperties
PropertyMetadata[] PublicWritableProperties
Dictionary<string, PropertyMetadata> PropertiesByName
```

`PropertyMetadata` deve conter:

```text
string Name
Type PropertyType
PropertyInfo PropertyInfo
bool CanRead
bool CanWrite
Func<object, object?>? Getter
Action<object, object?>? Setter
```

## Cuidados

- [ ] Não mudar regras de mapeamento existentes.
- [ ] Não ignorar propriedades que antes eram mapeadas.
- [ ] Respeitar propriedades ignoradas em `MapperConfiguration`.
- [ ] Manter suporte a propriedades herdadas.
- [ ] Manter suporte a naming convention atual.

## Testes obrigatórios

- [ ] SimpleMapTests
- [ ] NestedObjectTests
- [ ] IncludeBaseTests
- [ ] NullableTests
- [ ] ConfigurationValidationTests

## Critério de aceite

```text
120/120 testes passando
Nenhum comportamento alterado
Benchmark SimpleMap melhor ou igual ao baseline
```

---

# Sprint 2 — CompiledGetter e CompiledSetter

## Arquivos principais

```text
src/JMSAutoMapper/Reflection/CompiledGetter.cs
src/JMSAutoMapper/Reflection/CompiledSetter.cs
src/JMSAutoMapper/Expressions/ExpressionCompiler.cs
```

## Objetivo

Substituir reflection direta por delegates compilados.

Em vez de:

```csharp
property.GetValue(source);
property.SetValue(destination, value);
```

usar:

```csharp
Func<object, object?> getter;
Action<object, object?> setter;
```

## Estratégia

Getter com expression tree:

```text
object source
→ cast para tipo real
→ acessar propriedade
→ converter para object
```

Setter com expression tree:

```text
object destination, object? value
→ cast destination
→ converter value para tipo da propriedade
→ atribuir propriedade
```

## Cuidados

- [ ] Propriedades sem setter devem ser ignoradas como antes.
- [ ] Nullable deve continuar funcionando.
- [ ] Conversões devem continuar passando pelo mecanismo atual.
- [ ] Não lançar exceções novas para cenários já aceitos.

## Testes obrigatórios

- [ ] NullableTests
- [ ] NumericConversionTests
- [ ] StringConversionTests
- [ ] DateTimeConversionTests
- [ ] EnumConversionTests
- [ ] GuidConversionTests

## Critério de aceite

```text
120/120 testes passando
SimpleMap deve reduzir alocação ou tempo
ComplexMap não pode piorar
```

---

# Sprint 3 — MapperPlanCache

## Arquivos principais

```text
src/JMSAutoMapper/Cache/MapperPlanCache.cs
src/JMSAutoMapper/Core/MappingPlan.cs
src/JMSAutoMapper/Core/PropertyMap.cs
```

## Objetivo

Criar e reutilizar um plano de mapeamento por par:

```text
SourceType -> DestinationType
```

## MappingPlan deve conter

```text
Type SourceType
Type DestinationType
PropertyMap[] Properties
Func<object>? DestinationFactory
bool HasCustomMappings
bool HasResolvers
bool HasConditions
bool HasAsyncMembers
```

## PropertyMap deve conter

```text
string SourceName
string DestinationName
Type SourceType
Type DestinationType
Func<object, object?>? Getter
Action<object, object?>? Setter
Func<object, IMapper, object?>? CustomResolver
Func<object, bool>? Condition
bool Ignore
```

## Cuidados

- [ ] O plano deve refletir `ForMember`.
- [ ] O plano deve refletir `Ignore`.
- [ ] O plano deve refletir `ReverseMap`.
- [ ] O plano deve refletir `IncludeBase`.
- [ ] O plano deve refletir condições síncronas.
- [ ] O plano deve preservar resolvers.

## Critério de aceite

```text
120/120 testes passando
MapObject deve conseguir usar MappingPlan sem alterar resultado
```

---

# Sprint 4 — MappingPlanBuilder

## Arquivos principais

```text
src/JMSAutoMapper/Core/MappingPlanBuilder.cs
src/JMSAutoMapper/Core/MappingExecutionContext.cs
src/JMSAutoMapper/Validation/ConfigurationValidator.cs
```

## Objetivo

Centralizar a montagem dos planos de mapeamento.

## Responsabilidades

`MappingPlanBuilder` deve:

- [ ] Ler `MapperConfiguration`.
- [ ] Consultar `PropertyAccessorCache`.
- [ ] Aplicar `PropertyMappings`.
- [ ] Aplicar `CustomMappings`.
- [ ] Aplicar `IgnoredProperties`.
- [ ] Aplicar `ConditionalMappings`.
- [ ] Aplicar `ValueResolvers`.
- [ ] Aplicar `ConstructorSelection`.
- [ ] Aplicar `CustomConstructors`.
- [ ] Gerar `MappingPlan`.

## Cuidados

- [ ] Não duplicar lógica no `JMSMapper`.
- [ ] Não quebrar mapeamento assíncrono.
- [ ] Não misturar `ProjectTo` com `Map` em memória.
- [ ] Não executar reflection em cada mapeamento.

## Critério de aceite

```text
120/120 testes passando
Código do JMSMapper fica mais simples
Benchmark SimpleMap deve melhorar
```

---

# Sprint 5 — CompiledMappingPlan

## Arquivos principais

```text
src/JMSAutoMapper/Core/CompiledMappingPlan.cs
src/JMSAutoMapper/Expressions/AssignmentExpressionBuilder.cs
src/JMSAutoMapper/Expressions/NullGuardExpressionBuilder.cs
src/JMSAutoMapper/Expressions/ConstructorExpressionBuilder.cs
```

## Objetivo

Compilar o plano de mapeamento em delegate executável.

Exemplo conceitual:

```csharp
Func<object, IMapper, MappingExecutionContext, object>
```

## Estratégia

O delegate deve:

- [ ] Criar instância destino.
- [ ] Executar BeforeMap.
- [ ] Copiar propriedades simples.
- [ ] Mapear objetos complexos.
- [ ] Mapear coleções.
- [ ] Aplicar condições.
- [ ] Aplicar resolvers.
- [ ] Executar AfterMap.
- [ ] Retornar destino.

## Cuidados

- [ ] Se o mapeamento tiver resolver complexo não compilável, usar fallback.
- [ ] Se tiver async resolver, usar pipeline async.
- [ ] Se tiver ProjectTo, não reutilizar delegate de runtime.
- [ ] Manter fallback seguro para casos não otimizados.

## Critério de aceite

```text
120/120 testes passando
SimpleMap <= 200 ns como primeira meta
```

---

# Sprint 6 — CompiledCollectionMapper

## Arquivos principais

```text
src/JMSAutoMapper/Collections/CollectionMapper.cs
src/JMSAutoMapper/Collections/CollectionMappingPlan.cs
src/JMSAutoMapper/Collections/CompiledCollectionMapper.cs
src/JMSAutoMapper/Collections/ArrayMapper.cs
src/JMSAutoMapper/Collections/ListMapper.cs
src/JMSAutoMapper/Collections/DictionaryMapper.cs
src/JMSAutoMapper/Collections/ImmutableCollectionMapper.cs
src/JMSAutoMapper/Collections/CollectionTypeHelper.cs
```

## Objetivo

Reduzir o gargalo atual de coleção.

Baseline atual:

```text
JMSMapper_CollectionMap ~35 us
AutoMapper_CollectionMap ~1.68 us
```

Meta:

```text
JMSMapper_CollectionMap <= 10 us
```

## Estratégia

Não executar pipeline completo para cada item quando o plano já existir.

Criar plano:

```text
SourceItemType -> DestinationItemType
```

e reutilizar delegate compilado para cada item.

## Cuidados

- [ ] Preservar `List<T>`.
- [ ] Preservar arrays.
- [ ] Preservar `IEnumerable<T>`.
- [ ] Preservar `ICollection<T>`.
- [ ] Preservar `IReadOnlyList<T>`.
- [ ] Preservar `HashSet<T>`.
- [ ] Preservar `Dictionary<TKey,TValue>`.
- [ ] Preservar coleções imutáveis.
- [ ] Preservar tratamento de null.
- [ ] Preservar conversão de tipos simples.

## Critério de aceite

```text
120/120 testes passando
CollectionMap reduzido para <= 15 us inicialmente
Meta final <= 10 us
```

---

# Sprint 7 — Object Pooling

## Arquivos principais

```text
src/JMSAutoMapper/Performance/ObjectPoolProvider.cs
src/JMSAutoMapper/Performance/DictionaryPool.cs
src/JMSAutoMapper/Performance/HashSetPool.cs
src/JMSAutoMapper/Performance/MappingContextPool.cs
```

## Objetivo

Reduzir alocação de memória.

Baseline atual:

```text
SimpleMap      464 B
ComplexMap     816 B
CollectionMap  47.767 B
```

## Estratégia

Reutilizar estruturas temporárias:

```text
Dictionary<object, object>
HashSet<object>
MappingExecutionContext
```

## Cuidados

- [ ] Sempre limpar objetos antes de devolver ao pool.
- [ ] Nunca devolver contexto ainda em uso.
- [ ] Evitar pool global inseguro.
- [ ] Garantir thread safety.
- [ ] Não usar pooling em objetos que possam escapar para o usuário.

## Critério de aceite

```text
120/120 testes passando
Allocated reduzido pelo menos 30% inicialmente
Meta final: reduzir 60%
```

---

# Sprint 8 — Projection separado

## Arquivos principais

```text
src/JMSAutoMapper/Projection/QueryableMapper.cs
src/JMSAutoMapper/Projection/ProjectionBuilder.cs
src/JMSAutoMapper/Projection/ProjectionExpressionVisitor.cs
src/JMSAutoMapper/Projection/ProjectionMemberBinder.cs
src/JMSAutoMapper/Projection/EfSafeExpressionBuilder.cs
```

## Objetivo

Manter `ProjectTo` separado de mapeamento em memória.

## Regras

`ProjectTo` deve gerar expressão compatível com LINQ providers, especialmente Entity Framework.

Não usar dentro de `ProjectTo`:

```text
Activator.CreateInstance
Reflection runtime
ConvertValue complexo
Resolvers com lógica não traduzível
I/O
Async resolver
Cache distribuído
```

## Critério de aceite

```text
ProjectToTests passando
Nenhuma regressão em Map/MapAsync
```

---

# Sprint 9 — Diagnostics e erros melhores

## Arquivos principais

```text
src/JMSAutoMapper/Diagnostics/DiagnosticInfo.cs
src/JMSAutoMapper/Diagnostics/DiagnosticCollector.cs
src/JMSAutoMapper/Diagnostics/MappingDiagnosticEvent.cs
src/JMSAutoMapper/Diagnostics/MappingPerformanceTracker.cs
src/JMSAutoMapper/Diagnostics/MappingErrorInfo.cs
```

## Objetivo

Melhorar mensagens de erro e diagnóstico.

## Implementar

- [ ] Caminho da propriedade com erro.
- [ ] Tipo origem.
- [ ] Tipo destino.
- [ ] Nome da propriedade.
- [ ] Valor problemático, quando seguro.
- [ ] Tempo de execução.
- [ ] Mapeamentos lentos.
- [ ] Contadores de cache.

## Exemplo de erro ideal

```text
MappingException:
Erro ao mapear Source.TotalValue -> Destination.Total
SourceType: Pedido
DestinationType: PedidoDto
Property: Total
Value: "ABC"
ExpectedType: decimal
```

## Critério de aceite

```text
DiagnosticsTests passando
Mensagens mais claras
Sem impacto relevante de performance quando diagnostics estiver desligado
```

---

# Sprint 10 — Validação final

## Arquivos principais

```text
src/JMSAutoMapper/Validation/ConfigurationValidator.cs
src/JMSAutoMapper/Validation/MappingValidationResult.cs
src/JMSAutoMapper/Validation/MissingMemberValidator.cs
src/JMSAutoMapper/Validation/ConstructorValidator.cs
src/JMSAutoMapper/Validation/ProjectionValidator.cs
```

## Objetivo

Garantir que configuração inválida seja detectada cedo.

## Validar

- [ ] Membros destino sem origem.
- [ ] Membros origem não usados, quando aplicável.
- [ ] Construtores inválidos.
- [ ] Mapeamentos duplicados.
- [ ] Resolvers incompatíveis.
- [ ] ProjectTo com expressão não suportada.
- [ ] Conversões perigosas.

## Critério de aceite

```text
ConfigurationValidationTests passando
Mensagens claras
ValidateOnBuild funcionando
```

---

# Sprint 11 — Documentação

## Arquivos

```text
docs/README.md
docs/CONFIGURATION.md
docs/PERFORMANCE.md
docs/PROJECTION.md
docs/CACHE.md
docs/MIGRATION.md
docs/NUGET.md
docs/ROADMAP.md
docs/CHANGELOG.md
```

## Conteúdo mínimo

- [ ] Instalação.
- [ ] Exemplo simples.
- [ ] Profiles.
- [ ] ForMember.
- [ ] Ignore.
- [ ] ReverseMap.
- [ ] IncludeBase.
- [ ] Resolvers.
- [ ] Async resolvers.
- [ ] Collections.
- [ ] ProjectTo.
- [ ] Cache.
- [ ] DI.
- [ ] Benchmarks.
- [ ] Versionamento.
- [ ] Licença Apache-2.0.

---

# Sprint 12 — Preparação NuGet

## Arquivos

```text
src/JMSAutoMapper/JMSAutoMapper.csproj
build/Directory.Build.props
build/pack.ps1
build/publish-nuget.ps1
```

## Configuração recomendada

```xml
<PropertyGroup>
  <PackageId>JMSAutoMapper</PackageId>
  <Version>0.9.0.0</Version>
  <AssemblyVersion>0.9.0.0</AssemblyVersion>
  <FileVersion>0.9.0.0</FileVersion>
  <InformationalVersion>0.9.0.0</InformationalVersion>

  <Authors>José Mendes da Silva</Authors>
  <Company>JMS Platform Services</Company>
  <Description>Biblioteca .NET para mapeamento objeto-objeto simples, flexível e extensível.</Description>
  <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
  <RepositoryType>git</RepositoryType>
  <PackageTags>mapper;automapper;object-mapping;dotnet;csharp</PackageTags>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

## Comandos

```bash
dotnet clean
dotnet test
dotnet build -c Release
dotnet pack -c Release --output ./nupkgs
```

---

# Instruções específicas para Gemini/Codex

## Diretriz 1 — Não quebrar a API pública

Não remover ou alterar assinatura de:

```text
IMapper
MapperConfiguration
Profile
IMappingExpression
IMemberConfigurationExpression
IValueResolver
IAsyncValueResolver
ITypeConverter
```

Mudanças internas são permitidas.

Mudanças públicas exigem nova major version futura.

---

## Diretriz 2 — Não otimizar tudo de uma vez

Implementar uma melhoria por vez.

Ordem obrigatória:

```text
1. PropertyAccessorCache
2. CompiledGetter / CompiledSetter
3. MapperPlanCache
4. MappingPlanBuilder
5. CompiledMappingPlan
6. CompiledCollectionMapper
7. ObjectPoolProvider
```

---

## Diretriz 3 — Manter fallback seguro

Toda otimização deve ter fallback para a engine atual.

Exemplo:

```text
Se o plano compilado não suporta o caso,
usar pipeline atual.
```

Nunca substituir tudo de uma vez.

---

## Diretriz 4 — Testes são obrigatórios

Após cada arquivo implementado:

```bash
dotnet test
```

Após cada sprint:

```bash
dotnet run -c Release --project benchmarks/JMSAutoMapper.Benchmark
```

---

## Diretriz 5 — Não misturar ProjectTo com Map

`ProjectTo` deve ser tratado como subsistema separado.

Mapeamento em memória pode usar delegates compilados.

`ProjectTo` deve gerar expression tree traduzível.

---

## Diretriz 6 — Medir antes e depois

Cada otimização deve registrar:

```text
Benchmark antes
Benchmark depois
Diferença percentual
Alocação antes
Alocação depois
```

Salvar em:

```text
docs/benchmarks/
```

---

# Ordem final recomendada

```text
0.0.1.0  Congelar estado atual
0.1.0.0  PropertyAccessorCache
0.2.0.0  CompiledGetter/Setter
0.3.0.0  MapperPlanCache
0.4.0.0  MappingPlanBuilder
0.5.0.0  CompiledMappingPlan
0.6.0.0  CompiledCollectionMapper
0.7.0.0  Object Pooling
0.8.0.0  Diagnostics + Validation
0.9.0.0  Docs + NuGet RC
1.0.0.0  Versão pública estável
```

---

# Definição de pronto

A biblioteca estará pronta para publicação pública quando:

```text
✅ 120/120 testes passando
✅ Zero erros
✅ Zero warnings
✅ Benchmark documentado
✅ README pronto
✅ LICENSE Apache-2.0
✅ CHANGELOG.md atualizado
✅ PackageId definido
✅ Versionamento fixo
✅ NuGet local funcionando
✅ API pública congelada
✅ Exemplos funcionando
```
