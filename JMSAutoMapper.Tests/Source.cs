namespace JMSAutoMapper.Tests;

public partial class UnitTests
{
    // Classes de teste
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public Source Parent { get; set; }
        public List<Source> Children { get; set; } = new List<Source>();
    }
}