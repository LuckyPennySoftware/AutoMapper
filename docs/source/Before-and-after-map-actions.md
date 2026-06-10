# Before and After Map Action

Occasionally, you might need to perform custom logic before or after a map occurs. These should be a rarity, as it's more obvious to do this work outside of AutoMapper. You can create global before/after map actions:

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Source, Dest>()
    .BeforeMap((src, dest) => src.Value = src.Value + 10)
    .AfterMap((src, dest) => dest.Name = "John");
}, loggerFactory);
```

Or you can create before/after map callbacks during mapping:

```c#
int i = 10;
mapper.Map<Source, Dest>(src, opt => {
    opt.BeforeMap((src, dest) => src.Value = src.Value + i);
    opt.AfterMap((src, dest) => dest.Name = HttpContext.Current.Identity.Name);
});
```

The latter configuration is helpful when you need contextual information fed into before/after map actions.

## Using `IMappingAction`
You can encapsulate Before and After Map Actions into small reusable classes. Those classes need to implement the `IMappingAction<in TSource, in TDestination>` interface.

Using the previous example, here is an encapsulation of naming some objects "John":

``` csharp
public class NameMeJohnAction : IMappingAction<SomePersonObject, SomeOtherPersonObject>
{
    public void Process(SomePersonObject source, SomeOtherPersonObject destination, ResolutionContext context)
    {
        destination.Name = "John";
    }
}

var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<SomePersonObject, SomeOtherPersonObject>()
    .AfterMap<NameMeJohnAction>();
});
```

### Dependency Injection
You can't inject dependencies into `Profile` classes, but you can do it in `IMappingAction` implementations.

The following example shows how to connect an `IMappingAction` accessing the current `HttpContext` to a `Profile` after map action, leveraging dependency injection:

``` csharp
public class SetTraceIdentifierAction : IMappingAction<SomeModel, SomeOtherModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SetTraceIdentifierAction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public void Process(SomeModel source, SomeOtherModel destination, ResolutionContext context)
    {
        destination.TraceIdentifier = _httpContextAccessor.HttpContext.TraceIdentifier;
    }
}

public class SomeProfile : Profile
{
    public SomeProfile()
    {
        CreateMap<SomeModel, SomeOtherModel>()
            .AfterMap<SetTraceIdentifierAction>();
    }
}
```

Everything is connected together by:

``` csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(Startup).Assembly);
    }
    //..
}
```