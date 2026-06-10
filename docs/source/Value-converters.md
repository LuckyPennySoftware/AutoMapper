# Value Converters

Value converters are a cross between [Type Converters](Custom-type-converters.html) and [Value Resolvers](Custom-value-resolvers.html). Type converters are globally scoped, so that any time you map from type `Foo` to type `Bar` in any mapping, the type converter will be used. Value converters are scoped to a single map, and receive the source and destination objects to resolve to a value to map to the destination member. Optionally value converters can receive the source member as well.

In simplified syntax:

 - Type converter = `Func<TSource, TDestination, TDestination>`
 - Value resolver = `Func<TSource, TDestination, TDestinationMember>`
 - Member value resolver = `Func<TSource, TDestination, TSourceMember, TDestinationMember>`
 - Value converter = `Func<TSourceMember, TDestinationMember>`

 To configure a value converter, use at the member level:

 ```c#
 public class CurrencyFormatter : IValueConverter<decimal, string> {
     public string Convert(decimal source, ResolutionContext context)
         => source.ToString("c");
 }

 var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .ForMember(d => d.Amount, opt => opt.ConvertUsing(new CurrencyFormatter()));
    cfg.CreateMap<OrderLineItem, OrderLineItemDto>()
        .ForMember(d => d.Total, opt => opt.ConvertUsing(new CurrencyFormatter()));
 }, loggerFactory);
 ```

You can customize the source member when the source member name does not match:

 ```c#
 public class CurrencyFormatter : IValueConverter<decimal, string> {
     public string Convert(decimal source, ResolutionContext context)
         => source.ToString("c");
 }

 var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .ForMember(d => d.Amount, opt => opt.ConvertUsing(new CurrencyFormatter(), src => src.OrderAmount));
    cfg.CreateMap<OrderLineItem, OrderLineItemDto>()
        .ForMember(d => d.Total, opt => opt.ConvertUsing(new CurrencyFormatter(), src => src.LITotal));
 }, loggerFactory);
 ```

If you need the value converters instantiated by the [service locator](Dependency-injection.html), you can specify the type instead:

 ```c#
 public class CurrencyFormatter : IValueConverter<decimal, string> {
     public string Convert(decimal source, ResolutionContext context)
         => source.ToString("c");
 }

 var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .ForMember(d => d.Amount, opt => opt.ConvertUsing<CurrencyFormatter, decimal>());
    cfg.CreateMap<OrderLineItem, OrderLineItemDto>()
        .ForMember(d => d.Total, opt => opt.ConvertUsing<CurrencyFormatter, decimal>());
 }, loggerFactory);
 ```

If you do not know the types or member names at runtime, use the various overloads that accept `System.Type` and `string`-based members:

 ```c#
 public class CurrencyFormatter : IValueConverter<decimal, string> {
     public string Convert(decimal source, ResolutionContext context)
         => source.ToString("c");
 }

 var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap(typeof(Order), typeof(OrderDto))
        .ForMember("Amount", opt => opt.ConvertUsing(new CurrencyFormatter(), "OrderAmount"));
    cfg.CreateMap(typeof(OrderLineItem), typeof(OrderLineItemDto))
        .ForMember("Total", opt => opt.ConvertUsing(new CurrencyFormatter(), "LITotal"));
 }, loggerFactory);
 ```

 Value converters are only used for in-memory mapping execution. They will not work for [`ProjectTo`](Queryable-Extensions.html).
