# Flattening

One of the common usages of object-object mapping is to take a complex object model and flatten it to a simpler model.  You can take a complex model such as:

```c#
public class Order
{
	private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

	public Customer Customer { get; set; }

	public OrderLineItem[] GetOrderLineItems()
	{
		return _orderLineItems.ToArray();
	}

	public void AddOrderLineItem(Product product, int quantity)
	{
		_orderLineItems.Add(new OrderLineItem(product, quantity));
	}

	public decimal GetTotal()
	{
		return _orderLineItems.Sum(li => li.GetTotal());
	}
}

public class Product
{
	public decimal Price { get; set; }
	public string Name { get; set; }
}

public class OrderLineItem
{
	public OrderLineItem(Product product, int quantity)
	{
		Product = product;
		Quantity = quantity;
	}

	public Product Product { get; private set; }
	public int Quantity { get; private set;}

	public decimal GetTotal()
	{
		return Quantity*Product.Price;
	}
}

public class Customer
{
	public string Name { get; set; }
}
```

We want to flatten this complex Order object into a simpler OrderDto that contains only the data needed for a certain scenario:

```c#
public class OrderDto
{
	public string CustomerName { get; set; }
	public decimal Total { get; set; }
}
```

When you configure a source/destination type pair in AutoMapper, the configurator attempts to match properties and methods on the source type to properties on the destination type.  If for any property on the destination type a property, method, or a method prefixed with "Get" does not exist on the source type, AutoMapper splits the destination member name into individual words (by PascalCase conventions).

```c#
// Complex model

var customer = new Customer
	{
		Name = "George Costanza"
	};
var order = new Order
	{
		Customer = customer
	};
var bosco = new Product
	{
		Name = "Bosco",
		Price = 4.99m
	};
order.AddOrderLineItem(bosco, 15);

// Configure AutoMapper

var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>(), loggerFactory);

// Perform mapping

OrderDto dto = mapper.Map<Order, OrderDto>(order);

dto.CustomerName.ShouldEqual("George Costanza");
dto.Total.ShouldEqual(74.85m);
```

We configured the type map in AutoMapper with the CreateMap method.  AutoMapper can only map type pairs it knows about, so we have explicitly register the source/destination type pair with CreateMap.  To perform the mapping, we use the Map method.

On the OrderDto type, the Total property matched to the GetTotal() method on Order.  The CustomerName property matched to the Customer.Name property on Order.  As long as we name our destination properties appropriately, we do not need to configure individual property matching.

If you want to disable this behavior, you can use the `ExactMatchNamingConvention`:
```
cfg.DestinationMemberNamingConvention = new ExactMatchNamingConvention();
```

## IncludeMembers

If you need more control when flattening, you can use IncludeMembers. You can map members of a child object to the destination object when you already have a map from the child type to the destination type (unlike the classic flattening that doesn't require a map for the child type).

```c#
class Source
{
    public string Name { get; set; }
    public InnerSource InnerSource { get; set; }
    public OtherInnerSource OtherInnerSource { get; set; }
}
class InnerSource
{
    public string Name { get; set; }
    public string Description { get; set; }
}
class OtherInnerSource
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Title { get; set; }
}
class Destination
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Title { get; set; }
}

cfg.CreateMap<Source, Destination>().IncludeMembers(s=>s.InnerSource, s=>s.OtherInnerSource);
cfg.CreateMap<InnerSource, Destination>(MemberList.None);
cfg.CreateMap<OtherInnerSource, Destination>();

var source = new Source { Name = "name", InnerSource = new InnerSource{ Description = "description" }, 
                          OtherInnerSource = new OtherInnerSource{ Title = "title" } };
var destination = mapper.Map<Destination>(source);
destination.Name.ShouldBe("name");
destination.Description.ShouldBe("description");
destination.Title.ShouldBe("title");
```
So this allows you to reuse the configuration in the existing map for the child types `InnerSource` and `OtherInnerSource` when mapping the parent types `Source` and `Destination`. It works in a similar way to [mapping inheritance](Mapping-inheritance.html), but it uses composition, not inheritance.

The order of the parameters in the `IncludeMembers` call is relevant. When mapping a destination member, the first match wins, starting with the source object itself and then with the included child objects in the order you specified. So in the example above, `Name` is mapped from the source object itself and `Description` from `InnerSource` because it's the first match.

Note that this matching is static, it happens at configuration time, not at `Map` time, so the runtime types of the child objects are not considered.

IncludeMembers integrates with `ReverseMap`. An included member will be reversed to 
```c#
ForPath(destination => destination.IncludedMember, member => member.MapFrom(source => source))
```
and the other way around. If that's not what you want, you can avoid `ReverseMap` (explicitly create the reverse map) or you can override the default settings (using `Ignore` or `IncludeMembers` without parameters respectively).

For details, check [the tests](https://github.com/LuckyPennySoftware/AutoMapper/blob/main/src/UnitTests/IMappingExpression/IncludeMembers.cs).
