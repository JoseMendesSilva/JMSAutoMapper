# JMSAutoMapper — AGENTS.md

## Objetivo

Este documento define as regras obrigatórias para qualquer agente automatizado (Codex, ChatGPT, Copilot, Gemini, Claude, Cursor ou similar) que trabalhe neste repositório.

O objetivo principal é evoluir o JMSAutoMapper preservando compatibilidade pública, estabilidade, performance, cobertura de testes, arquitetura definida, licença Apache-2.0 e versionamento incremental.

O agente NÃO deve realizar refatorações destrutivas.

---

## Estado atual do projeto

O JMSAutoMapper encontra-se em fase avançada de desenvolvimento.

Status conhecido:

- Arquitetura definitiva V2 implementada.
- Código antigo distribuído nos novos arquivos.
- Testes automatizados existentes.
- BenchmarkDotNet configurado.
- Pipeline de mapeamento otimizado em evolução.
- Cache de planos implementado.
- Suporte a objetos complexos implementado.
- Suporte a coleções implementado.
- Licença Apache-2.0 definida.

O objetivo atual NÃO é reescrever a biblioteca.

O objetivo atual é:

1. Corrigir falhas de compilação.
2. Corrigir bugs.
3. Manter todos os testes passando.
4. Preservar a API pública.
5. Preservar ou melhorar performance.
6. Preparar a biblioteca para publicação futura no NuGet.

---

## Ordem obrigatória de trabalho

Sempre seguir esta ordem:

1. Build.
2. Testes.
3. Bugs.
4. Performance.
5. Documentação.
6. Novas funcionalidades.

Nunca implementar novas funcionalidades enquanto existirem erros de compilação, testes falhando, benchmarks quebrados ou regressão funcional conhecida.

---

## Regras de compatibilidade pública

Não alterar APIs públicas sem necessidade extrema.

Evitar mudanças em:

- `IMapper`
- `MapperConfiguration`
- `Profile`
- `IMappingExpression`
- `IMemberConfigurationExpression`
- `IValueResolver`
- `IAsyncValueResolver`
- `ITypeConverter`
- Extensões de Dependency Injection

Mudanças internas são permitidas. Mudanças públicas incompatíveis devem ser evitadas até a versão `1.0.0.0`.

---

## Regras de compilação

Antes de concluir qualquer tarefa, executar:

```bash
dotnet clean
dotnet restore
dotnet build -c Release
```

Se houver erro de compilação, a tarefa NÃO está concluída.

---

## Regras de testes

Executar:

```bash
dotnet test
```

Todos os testes existentes devem passar. É proibido remover, ignorar ou enfraquecer testes para obter sucesso aparente.

Se um teste falhar:

1. Entender a causa.
2. Corrigir o código.
3. Confirmar que não houve regressão.
4. Rodar a suíte completa novamente.

---

## Regras de benchmark

Após qualquer alteração em Mapping Engine, Collection Mapper, Nested Mapping, Cache, Reflection, Compiled Mapping Plan ou Object Pooling, executar BenchmarkDotNet em Release.

Exemplo:

```bash
dotnet run -c Release --project benchmarks/JMSAutoMapper.Benchmarks
```

Registrar Mean, Allocated, Gen0 e observações de regressão.

---

## Metas atuais de performance

```text
SimpleMap      <= 250 ns
ComplexMap     <= 400 ns
CollectionMap  <= 5 us
Allocated       mínimo possível
```

As metas podem ser ajustadas conforme os benchmarks reais evoluírem.

---

## Reflection

Reflection deve existir apenas durante descoberta de metadados, geração de planos ou fallback controlado.

Reflection não deve estar presente no caminho crítico quando houver plano compilado disponível.

Preferir delegates compilados, expression trees compiladas, cache de metadados e cache de planos.

---

## Collection Mapping

Prioridades:

1. Delegate compilado.
2. Plano compilado reutilizado.
3. Pré-alocação de listas.
4. Evitar boxing.
5. Evitar reflection.
6. Evitar `Map<T>()` genérico por item quando houver delegate direto.

---

## Nested Mapping

Objetos aninhados devem usar cache de planos, plano compilado do tipo filho, fallback seguro e evitar reflection repetitiva e recriação desnecessária de contexto.

---

## Memory

Reduzir alocações temporárias, boxing, closures desnecessárias, LINQ em loops críticos e criação repetida de dicionários/contextos.

Preferir `for`, arrays, caches, delegates e pré-alocação em caminhos críticos.

---

## Fallback seguro

Toda otimização deve ter fallback para a engine estável.

Se o plano compilado não suportar um caso, usar pipeline antigo ou fallback seguro.

Nunca substituir todo o pipeline de uma vez.

---

## ProjectTo

`ProjectTo` deve ser tratado como subsistema separado.

Mapeamento em memória pode usar delegates compilados. `ProjectTo` deve gerar expression tree traduzível para LINQ providers.

Não usar dentro de `ProjectTo`: I/O, async resolver, cache distribuído, reflection runtime, `Activator.CreateInstance` ou lógica não traduzível para SQL.

---

## Dependências

Não adicionar dependências externas desnecessárias. Qualquer nova dependência deve ser justificada.

---

## Segurança

Não adicionar telemetria oculta, código remoto, coleta de dados, chamadas HTTP automáticas ou dependência de serviço externo sem aprovação explícita.

---

## Versionamento

Seguir versionamento incremental explícito:

```text
0.0.1.0
0.0.2.0
0.0.3.0
...
0.1.0.0
...
0.9.0.0
...
1.0.0.0
```

Nunca usar wildcard, por exemplo `1.0.17.*`.

Não alterar versão automaticamente sem solicitação.

---

## Licença

A licença do projeto é Apache-2.0.

Manter LICENSE, NOTICE, README.md e CHANGELOG.md.

Não alterar licença sem aprovação explícita do autor.

---

## Critérios de aceite

Uma tarefa só pode ser considerada concluída quando compila em Release, testes passam, benchmarks executam quando aplicável, não há regressão funcional, não há regressão significativa de performance, API pública foi preservada e mudanças estão coerentes com a arquitetura V2.

---

## Objetivo final

Transformar o JMSAutoMapper em uma biblioteca estável, confiável, simples, flexível, performática, preparada para NuGet e adequada para uso real em projetos .NET.
