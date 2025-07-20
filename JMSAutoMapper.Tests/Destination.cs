namespace JMSAutoMapper.Tests;

public partial class UnitTests
{
    public class Destination
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public Destination Parent { get; set; }
        public List<Destination> Children { get; set; } = new List<Destination>();
    }
}