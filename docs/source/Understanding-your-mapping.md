# Understanding Your Mappings

AutoMapper creates an execution plan for your mapping. That execution plan can be viewed as [an expression tree](https://msdn.microsoft.com/en-us/library/mt654263.aspx?f=255&MSPPError=-2147217396) during debugging. You can get a better view of the resulting code by installing [the ReadableExpressions VS extension](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers). If you need to see the code outside VS, you can use [the ReadableExpressions package directly](https://www.nuget.org/packages/AgileObjects.ReadableExpressions). [This DotNetFiddle](https://dotnetfiddle.net/aJYTGZ) has a live demo  using the NuGet package, and [this article](https://agileobjects.co.uk/view-automapper-execution-plan-readableexpressions) describes using the VS extension.

```c#
var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Foo, Bar>(), loggerFactory);
var executionPlan = configuration.BuildExecutionPlan(typeof(Foo), typeof(Bar));
```

Be sure to remove all such code before release.

For ProjectTo, you need to inspect `IQueryable.Expression`.

```c#
var expression = context.Entities.ProjectTo<Dto>().Expression;
```
