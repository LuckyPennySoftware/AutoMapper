# Open Generics

AutoMapper can support an open generic type map. Create a map for the open generic types:

```c#
public class Source<T> {
    public T Value { get; set; }
}

public class Destination<T> {
    public T Value { get; set; }
}

// Create the mapping
var configuration = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Destination<>)), loggerFactory);
```

You don't need to create maps for closed generic types. AutoMapper will apply any configuration from the open generic mapping to the closed mapping at runtime:

```c#
var source = new Source<int> { Value = 10 };

var dest = mapper.Map<Source<int>, Destination<int>>(source);

dest.Value.ShouldEqual(10);
```

Because C# only allows closed generic type parameters, you have to use the System.Type version of CreateMap to create your open generic type maps. From there, you can use all of the mapping configuration available and the open generic configuration will be applied to the closed type map at runtime.
AutoMapper will skip open generic type maps during configuration validation, since you can still create closed types that don't convert, such as `Source<Foo> -> Destination<Bar>` where there is no conversion from Foo to Bar.

You can also create an open generic type converter:

```c#
var configuration = new MapperConfiguration(cfg =>
   cfg.CreateMap(typeof(Source<>), typeof(Destination<>)).ConvertUsing(typeof(Converter<>)), loggerFactory);
```

AutoMapper also supports open generic type converters with any number of generic arguments:

```c#
var configuration = new MapperConfiguration(cfg =>
   cfg.CreateMap(typeof(Source<>), typeof(Destination<>)).ConvertUsing(typeof(Converter<,>)), loggerFactory);
```

The closed type from `Source` will be the first generic argument, and the closed type of `Destination` will be the second argument to close `Converter<,>`.

The same idea applies to value resolvers. Check [the tests](https://github.com/LuckyPennySoftware/AutoMapper/blob/e8249d582d384ea3b72eec31408126a0b69619bc/src/UnitTests/OpenGenerics.cs#L11).
