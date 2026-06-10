# Null Substitution

Null substitution allows you to supply an alternate value for a destination member if the source value is null anywhere along the member chain. This means that instead of mapping from null, it will map from the value you supply.

```c#
var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>()
    .ForMember(destination => destination.Value, opt => opt.NullSubstitute("Other Value")), loggerFactory);

var source = new Source { Value = null };
var mapper = config.CreateMapper();
var dest = mapper.Map<Source, Dest>(source);

dest.Value.ShouldEqual("Other Value");

source.Value = "Not null";

dest = mapper.Map<Source, Dest>(source);

dest.Value.ShouldEqual("Not null");
```

The substitute is assumed to be of the source member type, and will go through any mapping/conversion after to the destination type.
