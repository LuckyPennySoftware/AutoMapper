using System.Collections.Specialized;
using System.Collections.Immutable;
namespace AutoMapper.UnitTests;
public class UnsupportedCollection : AutoMapperSpecBase
{
    class Source
    {
        public MyList<DateTime> List { get; set; } = new();
    }
    class Destination
    {
        public MyList<int> List { get; set; }
    }
    class MyList<T> : IEnumerable
    {
        public IEnumerator GetEnumerator() => new List<T>.Enumerator();
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<Source, Destination>());
    [Fact]
    public void ThrowsAtMapTime() => new Action(()=>Map<Destination>(new Source())).ShouldThrow<AutoMapperMappingException>()
        .InnerException.ShouldBeOfType<NotSupportedException>().Message.ShouldBe($"Unknown collection. Consider a custom type converter from {typeof(MyList<DateTime>)} to {typeof(MyList<int>)}.");
}
#if NET8_0_OR_GREATER
public class When_mapping_interface_to_interface_readonly_set : AutoMapperSpecBase
{
    public class Source
    {
        public IReadOnlySet<int> Values { get; set; }
    }
    public class Destination
    {
        public IReadOnlySet<int> Values { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(config => config.CreateMap<Source, Destination>());
    [Fact]
    public void Should_map_readonly_values()
    {
        HashSet<int> values = [1, 2, 3, 4];
        Map<Destination>(new Source { Values = values }).Values.ShouldBe(values);
    }
}
public class When_mapping_hashset_to_interface_readonly_set : AutoMapperSpecBase
{
    public class Source
    {
        public HashSet<int> Values { get; set; }
    }
    public class Destination
    {
        public IReadOnlySet<int> Values { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(config => config.CreateMap<Source, Destination>());
    [Fact]
    public void Should_map_readonly_values()
    {
        HashSet<int> values = [1, 2, 3, 4];
        Map<Destination>(new Source { Values = values }).Values.ShouldBe(values);
    }
}
#endif
public class NonPublicEnumeratorCurrent : AutoMapperSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public MyJObject Value { get; set; }
    }
    class MyJObject : List<int>
    {
	        public new MyEnumerator GetEnumerator() => new(base.GetEnumerator());
    }
    class MyEnumerator : IEnumerator
    {
        IEnumerator _enumerator;
        public MyEnumerator(IEnumerator enumerator)
        {
            _enumerator = enumerator;
        }
        object IEnumerator.Current => _enumerator.Current;
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom(_=>new MyJObject { 1, 2, 3 })));
    [Fact]
    public void Should_work() => Map<Destination>(new Source()).Value.ShouldBe(new[] { 1, 2, 3 });
}
public class ImmutableCollection : AutoMapperSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public ImmutableArray<int> Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom(_=>ImmutableArray.Create<int>())));
    [Fact]
    public void Should_work() => Map<Destination>(new Source()).Value.ShouldBeOfType<ImmutableArray<int>>();
}
public class AssignableCollection : AutoMapperSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public MyJObject Value { get; set; }
    }
    class MyJObject : IEnumerable
    {
        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom(_=>new MyJObject())));
    [Fact]
    public void Should_work() => Map<Destination>(new Source()).Value.ShouldBeOfType<MyJObject>();
}
public class RecursiveCollection : AutoMapperSpecBase
{
    class Source
    {
        public string Value { get; set; }
    }
    class Destination
    {
        public MyJObject Value { get; set; }
    }
    class MyJObject : List<MyJObject>{}
    protected override MapperConfiguration CreateConfiguration() => new(c => 
        c.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom(_=>new MyJObject())));
    [Fact]
    public void Should_work() => Map<Destination>(new Source()).Value.ShouldBeOfType<MyJObject>();
}
public class AmbigousMethod : AutoMapperSpecBase
{
    public class Source
    {
        public string Value { get; set; }
    }
    public class Destination
    {
        public string Value { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateMap<Source, Destination>());
    [Fact]
    public void Should_work() => Map<Destination[]>(new[] { new Source() }.OrderBy(s => s.Value));
}
public class Enumerator_disposable_at_runtime_class : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new EnumeratorBase GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator(), this);
            return _enumerator;
        }
        public bool Disposed { get; set; }
        public class EnumeratorBase
        {
            public EnumeratorBase(IEnumerator<T> enumerator, CustomList<T> list)
            {
                Enumerator = enumerator;
                List = list;
            }
            public IEnumerator<T> Enumerator { get; }
            public CustomList<T> List { get; }
            public T Current => Enumerator.Current;
            public void Dispose()
            {
                Enumerator.Dispose();
                List.Disposed = true;
            }
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }
        public class CustomEnumerator : EnumeratorBase, IDisposable
        {
            public CustomEnumerator(IEnumerator<T> enumerator, CustomList<T> list) : base(enumerator, list) { }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_call_dispose()
    {
        var source = new CustomList<int>();
        Mapper.Map<List<int>>(source);
        source.Disposed.ShouldBeTrue();
    }
}
public class Enumerator_non_disposable_struct : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new CustomEnumerator GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator(), this);
            return _enumerator;
        }
        public bool Disposed { get; set; }
        public struct CustomEnumerator
        {
            public CustomEnumerator(IEnumerator<T> enumerator, CustomList<T> list)
            {
                Enumerator = enumerator;
                List = list;
            }
            public IEnumerator<T> Enumerator { get; }
            public CustomList<T> List { get; }
            public T Current => Enumerator.Current;
            public void Dispose()
            {
                Enumerator.Dispose();
                List.Disposed = true;
            }
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_not_call_dispose()
    {
        var source = new CustomList<int>();
        Mapper.Map<List<int>>(source);
        source.Disposed.ShouldBeFalse();
    }
}
public class Enumerator_dispose : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new IEnumerator<T> GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator());
            return _enumerator;
        }
        public bool Disposed => _enumerator.Disposed;
        class CustomEnumerator : IEnumerator<T>
        {
            public CustomEnumerator(IEnumerator<T> enumerator) => Enumerator = enumerator;
            public bool Disposed { get; set; }
            public IEnumerator<T> Enumerator { get; }
            public T Current => Enumerator.Current;
            object IEnumerator.Current => Enumerator.Current;
            public void Dispose()
            {
                Enumerator.Dispose();
                Disposed = true;
            }
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_call_dispose()
    {
        var source = new CustomList<int>();
        Mapper.Map<List<int>>(source);
        source.Disposed.ShouldBeTrue();
    }
}

public class Enumerator_dispose_exception : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new IEnumerator<T> GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator());
            return _enumerator;
        }
        public bool Disposed => _enumerator.Disposed;
        class CustomEnumerator : IEnumerator<T>
        {
            public CustomEnumerator(IEnumerator<T> enumerator) => Enumerator = enumerator;
            public bool Disposed { get; set; }
            public IEnumerator<T> Enumerator { get; }
            public T Current => Enumerator.Current;
            object IEnumerator.Current => Enumerator.Current;
            public void Dispose()
            {
                Enumerator.Dispose();
                Disposed = true;
            }
            public bool MoveNext() => throw new NotImplementedException();
            public void Reset() => Enumerator.Reset();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_call_dispose()
    {
        var source = new CustomList<int>();
        try
        {
            Mapper.Map<List<int>>(source);
        }
        catch
        {
        }
        source.Disposed.ShouldBeTrue();
    }
}

public class Enumerator_dispose_struct : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new CustomEnumerator GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator(), this);
            return _enumerator;
        }
        public bool Disposed { get; set; }
        public struct CustomEnumerator : IEnumerator<T>
        {
            public CustomEnumerator(IEnumerator<T> enumerator, CustomList<T> list)
            {
                Enumerator = enumerator;
                List = list;
            }
            public IEnumerator<T> Enumerator { get; }
            public CustomList<T> List { get; }
            public T Current => Enumerator.Current;
            object IEnumerator.Current => Enumerator.Current;
            public void Dispose()
            {
                Enumerator.Dispose();
                List.Disposed = true;
            }
            public bool MoveNext() => Enumerator.MoveNext();
            public void Reset() => Enumerator.Reset();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_call_dispose()
    {
        var source = new CustomList<int>();
        Mapper.Map<List<int>>(source);
        source.Disposed.ShouldBeTrue();
    }
}

public class Enumerator_dispose_exception_struct : AutoMapperSpecBase
{
    class CustomList<T> : List<T>
    {
        private CustomEnumerator _enumerator;

        public new CustomEnumerator GetEnumerator()
        {
            _enumerator = new CustomEnumerator(base.GetEnumerator(), this);
            return _enumerator;
        }
        public bool Disposed { get; set; }
        public struct CustomEnumerator : IEnumerator<T>
        {
            public CustomEnumerator(IEnumerator<T> enumerator, CustomList<T> list)
            {
                Enumerator = enumerator;
                List = list;
            }
            public IEnumerator<T> Enumerator { get; }
            public T Current => Enumerator.Current;
            object IEnumerator.Current => Enumerator.Current;
            public CustomList<T> List { get; }
            public void Dispose()
            {
                Enumerator.Dispose();
                List.Disposed = true;
            }
            public bool MoveNext() => throw new NotImplementedException();
            public void Reset() => Enumerator.Reset();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(_ => { });

    [Fact]
    public void Should_call_dispose()
    {
        var source = new CustomList<int>();
        try
        {
            Mapper.Map<List<int>>(source);
        }
        catch
        {
        }
        source.Disposed.ShouldBeTrue();
    }
}

public class When_mapping_to_existing_observable_collection : AutoMapperSpecBase
{
    class CollectionHolder
    {
        public CollectionHolder()
        {
            Observable = new ObservableCollection<List<int>>();
        }

        public ObservableCollection<List<int>> Observable { get; set; }
    }

    class CollectionHolderDto
    {
        public CollectionHolderDto()
        {
            Observable = new List<List<int>>();
        }

        public List<List<int>> Observable { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<CollectionHolderDto, CollectionHolder>().ForMember(a => a.Observable, opt => opt.UseDestinationValue()));

    [Fact]
    public void Should_map_ok()
    {
        var ch = new CollectionHolderDto();
        var list = new List<int>{ 5, 6 };
        ch.Observable.Add(list);
        var mapped = Mapper.Map<CollectionHolder>(ch);
        mapped.Observable.Single().ShouldBe(list);
    }
}

public class When_mapping_to_member_typed_as_IEnumerable : AutoMapperSpecBase
{
    public class SourceItem { }
    public class DestItem { }
    public class SourceA
    {
        public IEnumerable<SourceItem> Items { get; set; }
        public IEnumerable<SourceB> Bs { get; set; } // Problem
    }

    public class SourceB
    {
        public IEnumerable<SourceItem> Items { get; set; }
    }

    public class DestA
    {
        public IEnumerable<DestItem> Items { get; set; }
        public IEnumerable<DestB> Bs { get; set; } // Problem
    }

    public class DestB
    {
        public IEnumerable<DestItem> Items { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<SourceA, DestA>();
        cfg.CreateMap<SourceB, DestB>();
        cfg.CreateMap<SourceItem, DestItem>();
    });

    [Fact]
    public void Should_map_ok()
    {
        Mapper.Map<DestB>(new SourceB()).Items.ShouldBeEmpty();
    }
}

public class When_mapping_to_existing_collection_typed_as_IEnumerable : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(_=>{ });

    [Fact]
    public void Should_map_ok()
    {
        IEnumerable<int> destination = new List<int>();
        var source = Enumerable.Range(1, 10).ToArray();
        Mapper.Map(source, destination);
        destination.SequenceEqual(source).ShouldBeTrue();
    }
}

public class When_mapping_to_readonly_property_as_IEnumerable_and_existing_destination : AutoMapperSpecBase
{
    public class Source
    {
        private readonly List<string> _myCollection = new List<string> { "one", "two" };

        public string[] MyCollection => _myCollection.ToArray();
    }

    public class Destination
    {
        private IList<string> _myCollection = new List<string>();
        public IEnumerable<string> MyCollection => _myCollection;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Destination>().ForMember(m => m.MyCollection, opt =>
        {
            opt.MapFrom(src => src.MyCollection);
        }));

    [Fact]
    public void Should_map_ok()
    {
        Mapper.Map(new Source(), new Destination())
            .MyCollection.SequenceEqual(new[] { "one", "two" }).ShouldBeTrue();
    }
}

public class When_mapping_to_readonly_collection_without_setter : AutoMapperSpecBase
{
    public class Source
    {
        public IEnumerable<string> MyCollection { get; } = new[] { "one", "two" };
    }
    public class Destination
    {
        public IEnumerable<string> MyCollection { get; } = new ReadOnlyCollection<string>(new string[0]);
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void Should_fail() => new Action(() => Mapper.Map(new Source(), new Destination()))
        .ShouldThrow<AutoMapperMappingException>()
        .InnerException.ShouldBeOfType<NotSupportedException>()
        .Message.ShouldBe("Collection is read-only.");
}

public class When_mapping_to_readonly_property_UseDestinationValue : AutoMapperSpecBase
{
    public class Source
    {
        private readonly List<string> _myCollection = new List<string> { "one", "two" };

        public string[] MyCollection => _myCollection.ToArray();
    }

    public class Destination
    {
        private IList<string> _myCollection = new List<string>();
        public IEnumerable<string> MyCollection => _myCollection;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateMap<Source, Destination>().ForMember(m => m.MyCollection, opt =>
        {
            opt.MapFrom(src => src.MyCollection);
        }));

    [Fact]
    public void Should_map_ok()
    {
        Mapper.Map<Destination>(new Source())
            .MyCollection.SequenceEqual(new[] { "one", "two" }).ShouldBeTrue();
    }
}

public class When_mapping_to_readonly_property_as_IEnumerable : AutoMapperSpecBase
{
    public class Source
    {
        private readonly List<string> _myCollection = new List<string> { "one", "two" };

        public string[] MyCollection => _myCollection.ToArray();
    }

    public class Destination
    {
        private IList<string> _myCollection = new List<string>();
        public IEnumerable<string> MyCollection => _myCollection;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => 
        cfg.CreateMap<Source, Destination>().ForMember(m => m.MyCollection, opt =>
            {
                opt.MapFrom(src => src.MyCollection);
                opt.UseDestinationValue();
            }));

    [Fact]
    public void Should_map_ok()
    {
        Mapper.Map<Destination>(new Source())
            .MyCollection.SequenceEqual(new[] { "one", "two" }).ShouldBeTrue();
    }
}

public class When_mapping_from_struct_collection : AutoMapperSpecBase
{
    public struct MyCollection : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            for(int i = 1; i <= 10; i++)
            {
                yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SourceItem
    {
        public string Name { get; set; }
        public MyCollection ShipsTo { get; set; }
    }

    public class DestItem
    {
        public string Name { get; set; }
        public List<int> ShipsTo { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() =>
        new MapperConfiguration(cfg => cfg.CreateMap<SourceItem, DestItem>());

    [Fact]
    public void Should_map_ok()
    {
        Mapper.Map<DestItem>(new SourceItem { ShipsTo = new MyCollection() })
            .ShipsTo.SequenceEqual(Enumerable.Range(1, 10)).ShouldBeTrue();
    }
}

public class When_mapping_to_custom_collection_type : AutoMapperSpecBase
{
    public class MyCollection : CollectionBase
    {
    }

    public class SourceItem
    {
        public string Name { get; set; }
        public List<string> ShipsTo { get; set; }
    }

    public class DestItem
    {
        public string Name { get; set; }
        public MyCollection ShipsTo { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() =>
        new MapperConfiguration(cfg => cfg.CreateMap<SourceItem, DestItem>());

    [Fact]
    public void Should_map_ok()
    {
        var items = Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray();
        Mapper.Map<DestItem>(new SourceItem { ShipsTo = new List<string>(items) })
            .ShipsTo.Cast<string>().SequenceEqual(items).ShouldBeTrue();
    }
}

public class When_mapping_to_unknown_collection_type : NonValidatingSpecBase
{
    public class MyCollection
    {
    }

    public class SourceItem
    {
        public string Name { get; set; }
        public List<string> ShipsTo { get; set; }
    }

    public class DestItem
    {
        public string Name { get; set; }
        public MyCollection ShipsTo { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => 
        new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceItem, DestItem>();
        });

    [Fact]
    public void Should_report_missing_map()
    {
        new Action(AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(ex =>
        {
            ex.MemberMap.SourceMember.ShouldBe(typeof(SourceItem).GetProperty("ShipsTo"));
            ex.Types.Value.ShouldBe(new TypePair(typeof(SourceItem), typeof(DestItem)));
        });
    } 
}

public class When_mapping_collections_with_inheritance : AutoMapperSpecBase
{
    public class Source
    {
        public IEnumerable<SourceItem> Items { get; set; }
    }
    public class Destination
    {
        public IEnumerable<DestinationItemBase> Items { get; set; }
    }
    public class SourceItem
    {
        public int Value { get; set; }
    }
    public class DestinationItemBase
    {
        public int Value { get; set; }
    }
    public class SpecificDestinationItem : DestinationItemBase
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<SourceItem, DestinationItemBase>().As<SpecificDestinationItem>();
        cfg.CreateMap<SourceItem, SpecificDestinationItem>();
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}

public class When_passing_a_not_empty_collection : AutoMapperSpecBase
{
    Destination _destination = new Destination();

    class Source
    {
        public List<SourceItem> Items { get; }
    }

    class SourceItem
    {
    }

    class Destination
    {
        public List<DestinationItem> Items { get; } = new List<DestinationItem> { new DestinationItem() };
    }

    class DestinationItem
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<SourceItem, DestinationItem>();
    });

    protected override void Because_of()
    {
        Mapper.Map(new Source(), _destination);
    }

    [Fact]
    public void It_should_be_cleared_first()
    {
        _destination.Items.ShouldBeEmpty();
    }
}

public class When_mapping_collections_with_structs : AutoMapperSpecBase
{
    BarDTO _destination;

    public struct Foo { }
    public struct Bar
    {
        public IEnumerable<Foo> Foos { get; set; }
    }

    public struct FooDTO { }
    public struct BarDTO
    {
        public IEnumerable<FooDTO> Foos { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Bar, BarDTO>();
        cfg.CreateMap<Foo, FooDTO>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<BarDTO>(new Bar { Foos = new Foo[5] });
    }

    [Fact]
    public void Should_map_ok()
    {
        _destination.Foos.SequenceEqual(new FooDTO[5]).ShouldBeTrue();
    }
}

public class CollectionMapping
{
    public class MasterWithList
    {
        private IList<Detail> _details = new List<Detail>();

        public int Id { get; set; }

        public IList<Detail> Details
        {
            get { return _details; }
            set { _details = value; }
        }
    }

    public class MasterWithCollection
    {
        public MasterWithCollection(ICollection<Detail> details)
        {
            Details = details;
        }

        public int Id { get; set; }

        public ICollection<Detail> Details { get; set; }
    }

    public class MasterWithNoExistingCollection
    {
        public int Id { get; set; }
        public HashSet<Detail> Details { get; set; }
    }

    public class Detail
    {
        public int Id { get; set; }
    }

    public class MasterDto
    {
        public int Id { get; set; }
        public DetailDto[] Details { get; set; }
    }

    public class DetailDto
    {
        public int Id { get; set; }
    }

    private static IMapper mapper;

    private static void FillCollection<TSource, TDestination, TSourceItem, TDestinationItem>(
        TSource s, TDestination d,
        Func<TSource, IEnumerable<TSourceItem>> getSourceEnum,
        Func<TDestination, ICollection<TDestinationItem>> getDestinationColl)
    {
        ICollection<TDestinationItem> collection = getDestinationColl(d);
        collection.Clear();
        foreach (TSourceItem sourceItem in getSourceEnum(s))
        {
            collection.Add(mapper.Map<TSourceItem, TDestinationItem>(sourceItem));
        }
    }

    [Fact]
    public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_list()
    {
        var config = new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithCollection(new List<Detail>());
        ICollection<Detail> originalCollection = master.Details;

        config.CreateMapper().Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
        originalCollection.Count.ShouldBe(master.Details.Count);
    }

    [Fact]
    public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithCollection(new HashSet<Detail>());
        ICollection<Detail> originalCollection = master.Details;

        config.CreateMapper().Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
        originalCollection.Count.ShouldBe(master.Details.Count);
    }

    [Fact]
    public void Should_keep_and_fill_destination_collection_when_collection_is_implemented_as_set_with_aftermap()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, o => o.Ignore())
                .AfterMap((s, d) => FillCollection(s, d, ss => ss.Details, dd => dd.Details));
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithCollection(new HashSet<Detail>());
        ICollection<Detail> originalCollection = master.Details;

        mapper = config.CreateMapper();

        mapper.Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
        originalCollection.Count.ShouldBe(master.Details.Count);
    }

    [Fact]
    public void Should_keep_and_fill_destination_list()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MasterDto, MasterWithList>()
                .ForMember(d => d.Details, o => o.UseDestinationValue());
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithList();
        IList<Detail> originalCollection = master.Details;

        config.CreateMapper().Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
        originalCollection.Count.ShouldBe(master.Details.Count);
    }

    [Fact]
    public void Should_not_replace_destination_collection()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MasterDto, MasterWithCollection>()
                .ForMember(d => d.Details, opt => opt.UseDestinationValue());
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithCollection(new List<Detail>());
        ICollection<Detail> originalCollection = master.Details;

        config.CreateMapper().Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
    }

    [Fact]
    public void Should_be_able_to_map_to_a_collection_type_that_implements_ICollection_of_T()
    {
        var config = new MapperConfiguration(cfg =>
        {

            cfg.CreateMap<MasterDto, MasterWithNoExistingCollection>();
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = config.CreateMapper().Map<MasterDto, MasterWithNoExistingCollection>(dto);

        master.Details.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_not_replace_destination_list()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MasterDto, MasterWithList>()
                .ForMember(d => d.Details, opt => opt.UseDestinationValue());
            cfg.CreateMap<DetailDto, Detail>();
        });

        var dto = new MasterDto
        {
            Id = 1,
            Details = new[]
            {
                new DetailDto {Id = 2},
                new DetailDto {Id = 3},
            }
        };

        var master = new MasterWithList();
        IList<Detail> originalCollection = master.Details;

        config.CreateMapper().Map(dto, master);

        originalCollection.ShouldBeSameAs(master.Details);
    }

    [Fact]
    public void Should_map_to_NameValueCollection() {
        var c = new NameValueCollection();
        var config = new MapperConfiguration(cfg => { });
        var mappedCollection = config.CreateMapper().Map<NameValueCollection, NameValueCollection>(c);
        mappedCollection.ShouldNotBeSameAs(c);
        mappedCollection.ShouldNotBeNull();
    }
}

public class When_mapping_from_ICollection_types_but_implementations_are_different : AutoMapperSpecBase
{
    public class Source
    {
        public ICollection<Item> Items { get; set; }

        public class Item
        {
            public int Value { get; set; }
        }
    }
    public class Dest
    {
        public ICollection<Item> Items { get; set; } = new HashSet<Item>();

        public class Item
        {
            public int Value { get; set; }
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
        cfg.CreateMap<Source.Item, Dest.Item>();
    });

    [Fact]
    public void Should_map_items()
    {
        var source = new Source
        {
            Items = new List<Source.Item>
            {
                new Source.Item { Value = 5 }
            }
        };
        var dest = new Dest();

        Mapper.Map(source, dest);

        dest.Items.Count.ShouldBe(1);
        dest.Items.First().Value.ShouldBe(5);
    }
}

public class When_mapping_enumerable_to_array : AutoMapperSpecBase
{
    public class Source
    {
        public int X { get; set; }
        public IEnumerable<SourceItem> Items { get; set; }
    }

    public class SourceItem
    {
        public int I { get; set; }
    }

    public class Target
    {
        public int X { get; set; }
        public TargetItem[] Items { get; set; }
    }

    public class TargetItem
    {
        public int I { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullCollections = true;

        cfg.CreateMap<Source, Target>();
        cfg.CreateMap<SourceItem, TargetItem>();
    });

    [Fact]
    public void IncludedMappings()
    {
        var src = new Source
        {
            X = 5,
            Items = new List<SourceItem>
            {
                new SourceItem {I = 1},
                new SourceItem {I = 2},
                new SourceItem {I = 3}
            }
        };

        var dest = Mapper.Map<Source, Target>(src);

        src.X.ShouldBe(dest.X);

        dest.Items.Length.ShouldBe(3);
        dest.Items[0].I.ShouldBe(1);
        dest.Items[1].I.ShouldBe(2);
        dest.Items[2].I.ShouldBe(3);
    }
}
