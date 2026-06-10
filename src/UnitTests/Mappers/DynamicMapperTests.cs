using System.Dynamic;

namespace AutoMapper.UnitTests.Mappers.Dynamic;

class Destination
{
    public string Foo { get; set; }
    public string Bar { get; set; }
    internal string Jack { get; set; }
    public int[] Data { get; set; }
    public int Baz { get; set; }
}

public class DynamicDictionary : DynamicObject
{
    private readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return dictionary.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        dictionary[binder.Name] = value;
        return true;
    }

    public int Count => dictionary.Count;
}

public class When_mapping_to_dynamic_from_getter_only_property
{
    class Source
    {
        public Source()
        {
            Value = 24;
        }

        public int Value { get; }
    }

    [Fact]
    public void Should_map_source_properties()
    {
        var config = new MapperConfiguration(cfg => { });
        dynamic destination = config.CreateMapper().Map<DynamicDictionary>(new Source());
        ((int)destination.Count).ShouldBe(1);
        Assert.Equal(24, destination.Value);
    }
}

public class When_mapping_to_dynamic
{
    dynamic _destination;

    [Fact]
    public void Should_map_source_properties()
    {
        var config = new MapperConfiguration(cfg => { });
        var data = new[] { 1, 2, 3 };
        _destination = config.CreateMapper().Map<DynamicDictionary>(new Destination { Foo = "Foo", Bar = "Bar", Data = data, Baz = 12 });
        ((int)_destination.Count).ShouldBe(4);
        Assert.Equal("Foo", _destination.Foo);
        Assert.Equal("Bar", _destination.Bar);
        Assert.Equal(12, _destination.Baz);
        ((int[])_destination.Data).SequenceEqual(data).ShouldBeTrue();
    }
    [Fact]
    public void Should_map_to_ExpandoObject()
    {
        var config = new MapperConfiguration(cfg => { });
        var data = new[] { 1, 2, 3 };
        _destination = config.CreateMapper().Map<ExpandoObject>(new Destination { Foo = "Foo", Bar = "Bar", Data = data, Baz = 12 });
        ((IDictionary<string, object>)_destination).Count.ShouldBe(4);
        Assert.Equal("Foo", _destination.Foo);
        Assert.Equal("Bar", _destination.Bar);
        Assert.Equal(12, _destination.Baz);
        ((int[])_destination.Data).SequenceEqual(data).ShouldBeTrue();
    }
}

public class When_mapping_from_dynamic
{
    Destination _destination;

    [Fact]
    public void Should_map_destination_properties()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        source.Bar = "Bar";
        source.Jack = "Jack";
        var config = new MapperConfiguration(cfg => { });
        _destination = config.CreateMapper().Map<Destination>((object)source);
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBe("Bar");
        _destination.Jack.ShouldBeNull();
    }
}

public class When_mapping_struct_from_dynamic
{
    Destination _destination;

    struct Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        internal string Jack { get; set; }
    }

    [Fact]
    public void Should_map_destination_properties()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        source.Bar = "Bar";
        source.Jack = "Jack";
        var config = new MapperConfiguration(cfg => { });
        _destination = config.CreateMapper().Map<Destination>((object)source);
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBe("Bar");
        _destination.Jack.ShouldBeNull();
    }
}

public class When_mapping_from_dynamic_with_missing_property
{
    [Fact]
    public void Should_map_existing_properties()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        var config = new MapperConfiguration(cfg => { });
        var destination = config.CreateMapper().Map<Destination>((object)source);
        destination.Foo.ShouldBe("Foo");
        destination.Bar.ShouldBeNull();
    }
    [Fact]
    public void Should_keep_existing_value()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        var config = new MapperConfiguration(cfg => { });
        var destination = new Destination { Baz = 42 };
        config.CreateMapper().Map((object)source, destination);
        destination.Foo.ShouldBe("Foo");
        destination.Baz.ShouldBe(42);
    }
}

public class When_mapping_from_dynamic_null_to_int
{
    Destination _destination;

    [Fact]
    public void Should_map_to_zero()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        source.Baz = null;
        var config = new MapperConfiguration(cfg => { });
        _destination = config.CreateMapper().Map<Destination>((object)source);
        _destination.Foo.ShouldBe("Foo");
        _destination.Bar.ShouldBeNull();
        _destination.Baz.ShouldBe(0);
    }
}

public class When_mapping_from_dynamic_to_dynamic
{
    dynamic _destination;

    [Fact]
    public void Should_map()
    {
        dynamic source = new DynamicDictionary();
        source.Foo = "Foo";
        source.Bar = "Bar";
        var config = new MapperConfiguration(cfg => { });
        _destination = config.CreateMapper().Map<DynamicDictionary>((object)source);
        Assert.Equal("Foo", _destination.Foo);
        Assert.Equal("Bar", _destination.Bar);
    }
}

public class When_mapping_from_dynamic_to_nullable
{
    class DestinationWithNullable
    {
        public string StringValue { get; set; }
        public int? NullIntValue { get; set; }
    }

    [Fact]
    public void Should_map_with_non_null_source()
    {
        dynamic source = new DynamicDictionary();
        source.StringValue = "Test";
        source.NullIntValue = 5;
        var config = new MapperConfiguration(cfg => { });
        var destination = config.CreateMapper().Map<DestinationWithNullable>((object)source);
        Assert.Equal("Test", destination.StringValue);
        Assert.Equal(5, destination.NullIntValue);
    }

    [Fact]
    public void Should_map_with_source_missing()
    {
        dynamic source = new DynamicDictionary();
        source.StringValue = "Test";
        var config = new MapperConfiguration(cfg => { });
        var destination = config.CreateMapper().Map<DestinationWithNullable>((object)source);
        Assert.Equal("Test", destination.StringValue);
        Assert.Equal((int?)null, destination.NullIntValue);
    }

    [Fact]
    public void Should_map_with_null_source()
    {
        dynamic source = new DynamicDictionary();
        source.StringValue = "Test";
        source.NullIntValue = null;
        var config = new MapperConfiguration(cfg => { });
        var destination = config.CreateMapper().Map<DestinationWithNullable>((object)source);
        Assert.Equal("Test", destination.StringValue);
        Assert.Equal((int?)null, destination.NullIntValue);
    }
}