namespace AutoMapper.UnitTests.Bug;

public class DeepNestingStackOverflow
{
    class Circular { public Circular Self { get; set; } }

    // Verifies that mapping a deeply nested self-referential object does not
    // crash the process with a StackOverflowException (GHSA-rvv3-g6hj-g44x).
    // AutoMapper auto-applies a default MaxDepth of 64 (matching System.Text.Json
    // and Newtonsoft.Json) when it detects a self-referential type mapping.
    [Fact]
    public void Mapping_deeply_nested_self_referential_object_should_not_stackoverflow()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Circular, Circular>());
        var mapper = config.CreateMapper();

        var root = new Circular();
        var current = root;
        for (int i = 0; i < 30_000; i++)
        {
            current.Self = new Circular();
            current = current.Self;
        }

        // Should complete without crashing; mapping is truncated at default MaxDepth (64)
        var result = mapper.Map<Circular>(root);
        result.ShouldNotBeNull();

        int depth = 0;
        current = result;
        while (current.Self != null)
        {
            depth++;
            current = current.Self;
        }
        depth.ShouldBeLessThanOrEqualTo(64);
    }

    // Verifies that configuration validation does not detect the vulnerability —
    // only the runtime mapping is affected, not the configuration itself.
    [Fact]
    public void AssertConfigurationIsValid_does_not_detect_deep_nesting_vulnerability()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Circular, Circular>());
        config.AssertConfigurationIsValid();
    }
}
