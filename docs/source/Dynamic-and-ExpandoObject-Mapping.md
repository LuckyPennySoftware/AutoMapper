# Dynamic and ExpandoObject Mapping

AutoMapper can map to/from dynamic objects without any explicit configuration:

```c#
public class Foo {
    public int Bar { get; set; }
    public int Baz { get; set; }
    public Foo InnerFoo { get; set; }
}
dynamic foo = new MyDynamicObject();
foo.Bar = 5;
foo.Baz = 6;

var configuration = new MapperConfiguration(cfg => {}, loggerFactory);

var result = mapper.Map<Foo>(foo);
result.Bar.ShouldEqual(5);
result.Baz.ShouldEqual(6);

dynamic foo2 = mapper.Map<MyDynamicObject>(result);
foo2.Bar.ShouldEqual(5);
foo2.Baz.ShouldEqual(6);
```

Similarly you can map straight from `Dictionary<string, object>` to objects, AutoMapper will line up the keys with property names.
For mapping to destination child objects, you can use the dot notation.
```c#
var result = mapper.Map<Foo>(new Dictionary<string, object> { ["InnerFoo.Bar"] = 42 });
result.InnerFoo.Bar.ShouldEqual(42);
```