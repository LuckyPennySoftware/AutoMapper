# Attribute Mapping

In addition to fluent configuration is the ability to declare and configure maps via attributes. Attribute maps can supplement or replace fluent mapping configuration.

## Type Map configuration

In order to search for maps to configure, use the `AddMaps` method:

```c#
var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MyAssembly"), loggerFactory);

var mapper = new Mapper(configuration);
```

`AddMaps` looks for fluent map configuration (`Profile` classes) and attribute-based mappings.

To declare an attribute map, decorate your destination type with the `AutoMapAttribute`:

```c#
[AutoMap(typeof(Order))]
public class OrderDto {
    // destination members
```

This is equivalent to a `CreateMap<Order, OrderDto>()` configuration.

### Customizing type map configuration

To customize the overall type map configuration, you can set the following properties on the `AutoMapAttribute`:

 - ReverseMap (bool)
 - ConstructUsingServiceLocator (bool)
 - MaxDepth (int)
 - PreserveReferences (bool)
 - DisableCtorValidation (bool)
 - IncludeAllDerived (bool)
 - TypeConverter (Type)
 - AsProxy (bool)
 
These all correspond to the similar fluent mapping configuration options. Only the `sourceType` value is required to map.

## Member configuration

For attribute-based maps, you can decorate individual members with additional configuration. Because attributes have limitations in C# (no expressions, for example), the configuration options available are a bit limited.

Member-based attributes are declared in the `AutoMapper.Configuration.Annotations` namespace.

If the attribute-based configuration is not available or will not work, you can combine both attribute and profile-based maps (though this may be confusing).

### Ignoring members

Use the `IgnoreAttribute` to ignore an individual destination member from mapping and/or validation:

```c#
using AutoMapper.Configuration.Annotations;

[AutoMap(typeof(Order))]
public class OrderDto {
    [Ignore]
    public decimal Total { get; set; }
```

### Redirecting to a different source member

It is not possible to use `MapFrom` with an expression in an attribute, but `SourceMemberAttribute` can redirect to a separate named member:

 ```c#
using AutoMapper.Configuration.Annotations;

[AutoMap(typeof(Order))]
public class OrderDto {
    [SourceMember("OrderTotal")]
    public decimal Total { get; set; }
```

Or use the `nameof` operator:

 ```c#
using AutoMapper.Configuration.Annotations;

[AutoMap(typeof(Order))]
public class OrderDto {
    [SourceMember(nameof(Order.OrderTotal))]
    public decimal Total { get; set; }
```

You cannot flatten with this attribute, only redirect source type members (i.e. no "Order.Customer.Office.Name" in the name). Configuring flattening is only available with the fluent configuration.

### Additional configuration options

Additional attribute-based configuration options include:

 - `MapAtRuntimeAttribute`
 - `MappingOrderAttribute`
 - `NullSubstituteAttribute`
 - `UseExistingValueAttribute`
 - `ValueConverterAttribute`
 - `ValueResolverAttribute`
 
Each corresponds to the same fluent configuration mapping option.