namespace AutoMapper.UnitTests.Bug;

public class ByrefConstructorParameter : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public TimeSpan X { get; set; }
    }

    class Destination
    {
        public Destination(in TimeSpan x)
        {
            Y = x;
        }

        public TimeSpan Y { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source
        {
            X = TimeSpan.FromSeconds(17)
        };
        _destination = Mapper.Map<Destination>(source);
    }

    [Fact]
    public void should_just_work()
    {
        _destination.Y.ShouldBe(TimeSpan.FromSeconds(17));
    }
}