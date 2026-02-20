namespace AutoMapper.UnitTests.Construction;

public class ClassBasedObjectConstructorTests
{
    // Test classes
    public class Source
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public int InitialValue { get; set; }
    }

    // Destination factory implementations
    public class CustomConstructor : IDestinationFactory<Source, Destination>
    {
        public Destination Construct(Source source, ResolutionContext context)
        {
            return new Destination { InitialValue = 100 };
        }
    }

    public class SourceAwareConstructor : IDestinationFactory<Source, Destination>
    {
        public Destination Construct(Source source, ResolutionContext context)
        {
            return new Destination { InitialValue = source.Value * 2 };
        }
    }

    [Fact]
    public void When_using_class_based_object_constructor()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConstructUsing<CustomConstructor>();
        });

        var mapper = config.CreateMapper();

        var source = new Source { Value = 10, Name = "Test" };
        var result = mapper.Map<Destination>(source);

        result.InitialValue.ShouldBe(100);
        result.Value.ShouldBe(10);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void When_using_source_aware_constructor()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConstructUsing<SourceAwareConstructor>();
        });

        var mapper = config.CreateMapper();

        var source = new Source { Value = 25, Name = "Test" };
        var result = mapper.Map<Destination>(source);

        result.InitialValue.ShouldBe(50); // 25 * 2
        result.Value.ShouldBe(25);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void When_using_type_based_constructor_non_generic()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Source), typeof(Destination))
                .ConstructUsing(typeof(CustomConstructor));
        });

        var mapper = config.CreateMapper();

        var source = new Source { Value = 10, Name = "Test" };
        var result = (Destination)mapper.Map(source, typeof(Source), typeof(Destination));

        result.InitialValue.ShouldBe(100);
        result.Value.ShouldBe(10);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void When_mixing_constructor_and_member_mapping()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ConstructUsing<SourceAwareConstructor>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.ToUpper()));
        });

        var mapper = config.CreateMapper();

        var source = new Source { Value = 30, Name = "test" };
        var result = mapper.Map<Destination>(source);

        result.InitialValue.ShouldBe(60); // 30 * 2
        result.Value.ShouldBe(30);
        result.Name.ShouldBe("TEST");
    }
}
