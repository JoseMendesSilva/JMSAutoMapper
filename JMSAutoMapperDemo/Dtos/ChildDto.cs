namespace JMSAutoMapper.Tests;

public partial class JMSMapperTest
{
    private class ChildDto
    {
        public int Id { get; set; }
        public ParentDto Parent { get; set; }
    }


}
