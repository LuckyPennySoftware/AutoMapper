# Custom Type Converters

Sometimes, you need to take complete control over the conversion of one type to another.  This is typically when one type looks nothing like the other, a conversion function already exists, and you would like to go from a "looser" type to a stronger type, such as a source type of string to a destination type of Int32.

For example, suppose we have a source type of:

```c#
public class Source
{
	public string Value1 { get; set; }
	public string Value2 { get; set; }
	public string Value3 { get; set; }
}
```

But you would like to map it to:

```c#
public class Destination
{
	public int Value1 { get; set; }
	public DateTime Value2 { get; set; }
	public Type Value3 { get; set; }
}
```

If we were to try and map these two types as-is, AutoMapper would throw an exception (at map time and configuration-checking time), as AutoMapper does not know about any mapping from string to int, DateTime or Type.  To create maps for these types, we must supply a custom type converter, and we have three ways of doing so:

```c#
void ConvertUsing(Func<TSource, TDestination> mappingFunction);
void ConvertUsing(ITypeConverter<TSource, TDestination> converter);
void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;
```

The first option is simply any function that takes a source and returns a destination (there are several overloads too). This works for simple cases, but becomes unwieldy for larger ones.  In more difficult cases, we can create a custom ITypeConverter\<TSource, TDestination\>:

```c#
public interface ITypeConverter<in TSource, TDestination>
{
	TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
}
```

And supply AutoMapper with either an instance of a custom type converter, or simply the type, which AutoMapper will instantiate at run time.  The mapping configuration for our above source/destination types then becomes:

```c#
[Test]
public void Example()
{
    var configuration = new MapperConfiguration(cfg => {
      cfg.CreateMap<string, int>().ConvertUsing(s => Convert.ToInt32(s));
      cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
      cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
      cfg.CreateMap<Source, Destination>();
    }, loggerFactory);
    configuration.AssertConfigurationIsValid();

    var source = new Source
    {
        Value1 = "5",
        Value2 = "01/01/2000",
        Value3 = "AutoMapperSamples.GlobalTypeConverters.GlobalTypeConverters+Destination"
    };

    Destination result = mapper.Map<Source, Destination>(source);
    result.Value3.ShouldEqual(typeof(Destination));
}

public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
{
    public DateTime Convert(string source, DateTime destination, ResolutionContext context)
    {
        return System.Convert.ToDateTime(source);
    }
}

public class TypeTypeConverter : ITypeConverter<string, Type>
{
    public Type Convert(string source, Type destination, ResolutionContext context)
    {
          return Assembly.GetExecutingAssembly().GetType(source);
    }
}
```

In the first mapping, from string to Int32, we simply use the built-in Convert.ToInt32 function (supplied as a method group).  The next two use custom ITypeConverter implementations.

The real power of custom type converters is that they are used any time AutoMapper finds the source/destination pairs on any mapped types.  We can build a set of custom type converters, on top of which other mapping configurations use, without needing any extra configuration.  In the above example, we never have to specify the string/int conversion again.  Where as [Custom Value Resolvers](Custom-value-resolvers.html) have to be configured at a type member level, custom type converters are global in scope.