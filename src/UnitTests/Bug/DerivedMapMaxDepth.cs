namespace AutoMapper.UnitTests.Bug;

// The objects in a polymorphic graph are mapped with the derived type maps, so those have to be
// depth limited too, both with an explicit MaxDepth on the base map and with the default for
// self-referential maps.
public class DerivedMapMaxDepth
{
    public class Base { public Base Next { get; set; } }
    public class Leaf : Base { public int Value { get; set; } }
    public class BaseDto { public BaseDto Next { get; set; } }
    public class LeafDto : BaseDto { public int Value { get; set; } }

    static Base Chain(int length)
    {
        Base root = new Leaf();
        var current = root;
        for (int i = 0; i < length; i++)
        {
            current.Next = new Leaf();
            current = current.Next;
        }
        return root;
    }

    static int Depth(BaseDto node)
    {
        int depth = 0;
        while (node.Next is { } next)
        {
            node = next;
            depth++;
        }
        return depth;
    }

    [Fact]
    public void Should_inherit_explicit_max_depth_with_include()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Base, BaseDto>().MaxDepth(10).Include<Leaf, LeafDto>();
            cfg.CreateMap<Leaf, LeafDto>();
        }).CreateMapper();
        var result = mapper.Map<BaseDto>(Chain(100));
        result.ShouldBeOfType<LeafDto>();
        Depth(result).ShouldBe(9);
    }

    [Fact]
    public void Should_inherit_explicit_max_depth_with_include_base()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Base, BaseDto>().MaxDepth(10);
            cfg.CreateMap<Leaf, LeafDto>().IncludeBase<Base, BaseDto>();
        }).CreateMapper();
        Depth(mapper.Map<BaseDto>(Chain(100))).ShouldBe(9);
    }

    [Fact]
    public void Should_keep_the_derived_max_depth()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Base, BaseDto>().MaxDepth(10).Include<Leaf, LeafDto>();
            cfg.CreateMap<Leaf, LeafDto>().MaxDepth(5);
        }).CreateMapper();
        Depth(mapper.Map<BaseDto>(Chain(100))).ShouldBe(4);
    }

    [Fact]
    public void Should_apply_default_max_depth_to_derived_maps()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Base, BaseDto>().Include<Leaf, LeafDto>();
            cfg.CreateMap<Leaf, LeafDto>();
        }).CreateMapper();
        Depth(mapper.Map<BaseDto>(Chain(30_000))).ShouldBeLessThanOrEqualTo(64);
    }
}
