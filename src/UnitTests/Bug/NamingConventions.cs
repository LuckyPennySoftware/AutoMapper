namespace AutoMapper.UnitTests.Bug.NamingConventions;

public class RemoveNameSplitMapper : NonValidatingSpecBase
{
    class Source
    {
        public InnerSource InnerSource { get; set; }
    }
    class InnerSource
    {
        public int Value { get; set; }
    }
    class Destination
    {
        public int InnerSourceValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.Internal().DestinationMemberNamingConvention = ExactMatchNamingConvention.Instance;
        c.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Should_not_validate() => Should.Throw<AutoMapperConfigurationException>(AssertConfigurationIsValid)
        .Errors.Single().UnmappedPropertyNames.Single().ShouldBe(nameof(Destination.InnerSourceValue));
}
public class DisableNamingConvention : NonValidatingSpecBase
{
    class Source
    {
        public string Name { get; set; }
    }
    class Destination
    {
        public string Name { get; set; }
        public string COMPANY_Name { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.DestinationMemberNamingConvention = ExactMatchNamingConvention.Instance;
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Should_not_use_pascal_naming_convention() =>
        new Action(Mapper.ConfigurationProvider.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>()
            .Errors[0].UnmappedPropertyNames.ShouldContain("COMPANY_Name");
}
public class Neda
{
    public string cmok { get; set; }

    public string moje_ime { get; set; }

    public string moje_prezime { get; set; }

    public string ja_se_zovem_imenom { get; set; }

}

public class Dario
{
    public string cmok { get; set; }

    public string MojeIme { get; set; }

    public string MojePrezime { get; set; }

    public string JaSeZovemImenom { get; set; }
}

public class When_mapping_with_lowercase_naming_conventions_two_ways_in_profiles : AutoMapperSpecBase
{
    private Dario _dario;
    private Neda _neda;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProfile("MyMapperProfile", prf =>
        {
            prf.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            prf.CreateMap<Neda, Dario>();
        });
        cfg.CreateProfile("MyMapperProfile2", prf =>
        {
            prf.DestinationMemberNamingConvention = new LowerUnderscoreNamingConvention();
            prf.CreateMap<Dario, Neda>();
        });
    });

    protected override void Because_of()
    {
        _dario = Mapper.Map<Neda, Dario>(new Neda {ja_se_zovem_imenom = "foo"});
        _neda = Mapper.Map<Dario, Neda>(_dario);
    }

    [Fact]
    public void Should_map_from_lower_to_pascal()
    {
        _neda.ja_se_zovem_imenom.ShouldBe("foo");
    }

    [Fact]
    public void Should_map_from_pascal_to_lower()
    {
        _dario.JaSeZovemImenom.ShouldBe("foo");
    }
}
public class PascalCaseAcronymInPropertyName : AutoMapperSpecBase
{
    class Source { public Inner Form { get; set; } }
    class Inner { public int XML { get; set; } }
    class Dest { public int FormXML { get; set; } }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Dest>());

    [Fact]
    public void Should_flatten_acronym_property()
    {
        var result = Mapper.Map<Dest>(new Source { Form = new Inner { XML = 42 } });
        result.FormXML.ShouldBe(42);
    }
}

public class When_mapping_with_lowercase_naming_conventions_two_ways : AutoMapperSpecBase
{
    private Dario _dario;
    private Neda _neda;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
        cfg.DestinationMemberNamingConvention = new LowerUnderscoreNamingConvention();
        cfg.CreateProfile("MyMapperProfile", prf =>
        {
            prf.DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
            prf.CreateMap<Neda, Dario>();
        });
        cfg.CreateProfile("MyMapperProfile2", prf =>
        {
            prf.SourceMemberNamingConvention = PascalCaseNamingConvention.Instance;
            prf.CreateMap<Dario, Neda>();
        });
    });

    protected override void Because_of()
    {
        _dario = Mapper.Map<Neda, Dario>(new Neda { ja_se_zovem_imenom = "foo" });
        _neda = Mapper.Map<Dario, Neda>(_dario);
    }

    [Fact]
    public void Should_map_from_lower_to_pascal()
    {
        _neda.ja_se_zovem_imenom.ShouldBe("foo");
    }

    [Fact]
    public void Should_map_from_pascal_to_lower()
    {
        _dario.JaSeZovemImenom.ShouldBe("foo");
    }
}