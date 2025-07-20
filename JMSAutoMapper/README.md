JMSAutoMapper - Extensões Avançadas para AutoMapper
.NET Version
xUnit Tests

O JMSAutoMapper é uma biblioteca de extensões para o AutoMapper que adiciona suporte a:
Coleções padrão e customizadas
Coleções thread-safe
Coleções imutáveis
Operações assíncronas
Integração com MemoryCache

📦 Instalação
Adicione via NuGet:
bash
dotnet add package JMSAutoMapper
🚀 Como Usar
Configuração Básica
csharp
// Inicialização
var config = new MapperConfiguration(cfg => 
{
    cfg.CreateMap<ClienteOrigem, ClienteDestino>();
});

IMapper mapper = new JMSAutoMapper(config);
📌 Mapeamento de Coleções
Coleções Padrão
csharp
// List<T>
var lista = mapper.Map<List<ClienteDestino>>(clientesOrigem);

// Dictionary<TKey, TValue>
var dicionario = mapper.Map<Dictionary<int, ClienteDestino>>(dictOrigem);
Coleções Thread-Safe

// ConcurrentDictionary
var concurrentDict = mapper.Map<ConcurrentDictionary<int, Pedido>>(dictOrigem);

// BlockingCollection
var blockingColl = mapper.Map<BlockingCollection<LogEntry>>(logsOrigem, boundedCapacity: 100);
🧊 Coleções Imutáveis

// ImmutableList
var imutavel = mapper.Map<ImmutableList<Cliente>>(clientesOrigem);

// ImmutableQueue (FIFO)
var filaImutavel = mapper.Map<ImmutableQueue<Pedido>>(pedidosOrigem);
⚡ Operações Assíncronas

// Mapeamento assíncrono individual
var cliente = await mapper.MapAsync<Cliente>(clienteOrigem);

// Mapeamento assíncrono de coleção (com paralelismo)
var produtos = await mapper.MapAsync<IEnumerable<Produto>>(listaProdutos, maxDegreeOfParallelism: 4);
🗄️ MemoryCache Integration

var cacheItem = mapper.MapToCacheItem<Cliente>(
    clienteOrigem, 
    key: "cliente_123",
    policy: new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(30) }
);

MemoryCache.Default.Add(cacheItem);
🔍 Testes
A biblioteca inclui testes abrangentes para todas as funcionalidades:

bash
dotnet test
Cobertura de testes:
100% para coleções padrão
95% para cenários assíncronos
90% para edge cases com null

📊 Comparação com AutoMapper Padrão
Feature	AutoMapper	JMSAutoMapper
Suporte a List<T>	✅	✅
ConcurrentDictionary	❌	✅
Immutable Collections	❌	✅
MapAsync	❌	✅
MemoryCache Support	❌	✅
💡 Melhores Práticas
Para coleções grandes (> 10k itens):

// Usar paralelismo controlado
await mapper.MapAsync<IEnumerable<Produto>>(bigList, maxDegreeOfParallelism: 8);
Para cenários multi-thread:

// Preferir coleções thread-safe
var concurrentData = mapper.Map<ConcurrentBag<DataPoint>>(data);
Cache de objetos mapeados:

// Reutilizar em chamadas frequentes
var cached = mapper.MapToCacheItem(obj, "key", policy);
📚 Dependências
AutoMapper (>= 12.0.0)
System.Collections.Immutable (>= 7.0.0)
System.Runtime.Caching (>= 7.0.0)

🤝 Contribuição
Faça fork do projeto
Crie sua branch (git checkout -b feature/awesome-feature)
Commit suas mudanças (git commit -m 'Add awesome feature')
Push para a branch (git push origin feature/awesome-feature)
Abra um Pull Request

📄 Licença
MIT License - Copyright (c) 2023 JMSAutoMapper Contributors
