using AutoMapper.Internal.Mappers;
using System.Globalization;
namespace AutoMapper.UnitTests.Mappers;
using static TypeDescriptor;
public class When_specifying_mapping_with_the_BCL_type_converter_class : NonValidatingSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.Internal().Mappers.Add(new TypeConverterMapper()));
    [TypeConverter(typeof(CustomTypeConverter))]
    public class Source
    {
        public int Value { get; set; }
    }
    public class Destination
    {
        public int OtherValue { get; set; }
    }
    public class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(Destination);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => 
            new Destination { OtherValue = ((Source)value).Value + 10 };
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(Destination);
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) =>
            new Source { Value = ((Destination)value).OtherValue - 10 };
    }
    [Fact]
    public void Should_convert_from_type_using_the_custom_type_converter() => Mapper.Map<Source, Destination>(new Source { Value = 5 }).OtherValue.ShouldBe(15);
    [Fact]
    public void Should_convert_to_type_using_the_custom_type_converter() => Mapper.Map<Destination, Source>(new Destination{ OtherValue = 15 }).Value.ShouldBe(5);
    public class TypeConverterMapper : ObjectMapper<object, object>
    {
        public override bool IsMatch(TypePair context) =>
            GetConverter(context.SourceType).CanConvertTo(context.DestinationType) || GetConverter(context.DestinationType).CanConvertFrom(context.SourceType);
        public override object Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext context)
        {
            var typeConverter = GetConverter(sourceType);
            return typeConverter.CanConvertTo(destinationType) ? typeConverter.ConvertTo(source, destinationType) : GetConverter(destinationType).ConvertFrom(source);
        }
    }
}
public class When_adding_a_custom_mapper : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ClassA, ClassB>()
            .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
        cfg.Internal().Mappers.Add(new TestObjectMapper());
    });

    public class TestObjectMapper : IObjectMapper
    {
        public object Map(ResolutionContext context)
        {
            return new DestinationType();
        }

        public bool IsMatch(TypePair context)
        {
            return context.SourceType == typeof(SourceType) && context.DestinationType == typeof(DestinationType);
        }

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap,
            Expression sourceExpression, Expression destExpression)
        {
            Expression<Func<DestinationType>> expr = () => new DestinationType();

            return expr.Body;
        }
#if NET471_OR_GREATER
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif        
    }

    public class ClassA
    {
        public SourceType Source { get; set; }
    }

    public class ClassB
    {
        public DestinationType Destination { get; set; }
    }

    public class SourceType
    {
        public int Value { get; set; }
    }

    public class DestinationType
    {
        public bool Value { get; set; }
    }
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_adding_a_simple_custom_mapper : AutoMapperSpecBase
{
    ClassB _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ClassA, ClassB>()
            .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
        cfg.Internal().Mappers.Add(new TestObjectMapper());
    });

    protected override void Because_of()
    {
        _destination = new ClassB { Destination = new DestinationType() };
        Mapper.Map(new ClassA { Source = new SourceType() }, _destination);
    }

    [Fact]
    public void Should_use_the_object_mapper()
    {
        _destination.Destination.ShouldBeSameAs(TestObjectMapper.Instance);
    }

    public class TestObjectMapper : ObjectMapper<SourceType, DestinationType>
    {
        public static DestinationType Instance = new DestinationType();

        public override DestinationType Map(SourceType source, DestinationType destination, Type sourceType, Type destinationType, ResolutionContext context)
        {
            source.ShouldNotBeNull();
            destination.ShouldNotBeNull();
            context.ShouldNotBeNull();
            sourceType.ShouldBe(typeof(SourceType));
            destinationType.ShouldBe(typeof(DestinationType));
            return Instance;
        }
    }

    public class ClassA
    {
        public SourceType Source { get; set; }
    }

    public class ClassB
    {
        public DestinationType Destination { get; set; }
    }

    public class SourceType
    {
        public int Value { get; set; }
    }

    public class DestinationType
    {
        public bool Value { get; set; }
    }
}

public class When_adding_an_object_based_custom_mapper : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public ConsoleColor? Color { get; set; }
    }

    class Destination
    {
        public string Color { get; set; }
    }

    class EnumMapper : ObjectMapper<object, string>
    {
        public override bool IsMatch(TypePair types)
        {
            var underlyingType = Nullable.GetUnderlyingType(types.SourceType) ?? types.SourceType;
            return underlyingType.IsEnum && types.DestinationType == typeof(string);
        }

        public override string Map(object source, string destination, Type sourceType, Type destinationType, ResolutionContext context)
        {
            sourceType.ShouldBe(typeof(ConsoleColor?));
            destinationType.ShouldBe(typeof(string));
            return "Test";
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.Internal().Mappers.Insert(0, new EnumMapper());
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Color = ConsoleColor.Black });
    }

    [Fact]
    public void Should_map_with_underlying_type()
    {
        _destination.Color.ShouldBe("Test");
    }
}