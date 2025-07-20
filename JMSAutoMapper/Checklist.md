# Checklist do JMSAutoMapper

Este checklist descreve melhorias potenciais, novos recursos e correções para a biblioteca JMSAutoMapper.

## Correções e Reparos de Bugs

-   [x] **Revisar o uso de `default`:** O uso de `default` em `MapObject` pode não ser ideal para todos os cenários, especialmente ao mapear para tipos de valor (structs). Agora, uma `ArgumentNullException` é lançada se a origem for nula e o tipo de destino for um tipo de valor não anulável.
-   [x] **Melhorar o Tratamento de Exceções:** O método `ConvertValue` possui um bloco `catch` genérico que retorna `null`. Agora, a exceção é registrada usando o logger, fornecendo mais contexto para depuração.

## Novos Recursos e Melhorias

-   [x] **Mapeamento Assíncrono:** Os métodos `MapAsync` e `MapIEnumerableAsync` agora suportam operações assíncronas verdadeiras, permitindo resolvedores assíncronos e operações ligadas a I/O.
-   [x] **Consistência de Tipos de Retorno:** Os tipos de retorno dos métodos de mapeamento de coleções em `MapperBase` agora correspondem aos definidos na interface `IMapper`.
-   [x] **Mapeamento aninhado de coleções:** Garantir que o mapeamento de coleções aninhadas (ex: `List<List<T>>`) funcione corretamente.
-   [x] **Ignorar Propriedades:** Adicionar uma forma de ignorar propriedades durante o mapeamento, tanto globalmente quanto para mapeamentos específicos.
-   [ ] **Injeção de Construtor:** Suporte para mapear objetos com construtores parametrizados, permitindo cenários de criação de objetos mais complexos.
-   [ ] **Achatamento (Flattening):** Implementar um recurso de achatamento baseado em convenção, onde as propriedades de um objeto aninhado são mapeadas para um objeto de destino plano (ex: `source.Endereco.Rua` -> `destination.EnderecoRua`).
-   [ ] **Projeção (Projection):** Adicionar suporte para projeção `IQueryable`, o que permitiria ao mapeador gerar consultas de banco de dados eficientes para ORMs como o Entity Framework.
-   [ ] **Perfis (Profiles):** Introduzir perfis para agrupar configurações de mapeamento relacionadas, facilitando o gerenciamento de mapeamentos em aplicações maiores.
-   [ ] **Configuração Global:** Permitir a configuração global de definições como convenções de correspondência de nomes de propriedades.

## Melhorias Futuras

-   [ ] **Benchmarking de Desempenho:** Criar um conjunto abrangente de benchmarks de desempenho para comparar o JMSAutoMapper com outras bibliotecas de mapeamento populares.
-   [ ] **Documentação:** Expandir a documentação com mais exemplos, cenários de uso avançado e uma referência detalhada da API.
-   [ ] **Testes Unitários:** Aumentar a cobertura de testes para garantir que todos os recursos funcionem como esperado e para prevenir regressões.
-   [ ] **Geradores de Código-Source (Source Generators):** Explorar o uso de Geradores de Código-Fonte do C# para gerar o código de mapeamento em tempo de compilação, o que poderia proporcionar um aumento significativo de desempenho.