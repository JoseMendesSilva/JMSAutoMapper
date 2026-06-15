using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Configuration;
using JMSAutoMapper.ConsoleSample.Dtos;
using JMSAutoMapper.ConsoleSample.Models;
using JMSAutoMapper.Core;

// =============================================
// PROGRAMA PRINCIPAL
// =============================================
Console.WriteLine("🚀 INICIANDO DEMONSTRAÇÃO AVANÇADA DO JMS AUTOMAPPER\n");

try
{
    TestesCompletos.ExecutarTodosTestes();
    TestesCompletos.TesteMapeamentoComplexoProfundidade();
    Console.WriteLine("\n🎉 TODOS OS TESTES FORAM CONCLUÍDOS COM SUCESSO!");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERRO CRÍTICO: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

// ---------------------------------------------------------
// RESOLVER CUSTOMIZADO
// ---------------------------------------------------------
public class ClienteScoreResolver : IValueResolver<Cliente, ClienteComplexoDTO, string>
{
    public string Resolve(Cliente source, ClienteComplexoDTO destination, string destMember, ResolutionContext context)
    {
        var total = source.ContratosAtivos.Sum(c => c.ValorMensal);
        if (total > 10000) return "⭐⭐⭐⭐⭐ (Premium)";
        if (total > 5000) return "⭐⭐⭐ (Standard)";
        return "⭐ (Basic)";
    }
}

// ---------------------------------------------------------
// PERFIL DE MAPEAMENTO COMPLEXO
// ---------------------------------------------------------
public class ClienteAvancadoProfile : Profile
{
    public override void Configure()
    {
        // Mapeamento Base
        CreateMap<EntidadeBase, ClienteComplexoDTO>()
            .ForMember(d => d.Codigo, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.UltimaAtualizacao, opt => opt.MapFrom(s => s.DataAlteracao.ToString("f")));

        // Mapeamento Especializado com IncludeBase e ForMember forçado
        CreateMap<Cliente, ClienteComplexoDTO>()
            .IncludeBase<EntidadeBase, ClienteComplexoDTO>()
            .ForMember(d => d.NomeEmpresa, opt => opt.MapFrom(s => s.RazaoSocial))
            .ForMember(d => d.Contato, opt => opt.MapFrom(s => s.EmailPrincipal))
            // Forçando Deep Mapping e Formatação
            .ForMember(d => d.EnderecoCompleto, opt => opt.MapFrom(s => 
                s.Sede != null ? $"{s.Sede.Logradouro}, {s.Sede.Numero} - {s.Sede.Cidade}/{s.Sede.UF}" : "N/A"))
            .ForMember(d => d.CidadeSede, opt => opt.MapFrom(s => s.Sede.Cidade))
            // Uso de Value Resolver para lógica de negócio
            .ForMember(d => d.ScoreFinanceiro, opt => opt.MapFrom<ClienteScoreResolver>())
            // Mapeamento de Coleção Aninhada (Deep) - Redireciona para o mapeamento recursivo automático
            .ForMember(d => d.ResumoContratos, opt => opt.MapFromSourceMember(nameof(Cliente.ContratosAtivos)))
            // Lógica de Agregação
            .ForMember(d => d.ValorTotalContratos, opt => opt.MapFrom(s => s.ContratosAtivos.Sum(c => c.ValorMensal)))
            // Condicional: Só mapeia metadados específicos
            .AfterMap((src, dest) => {
                if (src.Metadados.ContainsKey("Prioridade"))
                    dest.NomeEmpresa = $"[!] {dest.NomeEmpresa}";
            });

        CreateMap<Contrato, ContratoDTO>()
            .ForMember(d => d.Referencia, opt => opt.MapFrom(s => s.Numero))
            .ForMember(d => d.ValorFormatado, opt => opt.MapFrom(s => s.ValorMensal.ToString("C2")));
    }
}

// ---------------------------------------------------------
// PERFIS EXISTENTES (MANTIDOS E REVISADOS)
// ---------------------------------------------------------
public class UsuarioProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(d => d.Codigo, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => s.Nome))
            .ForMember(d => d.EmailContato, opt => opt.MapFrom(s => s.Email))
            .ForMember(d => d.DataCriacaoFormatada, opt => opt.MapFrom(src => src.DataCriacao.ToString("dd/MM/yyyy HH:mm")))
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Ativo ? "Ativo" : "Inativo"));
    }
}


public class PedidoProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Pedido, PedidoDashboardDTO>()
            .ConstructUsing(src => new PedidoDashboardDTO { Identificador = $"ORD-{src.Numero}" })
            .ForMember("Cliente", "ClienteNome")
            .ForMember("DataFormatada", src => src.Data.ToString("dd/MM/yyyy HH:mm"))
            .ForMember("ValorTotal", src => src.Valor.ToString("C"))
            .ForMember("TotalItens", src => CalcularTotalItens(src.Itens))
            .ForMember("CorStatus", src => GetCorStatus(src.Status))
            .ForMember("IconePrioridade", src => GetIconePrioridade(src.Prioridade))
            .ForMember("EhUrgente", src => src.Prioridade >= 8 && src.Status == "pendente")
            .ForMember("PodeDespachar", src => VerificarPodeDespachar(src))
            .BeforeMap((Pedido src, PedidoDashboardDTO dest) =>
            {
                if (src.Data < DateTime.Now.AddDays(-7))
                    dest.IconePrioridade = "⌛ Antigo";
            })
            .AfterMap((Pedido src, PedidoDashboardDTO dest) =>
            {
                if (dest.EhUrgente)
                    dest.ValorTotal += " ⚠️";
            });
    }

    private static int CalcularTotalItens(List<ItemPedido> itens) => itens?.Sum(i => i.Quantidade) ?? 0;

    private static bool VerificarPodeDespachar(Pedido pedido) =>
        pedido.Status == "aprovado" && pedido.Itens?.Any() == true && pedido.Valor > 0;

    private static string GetCorStatus(string status) =>
        string.IsNullOrEmpty(status) ? "Cinza" : status.ToLower() switch
        {
            "pendente" => "Amarelo",
            "aprovado" => "Verde",
            "cancelado" => "Vermelho",
            "entregue" => "Azul",
            _ => "Cinza"
        };

    private static string GetIconePrioridade(int prioridade) => prioridade switch
    {
        > 5 => "⚠️ Alta",
        > 2 => "🔶 Média",
        _ => "✅ Baixa"
    };
}

public class ProdutoProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Produto, ProdutoDTO>()
            .ForMember("Codigo", "Id")
            .ForMember("NomeProduto", "Nome")
            .ForMember("DescricaoResumida", src => GetDescricaoResumida(src.Descricao))
            .ForMember("PrecoFormatado", src => $"R$ {src.Preco:F2}")
            .ForMember("StatusEstoque", src => GetStatusEstoque(src.Estoque))
            .ForMember("Categoria", src => src.Categoria.Nome ?? "Sem categoria")
            .ForMember("TemEstoque", src => src.Estoque > 0 && src.Disponivel)
            .ConstructUsing(src => new ProdutoDTO { Codigo = src.Id * 1000 });
    }

    private static string GetDescricaoResumida(string descricao)
    {
        if (string.IsNullOrEmpty(descricao)) return string.Empty;
        return descricao.Length > 50 ? descricao.Substring(0, 50) + "..." : descricao;
    }

    private static string GetStatusEstoque(int estoque) => estoque switch
    {
        0 => "Esgotado",
        < 10 => "Estoque Baixo",
        < 50 => "Estoque Normal",
        _ => "Estoque Alto"
    };
}

public class DepartamentoProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Departamento, DepartamentoDTO>()
            .ForMember("NomeDepartamento", "Nome")
            .ForMember("TotalFuncionarios", src => src.Funcionarios != null ? src.Funcionarios.Count : 0)
            .ForMember("FuncionariosAtivos", src => GetFuncionariosAtivos(src.Funcionarios))
            .ForMember("OrcamentoFormatado", src => $"R$ {src.Orcamento:C}")
            .ForMember("NomesFuncionarios", src => GetNomesFuncionarios(src.Funcionarios))
            .ForMember("MediaSalarial", src => CalcularMediaSalarial(src.Funcionarios))
            .AfterMap((Departamento src, DepartamentoDTO dest) =>
            {
                Console.WriteLine($"Departamento {dest.NomeDepartamento} processado com {dest.TotalFuncionarios} funcionários");
            });
    }

    private static int GetFuncionariosAtivos(List<Funcionario> funcionarios) =>
        funcionarios?.Count(f => f.Ativo) ?? 0;

    private static List<string> GetNomesFuncionarios(List<Funcionario> funcionarios) =>
        funcionarios?.Select(f => f.Nome).ToList() ?? new List<string>();

    private static decimal CalcularMediaSalarial(List<Funcionario> funcionarios) =>
        funcionarios?.Any() == true ? funcionarios.Average(f => f.Salario) : 0;
}

public class EmpresaProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Empresa, EmpresaResumoDTO>()
            .ForMember("NomeEmpresa", "RazaoSocial")
            .ForMember("CNPJFormatado", src => FormatCNPJ(src.CNPJ))
            .ForMember("TotalFuncionarios", src => src.Funcionarios != null ? src.Funcionarios.Count : 0)
            .ForMember("Localizacao", src => FormatLocalizacao(src.Endereco))
            .ForMember("FaturamentoFormatado", src => src.FaturamentoAnual.ToString("C2"))
            .ForMember("Departamentos", src => ExtrairDepartamentos(src.Funcionarios))
            .ForMember("MediaSalarial", src => CalcularMediaSalarial(src.Funcionarios));
    }

    private static string FormatCNPJ(string cnpj) =>
        string.IsNullOrEmpty(cnpj) || cnpj.Length != 14
            ? "CNPJ Inválido"
            : $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";

    private static string FormatLocalizacao(Endereco endereco) =>
        endereco != null ? $"{endereco.Cidade} - {endereco.UF}" : "Localização não informada";

    private static List<string> ExtrairDepartamentos(List<Funcionario> funcionarios) =>
        funcionarios?
            .Where(f => f?.Cargo != null)
            .Select(f => f.Cargo)
            .Distinct()
            .ToList() ?? new List<string>();

    private static decimal CalcularMediaSalarial(List<Funcionario> funcionarios) =>
        funcionarios?.Any() == true ? funcionarios.Average(f => f.Salario) : 0;
}

// =============================================
// TESTES COMPLETOS
// =============================================

public class TestesCompletos
{
    public static void ExecutarTodosTestes()
    {
        var config = new MapperConfiguration();
        config.AddProfile<UsuarioProfile>();
        config.AddProfile<PedidoProfile>();
        config.AddProfile<ProdutoProfile>();
        config.AddProfile<DepartamentoProfile>();
        config.AddProfile<EmpresaProfile>();

        var mapper = config.CreateMapper();

        Console.WriteLine("🎯 DEMONSTRAÇÃO COMPLETA DO JMS AUTOMAPPER\n");

        TesteUsuario(mapper);
        TestePedido(mapper);
        TesteProduto(mapper);
        TesteDepartamento(mapper);
        TesteEmpresa(mapper);
        TesteColecoes(mapper);
        TesteValidacao(mapper);
    }

    // ---------------------------------------------------------
    // EXECUÇÃO DO TESTE COMPLEXO
    // ---------------------------------------------------------
    public static void TesteMapeamentoComplexoProfundidade()
    {
        Console.WriteLine("\n=== 🔥 TESTE DE ALTA COMPLEXIDADE E PROFUNDIDADE ===");

        var config = new MapperConfiguration();
        config.AddProfile<ClienteAvancadoProfile>();
        var mapper = config.CreateMapper();

        // Dados de Origem Complexos
        var cliente = new ClienteVip
        {
            Id = 5050,
            RazaoSocial = "Global Logistics Solutions S.A.",
            EmailPrincipal = "ops@globallog.com",
            DataAlteracao = DateTime.Now.AddHours(-2),
            BonusFidelidade = 500.00m,
            Metadados = new Dictionary<string, string> { { "Prioridade", "Alta" } },
            Sede = new Endereco 
            { 
                Logradouro = "Av. Paulista", 
                Numero = "1000", 
                Cidade = "São Paulo", 
                UF = "SP" 
            },
            ContratosAtivos = new List<Contrato>
            {
                new Contrato { Id = 1, Numero = "CNT-2024-A", ValorMensal = 4500.50m },
                new Contrato { Id = 2, Numero = "CNT-2024-B", ValorMensal = 8200.00m }
            }
        };

        // Execução do Mapeamento
        var dto = mapper.Map<ClienteComplexoDTO>(cliente);

        // Verificação de Resultados
        Console.WriteLine($"DÉ-PARA (Achatamento): {cliente.Sede.Cidade} -> {dto.CidadeSede}");
        Console.WriteLine($"DÉ-PARA (Custom ForMember): {dto.EnderecoCompleto}");
        Console.WriteLine($"DÉ-PARA (Value Resolver): {dto.ScoreFinanceiro}");
        Console.WriteLine($"DÉ-PARA (Deep List): {dto.ResumoContratos.Count} contratos mapeados.");
        Console.WriteLine($"DÉ-PARA (IncludeBase/Inheritance): Codigo {dto.Codigo} extraído de EntidadeBase.");
        Console.WriteLine($"DÉ-PARA (Aggregation): Valor Total: {dto.ValorTotalContratos:C2}");
        
        foreach(var c in dto.ResumoContratos)
            Console.WriteLine($"   > Contrato: {c.Referencia} | Valor: {c.ValorFormatado}");
    }

    static void TesteUsuario(IMapper mapper)
    {
        Console.WriteLine("=== 🧍 CONFIGURAÇÕES DE USUÁRIO ===");

        var usuario = new Usuario
        {
            Id = 1,
            Nome = "Maria Santos",
            Email = "maria@email.com",
            DataCriacao = new DateTime(2024, 1, 15, 10, 30, 0),
            Ativo = true
        };

        var dto = mapper.Map<UsuarioDTO>(usuario);

        Console.WriteLine($"ID: {dto.Codigo}");
        Console.WriteLine($"Nome: {dto.NomeCompleto}");
        Console.WriteLine($"Email: {dto.EmailContato}");
        Console.WriteLine($"Data: {dto.DataCriacaoFormatada}");
        Console.WriteLine($"Status: {dto.Status}");
    }

    static void TestePedido(IMapper mapper)
    {
        Console.WriteLine("\n=== 🚚 CONFIGURAÇÕES DE PEDIDO ===");

        var pedido = new Pedido
        {
            Id = 100,
            Numero = "2024-001",
            Data = DateTime.Now,
            ClienteNome = "Cliente Importante",
            Valor = 1500.00m,
            Status = "pendente",
            Prioridade = 9,
            Itens = new List<ItemPedido>
                {
                    new ItemPedido { Produto = "Produto A", Quantidade = 2, Preco = 500 },
                    new ItemPedido { Produto = "Produto B", Quantidade = 1, Preco = 500 }
                }
        };

        var dto = mapper.Map<PedidoDashboardDTO>(pedido);

        Console.WriteLine($"Identificador: {dto.Identificador}");
        Console.WriteLine($"Cliente: {dto.Cliente}");
        Console.WriteLine($"Valor: {dto.ValorTotal}");
        Console.WriteLine($"Status: {dto.CorStatus}");
        Console.WriteLine($"Prioridade: {dto.IconePrioridade}");
        Console.WriteLine($"Itens: {dto.TotalItens}");
        Console.WriteLine($"Urgente: {dto.EhUrgente}");
        Console.WriteLine($"Pode Despachar: {dto.PodeDespachar}");
    }

    static void TesteProduto(IMapper mapper)
    {
        Console.WriteLine("\n=== 📦 CONFIGURAÇÕES DE PRODUTO ===");

        var produto = new Produto
        {
            Id = 10,
            Nome = "Smartphone Galaxy S24",
            Descricao = "Smartphone flagship com câmera de 200MP, 512GB de armazenamento e 12GB de RAM",
            Preco = 4999.99m,
            Estoque = 5,
            DataCadastro = new DateTime(2024, 1, 1),
            Disponivel = true,
            Categoria = new CategoriaProduto { Id = 1, Nome = "Eletrônicos" }
        };

        var dto = mapper.Map<ProdutoDTO>(produto);

        Console.WriteLine($"Código: {dto.Codigo}");
        Console.WriteLine($"Nome: {dto.NomeProduto}");
        Console.WriteLine($"Descrição: {dto.DescricaoResumida}");
        Console.WriteLine($"Preço: {dto.PrecoFormatado}");
        Console.WriteLine($"Status: {dto.StatusEstoque}");
        Console.WriteLine($"Categoria: {dto.Categoria}");
        Console.WriteLine($"Tem estoque: {dto.TemEstoque}");
    }

    static void TesteDepartamento(IMapper mapper)
    {
        Console.WriteLine("\n=== 🏢 CONFIGURAÇÕES DE DEPARTAMENTO ===");

        var departamento = new Departamento
        {
            Id = 1,
            Nome = "Tecnologia da Informação",
            Sigla = "TI",
            Orcamento = 500000.00m,
            DataCriacao = new DateTime(2020, 5, 1),
            Funcionarios = new List<Funcionario>
                {
                    new Funcionario { Id = 1, Nome = "Carlos Souza", Cargo = "Desenvolvedor", Salario = 8000.00m, DataAdmissao = new DateTime(2021, 3, 15), Ativo = true },
                    new Funcionario { Id = 2, Nome = "Ana Lima", Cargo = "Arquiteta", Salario = 12000.00m, DataAdmissao = new DateTime(2020, 6, 1), Ativo = true },
                    new Funcionario { Id = 3, Nome = "Pedro Costa", Cargo = "Analista", Salario = 6000.00m, DataAdmissao = new DateTime(2023, 1, 10), Ativo = false }
                }
        };

        var dto = mapper.Map<DepartamentoDTO>(departamento);

        Console.WriteLine($"Departamento: {dto.NomeDepartamento} ({dto.Sigla})");
        Console.WriteLine($"Orçamento: {dto.OrcamentoFormatado}");
        Console.WriteLine($"Total Funcionários: {dto.TotalFuncionarios}");
        Console.WriteLine($"Funcionários Ativos: {dto.FuncionariosAtivos}");
        Console.WriteLine($"Média Salarial: {dto.MediaSalarial:C}");
        Console.WriteLine($"Funcionários: {string.Join(", ", dto.NomesFuncionarios)}");
    }

    static void TesteEmpresa(IMapper mapper)
    {
        Console.WriteLine("\n=== 🏭 CONFIGURAÇÕES DE EMPRESA ===");

        var empresa = new Empresa
        {
            Id = 1,
            RazaoSocial = "Tech Solutions LTDA",
            CNPJ = "12345678000195",
            FaturamentoAnual = 2500000.00m,
            Endereco = new Endereco { Cidade = "São Paulo", UF = "SP" },
            Funcionarios = new List<Funcionario>
                {
                    new Funcionario { Nome = "João", Cargo = "Desenvolvedor", Salario = 5000 },
                    new Funcionario { Nome = "Ana", Cargo = "Designer", Salario = 7000 },
                    new Funcionario { Nome = "Carlos", Cargo = "Vendedor", Salario = 6000 }
                }
        };

        var dto = mapper.Map<EmpresaResumoDTO>(empresa);

        Console.WriteLine($"Empresa: {dto.NomeEmpresa}");
        Console.WriteLine($"CNPJ: {dto.CNPJFormatado}");
        Console.WriteLine($"Funcionários: {dto.TotalFuncionarios}");
        Console.WriteLine($"Localização: {dto.Localizacao}");
        Console.WriteLine($"Faturamento: {dto.FaturamentoFormatado}");
        Console.WriteLine($"Média Salarial: {dto.MediaSalarial:C}");
        Console.WriteLine($"Cargos: {string.Join(", ", dto.Departamentos)}");
    }

    static void TesteColecoes(IMapper mapper)
    {
        Console.WriteLine("\n=== 📚 COLEÇÕES E MAPEAMENTO EM MASSA ===");

        var usuarios = new List<Usuario>
            {
                new Usuario { Id = 1, Nome = "A", Email = "a@email.com", DataCriacao = DateTime.Now, Ativo = true },
                new Usuario { Id = 2, Nome = "B", Email = "b@email.com", DataCriacao = DateTime.Now, Ativo = true },
                new Usuario { Id = 3, Nome = "C", Email = "c@email.com", DataCriacao = DateTime.Now, Ativo = false }
            };

        // Teste todos os tipos de coleção
        var lista = mapper.Map<List<UsuarioDTO>>(usuarios);
        var array = mapper.Map<UsuarioDTO[]>(usuarios);
        var enumerable = mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
        var hashSet = mapper.Map<HashSet<UsuarioDTO>>(usuarios);

        Console.WriteLine($"List: {lista.Count} itens");
        Console.WriteLine($"Array: {array.Length} itens");
        Console.WriteLine($"Enumerable: {enumerable.Count()} itens");
        Console.WriteLine($"HashSet: {hashSet.Count} itens");
    }

    static void TesteValidacao(IMapper mapper)
    {
        Console.WriteLine("\n=== ✅ VALIDAÇÃO DE CONFIGURAÇÃO ===");

        try
        {
            mapper.AssertConfigurationIsValid();
            Console.WriteLine("✓ Configuração válida - todos os mapeamentos estão corretos!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Erro na validação: {ex.Message}");
        }
    }
}
