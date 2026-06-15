namespace JMSAutoMapper.ConsoleSample.Dtos;

public class ParentDto
{
    public int Id { get; set; }
    public List<ChildDto> Children { get; set; }
}
