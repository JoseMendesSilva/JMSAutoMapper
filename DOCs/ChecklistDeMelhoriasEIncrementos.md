# Checklist de Melhorias e Incrementos para JMSAutoMapper

Com base na análise da solução, este checklist detalha as melhorias e incrementos sugeridos para a biblioteca JMSAutoMapper, visando aumentar sua robustez, manutenibilidade e clareza.

## 1. Design e Arquitetura

*   [x] **Remover/Refatorar Configuração Estática (`_staticConfig`)**:
    *   **Objetivo**: Eliminar potenciais problemas em ambientes multi-threaded/multi-tenant e promover a injeção de dependência como método principal de configuração.
    *   **Ação**: Remover completamente o `_staticConfig` e o método `SetConfiguration`. Garantir que a configuração do mapper seja sempre feita via injeção de dependência ou instanciamento direto com `MapperConfiguration`.

*   [x] **Verdadeira Assincronicidade para `MapAsync`**:
    *   **Objetivo**: Garantir que os métodos assíncronos ofereçam benefícios reais de não-bloqueio, especialmente para operações I/O-bound (se aplicável a custom resolvers).
    *   **Ação**: Reavaliar `MapAsync<T>(object? source)`. Se não houver operações intrinsecamente assíncronas no mapeamento de um único objeto, considerar:
        *   Remover o método `MapAsync` se ele for apenas um wrapper de `Task.FromResult`.
        *   Renomeá-lo para indicar que é um wrapper síncrono (ex: `MapSynchronouslyAsTask`).
        *   Documentar claramente que seu uso é para compatibilidade de assinatura, não para ganho de performance assíncrona.

*   [x] **Refinar `IQueryable` Projection**:
    *   **Objetivo**: Completar e validar a funcionalidade de projeção `IQueryable` para garantir que ela traduza corretamente a lógica de mapeamento para expressões de consulta de banco de dados.
    *   **Ação**: Desenvolver e testar exaustivamente o `ProjectionExpressionVisitor` para que ele construa expressões de projeção válidas e eficientes para provedores de `IQueryable` (ex: Entity Framework).

*   [x] **Seleção de Construtor Mais Explícita**:
    *   **Objetivo**: Permitir maior controle sobre qual construtor é usado para instanciar o tipo de destino.
    *   **Ação**: Adicionar uma opção de configuração em `IMappingExpression` ou `MapperConfiguration` para especificar qual construtor deve ser usado, ou uma estratégia de seleção de construtor (ex: `UseConstructor<TDestination>(params Type[] parameterTypes)`).

## 2. Robustez e Tratamento de Erros

*   [x] **Tratamento de Exceções Mais Granular em `ConvertValue`**:
    *   **Objetivo**: Evitar mascarar erros de conversão e fornecer feedback mais específico.
    *   **Ação**: Substituir o `catch (Exception ex)` genérico por capturas de exceções mais específicas (ex: `InvalidCastException`, `FormatException`). Em vez de retornar `null` silenciosamente, considerar:
        *   Lançar uma exceção mais descritiva (ex: `MappingException` customizada) que encapsule a exceção original.
        *   Permitir que o usuário configure o comportamento (ex: `ThrowOnConversionError` ou `ReturnDefaultOnConversionError`).

*   [x] **Validação de Entrada Robusta**:
    *   **Objetivo**: Garantir que os métodos recebam entradas válidas e lancem exceções claras em caso de uso incorreto.
    *   **Ação**: Adicionar validações explícitas para parâmetros de entrada em métodos como `MapIEnumerable`, `MapList`, `MapDictionary`, etc., verificando se a `source` é realmente uma coleção ou dicionário quando esperado.

## 3. Clareza e Manutenibilidade

*   [x] **Adicionar Comentários de Documentação XML (`///`)**:
    *   **Objetivo**: Melhorar a documentação da API para facilitar o uso e a compreensão da biblioteca.
    *   **Ação**: Adicionar comentários XML para todas as classes, interfaces, métodos e propriedades públicas em `JMSMapper.cs` e outras classes públicas, explicando seu propósito, parâmetros, retornos e exceções.

*   [ ] **Revisar Nomenclatura Interna**:
    *   **Objetivo**: Aumentar a clareza e consistência do código interno.
    *   **Ação**: Revisar nomes de métodos e variáveis internas (ex: `MapObject` e seus sobrecargas) para garantir que sejam descritivos e consistentes com suas responsabilidades.

*   [x] **Remover Código Comentado e Regiões Obsoletas**:
    *   **Objetivo**: Limpar o código-fonte e remover distrações.
    *   **Ação**: Remover a seção `#region Mapper melhorado` e qualquer código comentado que não seja mais relevante ou que esteja no controle de versão.

## 4. Testes

*   [x] **Aumentar Cobertura de Testes para `IQueryable` Projection**:
    *   **Objetivo**: Garantir que a funcionalidade de projeção `IQueryable` esteja totalmente testada.
    *   **Ação**: Implementar testes unitários e de integração detalhados para `MapQueryable`, verificando a correta tradução de expressões e o comportamento com diferentes provedores de `IQueryable`.

*   [x] **Testes Abrangentes para Coleções Imutáveis e Dicionários**:
    *   **Objetivo**: Assegurar que todos os métodos de mapeamento para coleções imutáveis e dicionários funcionem corretamente em diversos cenários.
    *   **Ação**: Adicionar testes específicos para `MapImmutableList`, `MapImmutableDictionary`, `MapImmutableArray`, `MapImmutableQueue`, `MapImmutableStack`, e `MapDictionary`, cobrindo casos de coleções vazias, nulas e com itens nulos.

*   [ ] **Testes de Desempenho e Carga (Benchmarking)**:
    *   **Objetivo**: Validar as afirmações de alta performance e identificar gargalos.
    *   **Ação**: Implementar benchmarks usando ferramentas como `BenchmarkDotNet` para comparar o desempenho do `JMSAutoMapper` com outras bibliotecas populares e medir o impacto de diferentes configurações e volumes de dados.

*   [x] **Testes de Construtor Parametrizado**:
    *   **Objetivo**: Verificar se o mapeador lida corretamente com classes de destino que possuem construtores com parâmetros.
    *   **Ação**: Adicionar testes que mapeiam para tipos de destino com construtores não-padrão, garantindo que os parâmetros sejam preenchidos corretamente.

## 5. Documentação Externa

*   [x] **Expandir `README.md` com Detalhes de Configuração Avançada**:
    *   **Objetivo**: Fornecer exemplos mais detalhados e cenários de uso avançado na documentação pública.
    *   **Ação**: Adicionar seções no `README.md` explicando o uso de `Profiles`, a configuração de `NamingConvention`, e exemplos mais complexos de `ForMember` e `Ignore`.

*   [x] **Guia de Contribuição**:
    *   **Objetivo**: Facilitar a contribuição da comunidade para o projeto.
    *   **Ação**: Criar um `CONTRIBUTING.md` com diretrizes para submissão de issues, pull requests, padrões de código e como executar os testes.
