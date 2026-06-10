# Value Transformers

Value transformers apply an additional transformation to a single type. Before assigning the value, AutoMapper will check to see if the value to be set has any value transformations associated, and will apply them before setting.

You can create value transformers at several different levels:

 - Globally
 - Profile
 - Map
 - Member

```c#
var configuration = new MapperConfiguration(cfg => {
    cfg.ValueTransformers.Add<string>(val => val + "!!!");
}, loggerFactory);

var source = new Source { Value = "Hello" };
var dest = mapper.Map<Dest>(source);

dest.Value.ShouldBe("Hello!!!");
```
