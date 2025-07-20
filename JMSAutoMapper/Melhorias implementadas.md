# Checklist de Melhorias e Correções para JMSMapper.cs

## 1. Performance

- [x] **Otimizar o uso de Reflection:**
    - [x] Considerar a geração de expressões (Expression Trees) ou IL (Intermediate Language) para o mapeamento de propriedades, em vez de `PropertyInfo.SetValue` e `GetValue` em cada mapeamento. Isso pode oferecer ganhos significativos de performance, especialmente para objetos complexos ou coleções grandes.
    - [x] Avaliar o impacto de `MakeGenericMethod` e `Invoke` em `MapObject` e `MapObjectGeneric`. Se for um gargalo, buscar alternativas (ex: pré-compilar delegados).
- [x] **Otimizar mapeamento de coleções:**
    - [x] As implementações de `MapList`, `MapICollection`, `MapIReadOnlyList`, `MapIReadOnlyCollection`, `MapArray`, `MapHashSet`, `MapImmutableList`, `MapImmutableDictionary`, `MapImmutableArray`, `MapImmutableQueue`, `MapImmutableStack` chamam `MapList` ou `MapDictionary` e depois convertem. Isso pode ser otimizado para evitar múltiplas iterações ou conversões desnecessárias.

## 2. Design e Arquitetura

- [x] **Remover ou refatorar configuração estática (`_staticConfig`):**
    - [x] O uso de `_staticConfig` em `JMSMapper` pode levar a problemas em ambientes multi-tenant, aplicações web com múltiplos requests ou em cenários de concorrência, pois a configuração seria global e compartilhada.
    - [x] Priorizar a injeção de dependência (`_instanceConfig`) e remover a dependência de estado estático para a configuração do mapeador. Se a configuração estática for estritamente necessária para algum cenário específico (ex: console app simples), documentar claramente suas limitações e casos de uso.
- [x] **Verdadeira assincronicidade:**
    - [x] Os métodos `MapAsync` e `MapIEnumerableAsync` atualmente apenas envolvem chamadas síncronas em `Task.FromResult`. Para que sejam verdadeiramente assíncronos e ofereçam benefícios de I/O bound operations, eles precisariam realizar operações que são intrinsecamente assíncronas (o que não é o caso para mapeamento em memória). Se não houver operações assíncronas reais, considerar remover os métodos `Async` ou renomeá-los para indicar que são apenas wrappers de `Task`.
- [x] **Consistência na interface `IMapper` e `MapperBase`:**
    - [x] O método `CreateMap` na interface `IMapper` e na classe `MapperBase` lança `NotImplementedException`. `CreateMap` é um método de configuração e deve ser exposto apenas através de `MapperConfiguration` ou de uma interface de configuração separada, não diretamente no `IMapper` que é para execução de mapeamento.
- [x] **Tratamento de tipos de coleção:**
    - [x] O método `CreateCollection` atualmente cria apenas `List<>` para tipos genéricos. Isso pode não ser o ideal para todos os tipos de `ICollection` (ex: `ISet<T>`, `Queue<T>`, `Stack<T>`). Considerar a criação de instâncias mais específicas ou permitir a configuração de construtores de coleção.
- [x] **Tratamento de circularidade:**
    - [x] O `HashSet<object> visited` ajuda a prevenir loops infinitos em referências circulares diretas. No entanto, ele não impede que o mesmo objeto seja mapeado várias vezes se ele aparecer em diferentes caminhos do grafo de objetos. Para cenários onde a identidade do objeto mapeado precisa ser preservada (ex: mapear um objeto para si mesmo em um grafo complexo), pode ser necessário um mecanismo mais sofisticado (ex: um cache de objetos já mapeados para o destino).

## 3. Robustez e Tratamento de Erros

- [x] **Tratamento de exceções mais granular:**
    - [x] O bloco `try-catch` genérico em `MapObject` que captura `Exception` e retorna `null` pode mascarar erros importantes. Considerar capturar exceções mais específicas (ex: `TargetInvocationException`, `InvalidCastException`) e talvez relançar outras ou fornecer mais contexto no log.
- [x] **Melhorar conversão de Enum:**
    - [x] O método `ConvertToEnum` possui múltiplos `if`s para diferentes tipos numéricos. Embora funcional, pode ser simplificado ou otimizado.
- [x] **Validação de entrada:**
    - [x] Adicionar validações mais robustas para os parâmetros de entrada dos métodos `MapX` (ex: verificar se `source` é realmente uma coleção quando `MapIEnumerable` é chamado).

## 4. Clareza e Manutenibilidade

- [x] **Comentários e documentação:**
    - [x] Adicionar comentários XML (`///`) para todas as classes, interfaces, métodos e propriedades públicas, explicando seu propósito, parâmetros, retornos e exceções. Isso melhora a documentação e a usabilidade da biblioteca.
- [x] **Nomenclatura:**
    - [x] Revisar a nomenclatura para garantir clareza e consistência (ex: `MapObjectGeneric` poderia ser mais descritivo se for uma parte interna da implementação).
- [x] **Remover código comentado:**
    - [x] Remover a seção `#region Esta versão funciona : versao 1.0.5` e o código comentado. Se for histórico, deve estar no controle de versão.

## 5. Extensibilidade

- [x] **Mapeamento de tipos de valor e structs:**
    - [x] Atualmente, o mapeador foca em `class, new()`. Considerar estender o suporte para tipos de valor (structs) e outros tipos que não possuem construtor padrão sem parâmetros.
- [x] **Mapeamento bidirecional:**
    - [x] Se o requisito surgir, adicionar suporte para mapeamento bidirecional (Source -> Destination e Destination -> Source) de forma configurável.
- [x] **Mapeamento condicional:**
    - [x] Adicionar a capacidade de definir mapeamentos condicionais (ex: mapear uma propriedade apenas se uma condição for verdadeira).
- [x] **Mapeamento aninhado de coleções:**
    - [x] Garantir que o mapeamento de coleções aninhadas (ex: `List<List<T>>`) funcione corretamente.