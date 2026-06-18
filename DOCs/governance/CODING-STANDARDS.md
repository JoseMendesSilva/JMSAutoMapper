# JMSAutoMapper — CODING-STANDARDS.md

## Objetivo

Definir padrões de código para manter o JMSAutoMapper consistente, legível e seguro.

## Regras gerais

- Usar C# moderno, mas sem prejudicar compatibilidade planejada.
- Manter nomes claros e explícitos.
- Evitar abreviações obscuras.
- Evitar código mágico sem comentários.
- Evitar duplicação de lógica.
- Não misturar responsabilidades.

## Caminhos críticos

Em código de performance:

- Evitar LINQ em loops críticos.
- Evitar reflection repetitiva.
- Evitar alocações desnecessárias.
- Evitar closures.
- Preferir delegates compilados.
- Preferir caches bem definidos.

## API pública

- Não alterar assinaturas públicas sem aprovação.
- Não remover métodos públicos existentes.
- Não trocar nomes públicos por estética.
- Manter compatibilidade até a versão 1.0.0.0.

## Erros

Mensagens de erro devem conter, quando possível, tipo origem, tipo destino, nome da propriedade, tipo esperado e valor recebido, se seguro.

## Nullability

- Respeitar nullable reference types quando habilitado.
- Evitar `null!` sem justificativa.
- Manter política clara para null em value types.

## Organização

Cada classe deve ficar no arquivo correspondente.

Evitar classes gigantes e arquivos que misturam configuração, execução, validação, diagnóstico e benchmark.
