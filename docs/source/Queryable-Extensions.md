# Queryable Extensions

When using an ORM such as NHibernate or Entity Framework with AutoMapper's standard `mapper.Map` functions, you may notice that the ORM will query all the fields of all the objects within a graph when AutoMapper is attempting to map the results to a destination type.

If your ORM exposes `IQueryable`s, you can use AutoMapper's QueryableExtensions helper methods to address this key pain.

Using Entity Framework for an example, say that you have an entity `OrderLine` with a relationship with an entity `Item`. If you want to map this to an `OrderLineDTO` with the `Item`'s `Name` property, the standard `mapper.Map` call will result in Entity Framework querying the entire `OrderLine` and `Item` table.

Use this approach instead.

Given the following entities:

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
```

And the following DTO:

```c#
public class OrderLineDTO
{
  public int Id { get; set; }
  public int OrderId { get; set; }
  public string Item { get; set; }
  public decimal Quantity { get; set; }
}
```

You can use the Queryable Extensions like so:

```c#
var configuration = new MapperConfiguration(cfg =>
    cfg.CreateProjection<OrderLine, OrderLineDTO>()
    .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol.Item.Name)), loggerFactory);

public List<OrderLineDTO> GetLinesForOrder(int orderId)
{
  using (var context = new orderEntities())
  {
    return context.OrderLines.Where(ol => ol.OrderId == orderId)
             .ProjectTo<OrderLineDTO>(configuration).ToList();
  }
}
```

The `.ProjectTo<OrderLineDTO>()` will tell AutoMapper's mapping engine to emit a `select` clause to the IQueryable that will inform entity framework that it only needs to query the Name column of the Item table, same as if you manually projected your `IQueryable` to an `OrderLineDTO` with a `Select` clause.

### Query Provider Limitations

`ProjectTo` must be the last call in the LINQ method chain. ORMs work with entities, not DTOs. Apply any filtering and sorting on entities and, as the last step, project to DTOs. Query providers are highly complex and making the `ProjectTo` call last ensures the query provider works as closely as designed to build valid queries against the underlying query target (SQL, Mongo QL etc.).

Note that for this feature to work, all type conversions must be explicitly handled in your Mapping. For example, you can not rely on the `ToString()` override of the `Item` class to inform entity framework to only select from the `Name` column, and any data type changes, such as `Double` to `Decimal` must be explicitly handled as well.

### The instance API

Starting with 8.0 there are similar ProjectTo methods on IMapper that feel more natural when you use IMapper with DI.

### Preventing lazy loading/SELECT N+1 problems

Because the LINQ projection built by AutoMapper is translated directly to a SQL query by the query provider, the mapping occurs at the SQL/ADO.NET level, and not touching your entities. All data is eagerly fetched and loaded into your DTOs.

Nested collections use a Select to project child DTOs:

```c#
from i in db.Instructors
orderby i.LastName
select new InstructorIndexData.InstructorModel
{
    ID = i.ID,
    FirstMidName = i.FirstMidName,
    LastName = i.LastName,
    HireDate = i.HireDate,
    OfficeAssignmentLocation = i.OfficeAssignment.Location,
    Courses = i.Courses.Select(c => new InstructorIndexData.InstructorCourseModel
    {
        CourseID = c.CourseID,
        CourseTitle = c.Title
    }).ToList()
};
```

This map through AutoMapper will result in a SELECT N+1 problem, as each child `Course` will be queried one at a time, unless specified through your ORM to eagerly fetch. With LINQ projection, no special configuration or specification is needed with your ORM. The ORM uses the LINQ projection to build the exact SQL query needed.

That means that you don't need to use explicit eager loading (`Include`) with `ProjectTo`. If you need something like filtered `Include`, have the filter in your map:

```c#

    CreateProjection<Entity, Dto>().ForMember(d => d.Collection, o => o.MapFrom(s => s.Collection.Where(i => ...));
```

### Custom projection

In the case where members names don't line up, or you want to create calculated property, you can use MapFrom (the expression-based overload) to supply a custom expression for a destination member:

```c#
var configuration = new MapperConfiguration(cfg => cfg.CreateProjection<Customer, CustomerDto>()
    .ForMember(d => d.FullName, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
    .ForMember(d => d.TotalContacts, opt => opt.MapFrom(c => c.Contacts.Count())), loggerFactory);
```

AutoMapper passes the supplied expression with the built projection. As long as your query provider can interpret the supplied expression, everything will be passed down all the way to the database.

If the expression is rejected from your query provider (Entity Framework, NHibernate, etc.), you might need to tweak your expression until you find one that is accepted.

### Custom Type Conversion

Occasionally, you need to completely replace a type conversion from a source to a destination type. In normal runtime mapping, this is accomplished via the ConvertUsing method. To perform the analog in LINQ projection, use the ConvertUsing method:

```c#
cfg.CreateProjection<Source, Dest>().ConvertUsing(src => new Dest { Value = 10 });
```

The expression-based `ConvertUsing` is slightly more limited than Func-based `ConvertUsing` overloads as only what is allowed in an Expression and the underlying LINQ provider will work.

### Custom destination type constructors

If your destination type has a custom constructor but you don't want to override the entire mapping, use the ConstructUsing expression-based method overload:

```c#
cfg.CreateProjection<Source, Dest>()
    .ConstructUsing(src => new Dest(src.Value + 10));
```

AutoMapper will automatically match up destination constructor parameters to source members based on matching names, so only use this method if AutoMapper can't match up the destination constructor properly, or if you need extra customization during construction.

### String conversion

AutoMapper will automatically add `ToString()` when the destination member type is a string and the source member type is not.

```c#
public class Order {
    public OrderTypeEnum OrderType { get; set; }
}
public class OrderDto {
    public string OrderType { get; set; }
}
var orders = dbContext.Orders.ProjectTo<OrderDto>(configuration).ToList();
orders[0].OrderType.ShouldEqual("Online");
```

### Explicit expansion

In some scenarios, such as OData, a generic DTO is returned through an IQueryable controller action. Without explicit instructions, AutoMapper will expand all members in the result. To control which members are expanded during projection, set ExplicitExpansion in the configuration and then pass in the members you want to explicitly expand:

```c#
dbContext.Orders.ProjectTo<OrderDto>(configuration,
    dest => dest.Customer,
    dest => dest.LineItems);
// or string-based
dbContext.Orders.ProjectTo<OrderDto>(configuration,
    null,
    "Customer",
    "LineItems");
// for collections
dbContext.Orders.ProjectTo<OrderDto>(configuration,
    null,
    dest => dest.LineItems.Select(item => item.Product));
```
For more information, see [the tests](https://github.com/LuckyPennySoftware/AutoMapper/search?p=1&q=ExplicitExpansion&utf8=%E2%9C%93).

### Aggregations

LINQ can support aggregate queries, and AutoMapper supports LINQ extension methods. In the custom projection example, if we renamed the `TotalContacts` property to `ContactsCount`, AutoMapper would match to the `Count()` extension method and the LINQ provider would translate the count into a correlated subquery to aggregate child records.

AutoMapper can also support complex aggregations and nested restrictions, if the LINQ provider supports it:

```c#
cfg.CreateProjection<Course, CourseModel>()
    .ForMember(m => m.EnrollmentsStartingWithA,
          opt => opt.MapFrom(c => c.Enrollments.Where(e => e.Student.LastName.StartsWith("A")).Count()));
```

This query returns the total number of students, for each course, whose last name starts with the letter 'A'.

### Parameterization

Occasionally, projections need runtime parameters for their values. Consider a projection that needs to pull in the current username as part of its data. Instead of using post-mapping code, we can parameterize our MapFrom configuration:

```c#
string currentUserName = null;
cfg.CreateProjection<Course, CourseModel>()
    .ForMember(m => m.CurrentUserName, opt => opt.MapFrom(src => currentUserName));
```

When we project, we'll substitute our parameter at runtime:

```c#
dbContext.Courses.ProjectTo<CourseModel>(Config, new { currentUserName = Request.User.Name });
```

This works by capturing the name of the closure's field name in the original expression, then using an anonymous object/dictionary to apply the value to the parameter value before the query is sent to the query provider.

You may also use a dictionary to build the projection values:

```c#
dbContext.Courses.ProjectTo<CourseModel>(Config, new Dictionary<string, object> { {"currentUserName", Request.User.Name} });
```

However, using a dictionary will result in hard-coded values in the query instead of a parameterized query, so use with caution.

### Recursive models

Ideally, you would avoid models that reference themselves (do some research). But if you must, you need to enable them:

```c#
configuration.Internal().RecursiveQueriesMaxDepth = someRandomNumber;
```

### Polymorphic Projection

Many ORMs support inheritance and polymorphic models, including custom projection. For example, Entity Framework Core supports [inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) using a variety of mapping strategies.

Some LINQ query providers also support polymorphic projections. AutoMapper attempts to use polymorphic projections when building the Select query. If there are problems attempting this and you encounter exceptions from the query provider, it's likely because the query provider cannot support polymorphic projections for your scenario.

If this is the case, you can turn off polymorphic projection:

```c#
configuration.PolymorphicProjectionsEnabled = false;
```

AutoMapper will no longer attempt to check the source types to pick a specific destination type for LINQ projections.

### Supported mapping options

Not all mapping options can be supported, as the expression generated must be interpreted by a LINQ provider. Only what is supported by LINQ providers is supported by AutoMapper:
* MapFrom (Expression-based)
* ConvertUsing (Expression-based)
* Ignore
* NullSubstitute
* Value transformers
* IncludeMembers
* Runtime polymorphic mapping with Include/IncludeBase

Not supported:
* Condition
* SetMappingOrder
* UseDestinationValue
* MapFrom (Func-based)
* Before/AfterMap
* Custom resolvers
* Custom type converters
* ForPath
* Value converters
* **Any calculated property on your domain object**