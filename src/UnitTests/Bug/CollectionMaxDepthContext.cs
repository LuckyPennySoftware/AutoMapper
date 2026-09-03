namespace AutoMapper.UnitTests.Bug;

public class CollectionMaxDepthContext
{
    public class Service { public string Description { get; set; } }
    public class ServiceDto { public string Description { get; set; } }

    // Self-referential, so CheckForCycles auto-enables PreserveReferences and MaxDepth
    public class RecursiveLine { public RecursiveLine Parent { get; set; } public List<Service> Services { get; set; } }
    public class RecursiveLineDto { public RecursiveLineDto Parent { get; set; } public IList<ServiceDto> Services { get; set; } }

    // Plain type mapping the same List<Service> -> IList<ServiceDto> pair
    public class PlainLine { public List<Service> Services { get; set; } }
    public class PlainLineDto { public IList<ServiceDto> Services { get; set; } }

    static IMapper CreateMapper() => new MapperConfiguration(cfg =>
    {
        cfg.CreateMap<Service, ServiceDto>();
        cfg.CreateMap<RecursiveLine, RecursiveLineDto>().ForMember(d => d.Services, o => o.MapAtRuntime());
        cfg.CreateMap<PlainLine, PlainLineDto>().ForMember(d => d.Services, o => o.MapAtRuntime());
    }).CreateMapper();

    static RecursiveLine NewRecursiveLine() => new() { Services = [new Service { Description = "recursive" }] };
    static PlainLine NewPlainLine() => new() { Services = [new Service { Description = "plain" }] };

    // The collection execution plan is cached per type pair, ignoring the member map it was built
    // for. When the recursive line compiles it first, the plan carries a max depth check for the
    // containing type map, which needs a non default context in every other caller too.
    [Fact]
    public void Should_map_the_plain_line_after_the_recursive_line()
    {
        var mapper = CreateMapper();
        mapper.Map<RecursiveLineDto>(NewRecursiveLine()).Services.Count.ShouldBe(1);
        mapper.Map<PlainLineDto>(NewPlainLine()).Services.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_map_the_recursive_line_after_the_plain_line()
    {
        var mapper = CreateMapper();
        mapper.Map<PlainLineDto>(NewPlainLine()).Services.Count.ShouldBe(1);
        mapper.Map<RecursiveLineDto>(NewRecursiveLine()).Services.Count.ShouldBe(1);
    }
}
