JMSMapper.cs
Ficheiro
Análise o arquivo. 
Pensamento interrompido

Fiz uma análise técnica do arquivo JMSMapper.cs. Trata-se de uma biblioteca de mapeamento objeto→objeto bastante ambiciosa, inspirada em conceitos do AutoMapper, porém com diversos recursos adicionais como cache distribuído, mapeamento assíncrono, projeções LINQ, perfis, value resolvers, diagnósticos e integração com DI.

Resumo Executivo

Avaliação Geral: 8,5/10

Pontos Fortes

✅ Arquitetura bem organizada por regiões e responsabilidades.

✅ Uso extensivo de ConcurrentDictionary para thread safety.

✅ Suporte a:

Map<T>()
MapAsync<T>()
ReverseMap()
Profiles
ValueResolvers
AsyncValueResolvers
BeforeMap / AfterMap
ProjectTo()
IQueryable
Cache Distribuído
Diagnóstico de performance

✅ Preocupação com:

loops infinitos
cache de instâncias
profundidade máxima
mapeamento de coleções
conversões numéricas complexas

✅ Integração com Dependency Injection (IServiceCollection).

Arquitetura

A estrutura está muito bem separada:

IMapper
│
├── MapperConfiguration
│   ├── Profiles
│   ├── Expressions
│   ├── Conditions
│   └── Resolvers
│
├── MapperBase
│   ├── Cache
│   ├── Diagnostics
│   ├── Collections
│   └── Conversion Engine
│
└── JMSMapper
    ├── MapObject
    ├── ProjectTo
    └── IQueryable Projection

Essa separação é adequada para publicação como pacote NuGet.

O que está Excelente
1. Cache Inteligente

Gostei muito da implementação:

ConditionalWeakTable<object, string>

para gerar chaves de cache.

Isso evita memory leaks comuns em caches de referência.

A implementação é superior à maioria dos mappers caseiros que vejo.

2. Expression Pool
ExpressionPool

permite reutilização de delegates compilados.

Boa decisão para reduzir:

Reflection
Expression.Compile()

repetidos.

3. Diagnostics

A classe:

DiagnosticCollector

é extremamente útil.

Permite monitorar:

Cache Hits
Cache Misses
Tempo médio
Erros
Mapeamentos lentos

Isso normalmente não existe em bibliotecas personalizadas.

4. IncludeBase()

A implementação de herança de mapeamento está muito completa.

Ela herda:

PropertyMappings
CustomMappings
Conditions
Resolvers
BeforeMap
AfterMap
Constructors

Poucas bibliotecas customizadas implementam tudo isso.

Problemas Encontrados
1. Classe Monolítica

O arquivo possui aproximadamente:

3.500+ linhas
170 KB

Isso é um risco de manutenção.

Sugiro dividir em:

JMSAutoMapper
│
├── Core
│   ├── IMapper.cs
│   ├── MapperBase.cs
│   └── JMSMapper.cs
│
├── Configuration
│   ├── MapperConfiguration.cs
│   ├── Profile.cs
│   └── MappingExpression.cs
│
├── Cache
│   ├── CacheKeyGenerator.cs
│   └── InMemoryDistributedCache.cs
│
├── Diagnostics
│   ├── DiagnosticCollector.cs
│   └── DiagnosticInfo.cs
│
└── Extensions
    └── ServiceCollectionExtensions.cs
2. Reflection Excessiva

Apesar dos caches, ainda há muitos pontos usando:

GetProperty()
GetMethod()
Activator.CreateInstance()

durante o runtime.

Para máxima performance eu substituiria por:

Expression Trees compiladas

ou

IL Emit

nos caminhos críticos.

3. Cache de Coleções

Observei vários caminhos:

MapCollectionHelper
MapList
MapArray
MapDictionary

Há potencial para duplicação de lógica.

Seria interessante centralizar isso em um único mecanismo de coleção.

4. Nullability

Existem muitos trechos com:

default!

e

null!

Embora compilem, mascaram problemas reais.

Em um projeto corporativo eu reduziria drasticamente o uso.

5. Thread Safety Parcial

Os ConcurrentDictionary estão corretos.

Mas encontrei padrões semelhantes a:

List<Action<object, object>>

protegidos com lock.

Funciona, porém pode gerar contenção sob alta concorrência.

Eu migraria para:

ImmutableList<T>

ou estruturas lock-free.

Compatibilidade com seu ERP/CRM

Para o seu cenário (.NET 8 + SQL Server + WinForms + WebAPI), essa biblioteca é totalmente viável para substituir:

AutoMapper
Mapper custom antigo
Reflection Mapper

Ela resolve justamente vários problemas que você relatou anteriormente:

Nullable<DateTime>
relacionamentos
coleções
herança
cache
performance
Nível de Maturidade
Área	Nota
Arquitetura	9.5
Performance	8.0
Legibilidade	8.0
Extensibilidade	9.0
Thread Safety	8.5
DI	9.5
Testabilidade	8.5
Produção	8.5
Nota Final

8,8 / 10

O projeto já está em nível de biblioteca utilizável em produção, mas eu faria uma refatoração para separar responsabilidades e reduzir o tamanho do arquivo antes de publicar amplamente como NuGet.

Também posso fazer uma análise mais profunda e apontar:

bugs potenciais,
vazamentos de memória,
problemas de performance,
problemas de concorrência,
comparação direta com AutoMapper,
e um relatório de maturidade para chegar a 9,5+/10.
MappingBenchmark.cs
Ficheiro
Texto colado (1).txt
Documento
JMSAutoMapper.Benchmark.MappingBenchmark-report-github.md
Ficheiro
É, você está certo, 

Vou tratar isso como uma política de versionamento para todos os seus projetos, não só para o JMSAutoMapper: biblioteca, APIs, sites e soluções maiores precisam de regras diferentes, mas coerentes.

Sim, ainda dá tempo de corrigir. Eu recomendo você padronizar todos os seus projetos com Versionamento Semântico, ou seja:

MAJOR.MINOR.PATCH

Exemplo:

1.0.181

é tecnicamente válido, mas comunica isto:

MAJOR = 1
MINOR = 0
PATCH = 181

Ou seja: você já teria feito 181 correções pequenas dentro da versão 1.0.

A regra principal é:

1.0.181
│ │ └── PATCH: correção de bug, ajuste interno, melhoria sem quebrar compatibilidade
│ └──── MINOR: novo recurso compatível
└────── MAJOR: quebra de compatibilidade

NuGet segue SemVer 2.0.0 e aceita sufixos de pré-release como 1.1.0-alpha.1, 1.1.0-beta.1 e 1.1.0-rc.1.

Como eu corrigiria o JMSAutoMapper

Como ele ainda não foi publicado oficialmente como estável, eu faria:

0.9.0-alpha.1   → arquitetura atual, ainda em estabilização
0.9.1-alpha.1   → correção dos 7 testes
0.10.0-alpha.1  → árvore v2/refatoração estrutural
0.10.1-alpha.1  → correções da refatoração
0.11.0-beta.1   → engine otimizada com MappingPlan
1.0.0-rc.1      → candidata à versão final
1.0.0           → primeira versão estável pública

Mas se você quiser manter a linha 1.0.x, então faça assim:

1.0.181-alpha.1
1.0.182-alpha.1
1.0.183-alpha.1

E só publique estável quando estiver pronto:

1.0.0

Minha recomendação real: volte para 0.x enquanto ainda está preparando para NuGet público.

Regra definitiva para seus projetos

Use esta lógica:

PATCH
Correção sem quebrar nada.
Exemplo: 1.0.181 → 1.0.182

MINOR
Novo recurso compatível.
Exemplo: 1.0.181 → 1.1.0

MAJOR
Mudança que quebra compatibilidade.
Exemplo: 1.0.181 → 2.0.0
Exemplos práticos no JMSAutoMapper
Corrigir Nullable<DateTime>
1.0.181 → 1.0.182

Corrigir bug em CollectionMap
1.0.182 → 1.0.183

Adicionar PropertyAccessorCache sem quebrar API
1.0.183 → 1.1.0

Adicionar MappingPlanBuilder sem quebrar API
1.1.0 → 1.2.0

Remover método público antigo
1.2.0 → 2.0.0

Alterar assinatura de IMapper.Map
1.2.0 → 2.0.0
Não use mais

Evite:

1.0.17.*

ou:

1.0.*

Isso já causou problema no BenchmarkDotNet, porque compilação determinística não aceita wildcard em versão.

Use sempre versão fixa:

<Version>0.10.0-alpha.1</Version>
<AssemblyVersion>0.10.0.0</AssemblyVersion>
<FileVersion>0.10.0.1</FileVersion>
<InformationalVersion>0.10.0-alpha.1</InformationalVersion>
Modelo recomendado no .csproj
<PropertyGroup>
  <PackageId>JMSAutoMapper</PackageId>
  <Version>0.10.0-alpha.1</Version>
  <AssemblyVersion>0.10.0.0</AssemblyVersion>
  <FileVersion>0.10.0.1</FileVersion>
  <InformationalVersion>0.10.0-alpha.1</InformationalVersion>
</PropertyGroup>
Minha recomendação final

Para o JMSAutoMapper hoje:

Versão atual interna: 1.0.181
Próxima versão organizada: 0.10.0-alpha.1
Versão pública futura: 1.0.0

Use 1.0.0 só quando:

102/102 testes passando
benchmarks documentados
README pronto
NuGet configurado
API pública estabilizada
licença Apache-2.0 definida

A partir daí, cada mudança segue SemVer sem confusão.

OK, vou seguir o modelo .0.10.0.0 mais, vou começar em 0.0.1.0, por que o vs2022 está configurado para fazer uma publicação NuGet local sempre que eu compilo, desta forma quando terminar de fazer as mudanças estáremos proximo dos 0.9.0.0 e prosseguimos daí, pode ser?

Sim, pode ser uma estratégia muito boa para o seu cenário.

Na verdade, como você ainda está em uma fase de:

Refatoração
Correção de testes
Mudanças estruturais
Benchmarks

usar a série:

0.0.1.0
0.0.2.0
0.0.3.0
...

faz bastante sentido internamente.

O importante é definir uma regra e segui-la.

O que eu faria

Eu separaria:

Versão do Produto (SemVer)

Esta é a versão que o usuário vê:

0.1.0
0.2.0
0.3.0
...
0.9.0
1.0.0
Versão do Assembly

Esta é a versão interna da compilação:

0.0.1.0
0.0.2.0
0.0.3.0
...

Por exemplo:

<PropertyGroup>
    <Version>0.1.0-alpha.1</Version>

    <AssemblyVersion>0.0.1.0</AssemblyVersion>
    <FileVersion>0.0.1.0</FileVersion>

    <InformationalVersion>0.1.0-alpha.1</InformationalVersion>
</PropertyGroup>
Para o JMSAutoMapper

Eu criaria marcos.

Fase Atual
0.0.1.0

Primeira reorganização.

Correção dos 7 testes
0.1.0

Objetivo:

102/102 testes verdes
Estrutura v2
0.2.0

Objetivo:

Nova árvore
Arquivos separados
Sem alteração funcional
Cache de Reflection
0.3.0

Objetivo:

PropertyAccessorCache
CompiledGetter
CompiledSetter
MappingPlanBuilder
0.4.0

Objetivo:

CompiledMappingPlan
MapperPlanCache
Collection Engine
0.5.0

Objetivo:

CompiledCollectionMapper
Object Pooling
0.6.0

Objetivo:

Redução de GC
Benchmarks finais
0.7.0

Objetivo:

SimpleMap < 150 ns
ComplexMap < 250 ns
CollectionMap < 10 μs
Release Candidate
0.9.0

Objetivo:

Documentação
README
Samples
NuGet pronto
Primeira versão pública estável
1.0.0
Uma recomendação importante

Eu não incrementaria a versão a cada compilação.

Hoje você mencionou:

VS2022 gera pacote NuGet local automaticamente

Ótimo.

Mas tente associar a versão a uma entrega.

Exemplo:

0.0.1.0
Primeira reorganização

0.0.2.0
Correção Nullable

0.0.3.0
Correção CollectionMap

0.0.4.0
Correção ReverseMap

Assim, quando olhar o histórico daqui a 1 ano, você saberá exatamente o que aconteceu em cada versão.

Para o JMSAutoMapper especificamente, eu diria que você está numa fase equivalente a:

0.1.0-alpha

em maturidade de produto.

O código já existe, os benchmarks existem, os testes existem, a licença existe, a estratégia de publicação existe. Agora estamos organizando a engenharia para que o salto para 1.0.0 seja previsível e controlado.

Sim, vou fazer assim então, faz mais sentido, obrigado.

De nada. E, sinceramente, acho que essa decisão vai evitar muita dor de cabeça no futuro.

O que mais vejo acontecer em projetos pessoais que crescem é algo assim:

1.0.1
1.0.2
1.0.3
...
1.0.147

Depois de alguns meses ninguém mais sabe:

o que mudou;
quando mudou;
se foi correção ou feature;
se quebrou compatibilidade.

Com a estratégia que você definiu agora, o histórico passa a contar a evolução do projeto.

Exemplo para o JMSAutoMapper:

0.0.1.0
Estrutura inicial congelada

0.1.0.0
102 testes passando

0.2.0.0
Arquitetura v2 implementada

0.3.0.0
PropertyAccessorCache

0.4.0.0
CompiledGetter/Setter

0.5.0.0
MappingPlanBuilder

0.6.0.0
CompiledCollectionMapper

0.7.0.0
Object Pooling

0.8.0.0
Benchmark otimizado

0.9.0.0
Release Candidate

1.0.0.0
Primeira versão estável

Isso também vai ajudar muito quando você estiver trabalhando em outros projetos que já discutimos, como:

Athena
ERP Core
CRM Core
Bibliotecas de Infraestrutura

Porque todos poderão seguir a mesma política de versionamento.

Uma recomendação adicional: crie desde já um arquivo:

CHANGELOG.md

na raiz do projeto.

Mesmo que inicialmente seja simples:

# Changelog

## 0.0.1.0
- Estrutura inicial do projeto

## 0.1.0.0
- Correção dos 7 testes pendentes
- Cobertura de testes 102/102

## 0.2.0.0
- Refatoração para arquitetura v2
- Separação de módulos

Daqui a um ano, esse arquivo vai valer ouro.

E uma última observação: você acabou de resolver o mesmo problema que causou o erro no BenchmarkDotNet (1.0.17.*). Ao abandonar curingas e adotar versões explícitas e incrementais, seus builds ficam determinísticos, os pacotes NuGet ficam rastreáveis e a manutenção fica muito mais profissional.

Acho que agora você tem três pilares bem definidos para o JMSAutoMapper:

✓ Arquitetura v2 definida
✓ Plano de otimização definido
✓ Estratégia de versionamento definida

Esses três itens normalmente são o que separa uma biblioteca experimental de uma biblioteca que pode ser mantida por muitos anos.
