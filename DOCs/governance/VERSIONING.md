# JMSAutoMapper — VERSIONING.md

## Objetivo

Padronizar o versionamento do JMSAutoMapper.

## Formato

Usar quatro partes:

```text
MAJOR.MINOR.PATCH.BUILD
```

Exemplo:

```text
0.0.1.0
```

## Regras

```text
MAJOR
Mudanças incompatíveis.

MINOR
Novas funcionalidades compatíveis.

PATCH
Correções de bug.

BUILD
Incremento local ou marco interno.
```

## Antes da versão 1.0.0.0

Usar versões incrementais:

```text
0.0.1.0
0.0.2.0
0.1.0.0
0.2.0.0
...
0.9.0.0
1.0.0.0
```

## Não usar

Nunca usar:

```text
1.0.17.*
```

Wildcards quebram compilação determinística e podem causar erro em BenchmarkDotNet.

## Propriedades recomendadas no csproj

```xml
<PropertyGroup>
  <Version>0.0.1.0</Version>
  <AssemblyVersion>0.0.1.0</AssemblyVersion>
  <FileVersion>0.0.1.0</FileVersion>
  <InformationalVersion>0.0.1.0</InformationalVersion>
</PropertyGroup>
```

## Marcos sugeridos

```text
0.0.1.0  Estrutura V2 estabilizada
0.1.0.0  Correções finais de build/testes
0.2.0.0  PropertyAccessorCache
0.3.0.0  CompiledGetter/Setter
0.4.0.0  MappingPlanBuilder
0.5.0.0  CompiledMappingPlan
0.6.0.0  CompiledCollectionMapper
0.7.0.0  Object Pooling
0.8.0.0  Documentação e benchmarks
0.9.0.0  Release candidate interna
1.0.0.0  Primeira versão estável pública
```
