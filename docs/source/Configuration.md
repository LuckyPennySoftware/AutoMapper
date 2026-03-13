# Configuration

Create a `MapperConfiguration` instance and initialize configuration via the constructor:

```c#
var config = new MapperConfiguration(cfg => {
    cfg.CreateMap<Foo, Bar>();
    cfg.AddProfile<FooProfile>();
}, loggerFactory);
```

The `MapperConfiguration` instance can be stored statically, in a static field or in a dependency injection container. Once created it cannot be changed/modified.

```c#
var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Foo, Bar>();
    cfg.AddProfile<FooProfile>();
}, loggerFactory);
```
Starting with 9.0, the static API is no longer available.

## Profile Instances

A good way to organize your mapping configurations is with profiles.
Create classes that inherit from `Profile` and put the configuration in the constructor:
```c#
// This is the approach starting with version 5
public class OrganizationProfile : Profile
{
	public OrganizationProfile()
	{
		CreateMap<Foo, FooDto>();
		// Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
	}
}

// How it was done in 4.x - as of 5.0 this is obsolete:
// public class OrganizationProfile : Profile
// {
//     protected override void Configure()
//     {
//         CreateMap<Foo, FooDto>();
//     }
// }
```

In earlier versions the `Configure` method was used instead of a constructor.
As of version 5, `Configure()` is obsolete. It will be removed in 6.0.

Configuration inside a profile only applies to maps inside the profile. Configuration applied to the root configuration applies to *all* maps created.

### Assembly Scanning for auto configuration

Profiles can be added to the main mapper configuration in a number of ways, either directly:

```
cfg.AddProfile<OrganizationProfile>();
cfg.AddProfile(new OrganizationProfile());
```

or by automatically scanning for profiles:

```c#
// Scan for all profiles in an assembly
// ... using instance approach:
var config = new MapperConfiguration(cfg => {
    cfg.AddMaps(myAssembly);
}, loggerFactory);
var configuration = new MapperConfiguration(cfg => cfg.AddMaps(myAssembly), loggerFactory);

// Can also use assembly names:
var configuration = new MapperConfiguration(cfg => {
    cfg.AddMaps(new [] {
        "Foo.UI",
        "Foo.Core"
    });
}, loggerFactory);

// Or marker types for assemblies:
var configuration = new MapperConfiguration(cfg => {
    cfg.AddMaps(new [] {
        typeof(HomeController),
        typeof(Entity)
    });
}, loggerFactory);
```

AutoMapper will scan the designated assemblies for classes inheriting from Profile and add them to the configuration.

## Naming Conventions

You can set the source and destination naming conventions

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.SourceMemberNamingConvention = LowerUnderscoreNamingConvention.Instance;
  cfg.DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
}, loggerFactory);
```

This will map the following properties to each other:
`  property_name -> PropertyName `

You can also set this per profile

```c#
public class OrganizationProfile : Profile
{
  public OrganizationProfile()
  {
    SourceMemberNamingConvention = LowerUnderscoreNamingConvention.Instance;
    DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
    //Put your CreateMap... Etc.. here
  }
}
```
If you don't need a naming convention, you can use the `ExactMatchNamingConvention`.

## Replacing characters

You can also replace individual characters or entire words in source members during member name matching:

```c#
public class Source
{
    public int Value { get; set; }
    public int Ävíator { get; set; }
    public int SubAirlinaFlight { get; set; }
}
public class Destination
{
    public int Value { get; set; }
    public int Aviator { get; set; }
    public int SubAirlineFlight { get; set; }
}
```

We want to replace the individual characters, and perhaps translate a word:

```c#
var configuration = new MapperConfiguration(c =>
{
    c.ReplaceMemberName("Ä", "A");
    c.ReplaceMemberName("í", "i");
    c.ReplaceMemberName("Airlina", "Airline");
}, loggerFactory);
```

## Recognizing pre/postfixes

Sometimes your source/destination properties will have common pre/postfixes that cause you to have to do a bunch of custom member mappings because the names don't match up. To address this, you can recognize pre/postfixes:

```c#
public class Source {
    public int frmValue { get; set; }
    public int frmValue2 { get; set; }
}
public class Dest {
    public int Value { get; set; }
    public int Value2 { get; set; }
}
var configuration = new MapperConfiguration(cfg => {
    cfg.RecognizePrefixes("frm");
    cfg.CreateMap<Source, Dest>();
}, loggerFactory);
configuration.AssertConfigurationIsValid();
```

By default AutoMapper recognizes the prefix "Get", if you need to clear the prefix:

```c#
var configuration = new MapperConfiguration(cfg => {
    cfg.ClearPrefixes();
    cfg.RecognizePrefixes("tmp");
}, loggerFactory);
```

## Global property/field filtering

By default, AutoMapper tries to map every public property/field. You can filter out properties/fields with the property/field filters:

```c#
var configuration = new MapperConfiguration(cfg =>
{
	// don't map any fields
	cfg.ShouldMapField = fi => false;

	// map properties with a public or private getter
	cfg.ShouldMapProperty = pi =>
		pi.GetMethod != null && (pi.GetMethod.IsPublic || pi.GetMethod.IsPrivate);
}, loggerFactory);
```

## Configuring visibility

By default, AutoMapper only recognizes public members. It can map to private setters, but will skip internal/private methods and properties if the entire property is private/internal. To instruct AutoMapper to recognize members with other visibilities, override the default filters ShouldMapField and/or ShouldMapProperty :

```c#
var configuration = new MapperConfiguration(cfg =>
{
    // map properties with public or internal getters
    cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
    cfg.CreateMap<Source, Destination>();
}, loggerFactory);
```

Map configurations will now recognize internal/private members.

## Configuration compilation

Because expression compilation can be a bit resource intensive, AutoMapper lazily compiles the type map plans on first map. However, this behavior is not always desirable, so you can tell AutoMapper to compile its mappings directly:

```c#
var configuration = new MapperConfiguration(cfg => {}, loggerFactory);
configuration.CompileMappings();
```

For a few hundred mappings, this may take a couple of seconds. If it's a lot more than that, you probably have some really big execution plans.

### Long compilation times

Compilation times increase with the size of the execution plan and that depends on the number of properties and their complexity. Ideally, you would fix your model so you have many small DTOs, each for a particular use case. But you can also decrease the size of the execution plan without changing your classes.

You can set `MapAtRuntime` per member or `MaxExecutionPlanDepth` globally (the default is one, set it to zero).

These will reduce the size of the execution plan by replacing the execution plan for a child object with a method call. The compilation will be faster, but the mapping itself might be slower. Search the repo for more details and use a profiler to better understand the effect.
Avoiding `PreserveReferences` and `MaxDepth` also helps.

## Circular and Self-Referential Types

When AutoMapper detects a self-referential type mapping (e.g., `CreateMap<Node, Node>()` where `Node` has a `Node` property), it automatically enables `PreserveReferences` to avoid re-mapping the same object instance. It also applies a default `MaxDepth` of **64** — matching System.Text.Json and Newtonsoft.Json — to prevent a Denial-of-Service condition from deeply nested object graphs (see GHSA-rvv3-g6hj-g44x).

If your object graphs legitimately exceed 64 levels, increase the limit explicitly:

```c#
cfg.CreateMap<Node, Node>().MaxDepth(128);
```

To disable the depth limit entirely and rely solely on object-identity caching, call `.PreserveReferences()` explicitly:

```c#
cfg.CreateMap<Node, Node>().PreserveReferences();
```