# Mapping Inheritance

Mapping inheritance serves two functions:

- Inheriting mapping configuration from a base class or interface configuration
- Runtime polymorphic mapping

Inheriting base class configuration is opt-in, and you can either explicitly specify the mapping to inherit from the base type configuration with `Include` or in the derived type configuration with `IncludeBase`:

```c#
CreateMap<BaseEntity, BaseDto>()
   .Include<DerivedEntity, DerivedDto>()
   .ForMember(dest => dest.SomeMember, opt => opt.MapFrom(src => src.OtherMember));

CreateMap<DerivedEntity, DerivedDto>();
```

or

```c#
CreateMap<BaseEntity, BaseDto>()
   .ForMember(dest => dest.SomeMember, opt => opt.MapFrom(src => src.OtherMember));

CreateMap<DerivedEntity, DerivedDto>()
    .IncludeBase<BaseEntity, BaseDto>();
```

In each case above, the derived mapping inherits the custom mapping configuration from the base map.

`Include`/`IncludeBase` applies recursively, so you only need to include the closest level in the hierarchy.

If for some base class you have many directly derived classes, as a convenience, you can include all derived maps from the base type map configuration:

```c#
CreateMap<BaseEntity, BaseDto>()
    .IncludeAllDerived();

CreateMap<DerivedEntity, DerivedDto>();
```
Note that this will search all your mappings for derived types and it will be slower than explicitly specifying the derived maps.

### Runtime polymorphism

Take:

```c#
public class Order { }
public class OnlineOrder : Order { }
public class MailOrder : Order { }

public class OrderDto { }
public class OnlineOrderDto : OrderDto { }
public class MailOrderDto : OrderDto { }

var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .Include<OnlineOrder, OnlineOrderDto>()
        .Include<MailOrder, MailOrderDto>();
    cfg.CreateMap<OnlineOrder, OnlineOrderDto>();
    cfg.CreateMap<MailOrder, MailOrderDto>();
}, loggerFactory);

// Perform Mapping
var order = new OnlineOrder();
var mapped = mapper.Map(order, order.GetType(), typeof(OrderDto));
Assert.IsType<OnlineOrderDto>(mapped);
```

You will notice that because the mapped object is a OnlineOrder, AutoMapper has seen you have a more specific mapping for OnlineOrder than OrderDto, and automatically chosen that.

## Specifying inheritance in derived classes

Instead of configuring inheritance from the base class, you can specify inheritance from the derived classes:

```c#
var configuration = new MapperConfiguration(cfg => {
  cfg.CreateMap<Order, OrderDto>()
    .ForMember(o => o.Id, m => m.MapFrom(s => s.OrderId));
  cfg.CreateMap<OnlineOrder, OnlineOrderDto>()
    .IncludeBase<Order, OrderDto>();
  cfg.CreateMap<MailOrder, MailOrderDto>()
    .IncludeBase<Order, OrderDto>();
}, loggerFactory);
```

## As

For simple cases, you can use `As` to redirect a base map to an existing derived map:

```c#
    cfg.CreateMap<Order, OnlineOrderDto>();
    cfg.CreateMap<Order, OrderDto>().As<OnlineOrderDto>();
    
    mapper.Map<OrderDto>(new Order()).ShouldBeOfType<OnlineOrderDto>();
```

## Inheritance Mapping Priorities

This introduces additional complexity because there are multiple ways a property can be mapped. The priority of these sources are as follows

 - Explicit Mapping (using .MapFrom())
 - Inherited Explicit Mapping
 - Ignore Property Mapping
 - Convention Mapping (Properties that are matched via convention)

To demonstrate this, lets modify our classes shown above

```c#
//Domain Objects
public class Order { }
public class OnlineOrder : Order
{
    public string Referrer { get; set; }
}
public class MailOrder : Order { }

//Dtos
public class OrderDto
{
    public string Referrer { get; set; }
}

//Mappings
var configuration = new MapperConfiguration(cfg => {
    cfg.CreateMap<Order, OrderDto>()
        .Include<OnlineOrder, OrderDto>()
        .Include<MailOrder, OrderDto>()
        .ForMember(o=>o.Referrer, m=>m.Ignore());
    cfg.CreateMap<OnlineOrder, OrderDto>();
    cfg.CreateMap<MailOrder, OrderDto>();
}, loggerFactory);

// Perform Mapping
var order = new OnlineOrder { Referrer = "google" };
var mapped = mapper.Map(order, order.GetType(), typeof(OrderDto));
Assert.IsNull(mapped.Referrer);
```

Notice that in our mapping configuration, we have ignored `Referrer` (because it doesn't exist in the order base class) and that has a higher priority than convention mapping, so the property doesn't get mapped.

If you do want the `Referrer` property to be mapped in the mapping from `OnlineOrder` to `OrderDto` you should include an explicit mapping in the mapping like this:

```
    cfg.CreateMap<OnlineOrder, OrderDto>()
        .ForMember(o=>o.Referrer, m=>m.MapFrom(x=>x.Referrer));
```

Overall this feature should make using AutoMapper with classes that leverage inheritance feel more natural.
