# Expression Translation (UseAsDataSource)

Automapper supports translating Expressions from one object to another in a separate [package](https://www.nuget.org/packages/AutoMapper.Extensions.ExpressionMapping/).
This is done by substituting the properties from the source class to what they map to in the destination class.

Given the example classes:

```c#
public class OrderLine
{
  public int Id { get; set; }
  public int OrderId { get; set; }
  public Item Item { get; set; }
  public decimal Quantity { get; set; }
}

public class Item
{
  public int Id { get; set; }
  public string Name { get; set; }
}

public class OrderLineDTO
{
  public int Id { get; set; }
  public int OrderId { get; set; }
  public string Item { get; set; }
  public decimal Quantity { get; set; }
}

var configuration = new MapperConfiguration(cfg =>
{
  cfg.AddExpressionMapping();
  
  cfg.CreateMap<OrderLine, OrderLineDTO>()
    .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol.Item.Name));
  cfg.CreateMap<OrderLineDTO, OrderLine>()
    .ForMember(ol => ol.Item, conf => conf.MapFrom(dto => dto));
  cfg.CreateMap<OrderLineDTO, Item>()
    .ForMember(i => i.Name, conf => conf.MapFrom(dto => dto.Item));
}, loggerFactory);
```
When mapping from DTO Expression

```c#
Expression<Func<OrderLineDTO, bool>> dtoExpression = dto=> dto.Item.StartsWith("A");
var expression = mapper.Map<Expression<Func<OrderLine, bool>>>(dtoExpression);
```

Expression will be translated to `ol => ol.Item.Name.StartsWith("A")`

Automapper knows `dto.Item` is mapped to `ol.Item.Name` so it substituted it for the expression.

Expression translation can work on expressions of collections as well.

```c#
Expression<Func<IQueryable<OrderLineDTO>,IQueryable<OrderLineDTO>>> dtoExpression = dtos => dtos.Where(dto => dto.Quantity > 5).OrderBy(dto => dto.Quantity);
var expression = mapper.Map<Expression<Func<IQueryable<OrderLine>,IQueryable<OrderLine>>>(dtoExpression);
```

Resulting in `ols => ols.Where(ol => ol.Quantity > 5).OrderBy(ol => ol.Quantity)`

### Mapping Flattened Properties to Navigation Properties

AutoMapper also supports mapping flattened (TModel or DTO) properties in expressions to their corresponding (TData) navigation properties (when the navigation property has been removed from the view model or DTO) e.g. CourseModel.DepartmentName from the model expression becomes Course.Department in the data expression.

Take the following set of classes:

```c#
public class CourseModel
{
    public int CourseID { get; set; }

    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; }
}
public class Course
{
    public int CourseID { get; set; }

    public int DepartmentID { get; set; }
    public Department Department { get; set; }
}

public class Department
{
    public int DepartmentID { get; set; }
    public string Name { get; set; }
}
```

Then map exp below to expMapped.

```c#
Expression<Func<IQueryable<CourseModel>, IIncludableQueryable<CourseModel, object>>> exp = i => i.Include(s => s.DepartmentName);
Expression<Func<IQueryable<Course>, IIncludableQueryable<Course, object>>> expMapped = mapper.MapExpressionAsInclude<Expression<Func<IQueryable<Course>, IIncludableQueryable<Course, object>>>>(exp);
```

The resulting mapped expression (expMapped.ToString()) is then ``` i => i.Include(s => s.Department);  ``` . This feature allows navigation properties for the query to be defined based on the view model alone.

### Supported Mapping options

Much like how Queryable Extensions can only support certain things that the LINQ providers support, expression translation follows the same rules as what it can and can't support.

## UseAsDataSource

Mapping expressions to one another is a tedious and produces long ugly code.

`UseAsDataSource().For<DTO>()` makes this translation clean by not having to explicitly map expressions.
It also calls `ProjectTo<TDO>()` for you as well, where applicable.

Using EntityFramework as an example

`dataContext.OrderLines.UseAsDataSource().For<OrderLineDTO>().Where(dto => dto.Name.StartsWith("A"))`

Does the equivalent of

`dataContext.OrderLines.Where(ol => ol.Item.Name.StartsWith("A")).ProjectTo<OrderLineDTO>()`

### When ProjectTo() is not called

Expression Translation works for all kinds of functions, including `Select` calls.  If `Select` is used after `UseAsDataSource()` and changes the return type, then `ProjectTo<>()` won't be called and `mapper.Map` will be used instead.

Example:

`dataContext.OrderLines.UseAsDataSource().For<OrderLineDTO>().Select(dto => dto.Name)`

Does the equivalent of

`dataContext.OrderLines.Select(ol => ol.Item.Name)`

### Register a callback, for when an UseAsDataSource() query is enumerated

Sometimes, you may want to edit the collection, that is returned from a mapped query before forwarding it to the next application layer.
With `.ProjectTo<TDto>` this is quite simple, as there is no sense in directly returning the resulting `IQueryable<TDto>` because you cannot edit it anymore anyways. So you will most likely do this:

```c#
var configuration = new MapperConfiguration(cfg =>
    cfg.CreateMap<OrderLine, OrderLineDTO>()
    .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol.Item.Name)), loggerFactory);

public List<OrderLineDTO> GetLinesForOrder(int orderId)
{
  using (var context = new orderEntities())
  {
    var dtos = context.OrderLines.Where(ol => ol.OrderId == orderId)
             .ProjectTo<OrderLineDTO>().ToList();
    foreach(var dto in dtos)
    {
        // edit some property, or load additional data from the database and augment the dtos
    }
    return dtos;
  }
}
```

However, if you did this with the `.UseAsDataSource()` approach, you would lose all of its power - namely its ability to modify the internal expression until it is enumerated.
To solve that problem, we introduced the `.OnEnumerated` callback.
Using it, you can do the following:

```c#
var configuration = new MapperConfiguration(cfg =>
    cfg.CreateMap<OrderLine, OrderLineDTO>()
    .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol.Item.Name)), loggerFactory);

public IQueryable<OrderLineDTO> GetLinesForOrder(int orderId)
{
  using (var context = new orderEntities())
  {
    return context.OrderLines.Where(ol => ol.OrderId == orderId)
             .UseAsDataSource()
             .For<OrderLineDTO>()
             .OnEnumerated((dtos) =>
             {
                foreach(var dto in dtosCast<OrderLineDTO>())
                {
                     // edit some property, or load additional data from the database and augment the dtos
                }
             }
   }
}
```

this `OnEnumerated(IEnumerable)`callback is executed, when the `IQueryable<OrderLineDTO>` itself is enumerated.
So this also works with the OData samples mentioned above: The OData $filter and $orderby expressions are still converted into SQL, and the `OnEnumerated()`callback is provided with the filtered, ordered resultset from the database.
