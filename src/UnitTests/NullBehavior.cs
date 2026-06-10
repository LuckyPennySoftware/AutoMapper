namespace AutoMapper.UnitTests.NullBehavior;
public class NullDestinationType : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c => { });
    [Fact]
    public void Should_require_destination_object()
    {
        Mapper.Map("", "", null, null).ShouldBe("");
        Mapper.Map("", null, null, typeof(string)).ShouldBe("");
        Mapper.Map("", "", null, null, _ => { }).ShouldBe("");
        Mapper.Map("", null, null, typeof(string), _=>{ }).ShouldBe("");
        Mapper.Map<string>("").ShouldBe("");
        Mapper.Map("", default(string)).ShouldBe("");
        Mapper.Map<string>("", _ => { }).ShouldBe("");
        Mapper.Map("", default(string), _ => { }).ShouldBe("");
    }
}
public class NullToExistingDestination : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<string, string>().DisableCtorValidation());
    [Fact]
    public void Should_return_the_destination()
    {
        var destination = "42";
        Mapper.Map(default(string), destination).ShouldBeSameAs(destination);
    }
}
public class NullToExistingValue : AutoMapperSpecBase
{
    private record Person
    {
        public string Name { get; set; }
        public Address TheAddress { get; set; } = new();
    }
    private record Address
    {
        public string Street { get; set; }
        public int Number { get; set; }
    }
    private record PersonModel
    {
        public string Name { get; set; }
        public AddressModel TheAddress { get; set; }
    }
    private record AddressModel
    {
        public string Street { get; set; }
        public int Number { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
    	c.CreateMap<PersonModel, Person>();
    	c.CreateMap<AddressModel, Address>();
    });
    [Fact]
    public void Should_overwrite() => Mapper.Map(new PersonModel(), new Person()).TheAddress.ShouldBeNull();
}
public class NullCheckDefault : AutoMapperSpecBase
{
    class Source
    {
        public string Value { get; }
    }
    class Destination
    {
        public int Length { get; set; } = 42;
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForMember(d => d.Length, o => o.MapFrom(s => s.Value.Length)));
    [Fact]
    public void Should_be_default() => Map<Destination>(new Source()).Length.ShouldBe(0);
}
public class When_mappping_null_with_DoNotAllowNull : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner { get; set; }
        public int[] Collection { get; set; }
    }
    public class InnerSource
    {
        public int Integer { get; set; }
    }
    class Destination
    {
        public InnerDestination Inner { get; set; }
        public int[] Collection { get; set; }
    }
    public class InnerDestination
    {
        public int Integer { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.DoNotAllowNull());
        cfg.CreateMap<InnerSource, InnerDestination>();
        cfg.AllowNullDestinationValues = true;
        cfg.AllowNullCollections = true;
    });
    [Fact]
    public void Should_map_to_non_null()
    {
        var destination = Mapper.Map<Destination>(new Source());
        destination.Collection.ShouldNotBeNull();
        destination.Inner.ShouldNotBeNull();
    }
}
public class When_mappping_null_with_AllowNull : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner { get; set; }
        public int[] Collection { get; set; }
    }
    public class InnerSource
    {
        public int Integer { get; set; }
    }
    class Destination
    {
        public InnerDestination Inner { get; set; }
        public int[] Collection { get; set; }
    }
    public class InnerDestination
    {
        public int Integer { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(o=>o.AllowNull());
        cfg.CreateMap<InnerSource, InnerDestination>();
        cfg.AllowNullDestinationValues = false;
        cfg.AllowNullCollections = false;
    });
    [Fact]
    public void Should_map_to_null()
    {
        var destination = Mapper.Map<Destination>(new Source());
        destination.Collection.ShouldBeNull();
        destination.Inner.ShouldBeNull();
    }
}
public class When_mappping_null_with_AllowNull_and_inheritance : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner { get; set; }
        public int[] Collection { get; set; }
    }
    class SourceDerived : Source
    {
    }
    public class InnerSource
    {
        public int Integer { get; set; }
    }
    class Destination
    {
        public InnerDestination Inner { get; set; }
        public int[] Collection { get; set; }
    }
    class DestinationDerived : Destination
    {
    }
    public class InnerDestination
    {
        public int Integer { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.AllowNull());
        cfg.CreateMap<SourceDerived, DestinationDerived>().IncludeBase<Source, Destination>();
        cfg.CreateMap<InnerSource, InnerDestination>();
        cfg.AllowNullDestinationValues = false;
        cfg.AllowNullCollections = false;
    });
    [Fact]
    public void Should_map_to_null()
    {
        var destination = Mapper.Map<DestinationDerived>(new SourceDerived());
        destination.Collection.ShouldBeNull();
        destination.Inner.ShouldBeNull();
    }
}
public class When_mappping_null_with_DoNotAllowNull_and_inheritance : AutoMapperSpecBase
{
    class Source
    {
        public InnerSource Inner { get; set; }
        public int[] Collection { get; set; }
    }
    class SourceDerived : Source
    {
    }
    public class InnerSource
    {
        public int Integer { get; set; }
    }
    class Destination
    {
        public InnerDestination Inner { get; set; }
        public int[] Collection { get; set; }
    }
    class DestinationDerived : Destination
    {
    }
    public class InnerDestination
    {
        public int Integer { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(o => o.AllowNull());
        cfg.CreateMap<SourceDerived, DestinationDerived>().IncludeBase<Source, Destination>().ForAllMembers(o => o.DoNotAllowNull());
        cfg.CreateMap<InnerSource, InnerDestination>();
        cfg.AllowNullDestinationValues = true;
        cfg.AllowNullCollections = true;
    });
    [Fact]
    public void Should_map_to_non_null()
    {
        var destination = Mapper.Map<DestinationDerived>(new SourceDerived());
        destination.Collection.ShouldNotBeNull();
        destination.Inner.ShouldNotBeNull();
    }
}
public class When_mappping_null_collection_with_AllowNullCollections_false : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => {});

    [Fact]
    public void Should_map_to_non_null()
    {
        Mapper.Map<int[]>(null).ShouldNotBeNull();
        Mapper.Map<int[]>(null, _=> { }).ShouldNotBeNull();
    }
}

public class When_mappping_null_collection_with_AllowNullCollections_true : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AllowNullCollections = true);

    [Fact]
    public void Should_map_to_null()
    {
        Mapper.Map<int[]>(null).ShouldBeNull();
        Mapper.Map<int[]>(null, _ => { }).ShouldBeNull();
    }
}

public class When_mappping_null_array_with_AllowNullDestinationValues_false : AutoMapperSpecBase
{
    class Source
    {
        public int[] Collection { get; set; }
    }

    class Destination
    {
        public int[] Collection { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.AllowNullDestinationValues = false;
    });

    [Fact]
    public void Should_map_to_non_null() => Mapper.Map<Destination>(new Source()).Collection.ShouldNotBeNull();
}

public class When_mappping_null_array_to_IEnumerable_with_MapAtRuntime : AutoMapperSpecBase
{
    class Source
    {
        public int[] Collection { get; set; }
    }

    class Destination
    {
        public IEnumerable<int> Collection { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>().ForMember(d=>d.Collection, o=>o.MapAtRuntime()));

    [Fact]
    public void Should_map_to_non_null()
    {
        Mapper.Map<Destination>(new Source()).Collection.ShouldNotBeNull();
    }
}

public class When_mappping_null_array_to_IEnumerable : AutoMapperSpecBase
{
    class Source
    {
        public int[] Collection { get; set; }
    }

    class Destination
    {
        public IEnumerable<int> Collection { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());

    [Fact]
    public void Should_map_to_non_null()
    {
        Mapper.Map<Destination>(new Source()).Collection.ShouldNotBeNull();
    }
}

public class When_mappping_null_list_to_ICollection : AutoMapperSpecBase
{
    class Source
    {
        public List<int> Collection { get; set; }
    }

    class Destination
    {
        public ICollection<int> Collection { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());

    [Fact]
    public void Should_map_to_non_null()
    {
        Mapper.Map<Destination>(new Source()).Collection.ShouldNotBeNull();
    }
}

public class When_mapping_untyped_null_to_IEnumerable_and_AllowNullCollections_is_true : AutoMapperSpecBase
{
    class Source
    {
        public object Value { get; set; }
    }

    class Destination
    {
        public IEnumerable Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.AllowNullCollections = true;
        c.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_map_to_null()
    {
        Mapper.Map<Destination>(new Source()).Value.ShouldBeNull();
    }
}

public class When_mapping_from_null_interface_and_AllowNullDestinationValues_is_false : AutoMapperSpecBase
{
    ElementDestination _destination;

    interface ILink
    {
        int Id { get; set; }
        string Type { get; set; }
    }

    class LinkImpl : ILink
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

    class ElementSource
    {
        public int Id { get; set; }
        public ILink Link { get; set; }
    }

    class ElementDestination
    {
        public int Id { get; set; }
        public LinkImpl Link { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullDestinationValues = false;
        cfg.CreateMap<ILink, LinkImpl>();
        cfg.CreateMap<ElementSource, ElementDestination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<ElementDestination>(new ElementSource());
    }

    [Fact]
    public void Should_not_get_null()
    {
        _destination.Link.ShouldNotBeNull();
    }
}

public class When_mapping_from_null_interface : AutoMapperSpecBase
{
    ElementDestination _destination;

    interface ILink
    {
        int Id { get; set; }
        string Type { get; set; }
    }

    class LinkImpl : ILink
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

    class ElementSource
    {
        public int Id { get; set; }
        public ILink Link { get; set; }
    }

    class ElementDestination
    {
        public int Id { get; set; }
        public LinkImpl Link { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ILink, LinkImpl>().ReverseMap();
        cfg.CreateMap<ElementSource, ElementDestination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<ElementDestination>(new ElementSource());
    }

    [Fact]
    public void Should_get_null()
    {
        _destination.Link.ShouldBeNull();
    }
}

public class When_mapping_a_model_with_null_items : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelDto
    {
        public ModelSubDto Sub { get; set; }
        public int SubSomething { get; set; }
        public int? SubSomethingNullDest { get; set; }
        public string NullString { get; set; }
    }

    public class ModelSubDto
    {
        public int[] Items { get; set; }
    }

    public class ModelObject
    {
        public ModelSubObject Sub { get; set; }
        public string NullString { get; set; }
    }

    public class ModelSubObject
    {
        public int[] GetItems()
        {
            return new[] { 0, 1, 2, 3 };
        }

        public int Something { get; set; }
        public int SomethingNullDest { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullDestinationValues = false;
        cfg.CreateMap<ModelObject, ModelDto>();
        cfg.CreateMap<ModelSubObject, ModelSubDto>();
    });

    protected override void Because_of()
    {
        var model = new ModelObject();
        model.Sub = null;

        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_populate_dto_items_with_a_value()
    {
        _result.Sub.ShouldNotBeNull();
    }

    [Fact]
    public void Should_provide_empty_array_for_array_type_values()
    {
        _result.Sub.Items.ShouldNotBeNull();
    }

    [Fact]
    public void Should_return_default_value_of_property_in_the_chain()
    {
        _result.SubSomething.ShouldBe(0);
    }

    [Fact]
    public void Should_return_null_for_nullable_properties()
    {
        _result.SubSomethingNullDest.ShouldBeNull();
    }

    [Fact]
    public void Default_value_for_string_should_be_empty()
    {
        _result.NullString.ShouldBe(string.Empty);
    }
}

public class When_overriding_null_behavior_with_null_source_items : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelDto
    {
        public ModelSubDto Sub { get; set; }
        public int SubSomething { get; set; }
        public int? NullableMapFrom { get; set; }
        public string NullString { get; set; }
        public int? SubExpressionName { get; set; }
    }

    public class ModelSubDto
    {
        public int[] Items { get; set; }
    }

    public class ModelObject
    {
        public ModelSubObject Sub { get; set; }
        public string NullString { get; set; }

        public ModelSubObject[] Subs { get; set; }

        public int Id { get; set; }
    }

    public class ModelSubObject
    {
        public int[] GetItems()
        {
            return new[] { 0, 1, 2, 3 };
        }

        public int Something { get; set; }

        public string Name { get; set; }
        public ModelSubObject Sub { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {

        cfg.AllowNullDestinationValues = true;
        cfg.CreateMap<ModelSubObject, ModelSubDto>();
        cfg.CreateMap<ModelObject, ModelDto>()
            .ForMember(d => d.SubExpressionName, opt => opt.MapFrom(src =>
                        src.Subs.FirstOrDefault(spt => spt.Sub.Something == src.Id).Something))
            .ForMember(d => d.NullableMapFrom, opt => opt.MapFrom(s => s.Sub.Something));
    });

    protected override void Because_of()
    {
        var model = new ModelObject();
        model.Sub = null;
        model.NullString = null;

        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_return_null_for_nullable_properties_that_are_complex_map_froms()
    {
        _result.SubExpressionName.ShouldBe(null);
    }

    [Fact]
    public void Should_return_null_for_nullable_properties_that_have_member_access_map_froms()
    {
        _result.NullableMapFrom.ShouldBe(null);
    }

    [Fact]
    public void Should_map_first_level_items_as_null()
    {
        _result.NullString.ShouldBeNull();
    }

    [Fact]
    public void Should_map_primitive_items_as_default()
    {
        _result.SubSomething.ShouldBe(0);
    }

    [Fact]
    public void Should_map_any_sub_mapped_items_as_null()
    {
        _result.Sub.ShouldBeNull();
    }
}

public class When_overriding_null_behavior_in_sub_profile : AutoMapperSpecBase
{
    private ModelDto _result;

    public class ModelDto
    {
        public ModelSubDto Sub { get; set; }
        public int SubSomething { get; set; }
        public int? NullableMapFrom { get; set; }
        public string NullString { get; set; }
        public int? SubExpressionName { get; set; }
    }

    public class ModelSubDto
    {
        public int[] Items { get; set; }
    }

    public class ModelObject
    {
        public ModelSubObject Sub { get; set; }
        public string NullString { get; set; }

        public ModelSubObject[] Subs { get; set; }

        public int Id { get; set; }
    }

    public class ModelSubObject
    {
        public int[] GetItems()
        {
            return new[] { 0, 1, 2, 3 };
        }

        public int Something { get; set; }

        public string Name { get; set; }
        public ModelSubObject Sub { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullDestinationValues = false;

        cfg.CreateProfile("Foo", p =>
        {
            p.AllowNullDestinationValues = true;
            p.CreateMap<ModelSubObject, ModelSubDto>();
            p.CreateMap<ModelObject, ModelDto>()
                .ForMember(d => d.SubExpressionName, opt => opt.MapFrom(src =>
                            src.Subs.FirstOrDefault(spt => spt.Sub.Something == src.Id).Something))
                .ForMember(d => d.NullableMapFrom, opt => opt.MapFrom(s => s.Sub.Something));
        });
    });

    protected override void Because_of()
    {
        var model = new ModelObject();
        model.Sub = null;
        model.NullString = null;

        _result = Mapper.Map<ModelObject, ModelDto>(model);
    }

    [Fact]
    public void Should_return_null_for_nullable_properties_that_are_complex_map_froms()
    {
        _result.SubExpressionName.ShouldBe(null);
    }

    [Fact]
    public void Should_return_null_for_nullable_properties_that_have_member_access_map_froms()
    {
        _result.NullableMapFrom.ShouldBe(null);
    }

    [Fact]
    public void Should_map_first_level_items_as_null()
    {
        _result.NullString.ShouldBeNull();
    }

    [Fact]
    public void Should_map_primitive_items_as_default()
    {
        _result.SubSomething.ShouldBe(0);
    }

    [Fact]
    public void Should_map_any_sub_mapped_items_as_null()
    {
        _result.Sub.ShouldBeNull();
    }
}

public class When_overriding_null_behavior_in_a_profile : AutoMapperSpecBase
{
    private DefaultDestination _defaultResult;
    private NullDestination _nullResult;

    public class DefaultSource
    {
        public object Value { get; set; }
    }

    public class DefaultDestination
    {
        public object Value { get; set; }
    }

    public class NullSource
    {
        public object Value { get; set; }
    }

    public class NullDestination
    {
        public object Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProfile("MapsNulls", p =>
        {
            p.AllowNullDestinationValues = false;
            p.CreateMap<NullSource, NullDestination>();
        });
        cfg.CreateMap<DefaultSource, DefaultDestination>();
    });

    protected override void Because_of()
    {
        _defaultResult = Mapper.Map<DefaultSource, DefaultDestination>(new DefaultSource());
        _nullResult = Mapper.Map<NullSource, NullDestination>(new NullSource());
    }

    [Fact]
    public void Should_use_default_behavior_in_default_profile()
    {
        _defaultResult.Value.ShouldBeNull();
    }

    [Fact]
    public void Should_use_overridden_null_behavior_in_profile()
    {
        _nullResult.Value.ShouldNotBeNull();
    }
}

public class When_using_a_custom_resolver_and_the_source_value_is_null : NonValidatingSpecBase
{
    public class NullResolver : IMemberValueResolver<Source, Destination, string, string>
    {
        public string Resolve(Source s, Destination d, string source, string dest, ResolutionContext context)
        {
            if(source == null)
                return "jon";
            return "fail";
        }
    }

    private static Source _source;
    private Destination _dest;

    public class Source
    {
        public string MyName { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom<NullResolver, string>(src => src.MyName));
        _source = new Source();
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Destination>(_source);
    }

    [Fact]
    public void Should_perform_the_translation()
    {
        _dest.Name.ShouldBe("jon");
    }
}

public class When_mapping_using_a_custom_member_mapping_and_source_is_null : AutoMapperSpecBase
{
    private Dest _dest;

    public class Source
    {
        public SubSource Sub { get; set; }
    }

    public class SubSource
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int OtherValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullDestinationValues = false;
        cfg.CreateMap<Source, Dest>()
            .ForMember(dest => dest.OtherValue, opt => opt.MapFrom(src => src.Sub.Value));
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source());
    }

    [Fact]
    public void Should_map_to_null_on_destination_values()
    {
        _dest.OtherValue.ShouldBe(0);
    }
}

public class When_specifying_a_resolver_for_a_nullable_type : AutoMapperSpecBase
{
    private FooViewModel _result;

    public class NullableBoolToLabel : ITypeConverter<bool?, string>
    {
        public string Convert(bool? source, string destination, ResolutionContext context)
        {
            if(source.HasValue)
            {
                if(source.Value)
                    return "Yes";
                else
                    return "No";
            }
            else
                return "(n/a)";
        }
    }

    public class Foo
    {
        public bool? IsFooBarred { get; set; }
    }

    public class FooViewModel
    {
        public string IsFooBarred { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<bool?, string>().ConvertUsing<NullableBoolToLabel>();
        cfg.CreateMap<Foo, FooViewModel>();
    });

    protected override void Because_of()
    {
        var foo3 = new Foo { IsFooBarred = null };
        _result = Mapper.Map<Foo, FooViewModel>(foo3);
    }

    [Fact]
    public void Should_allow_the_resolver_to_handle_null_values()
    {
        _result.IsFooBarred.ShouldBe("(n/a)");
    }
}

public class When_overriding_collection_null_behavior : AutoMapperSpecBase
{
    private Dest _dest;

    public class Source
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
    }

    public class Dest
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
        cfg.AllowNullCollections = true;
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source());
    }

    [Fact]
    public void Should_allow_null_ienumerables()
    {
        _dest.Values1.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_lists()
    {
        _dest.Values2.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_dictionaries()
    {
        _dest.Values3.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_arrays()
    {
        _dest.Values4.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_read_only_collections()
    {
        _dest.Values5.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_collections()
    {
        _dest.Values6.ShouldBeNull();
    }
}

public class When_overriding_collection_null_behavior_in_profile_with_MapAtRuntime : AutoMapperSpecBase
{
    private Dest _dest;

    public class Source
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
        public int[,] Values7 { get; set; }
    }

    public class Dest
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
        public int[,] Values7 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProfile("MyProfile", p =>
        {
            p.CreateMap<Source, Dest>().ForAllMembers(o=>o.MapAtRuntime());
            p.AllowNullCollections = true;
        });
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source());
    }

    [Fact]
    public void Should_allow_null_ienumerables()
    {
        _dest.Values1.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_lists()
    {
        _dest.Values2.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_dictionaries()
    {
        _dest.Values3.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_arrays()
    {
        _dest.Values4.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_read_only_collections()
    {
        _dest.Values5.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_collections()
    {
        _dest.Values6.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_multidimensional_arrays()
    {
        _dest.Values7.ShouldBeNull();
    }
}

public class When_overriding_collection_null_behavior_in_profile : AutoMapperSpecBase
{
    private Dest _dest;

    public class Source
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
        public int[,] Values7 { get; set; }
    }

    public class Dest
    {
        public IEnumerable<int> Values1 { get; set; }
        public List<int> Values2 { get; set; }
        public Dictionary<string, int> Values3 { get; set; }
        public int[] Values4 { get; set; }
        public ReadOnlyCollection<int> Values5 { get; set; }
        public Collection<int> Values6 { get; set; }
        public int[,] Values7 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProfile("MyProfile", p =>
        {
            p.CreateMap<Source, Dest>();
            p.AllowNullCollections = true;
        });
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source());
    }

    [Fact]
    public void Should_allow_null_ienumerables()
    {
        _dest.Values1.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_lists()
    {
        _dest.Values2.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_dictionaries()
    {
        _dest.Values3.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_arrays()
    {
        _dest.Values4.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_read_only_collections()
    {
        _dest.Values5.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_collections()
    {
        _dest.Values6.ShouldBeNull();
    }

    [Fact]
    public void Should_allow_null_multidimensional_arrays()
    {
        _dest.Values7.ShouldBeNull();
    }
}

public class When_mapping_a_null_model : AutoMapperSpecBase
{
    public class ModelDto
    {
    }

    public class ModelObject
    {
    }


    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ModelObject, ModelDto>();
    });

    [Fact]
    public void Should_populate_dto_items_with_a_value()
    {
        Mapper.Map<ModelDto>(null).ShouldBeNull();
    }
}