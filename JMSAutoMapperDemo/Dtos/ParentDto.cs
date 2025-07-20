namespace JMSAutoMapper.Tests;

public partial class JMSMapperTest
{
    private class ParentDto
    {
        public int Id { get; set; }
        public List<ChildDto> Children { get; set; }
    }


}
