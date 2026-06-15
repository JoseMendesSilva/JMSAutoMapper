namespace JMSAutoMapper.Tests;

public partial class UnitTests
{
    public class Destination
    {
        public int Id { get; set; }
        public string? FullRazaoSocial { get; set; }
        public int Age { get; set; }
        public Destination Parent { get; set; } = default!;
        public List<Destination> Children { get; set; } = new List<Destination>();
    }

    public class DestinationA
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public double Value { get; set; }
    }

    public class DestinationB
    {
        public string ReRazaoSocialdProperty { get; set; } = default!;
    }

    public class DestinationC
    {
        public string TextValue { get; set; } = default!;
    }

    public class DestinationD
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public string OtherProperty { get; set; } = default!;
    }

    public class DestinationE
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public string ConditionalProperty { get; set; } = default!;
    }

    public class DestinationF
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public NestedDestinationF Nested { get; set; } = default!;
    }

    public class NestedDestinationF
    {
        public string Value { get; set; } = default!;
    }

    public class PersonDestination
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public ChildDestination? Child { get; set; }
    }

    public class ChildDestination
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public PersonDestination? Parent { get; set; }
    }

    public class DestinationH
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = default!;
        public string UnmappedProperty { get; set; } = default!;
    }

    public class DestinationWithDefaultConstructor
    {
        public int Value1 { get; set; }
        public string Value2 { get; set; } = default!;
    }

    public class DestinationWithParameterizedConstructor
    {
        public int Value1 { get; }
        public string Value2 { get; } = default!;

        public DestinationWithParameterizedConstructor(int value1, string value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
    }

    public class DestinationEnum
    {
        public MyEnum StringValue { get; set; }
        public MyEnum IntValue { get; set; }
        public MyEnum EnumValue { get; set; }
        public string EnumStringValue { get; set; } = string.Empty;
    }
}