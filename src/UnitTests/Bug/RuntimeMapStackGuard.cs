using System.Dynamic;

namespace AutoMapper.UnitTests.Bug;

// Recursion that goes through a runtime Map call (dictionary and dynamic sources, members typed as object
// or an interface, recursive type converters) isn't bounded by the shape of the types, so a deep enough
// source must fail with a catchable exception instead of a StackOverflowException that kills the process.
public class RuntimeMapStackGuard
{
    const int TooDeep = 100_000;

    public class NodeDto { public NodeDto Next { get; set; } }
    public class Node { public Node Next { get; set; } }
    public class ObjectNode { public object Next { get; set; } }
    public interface INode { }
    public class InterfaceNode : INode { public INode Next { get; set; } }

    static void ShouldFailWithInsufficientStack(Action map)
    {
        var exception = Should.Throw<Exception>(map);
        while (exception is AutoMapperMappingException { InnerException: { } inner })
        {
            exception = inner;
        }
        exception.ShouldBeOfType<InsufficientExecutionStackException>();
    }

    // The guard checks the stack of the thread doing the mapping, so run on a small stack, like the default
    // for thread pool threads on Windows, to make sure it trips before the stack is exhausted.
    static void OnSmallStack(Action action)
    {
        Exception error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception e) { error = e; }
        }, 1024 * 1024);
        thread.Start();
        thread.Join();
        if (error != null)
        {
            throw error;
        }
    }

    [Fact]
    public void Should_guard_dictionary_source() => OnSmallStack(() =>
    {
        var mapper = new MapperConfiguration(cfg => { }).CreateMapper();
        var root = new Dictionary<string, object>();
        var current = root;
        for (int i = 0; i < TooDeep; i++)
        {
            var child = new Dictionary<string, object>();
            current["Next"] = child;
            current = child;
        }
        ShouldFailWithInsufficientStack(() => mapper.Map<NodeDto>(root));
    });

    [Fact]
    public void Should_guard_dynamic_source() => OnSmallStack(() =>
    {
        var mapper = new MapperConfiguration(cfg => { }).CreateMapper();
        dynamic root = new ExpandoObject();
        var current = root;
        for (int i = 0; i < TooDeep; i++)
        {
            dynamic child = new ExpandoObject();
            current.Next = child;
            current = child;
        }
        ShouldFailWithInsufficientStack(() => mapper.Map<NodeDto>((object)root));
    });

    [Fact]
    public void Should_guard_object_member() => OnSmallStack(() =>
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<ObjectNode, NodeDto>()).CreateMapper();
        var root = new ObjectNode();
        var current = root;
        for (int i = 0; i < TooDeep; i++)
        {
            var child = new ObjectNode();
            current.Next = child;
            current = child;
        }
        ShouldFailWithInsufficientStack(() => mapper.Map<NodeDto>(root));
    });

    [Fact]
    public void Should_guard_interface_member() => OnSmallStack(() =>
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<InterfaceNode, NodeDto>()).CreateMapper();
        var root = new InterfaceNode();
        var current = root;
        for (int i = 0; i < TooDeep; i++)
        {
            var child = new InterfaceNode();
            current.Next = child;
            current = child;
        }
        ShouldFailWithInsufficientStack(() => mapper.Map<NodeDto>(root));
    });

    [Fact]
    public void Should_guard_recursive_type_converter() => OnSmallStack(() =>
    {
        var mapper = new MapperConfiguration(cfg => cfg.CreateMap<Node, NodeDto>()
            .ConvertUsing((source, _, context) => new NodeDto { Next = source.Next == null ? null : context.Mapper.Map<NodeDto>(source.Next) }))
            .CreateMapper();
        var root = new Node();
        var current = root;
        for (int i = 0; i < TooDeep; i++)
        {
            current.Next = new Node();
            current = current.Next;
        }
        ShouldFailWithInsufficientStack(() => mapper.Map<NodeDto>(root, _ => { }));
    });

    [Fact]
    public void Should_map_shallow_dictionary_source()
    {
        var mapper = new MapperConfiguration(cfg => { }).CreateMapper();
        var source = new Dictionary<string, object> { ["Next"] = new Dictionary<string, object> { ["Next"] = new Dictionary<string, object>() } };
        mapper.Map<NodeDto>(source).Next.Next.ShouldNotBeNull();
    }
}
