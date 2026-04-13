# Sugestões de Melhoria - JMSAutoMapper

## 1. Refatoração Arquitetural
- [ ] **Modularização:** Dividir o arquivo `JMSMapper.cs` em arquivos menores por responsabilidade (Interfaces, Configuration, Core, Extensions).
- [ ] **IncludeBase Completo:** Atualizar `IncludeBase` para herdar expressões customizadas e condições, não apenas mapeamentos diretos.

## 2. Performance
- [ ] **Async Expression Trees:** Migrar a lógica de `MapObjectWithAsyncResolvers` de reflexão pura para delegados compilados via Expression Trees.
- [ ] **Lookup Table de Conversão:** Substituir os `if/else` de `ConvertValue` por um dicionário de funções de conversão `Func<object, object>` baseado em tipos (Source/Target).
- [ ] **Pre-compiled Collection Mappers:** Evitar o uso de `MakeGenericMethod` em tempo de execução dentro do método `Map<T>` para coleções.

## 3. Melhorias de Engine
- [ ] **Reuso de Instância no Contexto:** Ajustar `ResolutionContext` para não instanciar novos mappers via `CreateMapper()`, mas sim reutilizar a engine de execução.
- [ ] **Flattening Automático:** Implementar suporte para mapear `Source.Address.City` para `Destination.AddressCity` automaticamente por convenção de nome.

## 4. Qualidade e Testes
- [ ] **Políticas de Erro:** Adicionar configuração global para definir o comportamento em caso de valores nulos em tipos de valor (ex: `Throw`, `Default`, `Ignore`).
- [ ] **Limpeza de Código:** Remover regiões de código comentado e backups históricos dentro das classes de produção.

## 5. Próximos Passos Sugeridos
1. Iniciar a quebra do arquivo `JMSMapper.cs`.
2. Implementar o BenchmarkDotNet para validar o ganho de performance após a remoção de reflexão do fluxo assíncrono.