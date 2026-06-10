namespace AutoMapper.UnitTests.Bug;

public class MapAtRuntimeWithExtensionMethodAndNullable
{
    class SourceObject { public string MyValue { get; set; } }
    class DestObject { public int? MyValue { get; set; } }

    [Fact]
    public void Should_map_extension_method_result_to_nullable_destination()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceObject, DestObject>()
                .ForMember(dest => dest.MyValue, opt =>
                {
                    opt.MapFrom(src => src.MyValue.ExtractInt());
                    opt.MapAtRuntime();
                });
        }).CreateMapper();

        var result = mapper.Map<SourceObject, DestObject>(new SourceObject { MyValue = "1" });

        result.MyValue.ShouldBe(1);
    }

    [Fact]
    public void Should_handle_null_source_member_with_extension_method_and_nullable_destination()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceObject, DestObject>()
                .ForMember(dest => dest.MyValue, opt =>
                {
                    opt.MapFrom(src => src.MyValue.ExtractInt());
                    opt.MapAtRuntime();
                });
        }).CreateMapper();

        var result = mapper.Map<SourceObject, DestObject>(new SourceObject { MyValue = null });

        result.MyValue.ShouldBeNull();
    }
}

static class NullableExtensionMethodHelpers
{
    public static int ExtractInt(this string str) => int.Parse(str);
}
