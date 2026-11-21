namespace AutoMapper.UnitTests;

public class MaxExecutionPlanDepth : AutoMapperSpecBase
{
    class Source
    {
        public Source1 Inner { get; set; }
    }

    class Source1
    {
        public Source2 Inner { get; set; }
    }

    class Source2
    {
        public Source3 Inner { get; set; }
    }

    class Source3
    {
        public Source4 Inner { get; set; }
    }

    class Source4
    {
        public Source5 Inner { get; set; }
    }

    class Source5
    {
        public Source6 Inner { get; set; }
    }

    class Source6
    {
        public int Value { get; set; }
    }

    class Destination
    {
        public Destination1 Inner { get; set; }
    }

    class Destination1
    {
        public Destination2 Inner { get; set; }
    }

    class Destination2
    {
        public Destination3 Inner { get; set; }
    }

    class Destination3
    {
        public Destination4 Inner { get; set; }
    }

    class Destination4
    {
        public Destination5 Inner { get; set; }
    }

    class Destination5
    {
        public Destination6 Inner { get; set; }
    }

    class Destination6
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.Internal().MaxExecutionPlanDepth = 2;
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<Source1, Destination1>();
        cfg.CreateMap<Source2, Destination2>();
        cfg.CreateMap<Source3, Destination3>();
        cfg.CreateMap<Source4, Destination4>();
        cfg.CreateMap<Source5, Destination5>();
        cfg.CreateMap<Source6, Destination6>();
    });
    [Fact]
    public void Should_set_inline_accordingly()
    {
        TypeMap map;
        map = FindTypeMapFor<Source, Destination>();
        map.PropertyMaps.First().Inline.ShouldBeTrue();
        map = FindTypeMapFor<Source1, Destination1>();
        map.PropertyMaps.First().Inline.ShouldBeTrue();
        map = FindTypeMapFor<Source2, Destination2>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source3, Destination3>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source4, Destination4>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source5, Destination5>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source6, Destination6>();
        map.PropertyMaps.First().Inline.ShouldBeTrue();
    }
}
public class MaxExecutionPlanDepthDefault : AutoMapperSpecBase
{
    class Source
    {
        public Source1 Inner { get; set; }
    }

    class Source1
    {
        public Source2 Inner { get; set; }
    }

    class Source2
    {
        public Source3 Inner { get; set; }
    }

    class Source3
    {
        public Source4 Inner { get; set; }
    }

    class Source4
    {
        public Source5 Inner { get; set; }
    }

    class Source5
    {
        public Source6 Inner { get; set; }
    }

    class Source6
    {
        public int Value { get; set; }
    }

    class Destination
    {
        public Destination1 Inner { get; set; }
    }

    class Destination1
    {
        public Destination2 Inner { get; set; }
    }

    class Destination2
    {
        public Destination3 Inner { get; set; }
    }

    class Destination3
    {
        public Destination4 Inner { get; set; }
    }

    class Destination4
    {
        public Destination5 Inner { get; set; }
    }

    class Destination5
    {
        public Destination6 Inner { get; set; }
    }

    class Destination6
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<Source1, Destination1>();
        cfg.CreateMap<Source2, Destination2>();
        cfg.CreateMap<Source3, Destination3>();
        cfg.CreateMap<Source4, Destination4>();
        cfg.CreateMap<Source5, Destination5>();
        cfg.CreateMap<Source6, Destination6>();
    });
    [Fact]
    public void Should_set_inline_accordingly()
    {
        TypeMap map;
        map = FindTypeMapFor<Source, Destination>();
        map.PropertyMaps.First().Inline.ShouldBeTrue();
        map = FindTypeMapFor<Source1, Destination1>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source2, Destination2>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source3, Destination3>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source4, Destination4>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source5, Destination5>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source6, Destination6>();
        map.PropertyMaps.First().Inline.ShouldBeTrue();
    }
}
public class MaxExecutionPlanDepthWithPreserveReferences : AutoMapperSpecBase
{
    class Source
    {
        public Source1 Inner { get; set; }
    }

    class Source1
    {
        public Source2 Inner { get; set; }
    }

    class Source2
    {
        public Source3 Inner { get; set; }
    }

    class Source3
    {
        public Source4 Inner { get; set; }
    }

    class Source4
    {
        public Source5 Inner { get; set; }
    }

    class Source5
    {
        public Source6 Inner { get; set; }
    }

    class Source6
    {
        public int Value { get; set; }
    }

    class Destination
    {
        public Destination1 Inner { get; set; }
    }

    class Destination1
    {
        public Destination2 Inner { get; set; }
    }

    class Destination2
    {
        public Destination3 Inner { get; set; }
    }

    class Destination3
    {
        public Destination4 Inner { get; set; }
    }

    class Destination4
    {
        public Destination5 Inner { get; set; }
    }

    class Destination5
    {
        public Destination6 Inner { get; set; }
    }

    class Destination6
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.Internal().MaxExecutionPlanDepth = 0;
        cfg.CreateMap<Source, Destination>().PreserveReferences();
        cfg.CreateMap<Source1, Destination1>().PreserveReferences();
        cfg.CreateMap<Source2, Destination2>().PreserveReferences();
        cfg.CreateMap<Source3, Destination3>().PreserveReferences();
        cfg.CreateMap<Source4, Destination4>().PreserveReferences();
        cfg.CreateMap<Source5, Destination5>().PreserveReferences();
        cfg.CreateMap<Source6, Destination6>().PreserveReferences();
    });
   
    [Fact]
    public void Should_set_inline_accordingly()
    {
        TypeMap map;
        map = FindTypeMapFor<Source, Destination>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source1, Destination1>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source2, Destination2>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source3, Destination3>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source4, Destination4>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
        map = FindTypeMapFor<Source5, Destination5>();
        map.PropertyMaps.First().Inline.ShouldBeFalse();
    }
}
public class InlineWithoutPreserveReferences : AutoMapperSpecBase
{
    class Source
    {
        public Source Inner { get; set; }
    }
    class Destination
    {
        public Destination Inner { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void Should_set_inline_accordingly() => FindTypeMapFor<Source, Destination>().PropertyMaps.First().Inline.ShouldBeFalse();
}

public class CycleCheckTypeMapsPathInlining : AutoMapperSpecBase
{
    class OrderSource
    {
        public LineSource Line { get; set; }
    }
    class OrderDestination
    {
        public LineDestination Line { get; set; }
    }
    class LineSource
    {
        public OrderSource Parent { get; set; }
        public ServiceSource Services { get; set; }
    }
    class LineDestination
    {
        public OrderDestination Parent { get; set; }
        public ServiceDestination Services { get; set; }
    }
    class ServiceSource
    {
        public int Id { get; set; }
    }
    class ServiceDestination
    {
        public int Id { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderSource, OrderDestination>();
        cfg.CreateMap<LineSource, LineDestination>();
        cfg.CreateMap<ServiceSource, ServiceDestination>();
    });

    [Fact]
    public void Should_not_inline_services_when_parent_cycle_declared_first()
    {
        var lineMap = FindTypeMapFor<LineSource, LineDestination>();
        lineMap.PropertyMaps.Single(m => m.DestinationMember.Name == nameof(LineDestination.Services)).Inline.ShouldBeFalse();
    }
}

public class CycleCheckTypeMapsPathInliningReversed : AutoMapperSpecBase
{
    class OrderSource
    {
        public LineSource Line { get; set; }
    }
    class OrderDestination
    {
        public LineDestination Line { get; set; }
    }
    class LineSource
    {
        public ServiceSource Services { get; set; }
        public OrderSource Parent { get; set; }
    }
    class LineDestination
    {
        public ServiceDestination Services { get; set; }
        public OrderDestination Parent { get; set; }
    }
    class ServiceSource
    {
        public int Id { get; set; }
    }
    class ServiceDestination
    {
        public int Id { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderSource, OrderDestination>();
        cfg.CreateMap<LineSource, LineDestination>();
        cfg.CreateMap<ServiceSource, ServiceDestination>();
    });

    [Fact]
    public void Should_not_inline_services_when_services_declared_first()
    {
        var lineMap = FindTypeMapFor<LineSource, LineDestination>();
        lineMap.PropertyMaps.Single(m => m.DestinationMember.Name == nameof(LineDestination.Services)).Inline.ShouldBeFalse();
    }
}
