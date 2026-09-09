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

## Nullable source members

When the condition's source member parameter can hold it, `Condition` receives the value resolved from the source object *before* it is converted to the destination member type; otherwise it receives the converted value. This matters when the source member is a `Nullable<T>` and the destination member is a non-nullable `T`, because the parameter type decides whether the condition sees `null` or `default(T)`.

This makes the common PATCH scenario -- "only assign members the caller actually supplied" -- expressible with `ForAllMembers`:

```c#
class Source {
  public int? Count { get; set; }
}

class Destination {
  public int Count { get; set; }
}

var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Source, Destination>()
    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
}, loggerFactory);

var destination = new Destination { Count = 7 };
mapper.Map(new Source { Count = null }, destination);
// destination.Count is still 7 -- the null source member was skipped
```

`ForAllMembers` types the source member parameter as `object`, which is what allows a `Nullable<T>` to arrive intact. A `ForMember` condition types both member parameters as the *destination* member type, so a `Nullable<T>` source cannot be represented there and the condition falls back to the converted value (`0` for `int`, `false` for `bool`). To check a nullable source member for a single member, use a `PreCondition` against the source object:

```c#
cfg.CreateMap<Source, Destination>()
  .ForMember(dest => dest.Count, opt => opt.PreCondition(src => src.Count != null));
```

For this particular shape -- keep whatever the destination already holds when the source member is null -- `UseDestinationValue` does the same job without a condition at all, and works for a `ForMember` too (where a condition can't see the null):

```c#
cfg.CreateMap<Source, Destination>()
  .ForAllMembers(opt => opt.UseDestinationValue());
```

Reach for it when the members are scalars. It is not a general PATCH switch: on a member that is itself a mapped object, `UseDestinationValue` maps *into* the existing destination instance rather than replacing it, so the nested map still overwrites that object's own members with the source's (unset ones landing as defaults). Members you want left alone entirely still need a condition.

Without a condition, a null `Nullable<T>` source member is always assigned as `default(T)`. AutoMapper does not decide to map zero -- a name match always produces an assignment, and a non-nullable destination member has no way to represent the absence of a value. Roughly:

```c#
dest.Count = src.Count ?? default(int);
```

If you need a value other than `default(T)`, see [Null substitution](Null-substitution.html).

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

`sourceMember` follows the same rule as the lambda overloads: the pre-conversion source value when `TMember` can hold it, the converted value otherwise -- see [Nullable source members](#nullable-source-members).

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

Condition instances are resolved when the map executes, via `ResolutionContext.CreateInstance` (using your configured `ServiceCtor`/DI container). This means DI resolution can occur per mapping operation (and potentially per condition evaluation), rather than only once at configuration time. For per-map scoping or nested containers, you can supply a per-map `ServiceCtor` through `IMappingOperationOptions` when invoking the map.
