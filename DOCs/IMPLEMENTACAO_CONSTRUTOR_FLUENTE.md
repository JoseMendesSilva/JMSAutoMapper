# Guia de Implementação: Construtor Fluente para MapperConfiguration

Este documento descreve como adicionar o suporte para inicialização via expressão lambda na classe `MapperConfiguration`, permitindo uma sintaxe mais limpa e próxima de outras bibliotecas populares.

## Passo 1: Localizar a Classe
Abra o arquivo `JMSMapper.cs` localizado em `d:\Projetos\10-04-2025\Prj03\Auto Mapper\JMSAutoMapper\`.

## Passo 2: Adicionar os Construtores
Para suportar a nova sintaxe sem quebrar o código existente (que depende do construtor sem parâmetros), precisamos adicionar dois construtores explicitamente dentro da classe `MapperConfiguration`:

1.  **Construtor Padrão**: Mantém a compatibilidade com instanciamento simples `new MapperConfiguration()`.
2.  **Construtor com Action**: Recebe a configuração via parâmetro e a executa imediatamente.

### Código a ser inserido:
```csharp
/// <summary>Inicializa uma nova instância de MapperConfiguration.</summary>
public MapperConfiguration() { }

/// <summary>Inicializa uma nova instância de MapperConfiguration com uma ação de configuração.</summary>
/// <param name="configure">Ação para configurar o mapeador.</param>
public MapperConfiguration(Action<MapperConfiguration> configure)
{
    configure(this);
}
```

## Passo 3: Exemplo de Uso
Após a alteração, você poderá utilizar a biblioteca desta forma:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MyProfile>();
    cfg.CreateMap<Source, Dest>();
});
```