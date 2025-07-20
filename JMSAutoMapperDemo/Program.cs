using JMSAutoMapper;
using JMSAutoMapperDemo.Dtos;
using JMSAutoMapperDemo.Models;

// try
// {
    // Console.WriteLine("Hello, World!");
    var mapper = new JMSMapper();
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
        ItensPedido = new List<ItensPedido>
        {
        new ItensPedido { ItensPedidoId = 1, Name = "Item 1", Quantidade = 1 },
        new ItensPedido { ItensPedidoId = 2, Name = "Item 2", Quantidade = 2 }
        }
},
   new SourceClass
{
    SourceClassId = 2,
    Name = "456", // Valor como string
    Age = 24,
    Data = DateTime.Now,
    EnumTeste = eTeste.Inativo,
    ItensPedido = new List<ItensPedido>
    {
        new ItensPedido { ItensPedidoId = 3, Name = "Item 3", Quantidade = 3 },
        new ItensPedido { ItensPedidoId = 4, Name = "Item 4", Quantidade = 4 }
    }
},
   new SourceClass
{
    SourceClassId = 3,
    Name = "456", // Valor como string
    Age = 24,
    Data = DateTime.Now,
    EnumTeste = eTeste.Inativo,
    ItensPedido = new List<ItensPedido>
    {
        new ItensPedido { ItensPedidoId = 5, Name = "Item 3", Quantidade = 5 },
        new ItensPedido { ItensPedidoId = 6, Name = "Item 4", Quantidade = 6 }
    }
}
};

    //// Mapear para o objeto de destino
    var target = mapper.MapList<SourceClassDto>(source); // \ok

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
// }
// catch(Exception ex)
// {
//     Console.WriteLine("");
//     Console.WriteLine(ex.Message);
// }