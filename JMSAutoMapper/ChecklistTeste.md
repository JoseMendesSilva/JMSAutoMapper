# Checklist de Testes Unitários para JMSAutoMapper

Este documento descreve uma série de testes unitários sugeridos para o projeto `JMSAutoMapper` a fim de aumentar a cobertura de testes, garantir o funcionamento esperado de todos os recursos e prevenir regressões.

## 1. Testes de Mapeamento Básico

*   **Mapeamento de Tipos Simples:**
    *   [x] Testar o mapeamento de `int` para `int`.
    *   [x] Testar o mapeamento de `string` para `string`.
    *   [x] Testar o mapeamento de `int` para `string` e vice-versa (conversão de tipo).
    *   [x] Testar o mapeamento de `DateTime` para `DateTime`.
    *   [x] Testar o mapeamento de `Guid` para `Guid`.

*   **Mapeamento de Objetos Complexos (Propriedades Correspondentes):**
    *   [x] Criar classes `SourceA` e `DestinationA` com propriedades de mesmo nome e tipo.
    *   [x] Testar o mapeamento de `SourceA` para `DestinationA`.
    *   [x] Verificar se todas as propriedades são mapeadas corretamente.

*   **Mapeamento com `ForMember` (Renomear Propriedades):**
    *   [x] Criar classes `SourceB` (ex: `PropriedadeOriginal`) e `DestinationB` (ex: `PropriedadeRenomeada`).
    *   [x] Configurar `ForMember` para mapear `PropriedadeOriginal` para `PropriedadeRenomeada`.
    *   [x] Testar o mapeamento e verificar se a propriedade foi renomeada corretamente.

*   **Mapeamento com `ForMember` (Resolução de Valor Customizada):**
    *   [x] Criar classes `SourceC` (ex: `ValorNumerico`) e `DestinationC` (ex: `ValorTexto`).
    *   [x] Configurar `ForMember` para converter `ValorNumerico` para `ValorTexto` usando uma função customizada (ex: `s => s.ValorNumerico.ToString()`).
    *   [x] Testar o mapeamento e verificar o valor customizado.

*   **Mapeamento com `Ignore`:**
    *   [x] Criar classes `SourceD` e `DestinationD` com uma propriedade que deve ser ignorada.
    *   [x] Configurar `Ignore` para a propriedade específica.
    *   [x] Testar o mapeamento e verificar se a propriedade ignorada não foi mapeada.

*   **Mapeamento com `ForMember` (Condicional):**
    *   [x] Criar classes `SourceE` e `DestinationE`.
    *   [x] Configurar `ForMember` com uma condição (ex: mapear uma propriedade apenas se outra propriedade na origem atender a um critério).
    *   [x] Testar o mapeamento com a condição sendo verdadeira e falsa.

*   **Mapeamento de Tipos Anuláveis:**
    *   [x] Testar o mapeamento de `int?` para `int` e vice-versa.
    *   [x] Testar o mapeamento de `string` para `string?`.
    *   [x] Testar o mapeamento de `null` para tipos anuláveis e não anuláveis.

## 2. Testes de Mapeamento de Coleções

*   **`IEnumerable<T>`:**
    *   [x] Mapear `IEnumerable<Source>` para `IEnumerable<Destination>`.
    *   [x] Testar com uma coleção vazia.
    *   [x] Testar com uma coleção contendo itens nulos.

*   **`List<T>`:**
    *   [x] Mapear `List<Source>` para `List<Destination>`.
    *   [x] Testar com uma lista vazia.

*   **`T[]` (Array):**
    *   [x] Mapear `Source[]` para `Destination[]`.
    *   [x] Testar com um array vazio.

*   **`HashSet<T>`:**
    *   [x] Mapear `HashSet<Source>` para `HashSet<Destination>`.
    *   [x] Testar com um HashSet vazio.

*   **`Dictionary<TKey, TValue>`:**
    *   [x] Mapear `Dictionary<SourceKey, SourceValue>` para `Dictionary<DestinationKey, DestinationValue>`.
    *   [x] Testar com um dicionário vazio.
    *   [x] Testar com chaves e valores nulos (se aplicável e permitido).

*   **Coleções Imutáveis:**
    *   [x] `ImmutableList<T>`: Mapear `IEnumerable<Source>` para `ImmutableList<Destination>`.
    *   [x] `ImmutableDictionary<TKey, TValue>`: Mapear `IDictionary<SourceKey, SourceValue>` para `ImmutableDictionary<DestinationKey, DestinationValue>`.
    *   [x] `ImmutableArray<T>`: Mapear `IEnumerable<Source>` para `ImmutableArray<Destination>`.
    *   [x] `ImmutableQueue<T>`: Mapear `IEnumerable<Source>` para `ImmutableQueue<Destination>`.
    *   [x] `ImmutableStack<T>`: Mapear `IEnumerable<Source>` para `ImmutableStack<Destination>`.

## 3. Testes de Mapeamento Assíncrono

*   **`MapAsync<T>`:**
    *   [x] Testar o mapeamento assíncrono de um objeto simples.
    *   [x] Testar o mapeamento assíncrono de um objeto complexo.
    *   [x] Testar com origem nula.

*   **`MapIEnumerableAsync<T>`:**
    *   [x] Testar o mapeamento assíncrono de uma coleção sem `maxDegreeOfParallelism`.
    *   [x] Testar o mapeamento assíncrono de uma coleção com `maxDegreeOfParallelism` definido (ex: 2, 5).
    *   [x] Testar com uma coleção vazia.
    *   [x] Testar com uma coleção contendo itens nulos.

## 4. Testes de Mapeamento `IQueryable`

*   **`MapQueryable<TSource, TDestination>` (Básico):**
    *   [x] Criar um `IQueryable<Source>` e mapeá-lo para `IQueryable<Destination>`.
    *   [x] Verificar se a projeção é aplicada corretamente no nível da query (ex: usando um provedor de teste ou inspecionando a expressão).

*   **`MapQueryable` com Propriedades Aninhadas:**
    *   [x] Testar o mapeamento de objetos com propriedades aninhadas dentro de um `IQueryable`.

*   **`MapQueryable` com `ForMember` e `Ignore`:**
    *   [x] Verificar se as configurações de `ForMember` e `Ignore` são respeitadas no mapeamento `IQueryable`.

## 5. Testes de Configuração e Perfis

*   **`AddJMSMapper` (Extensão `IServiceCollection`):**
    *   [x] Testar a adição do mapper ao contêiner de DI sem configurações.
    *   [x] Testar a adição com uma configuração básica.
    *   [x] Testar a adição com perfis de mapeamento.
    *   [x] Verificar se a instância de `IMapper` é resolvida corretamente do DI.

*   **`Profile` (Configuração de Perfis):**
    *   [x] Criar um `Profile` customizado e adicionar mapeamentos dentro dele.
    *   [x] Testar se os mapeamentos definidos no perfil são aplicados corretamente.
    *   [x] Testar a adição de múltiplos perfis.

*   **`ReverseMap`:**
    *   [x] Configurar um mapeamento de `Source` para `Destination`.
    *   [x] Chamar `ReverseMap()` e testar o mapeamento de `Destination` para `Source`.
    *   [x] Verificar se as propriedades são mapeadas corretamente na direção inversa.

*   **`NamingConvention`:**
    *   [x] Definir uma convenção de nomenclatura customizada (ex: `CamelCase` para `PascalCase`).
    *   [x] Testar se a convenção é aplicada corretamente durante o mapeamento.

## 6. Testes de Casos de Borda e Tratamento de Erros

*   **Origem Nula:**
    *   [x] Testar o mapeamento de um objeto de origem `null` para tipos de referência e tipos de valor.
    *   [x] Verificar se `default!` ou `ArgumentNullException` é retornado/lançado conforme o esperado.

*   **Erros de Conversão de Tipo (`ConvertValue`):**
    *   [x] Testar cenários onde a conversão de tipo falha (ex: tentar converter uma string não numérica para um `int`).
    *   [x] Verificar se o logger é invocado e se o valor padrão ou `null` é retornado.

*   **Referências Circulares:**
    *   [x] Criar um cenário com referências circulares entre objetos (ex: `Pessoa` tem `Filho`, `Filho` tem `Pai`).
    *   [x] Verificar se o mapeador lida corretamente com isso para evitar `StackOverflowException` (o uso de `mappedObjects` deve ajudar aqui).

*   **Propriedades Não Encontradas:**
    *   [x] Testar o mapeamento quando uma propriedade de destino não tem uma propriedade correspondente na origem e não há mapeamento customizado.
    *   [x] Verificar se a propriedade de destino mantém seu valor padrão ou é ignorada.

*   **Tratamento de Construtores:**
    *   [x] Testar classes de destino com diferentes tipos de construtores (construtor padrão, construtor com parâmetros).
    *   [x] Verificar se o mapeador seleciona e utiliza o construtor apropriado.

*   **Mapeamento de Enum:**
    *   [x] Testar mapeamento de `string` para `enum`.
    *   [x] Testar mapeamento de `int` para `enum`.
    *   [x] Testar mapeamento de `enum` para `enum`.
    *   [x] Testar mapeamento de `enum` para `string`.
    *   [x] Testar casos de erro na conversão de enum (ex: string inválida).

## 7. Testes de Desempenho (Opcional)

*   **Mapeamento de Grande Volume de Dados:**
    *   [x] Testar o desempenho do mapeador com grandes coleções de objetos (ex: 10.000, 100.000 itens).
    *   [x] Comparar o desempenho do mapeamento síncrono vs. assíncrono.

## 8. Testes de Integração (Opcional)

*   **Integração com DI:**
    *   [x] Testar o uso do `JMSAutoMapper` em um ambiente de aplicação real com injeção de dependência.

Ao implementar esses testes, certifique-se de usar um framework de testes unitários (ex: xUnit, NUnit) e bibliotecas de asserção (ex: FluentAssertions) para tornar os testes legíveis e robustos.