#if false
// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

namespace JMSAutoMapper.Configuration
{
    /// <summary>
    /// Define a estratégia global para quando um valor de origem nulo é mapeado 
    /// para um tipo de valor não anulável (int, decimal, etc) no destino.
    /// </summary>
    public enum NullValueMappingPolicy { Throw, Default, Ignore }

    

}
#endif
