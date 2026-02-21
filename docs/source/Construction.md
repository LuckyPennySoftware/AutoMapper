# Construction

AutoMapper can map to destination constructors based on source members:

```c#
public class Source {
    public int Value { get; set; }
}
public class SourceDto {
    public SourceDto(int value) {
        _value = value;
    }
    private int _value;
    public int Value {
        get { return _value; }
    }
}
var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, SourceDto>(), loggerFactory);
```

If the destination constructor parameter names don't match, you can modify them at config time:

```c#
public class Source {
    public int Value { get; set; }
}
public class SourceDto {
    public SourceDto(int valueParamSomeOtherName) {
        _value = valueParamSomeOtherName;
    }
    private int _value;
    public int Value {
        get { return _value; }
    }
}
var configuration = new MapperConfiguration(cfg =>
  cfg.CreateMap<Source, SourceDto>()
    .ForCtorParam("valueParamSomeOtherName", opt => opt.MapFrom(src => src.Value))
, loggerFactory);
```

This works for both LINQ projections and in-memory mapping.

You can also disable constructor mapping:    

```c#
var configuration = new MapperConfiguration(cfg => cfg.DisableConstructorMapping(), loggerFactory);
```

You can configure which constructors are considered for the destination object:

```c#
// use only public constructors
var configuration = new MapperConfiguration(cfg => cfg.ShouldUseConstructor = constructor => constructor.IsPublic, loggerFactory);
```
When mapping to records, consider using only public constructors.

## Class-Based Destination Factories

Instead of automatic constructor matching or inline `ConstructUsing` lambdas, you can implement custom constructor logic as a class. This enables reuse across multiple mappings and supports dependency injection.

This is different than `ConvertUsing` which replaces the entire mapping operation.

### Interface

`IDestinationFactory<TSource, TDestination>` is used to implement custom object construction:

```csharp
public interface IDestinationFactory<in TSource, out TDestination>
{
    TDestination Construct(TSource source, ResolutionContext context);
}
```

### Usage

```csharp
public class CustomConstructor : IDestinationFactory<Source, Destination>
{
    public Destination Construct(Source source, ResolutionContext context)
    {
        // Custom instantiation logic
        return new Destination { InitialValue = source.Value * 2 };
    }
}

cfg.CreateMap<Source, Destination>()
    .ConstructUsing<CustomConstructor>();
```

### Dependency Injection

Destination factories are resolved from the DI container, enabling constructor injection of services:

```csharp
public class DIAwareConstructor : IDestinationFactory<Source, Destination>
{
    private readonly IMyService _service;

    public DIAwareConstructor(IMyService service)
    {
        _service = service;
    }

    public Destination Construct(Source source, ResolutionContext context)
    {
        return new Destination 
        { 
            InitialValue = _service.CalculateValue(source.Value) 
        };
    }
}

// Registration
services.AddScoped<IMyService, MyService>();
services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<Source, Destination>()
        .ConstructUsing<DIAwareConstructor>();
}, typeof(IMyService).Assembly);
```

For runtime type resolution, use the non-generic overload:

```csharp
cfg.CreateMap(typeof(Source), typeof(Destination))
    .ConstructUsing(typeof(CustomConstructor));
```

