using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper.UnitTests;
public class SeparateConfiguration : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }
    public class Dest
    {
        public int Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration()
    {
        var expr = new MapperConfigurationExpression();

        expr.CreateMap<Source, Dest>();

        return new MapperConfiguration(expr, new NullLoggerFactory());
    }

    [Fact]
    public void Should_use_passed_in_configuration()
    {
        var source = new Source {Value = 5};
        var dest = Mapper.Map<Source, Dest>(source);

        dest.Value.ShouldBe(source.Value);
    }
}