# JMSAutoMapper — PERFORMANCE-GUIDELINES.md

## Objetivo

Definir regras para otimização de performance.

## Baseline conhecido

Resultados históricos aproximados:

```text
SimpleMap      ~325 ns  | 464 B
CollectionMap  ~35 us   | 47 KB
ComplexMap     ~671 ns  | 816 B
```

Após otimizações iniciais, alguns resultados chegaram a:

```text
CollectionMap  ~5.6 us | ~6.7 KB
ComplexMap     ~400 ns | ~368 B
```

## Regras

- Medir antes e depois.
- Não otimizar sem benchmark.
- Não trocar comportamento correto por velocidade.
- Otimização deve preservar testes.
- Caminho crítico deve evitar reflection.

## Prioridades

1. SimpleMap Fast Path.
2. Collection Fast Path.
3. Nested Compiled Mapping.
4. Redução de alocação.
5. Cache de planos.
6. Cache de metadados.
7. Object Pooling com cuidado.

## Métricas obrigatórias

Registrar Mean, Median, Allocated, Gen0 e observações de outliers.

## Metas atuais

```text
SimpleMap      <= 250 ns
ComplexMap     <= 400 ns
CollectionMap  <= 5 us
```

## Cuidados

Não deixar SimpleMap pagar custo de coleção, nested mapping, fallback, contexto pesado, diagnóstico detalhado ou tracking de referência quando desnecessário.

Usar fast path quando o plano for simples.
