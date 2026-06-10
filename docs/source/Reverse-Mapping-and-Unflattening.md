# Reverse Mapping and Unflattening

Starting with 6.1.0, AutoMapper now supports richer reverse mapping support. Given our entities:

```c#
public class Order {
  public decimal Total { get; set; }
  public Customer Customer { get; set; }
}

public class Customer {
  public string Name { get; set; }
}
```

We can flatten this into a DTO:

```c#
public class OrderDto {
  public decimal Total { get; set; }
  public string CustomerName { get; set; }
}
```

We can map both directions, including unflattening:

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Order, OrderDto>()
     .ReverseMap();
}, loggerFactory);
```

By calling `ReverseMap`, AutoMapper creates a reverse mapping configuration that includes unflattening:

```c#
var customer = new Customer {
  Name = "Bob"
};

var order = new Order {
  Customer = customer,
  Total = 15.8m
};

var orderDto = mapper.Map<Order, OrderDto>(order);

orderDto.CustomerName = "Joe";

mapper.Map(orderDto, order);

order.Customer.Name.ShouldEqual("Joe");
```

Unflattening is only configured for `ReverseMap`. If you want unflattening, you must configure `Entity` -> `Dto` then call `ReverseMap` to create an unflattening type map configuration from the `Dto` -> `Entity`.

### Customizing reverse mapping

AutoMapper will automatically reverse map "Customer.Name" from "CustomerName" based on the original flattening. If you use MapFrom, AutoMapper will attempt to reverse the map:

```c#
cfg.CreateMap<Order, OrderDto>()
  .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
  .ReverseMap();
```

As long as the `MapFrom` path are member accessors, AutoMapper will unflatten from the same path (`CustomerName` => `Customer.Name`).

If you need to customize this, for a reverse map you can use `ForPath`:

```c#
cfg.CreateMap<Order, OrderDto>()
  .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
  .ReverseMap()
  .ForPath(s => s.Customer.Name, opt => opt.MapFrom(src => src.CustomerName));
```

For most cases you shouldn't need this, as the original MapFrom will be reversed for you. Use ForPath when the path to get and set the values are different.

If you do not want unflattening behavior, you can remove the call to `ReverseMap` and create two separate maps. Or, you can use Ignore:

```c#
cfg.CreateMap<Order, OrderDto>()
  .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
  .ReverseMap()
  .ForPath(s => s.Customer.Name, opt => opt.Ignore());
```
### IncludeMembers

`ReverseMap` also integrates with [`IncludeMembers`](Flattening.html#includemembers) and configuration like 
```c#
ForMember(destination => destination.IncludedMember, member => member.MapFrom(source => source))
```
