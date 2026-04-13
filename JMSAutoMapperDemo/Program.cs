using JMSAutoMapper;
using JMSAutoMapperDemo.Dtos;
using JMSAutoMapperDemo.Models;

// try
// {
// Console.WriteLine("Hello, World!");
var config = new MapperConfiguration();
config.CreateMap<SourceClass, SourceClassDto>();
var mapper = new JMSMapper(config);

// Criar objeto de origem
var source = new List<SourceClass>
{
   new SourceClass
    {
        SourceClassId = 1,
        Name = "123", // Valor como string
        Age = 30,
        Data = DateTime.Now,
        EnumTeste = eTeste.Ativo,
        ItensPedido = new List<ItemPedido>
        {
        new ItemPedido { ItemPedidoId = 1, Name = "Item 1", Quantidade = 1 },
        new ItemPedido { ItemPedidoId = 2, Name = "Item 2", Quantidade = 2 }
        }
},
   new SourceClass
{
    SourceClassId = 2,
    Name = "456", // Valor como string
    Age = 24,
    Data = DateTime.Now,
    EnumTeste = eTeste.Inativo,
    ItensPedido = new List<ItemPedido>
    {
        new ItemPedido { ItemPedidoId = 3, Name = "Item 3", Quantidade = 3 },
        new ItemPedido { ItemPedidoId = 4, Name = "Item 4", Quantidade = 4 }
    }
},
   new SourceClass
{
    SourceClassId = 3,
    Name = "456", // Valor como string
    Age = 24,
    Data = DateTime.Now,
    EnumTeste = eTeste.Inativo,
    ItensPedido = new List<ItemPedido>
    {
        new ItemPedido { ItemPedidoId = 5, Name = "Item 3", Quantidade = 5 },
        new ItemPedido { ItemPedidoId = 6, Name = "Item 4", Quantidade = 6 }
    }
}
};

//// Mapear para o objeto de destino
var target = mapper.Map<List<SourceClassDto>>(source); // \ok

// Mapear para o objeto de destino
//var target = mapper.MapIEnumerable<SourceClassDto>(source); //

// Exibir resultados
foreach (var item in target!)
{
    Console.WriteLine($"ID: {item.SourceClassId}, Name: {item.Name}, Data: {item.Data}, Age: {item.Age}, EnumTeste: {item.EnumTeste}");
    foreach (var ped in item.ItensPedido)
    {
        Console.WriteLine($"ID: {ped!.ItensPedidoId}, Name: {ped!.Name}, Quantidade: {ped.Quantidade}");
    }
    Console.WriteLine();
}



// =============================================
// PROGRAMA PRINCIPAL
// =============================================
Console.WriteLine("🚀 INICIANDO TESTES DO JMS AUTOMAPPER\n");

try
{
    TestesCompletos.ExecutarTodosTestes();
    Console.WriteLine("\n🎉 TODOS OS TESTES FORAM CONCLUÍDOS COM SUCESSO!");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERRO DURANTE OS TESTES: {ex.Message}");
    Console.WriteLine($"Detalhes: {ex}");
}

public class UsuarioProfile : Profile
{
    public override void Configure()
    {
        CreateMap<Usuario, UsuarioDTO>()
            .ForMember("Codigo", "Id")
            .ForMember("NomeCompleto", "Nome")
            .ForMember("EmailContato", "Email")
            .ForMember("DataCriacaoFormatada", src => src.DataCriacao.ToString("dd/MM/yyyy HH:mm"))
            .ForMember("Status", src => src.Ativo ? "Ativo" : "Inativo")
            .BeforeMap((Usuario src, UsuarioDTO dest) =>
                Console.WriteLine($"Mapeando usuário: {src.Nome}"))
            .AfterMap((Usuario src, UsuarioDTO dest) =>
                Console.WriteLine($"Usuário mapeado: {dest.NomeCompleto}"));
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
        var array = mapper.MapArray<UsuarioDTO>(usuarios);
        var enumerable = mapper.MapIEnumerable<UsuarioDTO>(usuarios);
        var hashSet = mapper.MapHashSet<UsuarioDTO>(usuarios);

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

