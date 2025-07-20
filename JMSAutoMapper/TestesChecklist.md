# Testes Checklist para JMSAutoMapper

Este documento lista os testes existentes e propõe novos cenários para aumentar a cobertura e a robustez da biblioteca JMSAutoMapper.

## Testes Existentes (Assumidos com base nos nomes de arquivos comuns)

*   **JMSMapperTest.cs:**
    *   Testes básicos de mapeamento de propriedades.
    *   Testes de configuração de mapeamento (`CreateMap`, `ForMember`).
    *   Testes de mapeamento reverso (`ReverseMap`).
    *   Testes de ignorar propriedades (`Ignore`).
*   **LoadTests.cs:**
    *   Testes de desempenho sob carga controlada.
    *   Avaliação do consumo de memória e CPU durante operações de mapeamento em massa.
*   **StressTests.cs:**
    *   Testes de resiliência sob condições extremas.
    *   Verificação do comportamento do mapeador com grandes volumes de dados ou operações concorrentes.

## Testes Unitários Propostos (Cenários Detalhados)

### Mapeamento Básico e Configuração
*   **Tipos de Dados Diversos:**
    *   [x] Mapeamento de todos os tipos primitivos (int, float, double, bool, char).
    *   [x] Mapeamento de tipos `decimal`, `DateTime`, `Guid`, `TimeSpan`, `DateTimeOffset`.
    *   [x] Mapeamento de tipos anuláveis (nullable types).
*   **Propriedades com Nomes Diferentes:**
    *   [x] Testar `ForMember` com mapeamento de nome de propriedade explícito.
    *   [x] Testar `ForMember` com `Func<TSource, TMember>` para resolver valores complexos.
*   **Mapeamento Condicional:**
    *   [x] Testar `ForMember` com `Func<TSource, bool>` para mapear propriedades apenas sob certas condições.
    *   [x] Verificar que a propriedade não é mapeada quando a condição é falsa.
*   **Mapeamento Bidirecional (`ReverseMap`):**
    *   [x] Testar mapeamento de ida e volta com configurações simples e complexas.
    *   [x] Verificar que `ReverseMap` funciona corretamente com `ForMember` e `Ignore`.
*   **Ignorar Propriedades (`Ignore`):**
    *   [x] Testar ignorar propriedades específicas no mapeamento.
    *   [x] Verificar que a propriedade ignorada mantém seu valor padrão ou existente no destino.

### Mapeamento de Coleções
*   **Tipos de Coleção Padrão:**
    *   `IEnumerable<T>`, `List<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `T[]` (arrays), `HashSet<T>`.
    *   Mapeamento de coleções vazias.
    *   Mapeamento de coleções contendo itens nulos.
*   **Coleções Imutáveis:**
    *   [x] `ImmutableList<T>`.
    *   [x] `ImmutableDictionary<TKey, TValue>`.
    *   [x] `ImmutableArray<T>`.
    *   [x] `ImmutableQueue<T>`.
    *   [x] `ImmutableStack<T>`.
*   **Dicionários:**
    *   [x] `Dictionary<TKey, TValue>`.
    *   [x] Mapeamento de dicionários vazios e com valores nulos.

### Mapeamento de Tipos Complexos e Aninhados
*   **Objetos Aninhados:**
    *   Mapeamento de objetos que contêm outros objetos como propriedades.
    *   Testar múltiplos níveis de aninhamento (e.g., `A` contém `B`, `B` contém `C`).
*   **Mapeamento de Múltiplas Fontes para o Mesmo Destino:**
    *   Testar cenários onde diferentes tipos de origem podem ser mapeados para o mesmo tipo de destino.

### Mapeamento Assíncrono
*   **`MapAsync<T>`:**
    *   Testar mapeamento de objeto único de forma assíncrona.
*   **`MapIEnumerableAsync<T>`:**
    *   Testar mapeamento de coleções de forma assíncrona.
    *   Testar com e sem `maxDegreeOfParallelism`.
    *   Verificar o comportamento com coleções grandes.

### Mapeamento Relacional (Múltiplos Relacionamentos)
*   **Cenários com Três ou Mais Entidades Relacionadas:**
    *   Exemplo: `Empresa` -> `Departamento` -> `Funcionário`.
    *   Exemplo: `Pedido` -> `ItemPedido` -> `Produto`.
    *   Verificar a integridade dos dados e dos relacionamentos após o mapeamento.
    *   Testar coleções aninhadas dentro de relacionamentos.
*   **Relacionamentos Opcionais/Nulos:**
    *   Testar mapeamento quando entidades relacionadas são nulas.
    *   Verificar que as propriedades de destino correspondentes são nulas ou seus valores padrão.

### Testes de Construtor
*   **Mapeamento para Construtores Parametrizados:**
    *   Testar mapeamento para classes de destino que possuem construtores com parâmetros.
    *   Verificar que os parâmetros do construtor são preenchidos corretamente a partir das propriedades da origem.

### Tratamento de Referências Circulares
*   **Detecção e Prevenção de Loops Infinitos:**
    *   [x] Testar cenários onde objetos se referenciam mutuamente (e.g., `Pessoa` tem `Pai`, `Pai` tem `Filhos` que incluem `Pessoa`).
    *   [x] Verificar que o mapeador não entra em loop infinito e retorna as referências corretas.
    *   [x] Testar com múltiplos níveis de referências circulares.

## Testes de Carga e Estresse (Aprofundamento)

*   **Carga Crescente:**
    *   [x] Simular um número crescente de operações de mapeamento para identificar gargalos.
    *   [x] Monitorar o uso de recursos (CPU, memória) em diferentes níveis de carga.
*   **Mapeamento de Objetos Grandes:**
    *   [x] Testar o mapeamento de objetos com um grande número de propriedades ou estruturas aninhadas profundas.
*   **Concorrência:**
    *   [x] Executar operações de mapeamento a partir de múltiplas threads simultaneamente para testar a segurança e o desempenho em ambientes concorrentes.

## Testes de Uso Contínuo (Long-Running)

*   **Mapeamento em Loop:**
    *   [x] Executar o mapeador em um loop por um longo período (horas) para detectar vazamentos de memória ou degradação de desempenho.
*   **Variação de Dados:**
    *   [x] Mapear diferentes tipos de dados e estruturas de objeto ao longo do tempo para garantir estabilidade.

## Testes de Implantação

*   **Compatibilidade de Versão:**
    *   [x] Testar a biblioteca em diferentes versões do .NET (e.g., .NET 6, .NET 7, .NET 8) para garantir compatibilidade.
*   **Integração com DI:**
    *   [x] Testar a integração do `AddJMSMapper` com diferentes configurações de contêineres de injeção de dependência.
*   **Cenários de Publicação:**
    *   [x] Verificar que a biblioteca funciona corretamente após ser empacotada e publicada como um pacote NuGet.
