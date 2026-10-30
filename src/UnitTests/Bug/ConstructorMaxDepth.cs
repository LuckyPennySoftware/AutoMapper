namespace AutoMapper.UnitTests.Bug;

public class ConstructorMaxDepth
{
    public class CtorNode
    {
        public CtorNode(CtorNode next) => Next = next;
        public CtorNode Next { get; set; }
    }
    public class Node { public Node Next { get; set; } public int Id { get; set; } }
    public record NodeRecord(NodeRecord Next, int Id);
    public class NodeDto { public NodeDto Next { get; set; } public int Id { get; set; } }

    static CtorNode CtorChain(int length)
    {
        var root = new CtorNode(null);
        var current = root;
        for (int i = 0; i < length; i++)
        {
            current.Next = new CtorNode(null);
            current = current.Next;
        }
        return root;
    }

    static Node Chain(int length)
    {
        var root = new Node();
        var current = root;
        for (int i = 0; i < length; i++)
        {
            current.Next = new Node { Id = i + 1 };
            current = current.Next;
        }
        return root;
    }

    static int Depth<T>(T node, Func<T, T> next) where T : class
    {
        int depth = 0;
        while (next(node) is { } child)
        {
            node = child;
            depth++;
        }
        return depth;
    }

    // Constructor parameters are mapped while the destination is created, so the type depth
    // has to be counted before that, or recursion through a constructor parameter never
    // reaches MaxDepth.
    [Fact]
    public void Should_honor_explicit_max_depth_through_constructor_parameters()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<CtorNode, CtorNode>().MaxDepth(10)).CreateMapper();
        var result = mapper.Map<CtorNode>(CtorChain(100));
        Depth(result, n => n.Next).ShouldBe(9);
    }

    [Fact]
    public void Should_honor_max_depth_through_positional_record_parameters()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Node, NodeRecord>().MaxDepth(10)).CreateMapper();
        var result = mapper.Map<NodeRecord>(Chain(100));
        Depth(result, n => n.Next).ShouldBe(9);
    }

    [Fact]
    public void Should_match_property_mapping_depth()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CtorNode, CtorNode>().MaxDepth(10);
            cfg.CreateMap<Node, NodeDto>().MaxDepth(10);
        }).CreateMapper();
        Depth(mapper.Map<CtorNode>(CtorChain(100)), n => n.Next)
            .ShouldBe(Depth(mapper.Map<NodeDto>(Chain(100)), n => n.Next));
    }

    [Fact]
    public void Should_apply_default_max_depth_through_constructor_parameters()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<CtorNode, CtorNode>()).CreateMapper();
        var result = mapper.Map<CtorNode>(CtorChain(30_000));
        Depth(result, n => n.Next).ShouldBeLessThanOrEqualTo(64);
    }

    [Fact]
    public void Should_honor_max_depth_when_construct_using_maps_recursively()
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Node, NodeRecord>().MaxDepth(10)
            .ConstructUsing((src, ctx) => new NodeRecord(src.Next == null ? null : ctx.Mapper.Map<NodeRecord>(src.Next), src.Id))
            .ForAllMembers(o => o.Ignore()))
            .CreateMapper();
        var result = mapper.Map<NodeRecord>(Chain(100), o => { });
        Depth(result, n => n.Next).ShouldBe(9);
    }
}
