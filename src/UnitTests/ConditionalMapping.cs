namespace AutoMapper.UnitTests.ConditionalMapping;

public class When_adding_a_condition_for_all_members : AutoMapperSpecBase
{
    Source _source = new Source { Value = 3 };
    Destination _destination = new Destination { Value = 7 };

    class Source
    {
        public int Value { get; set; }
    }

    class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.Condition((source, destination, sourceProperty, destinationProperty) =>
        {
            source.ShouldBeSameAs(_source);
            destination.ShouldBeSameAs(_destination);
            ((int)sourceProperty).ShouldBe(3);
            ((int)destinationProperty).ShouldBe(7);
            return true;
        }));
    });
    [Fact]
    public void Should_work() => Mapper.Map(_source, _destination);
}

public class When_ignoring_all_properties_with_an_inaccessible_setter_and_explicitly_implemented_member : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<SourceClass, DestinationClass>().IgnoreAllPropertiesWithAnInaccessibleSetter());

    interface Interface
    {
        int Value { get; }
    }

    class SourceClass
    {
        public int PublicProperty { get; set; }
    }

    class DestinationClass : Interface
    {
        int Interface.Value { get { return 123; } }

        public int PrivateProperty { get; private set; }

        public int PublicProperty { get; set; }
    }
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_configuring_a_member_to_skip_based_on_the_property_value : AutoMapperSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Value, opt => opt.Condition(src => src.Value > 0));
    });

    [Fact]
    public void Should_skip_the_mapping_when_the_condition_is_true()
    {
        var destination = Mapper.Map<Source, Destination>(new Source {Value = -1});

        destination.Value.ShouldBe(0);
    }

    [Fact]
    public void Should_execute_the_mapping_when_the_condition_is_false()
    {
        var destination = Mapper.Map<Source, Destination>(new Source { Value = 7 });

        destination.Value.ShouldBe(7);
    }
}

public class When_configuring_a_member_to_skip_based_on_the_property_value_with_custom_mapping : AutoMapperSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Value, opt =>
            {
                opt.Condition(src => src.Value > 0);
                opt.MapFrom(src => 10);
            });
    });

    [Fact]
    public void Should_skip_the_mapping_when_the_condition_is_true()
    {
        var destination = Mapper.Map<Source, Destination>(new Source { Value = -1 });

        destination.Value.ShouldBe(0);
    }

    [Fact]
    public void Should_execute_the_mapping_when_the_condition_is_false()
    {
        Mapper.Map<Source, Destination>(new Source { Value = 7 }).Value.ShouldBe(10);
    }
}

public class When_configuring_a_map_to_ignore_all_properties_with_an_inaccessible_setter : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CodeName { get; set; }
        public string Nickname { get; set; }
        public string ScreenName { get; set; }
    }

    public class Destination
    {
        private double _height;

        public int Id { get; set; }
        public virtual string Name { get; protected set; }
        public string Title { get; internal set; }
        public string CodeName { get; private set; }
        public string Nickname { get; private set; }
        public string ScreenName { get; private set; }
        public int Age { get; private set; }

        public double Height
        {
            get { return _height; }
        }

        public Destination()
        {
            _height = 60;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.ScreenName, opt => opt.MapFrom(src => src.ScreenName))
            .IgnoreAllPropertiesWithAnInaccessibleSetter()
            .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Nickname));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Id = 5, CodeName = "007", Nickname = "Jimmy", ScreenName = "jbogard" });
    }

    [Fact]
    public void Should_consider_the_configuration_valid_even_if_some_properties_with_an_inaccessible_setter_are_unmapped()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }

    [Fact]
    public void Should_map_a_property_with_an_inaccessible_setter_if_a_specific_mapping_is_configured_after_the_ignore_method()
    {
        _destination.Nickname.ShouldBe("Jimmy");
    }

    [Fact]
    public void Should_not_map_a_property_with_an_inaccessible_setter_if_no_specific_mapping_is_configured_even_though_name_and_type_match()
    {
        _destination.CodeName.ShouldBeNull();
    }

    [Fact]
    public void Should_not_map_a_property_with_no_public_setter_if_a_specific_mapping_is_configured_before_the_ignore_method()
    {
        _destination.ScreenName.ShouldBeNull();
    }
}

public class When_configuring_a_reverse_map_to_ignore_all_source_properties_with_an_inaccessible_setter : AutoMapperSpecBase
{
    private Destination _destination;
    private Source _source;

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Force { get; set; }
        public string ReverseForce { get; private set; }
        public string Respect { get; private set; }
        public int Foo { get; private set; }
        public int Bar { get; protected set; }

        public void Initialize()
        {
            ReverseForce = "You With";
            Respect = "R-E-S-P-E-C-T";
        }
    }

    public class Destination
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsVisible { get; set; }
        public string Force { get; private set; }
        public string ReverseForce { get; set; }
        public string Respect { get; set; }
        public int Foz { get; private set; }
        public int Baz { get; protected set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .IgnoreAllPropertiesWithAnInaccessibleSetter()
            .ForMember(dest => dest.IsVisible, opt => opt.Ignore())
            .ForMember(dest => dest.Force, opt => opt.MapFrom(src => src.Force))
            .ReverseMap()
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .ForMember(dest => dest.ReverseForce, opt => opt.MapFrom(src => src.ReverseForce))
            .ForSourceMember(dest => dest.IsVisible, opt => opt.DoNotValidate());
    });

    protected override void Because_of()
    {
        var source = new Source { Id = 5, Name = "Bob", Age = 35, Force = "With You" };
        source.Initialize();
        _destination = Mapper.Map<Source, Destination>(source);
        _source = Mapper.Map<Destination, Source>(_destination);
    }

    [Fact]
    public void Should_consider_the_configuration_valid_even_if_some_properties_with_an_inaccessible_setter_are_unmapped()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }

    [Fact]
    public void Should_forward_and_reverse_map_a_property_that_is_accessible_on_both_source_and_destination()
    {
        _source.Name.ShouldBe("Bob");
    }

    [Fact]
    public void Should_forward_and_reverse_map_an_inaccessible_destination_property_if_a_mapping_is_defined()
    {
        _source.Force.ShouldBe("With You");
    }

    [Fact]
    public void Should_forward_and_reverse_map_an_inaccessible_source_property_if_a_mapping_is_defined()
    {
        _source.ReverseForce.ShouldBe("You With");
    }

    [Fact]
    public void Should_forward_and_reverse_map_an_inaccessible_source_property_even_if_a_mapping_is_not_defined()
    {
        _source.Respect.ShouldBe("R-E-S-P-E-C-T"); // justification: if the mapping works one way, it should work in reverse
    }
}
public class When_using_a_condition_with_a_nullable_source_member_and_a_value_type_destination : AutoMapperSpecBase
{
    class Source
    {
        public int? Value { get; set; }
        public bool? Toggle { get; set; }
    }

    class Destination
    {
        public int Value { get; set; } = 7;
        public bool Toggle { get; set; } = true;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.Condition((source, destination, sourceMember) => sourceMember != null)));

    [Fact]
    public void Should_not_map_a_null_source_member()
    {
        var destination = Mapper.Map(new Source(), new Destination());
        destination.Value.ShouldBe(7);
        destination.Toggle.ShouldBeTrue();
    }

    [Fact]
    public void Should_map_a_source_member_with_a_value()
    {
        var destination = Mapper.Map(new Source { Value = 3, Toggle = false }, new Destination());
        destination.Value.ShouldBe(3);
        destination.Toggle.ShouldBeFalse();
    }
}

public class When_using_a_class_based_condition_with_a_nullable_source_member : AutoMapperSpecBase
{
    class Source
    {
        public int? Value { get; set; }
    }

    class Destination
    {
        public int Value { get; set; } = 7;
    }

    class SourceMemberNotNull : ICondition<Source, Destination, object>
    {
        public bool Evaluate(Source source, Destination destination, object sourceMember, object destMember, ResolutionContext context) =>
            sourceMember != null;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.Condition<SourceMemberNotNull>()));

    [Fact]
    public void Should_not_map_a_null_source_member() => Mapper.Map(new Source(), new Destination()).Value.ShouldBe(7);

    [Fact]
    public void Should_map_a_source_member_with_a_value() => Mapper.Map(new Source { Value = 3 }, new Destination()).Value.ShouldBe(3);
}

public class When_using_a_condition_typed_as_the_destination_member : AutoMapperSpecBase
{
    class Source
    {
        public int? Value { get; set; }
    }

    class Destination
    {
        public int Value { get; set; } = 7;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.Condition((source, destination, sourceMember) => sourceMember > 0)));

    [Fact]
    public void Should_receive_the_mapped_value_because_the_source_value_does_not_fit_the_condition_member_type() =>
        Mapper.Map(new Source(), new Destination()).Value.ShouldBe(7);

    [Fact]
    public void Should_map_a_source_member_with_a_value() => Mapper.Map(new Source { Value = 3 }, new Destination()).Value.ShouldBe(3);
}

public class When_using_a_condition_for_all_members_with_different_source_and_destination_member_types : AutoMapperSpecBase
{
    class Source
    {
        public Inner Value { get; set; }
    }

    class Destination
    {
        public InnerDto Value { get; set; }
    }

    class Inner
    {
        public int Number { get; set; }
    }

    class InnerDto
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Inner, InnerDto>();
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.Condition((source, destination, sourceMember) => sourceMember is Inner));
    });

    [Fact]
    public void Should_receive_the_source_member() => Mapper.Map<Destination>(new Source { Value = new Inner { Number = 3 } }).Value.Number.ShouldBe(3);
}
