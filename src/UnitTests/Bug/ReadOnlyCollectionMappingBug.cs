namespace AutoMapper.UnitTests.Bug;

// Bug #511
// https://github.com/LuckyPennySoftware/AutoMapper/issues/511
public class ReadOnlyCollectionMappingBug
{
    class Source { public int X { get; set; } }
    class Target { public int X { get; set; } }

    [Fact]
    public void Example()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Target>());

        var source = new List<Source> { new Source { X = 42 } };
        var target = config.CreateMapper().Map<ReadOnlyCollection<Target>>(source);

        target.Count.ShouldBe(source.Count);
        target[0].X.ShouldBe(source[0].X);
    }
}
