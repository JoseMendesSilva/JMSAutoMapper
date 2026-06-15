// dotnet pack --configuration Release --output D:\nupkgs -p:JMSAutoMapper=1.0.17 -p:Authors="José Mendes da Silva" -p:Description="Biblioteca para mapeamento de objeto-objeto"

namespace JMSAutoMapper.Cache
{
    /// <summary>
    /// Atributo para marcar classes que podem ser cacheadas estaticamente.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// Tempo de expiração em minutos (opcional).
        /// </summary>
        public int ExpirationMinutes { get; set; } = 30;
    }

}
