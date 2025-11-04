using System.Dynamic;
using AutoMapper.Internal.Mappers;
namespace AutoMapper.UnitTests.ArraysAndLists;

public class When_mapping_to_Existing_IEnumerable : AutoMapperSpecBase
{
    public class Source
    {
        public IEnumerable<SourceItem> Items { get; set; } = Enumerable.Empty<SourceItem>();
    }
    public class Destination
    {
        public IEnumerable<DestinationItem> Items { get; set; } = Enumerable.Empty<DestinationItem>();
    }
    public class SourceItem
    {
        public string Value { get; set; }
    }
    public class DestinationItem
    {
        public string Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Source, Destination>();
        c.CreateMap<SourceItem, DestinationItem>();
    });
    [Fact]
    public void Should_overwrite_the_existing_list()
    {
        var destination = new Destination();
        var existingList = destination.Items;
        Mapper.Map(new Source(), destination);
        destination.Items.ShouldNotBeSameAs(existingList);
        destination.Items.ShouldBeEmpty();
    }
}
public class When_mapping_to_an_array_as_ICollection_with_MapAtRuntime : AutoMapperSpecBase
{
    Destination _destination;
    SourceItem[] _sourceItems = new [] { new SourceItem { Value = "1" }, new SourceItem { Value = "2" }, new SourceItem { Value = "3" } };

    public class Source
    {
        public ICollection<SourceItem> Items { get; set; }
    }

    public class Destination
    {
        public ICollection<DestinationItem> Items { get; set; }
    }

    public class SourceItem
    {
        public string Value { get; set; }
    }

    public class DestinationItem
    {
        public string Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c => 
    {
        c.CreateMap<Source, Destination>().ForMember(d=>d.Items, o=>o.MapAtRuntime());
        c.CreateMap<SourceItem, DestinationItem>();
    });

    protected override void Because_of()
    {
        var source = new Source { Items = _sourceItems };
        _destination = Mapper.Map(source, new Destination { Items = new[] { new DestinationItem { Value = "4" } } });
    }

    [Fact]
    public void Should_map_ok()
    {
        _destination.Items.Select(i => i.Value).SequenceEqual(_sourceItems.Select(i => i.Value)).ShouldBeTrue();
    }
}

public class When_mapping_an_array : AutoMapperSpecBase
{
    decimal[] _source = Enumerable.Range(1, 10).Select(i=>(decimal)i).ToArray();
    decimal[] _destination;

    protected override MapperConfiguration CreateConfiguration() => new(c =>{});

    protected override void Because_of()
    {
        _destination = Mapper.Map<decimal[]>(_source);
    }

    [Fact]
    public void Should_return_a_copy()
    {
        _destination.ShouldNotBeSameAs(_source);
    }
}

public class When_mapping_a_primitive_array : AutoMapperSpecBase
{
    int[] _source = Enumerable.Range(1, 10).ToArray();
    long[] _destination;

    protected override MapperConfiguration CreateConfiguration() => new(c =>{});

    protected override void Because_of()
    {
        _destination = Mapper.Map<long[]>(_source);
    }

    [Fact]
    public void Should_return_a_copy()
    {
        var source = new int[] {1, 2, 3, 4};
        var dest = new long[4];
        Array.Copy(source, dest, 4);
        dest[3].ShouldBe(4L);

        var plan = Configuration.BuildExecutionPlan(typeof(int[]), typeof(long[]));
        _destination.ShouldNotBeSameAs(_source);
    }
}

public class When_mapping_a_primitive_array_with_custom_mapping_function : AutoMapperSpecBase
{
    int[] _source = Enumerable.Range(1, 10).ToArray();
    int[] _destination;

    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<int, int>().ConstructUsing(i => i * 1000));

    protected override void Because_of()
    {
        _destination = Mapper.Map<int[]>(_source);
    }

    [Fact]
    public void Should_map_each_item()
    {
        for (var i = 0; i < _source.Length; i++)
        {
            _destination[i].ShouldBe((i+1) * 1000);
        }
    }
}

public class When_mapping_a_primitive_array_with_custom_object_mapper : AutoMapperSpecBase
{
    int[] _source = Enumerable.Range(1, 10).ToArray();
    int[] _destination;

    private class IntToIntMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
            => context.SourceType == typeof(int) && context.DestinationType == typeof(int);

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap,
            Expression sourceExpression, Expression destExpression)
            => Expression.Multiply(Expression.Convert(sourceExpression, typeof(int)), Expression.Constant(1000));
#if NET471_OR_GREATER
        public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif        
    }

    protected override MapperConfiguration CreateConfiguration() => new(c => c.Internal().Mappers.Insert(0, new IntToIntMapper()));

    protected override void Because_of()
    {
        _destination = Mapper.Map<int[]>(_source);
    }

    [Fact]
    public void Should_not_use_custom_mapper_but_probably_should()
    {
        for (var i = 0; i < _source.Length; i++)
        {
            _destination[i].ShouldBe(i + 1);
        }
    }
}

public class When_mapping_null_list_to_array: AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public List<SourceItem> Items { get; set; }
    }

    class Destination
    {
        public DestinationItem[] Items { get; set; }
    }

    class SourceItem
    {
        public int Value { get; set; }
    }

    class DestinationItem
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<SourceItem, DestinationItem>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source());
    }

    [Fact]
    public void Should_map_ok()
    {
        _destination.Items.Length.ShouldBe(0);
    }
}

public class When_mapping_null_array_to_list : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public SourceItem[] Items { get; set; }
    }

    class Destination
    {
        public List<DestinationItem> Items { get; set; }
    }

    class SourceItem
    {
        public int Value { get; set; }
    }

    class DestinationItem
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<SourceItem, DestinationItem>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source());
    }

    [Fact]
    public void Should_map_ok()
    {
        _destination.Items.Count.ShouldBe(0);
    }
}

public class When_mapping_collections : AutoMapperSpecBase
{
    Author mappedAuthor;

    protected override MapperConfiguration CreateConfiguration() => new(delegate{});

    protected override void Because_of()
    {
        dynamic authorDynamic = new ExpandoObject();
        authorDynamic.Name = "Charles Dickens";
        dynamic book1 = new ExpandoObject();
        book1.Name = "Great Expectations";
        dynamic book2 = new ExpandoObject();
        book2.Name = "Oliver Twist";
        authorDynamic.Books = new List<object> { book1, book2 };
        mappedAuthor = Mapper.Map<Author>((object)authorDynamic);
    }

    [Fact]
    public void Should_map_by_item_type()
    {
        mappedAuthor.Name.ShouldBe("Charles Dickens");
        mappedAuthor.Books[0].Name.ShouldBe("Great Expectations");
        mappedAuthor.Books[1].Name.ShouldBe("Oliver Twist");
    }

    public class Author
    {
        public string Name { get; set; }
        public Book[] Books { get; set; }
    }

    public class Book
    {
        public string Name { get; set; }
    }
}

public class When_mapping_to_an_existing_HashSet_typed_as_IEnumerable : AutoMapperSpecBase
{
    private Destination _destination = new Destination();

    public class Source
    {
        public int[] IntCollection { get; set; } = new int[0];
    }

    public class Destination
    {
        public IEnumerable<int> IntCollection { get; set; } = new HashSet<int> { 1, 2, 3, 4, 5 };
        public string Unmapped { get; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map(new Source(), _destination);
    }

    [Fact]
    public void Should_clear_the_destination()
    {
        _destination.IntCollection.Count().ShouldBe(0);
    }
}

public class When_mapping_to_an_existing_array_typed_as_IEnumerable : AutoMapperSpecBase
{
    private Destination _destination = new Destination();

    public class Source
    {
        public int[] IntCollection { get; set; } = new int[0];
    }

    public class Destination
    {
        public IEnumerable<int> IntCollection { get; set; } = new[] { 1, 2, 3, 4, 5 };
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map(new Source(), _destination);
    }

    [Fact]
    public void Should_create_destination_array_the_same_size_as_the_source()
    {
        _destination.IntCollection.Count().ShouldBe(0);
    }
}

public class When_mapping_to_a_concrete_non_generic_ienumerable : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
        public List<int> Values2 { get; set; }
    }

    public class Destination
    {
        public IEnumerable Values { get; set; }
        public IEnumerable Values2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain(1);
        _destination.Values.ShouldContain(2);
        _destination.Values.ShouldContain(3);
        _destination.Values.ShouldContain(4);
    }

    [Fact]
    public void Should_map_from_the_generic_list_of_values()
    {
        _destination.Values2.ShouldNotBeNull();
        _destination.Values2.ShouldContain(9);
        _destination.Values2.ShouldContain(8);
        _destination.Values2.ShouldContain(7);
        _destination.Values2.ShouldContain(6);
    }
}

public class When_mapping_to_a_concrete_generic_ienumerable : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
        public List<int> Values2 { get; set; }
    }

    public class Destination
    {
        public IEnumerable<int> Values { get; set; }
        public IEnumerable<string> Values2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain(1);
        _destination.Values.ShouldContain(2);
        _destination.Values.ShouldContain(3);
        _destination.Values.ShouldContain(4);
    }

    [Fact]
    public void Should_map_from_the_generic_list_of_values_with_formatting()
    {
        _destination.Values2.ShouldNotBeNull();
        _destination.Values2.ShouldContain("9");
        _destination.Values2.ShouldContain("8");
        _destination.Values2.ShouldContain("7");
        _destination.Values2.ShouldContain("6");
    }
}

public class When_mapping_to_a_getter_only_ienumerable : AutoMapperSpecBase
{
    private Destination _destination = new Destination();
    public class Source
    {
        public int[] Values { get; set; }
        public List<int> Values2 { get; set; }
    }
    public class Destination
    {
        public IEnumerable<int> Values { get; } = new List<int>();
        public IEnumerable<string> Values2 { get; } = new List<string>();
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });
    protected override void Because_of() => _destination = Mapper.Map<Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldBe(new[] { 1, 2, 3, 4 });
        _destination.Values2.ShouldBe(new[] { "9", "8", "7", "6" });
    }
}

public class When_mapping_to_a_getter_only_existing_ienumerable : AutoMapperSpecBase
{
    private Destination _destination = new Destination();
    public class Source
    {
        public int[] Values { get; set; }
        public List<int> Values2 { get; set; }
    }
    public class Destination
    {
        public IEnumerable<int> Values { get; } = new List<int>();
        public IEnumerable<string> Values2 { get; } = new List<string>();
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });
    protected override void Because_of() => Mapper.Map(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } }, _destination);
    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldBe(new[] { 1, 2, 3, 4 });
        _destination.Values2.ShouldBe(new[]{ "9", "8", "7", "6" });
    }
}

public class When_mapping_to_a_concrete_non_generic_icollection : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
        public List<int> Values2 { get; set; }
    }

    public class Destination
    {
        public ICollection Values { get; set; }
        public ICollection Values2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 }, Values2 = new List<int> { 9, 8, 7, 6 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain(1);
        _destination.Values.ShouldContain(2);
        _destination.Values.ShouldContain(3);
        _destination.Values.ShouldContain(4);
    }

    [Fact]
    public void Should_map_from_a_non_array_source()
    {
        _destination.Values2.ShouldNotBeNull();
        _destination.Values2.ShouldContain(9);
        _destination.Values2.ShouldContain(8);
        _destination.Values2.ShouldContain(7);
        _destination.Values2.ShouldContain(6);
    }
}

public class When_mapping_to_a_concrete_generic_icollection : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
    }

    public class Destination
    {
        public ICollection<string> Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain("1");
        _destination.Values.ShouldContain("2");
        _destination.Values.ShouldContain("3");
        _destination.Values.ShouldContain("4");
    }
}

public class When_mapping_to_a_concrete_ilist : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
    }

    public class Destination
    {
        public IList Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain(1);
        _destination.Values.ShouldContain(2);
        _destination.Values.ShouldContain(3);
        _destination.Values.ShouldContain(4);
    }
}

public class When_mapping_to_a_concrete_generic_ilist : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
    }

    public class Destination
    {
        public IList<string> Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source { Values = new[] { 1, 2, 3, 4 } });
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldContain("1");
        _destination.Values.ShouldContain("2");
        _destination.Values.ShouldContain("3");
        _destination.Values.ShouldContain("4");
    }
}

public class When_mapping_to_a_custom_list_with_the_same_type : AutoMapperSpecBase
{
    private Destination _destination;
    private Source _source;

    public class ValueCollection : Collection<int>
    {
    }

    public class Source
    {
        public ValueCollection Values { get; set; }
    }

    public class Destination
    {
        public ValueCollection Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _source = new Source { Values = new ValueCollection { 1, 2, 3, 4 } };
        _destination = Mapper.Map<Source, Destination>(_source);
    }

    [Fact]
    public void Should_assign_the_value_directly()
    {
        _source.Values.ShouldBe(_destination.Values);
    }
}
public class When_mapping_to_a_collection_with_instantiation_managed_by_the_destination : AutoMapperSpecBase
{
    private Destination _destination;
    private Source _source;

    public class SourceItem
    {
        public int Value { get; set; }
    }

    public class DestItem
    {
        public int Value { get; set; }
    }

    public class Source
    {
        public List<SourceItem> Values { get; set; }
    }

    public class Destination
    {
        private List<DestItem> _values = new List<DestItem>();

        public List<DestItem> Values
        {
            get { return _values; }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
        cfg.CreateMap<SourceItem, DestItem>();
    });

    protected override void Because_of()
    {
        _source = new Source { Values = new List<SourceItem> { new SourceItem { Value = 5 }, new SourceItem { Value = 10 } } };
        _destination = Mapper.Map<Source, Destination>(_source);
    }

    [Fact]
    public void Should_assign_the_value_directly()
    {
        _destination.Values.Count.ShouldBe(2);
        _destination.Values[0].Value.ShouldBe(5);
        _destination.Values[1].Value.ShouldBe(10);
    }
}

public class When_mapping_to_an_existing_list_with_existing_items : AutoMapperSpecBase
{
    private Destination _destination;
    private Source _source;

    public class SourceItem
    {
        public int Value { get; set; }
    }

    public class DestItem
    {
        public int Value { get; set; }
    }

    public class Source
    {
        public List<SourceItem> Values { get; set; }
    }

    public class Destination
    {
        private List<DestItem> _values = new List<DestItem>();

        public List<DestItem> Values
        {
            get { return _values; }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Values, opt => opt.UseDestinationValue());
        cfg.CreateMap<SourceItem, DestItem>();
    });

    protected override void Because_of()
    {
        _source = new Source { Values = new List<SourceItem> { new SourceItem { Value = 5 }, new SourceItem { Value = 10 } } };
        _destination = new Destination();
        _destination.Values.Add(new DestItem());
        Mapper.Map(_source, _destination);
    }

    [Fact]
    public void Should_clear_the_list_before_mapping()
    {
        _destination.Values.Count.ShouldBe(2);
    }
}

public class When_mapping_to_getter_only_list_with_existing_items : AutoMapperSpecBase
{
    public class SourceItem
    {
        public int Value { get; set; }
    }
    public class DestItem
    {
        public int Value { get; set; }
    }
    public class Source
    {
        public List<SourceItem> Values { get; set; }
        public List<SourceItem> IValues { get; set; }
    }
    public class Destination
    {
        public List<DestItem> Values { get; } = new();
        public IEnumerable<DestItem> IValues { get; } = new List<DestItem>();
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<SourceItem, DestItem>();
    });
    [Fact]
    public void Should_clear_the_list_before_mapping()
    {
        var destination = new Destination { Values = { new DestItem() } };
        ((List<DestItem>)destination.IValues).Add(new DestItem());
        Mapper.Map(new Source(), destination);
        destination.Values.ShouldBeEmpty();
        destination.IValues.ShouldBeEmpty();
    }
}
public class When_mapping_to_list_with_existing_items : AutoMapperSpecBase
{
    public class SourceItem
    {
        public int Value { get; set; }
    }
    public class DestItem
    {
        public int Value { get; set; }
    }
    public class Source
    {
        public List<SourceItem> Values { get; set; } = new();
    }
    public class Destination
    {
        public List<DestItem> Values { get; set; } = new();
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<SourceItem, DestItem>();
    });
    [Fact]
    public void Should_clear_the_list_before_mapping()
    {
        var destination = new Destination { Values = { new DestItem { } } };
        Mapper.Map(new Source { Values = { new SourceItem { Value = 42 } } }, destination);
        destination.Values.Single().Value.ShouldBe(42);
    }
    [Fact]
    public void Should_clear_the_list_before_mapping_when_the_source_is_null()
    {
        var destination = new Destination { Values = { new DestItem { } } };
        Mapper.Map(new Source { Values = null }, destination);
        destination.Values.ShouldBeEmpty();
    }
}

public class When_mapping_a_collection_with_null_members : AutoMapperSpecBase
{
    const string FirstString = null;

    private IEnumerable<string> _strings = new List<string> { FirstString };
    private List<string> _mappedStrings = new List<string>();

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullDestinationValues = true;
    });

    protected override void Because_of()
    {
        _mappedStrings = Mapper.Map<IEnumerable<string>, List<string>>(_strings);
    }

    [Fact]
    public void Should_map_correctly()
    {
        _mappedStrings.ShouldNotBeNull();
        _mappedStrings.Count.ShouldBe(1);
        _mappedStrings[0].ShouldBeNull();
    }
}