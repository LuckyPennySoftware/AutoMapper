# Nested Mappings

As the mapping engine executes the mapping, it can use one of a variety of methods to resolve a destination member value.  One of these methods is to use another type map, where the source member type and destination member type are also configured in the mapping configuration.  This allows us to not only flatten our source types, but create complex destination types as well.  For example, our source type might contain another complex type:

```c#
public class OuterSource
{
	public int Value { get; set; }
	public InnerSource Inner { get; set; }
}

public class InnerSource
{
	public int OtherValue { get; set; }
}
```

We _could_ simply flatten the OuterSource.Inner.OtherValue to one InnerOtherValue property, but we might also want to create a corresponding complex type for the Inner property:

```c#
public class OuterDest
{
	public int Value { get; set; }
	public InnerDest Inner { get; set; }
}

public class InnerDest
{
	public int OtherValue { get; set; }
}
```

In that case, we would need to configure the additional source/destination type mappings:

```c#
var config = new MapperConfiguration(cfg => {
    cfg.CreateMap<OuterSource, OuterDest>();
    cfg.CreateMap<InnerSource, InnerDest>();
}, loggerFactory);
config.AssertConfigurationIsValid();

var source = new OuterSource
	{
		Value = 5,
		Inner = new InnerSource {OtherValue = 15}
	};
var mapper = config.CreateMapper();
var dest = mapper.Map<OuterSource, OuterDest>(source);

dest.Value.ShouldEqual(5);
dest.Inner.ShouldNotBeNull();
dest.Inner.OtherValue.ShouldEqual(15);
```

A few things to note here:

* Order of configuring types does not matter
* Call to Map does not need to specify any inner type mappings, only the type map to use for the source value passed in

With both flattening and nested mappings, we can create a variety of destination shapes to suit whatever our needs may be.
