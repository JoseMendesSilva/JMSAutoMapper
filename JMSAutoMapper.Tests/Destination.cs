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

    public class DestinationA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
    }

    public class DestinationB
    {
        public string RenamedProperty { get; set; }
    }

    public class DestinationC
    {
        public string TextValue { get; set; }
    }

    public class DestinationD
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OtherProperty { get; set; }
    }

    public class DestinationE
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConditionalProperty { get; set; }
    }

    public class DestinationF
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public NestedDestinationF Nested { get; set; }
    }

    public class NestedDestinationF
    {
        public string Value { get; set; }
    }

    public class PersonDestination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ChildDestination? Child { get; set; }
    }

    public class ChildDestination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PersonDestination? Parent { get; set; }
    }

    public class DestinationH
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UnmappedProperty { get; set; }
    }

    public class DestinationWithDefaultConstructor
    {
        public int Value1 { get; set; }
        public string Value2 { get; set; }
    }

    public class DestinationWithParameterizedConstructor
    {
        public int Value1 { get; }
        public string Value2 { get; }

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
        public string EnumStringValue { get; set; }
    }
}