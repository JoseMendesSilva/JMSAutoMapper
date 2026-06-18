# JMSAutoMapper — TESTING-GUIDELINES.md

## Objetivo

Definir regras para testes automatizados.

## Regra principal

Todos os testes existentes devem passar.

É proibido remover ou ignorar testes para esconder falhas.

## Antes de alterar código

Executar:

```bash
dotnet test
```

## Depois de alterar código

Executar novamente:

```bash
dotnet test
```

## Quando rodar benchmark

Rodar benchmark após alterações em Mapping Engine, Collection Mapper, Cache, Reflection, Expression Compilation, Nested Mapping e Object Pooling.

## Tipos de teste importantes

Manter cobertura para simple mapping, nullable, conversões numéricas, string, DateTime, Guid, enum, coleções, dictionary, immutable collections, nested objects, circular references, ReverseMap, IncludeBase, resolvers, async resolvers, ProjectTo, cache, diagnostics, DI e validação de configuração.

## Regressões

Quando um bug for corrigido, adicionar teste de regressão.

O bug só é considerado corrigido quando o teste novo falha antes da correção, passa depois da correção e a suíte inteira continua verde.
