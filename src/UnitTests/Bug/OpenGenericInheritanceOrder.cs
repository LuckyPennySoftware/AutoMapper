namespace AutoMapper.UnitTests.Bug;

public class OpenGenericInheritanceOrder
{
    class FooBar { }
    class SourceBase<T> { public string Name { get; set; } }
    class DestinationBase { public string Name { get; set; } }
    class SourceDerived : SourceBase<FooBar> { }
    class DestinationDerived : DestinationBase { }

    [Fact]
    public void Should_work_when_derived_map_declared_before_open_generic_base_map()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceDerived, DestinationDerived>()
                .IncludeBase<SourceBase<FooBar>, DestinationBase>();
            cfg.CreateMap(typeof(SourceBase<>), typeof(DestinationBase));
        });

        var mapper = config.CreateMapper();
        var result = mapper.Map<DestinationDerived>(new SourceDerived { Name = "test" });
        result.Name.ShouldBe("test");
    }

    [Fact]
    public void Should_work_when_open_generic_base_map_declared_first()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(SourceBase<>), typeof(DestinationBase));
            cfg.CreateMap<SourceDerived, DestinationDerived>()
                .IncludeBase<SourceBase<FooBar>, DestinationBase>();
        });

        var mapper = config.CreateMapper();
        var result = mapper.Map<DestinationDerived>(new SourceDerived { Name = "test" });
        result.Name.ShouldBe("test");
    }
}
