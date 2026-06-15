namespace JMSAutoMapper.Tests;

public partial class UnitTests
{
    // Classes de teste
    public class Source
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public Source Parent { get; set; } = default!;
        public List<Source> Children { get; set; } = new List<Source>();
    }

    public class SourceA
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public double Value { get; set; }
    }

    public class SourceB
    {
        public string OriginalProperty { get; set; } = default!;
    }

    public class SourceC
    {
        public int NumericValue { get; set; }
    }

    public class SourceD
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public string IgnoredProperty { get; set; } = default!;
    }

    public class SourceE
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public bool Condition { get; set; }
    }

    public class SourceF
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public NestedSourceF Nested { get; set; } = default!;
    }

    public class NestedSourceF
    {
        public string Value { get; set; } = default!;
    }

    public class PersonSource
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public ChildSource? Child { get; set; }
    }

    public class ChildSource
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public PersonSource? Parent { get; set; }
    }

    public class SourceH
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
    }

    public class SourceWithData
    {
        public int Value1 { get; set; }
        public string Value2 { get; set; } = default!;
    }

    public enum MyEnum
    {
        Value1,
        Value2,
        Value3
    }

    public class SourceEnum
    {
        public string StringValue { get; set; } = default!;
        public int IntValue { get; set; }
        public MyEnum EnumValue { get; set; }
    }
}