namespace AutoMapper.UnitTests.ConfigurationValidation;
public class When_testing_a_dto_with_mismatched_member_names_and_mismatched_types : AutoMapperSpecBase
{
    public class Source
    {
        public decimal Foo { get; set; }
    }
    public class Destination
    {
        public Type Foo { get; set; }
        public string Bar { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void Should_throw_unmapped_member_and_mismatched_type_exceptions()
    {
        new Action(AssertConfigurationIsValid)
            .ShouldThrow<AggregateException>()
            .ShouldSatisfyAllConditions(
                aex => aex.InnerExceptions.ShouldBeOfLength(2),
                aex => aex.InnerExceptions[0]
                    .ShouldBeOfType<AutoMapperConfigurationException>()
                    .ShouldSatisfyAllConditions(
                        ex => ex.Errors.ShouldBeOfLength(1),
                        ex => ex.Errors[0].UnmappedPropertyNames.ShouldContain("Bar")),
                aex => aex.InnerExceptions[1]
                    .ShouldBeOfType<AutoMapperConfigurationException>()
                    .ShouldSatisfyAllConditions(
                        ex => ex.MemberMap.ShouldNotBeNull(),
                        ex => ex.MemberMap.DestinationName.ShouldBe("Foo"))
            );
    }
}
public class When_testing_a_dto_with_mismatches_in_multiple_children : AutoMapperSpecBase
{
    public class Source
    {
        public Type Foo { get; set; }
        public Type Bar { get; set; }
    }
    public class Destination
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void Should_throw_for_both_mismatched_children()
    {
        new Action(AssertConfigurationIsValid)
            .ShouldThrow<AggregateException>()
            .ShouldSatisfyAllConditions(
                aex => aex.InnerExceptions.ShouldBeOfLength(2),
                aex => aex.InnerExceptions[0]
                    .ShouldBeOfType<AutoMapperConfigurationException>()
                    .ShouldSatisfyAllConditions(
                        ex => ex.MemberMap.ShouldNotBeNull(),
                        ex => ex.MemberMap.DestinationName.ShouldBe("Foo")),
                aex => aex.InnerExceptions[1]
                    .ShouldBeOfType<AutoMapperConfigurationException>()
                    .ShouldSatisfyAllConditions(
                        ex => ex.MemberMap.ShouldNotBeNull(),
                        ex => ex.MemberMap.DestinationName.ShouldBe("Bar"))
            );
    }
}
public class ConstructorMappingValidation : NonValidatingSpecBase
{
    public class Destination
    {
        public Destination(ComplexType myComplexMember)
        {
            MyComplexMember = myComplexMember;
        }
        public ComplexType MyComplexMember { get; }
    }
    public class Source
    {
        public string MyComplexMember { get; set; }
    }
    public class ComplexType
    {
        public int SomeMember { get; }
        private ComplexType(int someMember)
        {
            SomeMember = someMember;
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_validation() => new Action(AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(ex=>
        ex.MemberMap.ToString().ShouldBe("Void .ctor(ComplexType), parameter myComplexMember"));
}

public class When_using_a_type_converter : AutoMapperSpecBase
{
    public class A
    {
        public string Foo { get; set; }
    }
    public class B
    {
        public C Foo { get; set; }
    }
    public class C { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<A, B>().ConvertUsing(x => new B { Foo = new C() }));
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_using_a_type_converter_class : AutoMapperSpecBase
{
    public class A
    {
        public string Foo { get; set; }
    }
    public class B
    {
        public C Foo { get; set; }
    }
    public class C { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<A, B>().ConvertUsing<Converter>());

    class Converter : ITypeConverter<A, B>
    {
        public B Convert(A source, B dest, ResolutionContext context) => new B { Foo = new C() };
    }
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_skipping_validation : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Blarg { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Dest>(MemberList.None));

    [Fact]
    public void Should_skip_validation()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Mapper.ConfigurationProvider.AssertConfigurationIsValid());
    }
}

public class When_constructor_does_not_match : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int blarg)
        {
            Value = blarg;
        }
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Dest>());

    [Fact]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_constructor_does_not_match_ForCtorParam : AutoMapperSpecBase
{
    public class Source
    {
    }
    public class Dest
    {
        public Dest(int value)
        {
            Value = value;
        }
        public int Value { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Dest>().ForCtorParam("value", o=>o.MapFrom(s=>4)));

    [Fact]
    public void Should_map() => Mapper.Map<Dest>(new Source()).Value.ShouldBe(4);
}

public class When_constructor_partially_matches : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int value, int blarg)
        {
            Value = blarg;
        }

        public int Value { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Dest>());

    [Fact]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_constructor_partially_matches_and_ctor_param_configured : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int value, int blarg)
        {
            Value = blarg;
        }

        public int Value { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForCtorParam("blarg", opt => opt.MapFrom(src => src.Value));
    });

    [Fact]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_constructor_partially_matches_and_constructor_validation_skipped : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public Dest(int value, int blarg)
        {
            Value = blarg;
        }

        public int Value { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>().DisableCtorValidation();
    });

    [Fact]
    public void Should_throw()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_mismatched_members : NonValidatingSpecBase
{
    public class ModelObject
    {
        public string Foo { get; set; }
        public string Barr { get; set; }
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class ModelObject2
    {
        public string Foo { get; set; }
        public string Barr { get; set; }
    }

    public class ModelDto2
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        public string Bar1 { get; set; }
        public string Bar2 { get; set; }
        public string Bar3 { get; set; }
        public string Bar4 { get; set; }
    }

    public class ModelObject3
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        public string Bar1 { get; set; }
        public string Bar2 { get; set; }
        public string Bar3 { get; set; }
        public string Bar4 { get; set; }
    }

    public class ModelDto3
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }


    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
        cfg.CreateMap<ModelObject2, ModelDto2>();
        cfg.CreateMap<ModelObject3, ModelDto3>(MemberList.Source);
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class ResolversWithSourceValidation : AutoMapperSpecBase
{
    class Source
    {
        public int Resolved { get; set; }
        public int TypedResolved { get; set; }
        public int Converted { get; set; }
        public int TypedConverted { get; set; }
    }
    class Destination
    {
        public int ResolvedDest { get; set; }
        public int TypedResolvedDest { get; set; }
        public int ConvertedDest { get; set; }
        public int TypedConvertedDest { get; set; }
    }
    class MemberResolver : IMemberValueResolver<Source, Destination, int, int>
    {
        public int Resolve(Source source, Destination destination, int sourceMember, int destinationMember, ResolutionContext context) => 5;
    }
    class ValueConverter : IValueConverter<int, int>
    {
        public int Convert(int sourceMember, ResolutionContext context) => 5;
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>(MemberList.Source)
            .ForMember(d => d.ResolvedDest, o => o.MapFrom<MemberResolver, int>("Resolved"))
            .ForMember(d=>d.TypedResolvedDest, o => o.MapFrom<MemberResolver, int>(s => s.TypedResolved))
            .ForMember(d => d.ConvertedDest, o => o.ConvertUsing<ValueConverter, int>("Converted"))
            .ForMember(d => d.TypedConvertedDest, o => o.ConvertUsing<ValueConverter, int>(s => s.TypedConverted));
    });
    [Fact]
    public void Should_work()
    {
        var result = Mapper.Map<Source, Destination>(new Source());
        result.ResolvedDest.ShouldBe(5);
        result.TypedResolvedDest.ShouldBe(5);
        result.ConvertedDest.ShouldBe(5);
        result.TypedConvertedDest.ShouldBe(5);
    }
}

public class NonMemberExpressionWithSourceValidation : NonValidatingSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public string OtherValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateMap<Source, Destination>(MemberList.Source)
        .ForMember(d=>d.OtherValue, o=>o.MapFrom(s=>s.Value ?? "")));
    [Fact]
    public void Should_be_ignored() => new Action(AssertConfigurationIsValid)
        .ShouldThrow<AutoMapperConfigurationException>().Errors[0].UnmappedPropertyNames[0].ShouldBe(nameof(Source.Value));
}

public class MatchingNonMemberExpressionWithSourceValidation : NonValidatingSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public string Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<Source, Destination>(MemberList.Source)
        .ForMember(d => d.Value, o => o.MapFrom(s => s.Value ?? "")));
    [Fact]
    public void Should_be_ignored() => new Action(AssertConfigurationIsValid)
        .ShouldThrow<AutoMapperConfigurationException>().Errors[0].UnmappedPropertyNames[0].ShouldBe(nameof(Source.Value));
}

public class When_testing_a_dto_with_fully_mapped_and_custom_matchers : AutoMapperSpecBase
{
    public class ModelObject
    {
        public string Foo { get; set; }
        public string Barr { get; set; }
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>()
            .ForMember(dto => dto.Bar, opt => opt.MapFrom(m => m.Barr));
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_testing_a_dto_with_matching_member_names_but_mismatched_types : NonValidatingSpecBase
{
    public class Source
    {
        public decimal Value { get; set; }
    }

    public class Destination
    {
        public Type Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_member_type_mapped_mappings : AutoMapperSpecBase
{
    private AutoMapperConfigurationException _exception;

    public class Source
    {
        public int Value { get; set; }
        public OtherSource Other { get; set; }
    }

    public class OtherSource
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public OtherDest Other { get; set; }
    }

    public class OtherDest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<OtherSource, OtherDest>();
    });

    protected override void Because_of()
    {
        try
        {
           AssertConfigurationIsValid();
        }
        catch (AutoMapperConfigurationException ex)
        {
            _exception = ex;
        }
    }

    [Fact]
    public void Should_pass_a_configuration_check()
    {
        _exception.ShouldBeNull();
    }
}

public class When_testing_a_dto_with_matched_members_but_mismatched_types_that_are_ignored : AutoMapperSpecBase
{
    private AutoMapperConfigurationException _exception;

    public class ModelObject
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public int Bar { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>()
            .ForMember(dest => dest.Bar, opt => opt.Ignore());
    });

    protected override void Because_of()
    {
        try
        {
            AssertConfigurationIsValid();
        }
        catch (AutoMapperConfigurationException ex)
        {
            _exception = ex;
        }
    }

    [Fact]
    public void Should_pass_a_configuration_check()
    {
        _exception.ShouldBeNull();
    }
}

public class When_testing_a_dto_with_array_types_with_mismatched_element_types : NonValidatingSpecBase
{
    public class Source
    {
        public SourceItem[] Items;
    }

    public class Destination
    {
        public DestinationItem[] Items;
    }

    public class SourceItem
    {

    }

    public class DestinationItem
    {

    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_list_types_with_mismatched_element_types : NonValidatingSpecBase
{
    public class Source
    {
        public List<SourceItem> Items;
    }

    public class Destination
    {
        public List<DestinationItem> Items;
    }

    public class SourceItem
    {

    }

    public class DestinationItem
    {

    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_readonly_members : NonValidatingSpecBase
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public string ValuePlusOne { get { return (Value + 1).ToString(); } }
        public int ValuePlusTwo { get { return Value + 2; } }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        Mapper.Map<Source, Destination>(new Source { Value = 5 });
    }

    [Fact]
    public void Should_be_valid()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_in_a_specfic_profile : NonValidatingSpecBase
{
    public class GoodSource
    {
        public int Value { get; set; }
    }

    public class GoodDest
    {
        public int Value { get; set; }
    }

    public class BadDest
    {
        public int Valufffff { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProfile("Good", profile =>
        {
            profile.CreateMap<GoodSource, GoodDest>();
        });
        cfg.CreateProfile("Bad", profile =>
        {
            profile.CreateMap<GoodSource, BadDest>();
        });
    });

    [Fact]
    public void Should_ignore_bad_dtos_in_other_profiles() =>
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => AssertConfigurationIsValid("Good"));
    [Fact]
    public void Should_throw_when_profile_name_does_not_exist() =>
        typeof(ArgumentOutOfRangeException).ShouldBeThrownBy(() => AssertConfigurationIsValid("Does not exist"));
}

public class When_testing_a_dto_with_mismatched_custom_member_mapping : NonValidatingSpecBase
{
    public class SubBarr { }

    public class SubBar { }

    public class ModelObject
    {
        public string Foo { get; set; }
        public SubBarr Barr { get; set; }
    }

    public class ModelDto
    {
        public string Foo { get; set; }
        public SubBar Bar { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>()
            .ForMember(dest => dest.Bar, opt => opt.MapFrom(src => src.Barr));
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_value_specified_members : NonValidatingSpecBase
{
    public class Source { }
    public class Destination
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        object i = 7;
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => i));
    });

    [Fact]
    public void Should_validate_successfully()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_setter_only_peroperty_member : NonValidatingSpecBase
{
    public class Source
    {
        public string Value { set { } }
    }

    public class Destination
    {
        public string Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_testing_a_dto_with_matching_void_method_member : NonValidatingSpecBase
{
    public class Source
    {
        public void Method()
        {
        }
    }

    public class Destination
    {
        public string Method { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_fail_a_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
    }
}

public class When_redirecting_types : NonValidatingSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ConcreteSource, ConcreteDest>()
            .ForMember(d => d.DifferentName, opt => opt.MapFrom(s => s.Name));
        cfg.CreateMap<ConcreteSource, IAbstractDest>().As<ConcreteDest>();
    });

    [Fact]
    public void Should_pass_configuration_check()
    {
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(AssertConfigurationIsValid);
    }

    class ConcreteSource
    {
        public string Name { get; set; }
    }

    class ConcreteDest : IAbstractDest
    {
        public string DifferentName { get; set; }
    }

    interface IAbstractDest
    {
        string DifferentName { get; set; }
    }
}

public class When_configuring_a_resolver : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Query, Command>().ForMember(d => d.Details, o => o.MapFrom<DetailsValueResolver>());
    });
    public class DetailsValueResolver : IValueResolver<Query, Command, List<KeyValuePair<string, string>>>
    {
        public List<KeyValuePair<string, string>> Resolve(Query source, Command destination, List<KeyValuePair<string, string>> destMember, ResolutionContext context)
        {
            return source.Details
                .Select(d => new KeyValuePair<string, string>(d.ToString(), d.ToString()))
                .ToList();
        }
    }
    public class Query
    {
        public List<int> Details { get; set; }
    }

    public class Command
    {
        public List<KeyValuePair<string, string>> Details { get; private set; }
    }
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}
public class ObjectPropertyAndNestedTypes : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<RootLevel, RootLevelDto>());
    public class RootLevel
    {
        public object ObjectProperty { get; set; }
        public SecondLevel SecondLevel { get; set; }
    }
    public class RootLevelDto
    {
        public object ObjectProperty { get; set; }
        public SecondLevelDto SecondLevel { get; set; }
    }
    public class SecondLevel
    {
    }
    public class SecondLevelDto
    {
    }
    [Fact]
    public void Should_fail_validation() => new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>().MemberMap.DestinationName.ShouldBe(nameof(RootLevelDto.SecondLevel));
}