namespace JMSAutoMapper.Tests;

public partial class JMSMapperTest
{
    private class Parent
    {
        public int Id { get; set; }
        public List<Child> Children { get; set; }
    }


}
