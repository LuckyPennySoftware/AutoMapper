using System.Text.RegularExpressions;

namespace AutoMapper.UnitTests.Tests;
public class StubNamingConvention : INamingConvention
{
    public Regex SplittingExpression { get; set; }
    public string SeparatorCharacter { get; set; }
    public string[] Split(string input) => SplittingExpression.Matches(input).Cast<Match>().Select(m=>m.Value).ToArray();
}

public class When_constructing_type_maps_with_matching_property_names : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
        public int SomeOtherValue { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int SomeOtherValue { get; set; }
    }

    private class TestProfile : Profile
    {
        public override string ProfileName => "Test";
    }

    [Fact]
    public void Should_map_properties_with_same_name()
    {
        var mappingOptions = new TestProfile();
        //mappingOptions.SourceMemberNamingConvention = new PascalCaseNamingConvention();
        //mappingOptions.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        var profile = new ProfileMap(mappingOptions);

        var typeMap = new TypeMap(typeof(Source), typeof(Destination), profile, null);

        var propertyMaps = typeMap.PropertyMaps;

        propertyMaps.Count().ShouldBe(2);
    }
}
public class When_using_a_custom_source_naming_convention : AutoMapperSpecBase
{
    private class Source
    {
        public SubSource some__source { get; set; }
    }
    private class SubSource
    {
        public int value { get; set; }
    }
    private class Destination
    {
        public int SomeSourceValue { get; set; }
    }
    private class TestProfile : Profile
    {
        public TestProfile() => SourceMemberNamingConvention = new StubNamingConvention{ SeparatorCharacter = "__", SplittingExpression = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=__?)") };
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.AddProfile<TestProfile>());
    [Fact]
    public void Should_split_using_naming_convention_rules() => AssertConfigurationIsValid();
}
public class When_using_a_custom_destination_naming_convention : AutoMapperSpecBase
{
    private class Source
    {
        public SubSource SomeSource { get; set; }
    }
    private class SubSource
    {
        public int Value { get; set; }
    }
    private class Destination
    {
        public int some__source__value { get; set; }
    }
    private class TestProfile : Profile
    {
        public TestProfile() => DestinationMemberNamingConvention = new StubNamingConvention{ SeparatorCharacter = "__", SplittingExpression = new Regex(@"[\p{Ll}\p{Lu}0-9]+(?=__?)") };
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.AddProfile<TestProfile>());
    [Fact]
    public void Should_split_using_naming_convention_rules() => AssertConfigurationIsValid();
}

public class When_using_a_source_member_name_replacer : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
        public int Ävíator { get; set; }
        public int SubAirlinaFlight { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int Aviator { get; set; }
        public int SubAirlineFlight { get; set; }
    }

    [Fact]
    public void Should_map_properties_with_different_names()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ReplaceMemberName("A", "Ä");
            cfg.ReplaceMemberName("i", "í");
            cfg.ReplaceMemberName("Airline", "Airlina");
            cfg.CreateMap<Source, Destination>();
        });

        var mapper = config.CreateMapper();
        var dest = mapper.Map<Destination>(new Source {Ävíator = 3, SubAirlinaFlight = 4, Value = 5});
        dest.Aviator.ShouldBe(3);
        dest.SubAirlineFlight.ShouldBe(4);
        dest.Value.ShouldBe(5);
    }
}

public class When_using_a_source_member_name_replacer_with_profile : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
        public int Ävíator { get; set; }
        public int SubAirlinaFlight { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int Aviator { get; set; }
        public int SubAirlineFlight { get; set; }
    }

    public class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<Source, Destination>();
        }
    }

    [Fact]
    public void Should_map_properties_with_different_names()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ReplaceMemberName("A", "Ä");
            cfg.ReplaceMemberName("i", "í");
            cfg.ReplaceMemberName("Airline", "Airlina");
            cfg.AddProfile<TestProfile>();
        });

        var mapper = config.CreateMapper();
        var dest = mapper.Map<Destination>(new Source { Ävíator = 3, SubAirlinaFlight = 4, Value = 5 });
        dest.Aviator.ShouldBe(3);
        dest.SubAirlineFlight.ShouldBe(4);
        dest.Value.ShouldBe(5);
    }
}