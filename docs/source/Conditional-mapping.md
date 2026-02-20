# Conditional Mapping

AutoMapper allows you to add conditions to properties that must be met before that property will be mapped.

This can be used in situations like the following where we are trying to map from an int to an unsigned int.
```c#
class Foo{
  public int baz;
}

class Bar {
  public uint baz;
}
```

In the following mapping the property baz will only be mapped if it is greater than or equal to 0 in the source object.

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Foo,Bar>()
    .ForMember(dest => dest.baz, opt => opt.Condition(src => (src.baz >= 0)));
}, loggerFactory);
```
If you have a resolver, see [here](Custom-value-resolvers.html#resolvers-and-conditions) for a concrete example.

## Preconditions

Similarly, there is a PreCondition method. The difference is that it runs sooner in the mapping process, before the source value is resolved (think MapFrom). So the precondition is called, then we decide which will be the source of the mapping (resolving), then the condition is called and finally the destination value is assigned.

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Foo,Bar>()
    .ForMember(dest => dest.baz, opt => {
        opt.PreCondition(src => (src.baz >= 0));
        opt.MapFrom(src => {
            // Expensive resolution process that can be avoided with a PreCondition
        });
    });
}, loggerFactory);
```

You can [see the steps](Understanding-your-mapping.html) yourself.

See [here](Custom-value-resolvers.html#resolvers-and-conditions) for a concrete example.

## Class-Based Conditions

Instead of inline lambdas, conditions can be implemented as classes. This enables reuse across multiple mappings and supports dependency injection for accessing services.

### Interfaces

`ICondition<TSource, TDestination, TMember>` is evaluated after source member resolution, with access to both source and destination member values:

```csharp
public interface ICondition<in TSource, in TDestination, in TMember>
{
    bool Evaluate(TSource source, TDestination destination, TMember sourceMember, TMember destMember, ResolutionContext context);
}
```

`IPreCondition<TSource, TDestination>` is evaluated before source member resolution and does not have access to member values:

```csharp
public interface IPreCondition<in TSource, in TDestination>
{
    bool Evaluate(TSource source, TDestination destination, ResolutionContext context);
}
```

### Usage

```csharp
public class AgeCondition : ICondition<Person, PersonDto, int>
{
    public bool Evaluate(Person source, PersonDto destination, int sourceMember, int destMember, ResolutionContext context)
        => sourceMember >= 18;
}

public class SourceNotNullPreCondition : IPreCondition<Person, PersonDto>
{
    public bool Evaluate(Person source, PersonDto destination, ResolutionContext context)
        => source != null;
}

cfg.CreateMap<Person, PersonDto>()
    .ForMember(d => d.Age, o =>
    {
        o.Condition<AgeCondition>();
        o.MapFrom(s => s.Age);
    })
    .ForMember(d => d.Name, o =>
    {
        o.PreCondition<SourceNotNullPreCondition>();
        o.MapFrom(s => s.Name);
    });
```

For runtime type resolution, use the non-generic overloads:

```csharp
cfg.CreateMap(typeof(Person), typeof(PersonDto))
    .ForMember(nameof(PersonDto.Age), o =>
    {
        o.Condition(typeof(AgeCondition));
        o.MapFrom(nameof(Person.Age));
    });
```

### Dependency Injection

Class-based conditions are resolved from the DI container, enabling constructor injection of services:

```csharp
public class AgeRestrictedContentCondition : ICondition<User, UserDto, int>
{
    private readonly IAgeRestrictionService _ageService;

    public AgeRestrictedContentCondition(IAgeRestrictionService ageService)
    {
        _ageService = ageService;
    }

    public bool Evaluate(User source, UserDto destination, int sourceMember, int destMember, ResolutionContext context)
        => _ageService.CanAccessAdultContent(sourceMember);
}

// Registration
services.AddScoped<IAgeRestrictionService, AgeRestrictionService>();
services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<User, UserDto>()
        .ForMember(d => d.RestrictedContent, o =>
        {
            o.Condition<AgeRestrictedContentCondition>();
            o.MapFrom(s => s.RestrictedContent);
        });
});
```

Condition instances are resolved at mapper configuration time and cached in the compiled expression, so DI resolution does not occur during individual mapping calls.
