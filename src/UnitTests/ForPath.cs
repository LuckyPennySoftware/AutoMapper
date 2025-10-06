namespace AutoMapper.UnitTests;

public class InheritForPath : AutoMapperSpecBase
{
    public class RootModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public NestedModel Nested { get; set; }
    }

    public class NestedModel
    {
        public int NestedID { get; set; }
        public string NestedTitle { get; set; }
        public string NestedTitle2 { get; set; }
    }

    public class DerivedModel : RootModel
    {
        public string DescendantField { get; set; }
    }
    // destination types
    public class DataModel
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int OtherID { get; set; }
        public string Title2 { get; set; }
    }

    public class DerivedDataModel : DataModel
    {
        public string DescendantField { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<RootModel, DataModel>()
                        .ForMember(dest => dest.OtherID, opt => opt.MapFrom(src => src.Nested.NestedID))
                        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Nested.NestedTitle))
                        .ForMember(dest => dest.Title2, opt => opt.MapFrom(src => src.Nested.NestedTitle2))
                        .ReverseMap()
                        .ForPath(d=>d.Nested.NestedTitle2, o=>o.Ignore());

        cfg.CreateMap<DerivedModel, DerivedDataModel>()
                        .IncludeBase<RootModel, DataModel>().ReverseMap()
                        .ForPath(d=>d.Nested.NestedTitle, o=>o.Ignore())
                        .ForPath(d => d.Nested.NestedTitle2, opt => opt.MapFrom(src => src.Title2));
    });

    [Fact]
    public void Should_work()
    {
        var source = new DerivedDataModel() { OtherID = 2, Title2 = "nested test", ID = 1, Title = "test", DescendantField = "descendant field" };
        var destination = Mapper.Map<DerivedModel>(source);
        destination.Nested.NestedID.ShouldBe(2);
        destination.Nested.NestedTitle.ShouldBeNull();
        destination.Nested.NestedTitle2.ShouldBe("nested test");
    }
}

public class ForPath : AutoMapperSpecBase
{
    public class Order
    {
        public CustomerHolder CustomerHolder { get; set; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForPath(o=>o.CustomerHolder.Customer.Name, o=>o.MapFrom(s=>s.CustomerName))
            .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
    });

    [Fact]
    public void Should_unflatten()
    {
        var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
        var model = Mapper.Map<Order>(dto);
        model.CustomerHolder.Customer.Name.ShouldBe("George Costanza");
        model.CustomerHolder.Customer.Total.ShouldBe(74.85m);
    }
}

public class ForPathWithoutSettersForSubObjects : AutoMapperSpecBase
{
    public class Order
    {
        public CustomerHolder CustomerHolder { get; set; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
            .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
    });

    [Fact]
    public void Should_unflatten()
    {
        new Action(() => Mapper.Map<Order>(new OrderDto())).ShouldThrowException<AutoMapperMappingException>(ex =>
              ex.InnerException?.Message.ShouldBe("typeMapDestination.CustomerHolder.Customer cannot be null because it's used by ForPath."));
    }
}

public class ForPathWithoutSettersShouldBehaveAsForMember : AutoMapperSpecBase
{
    public class Order
    {
        public CustomerHolder CustomerHolder { get; set; }
        public int Value { get; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; }
        public decimal Total { get; }
    }

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForMember(o=>o.Value, o=>o.MapFrom(src => 9))
            .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
            .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
    });

    [Fact]
    public void Should_unflatten()
    {
        var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
        var model = Mapper.Map<Order>(dto);
        model.Value.ShouldBe(0);
        model.CustomerHolder.Customer.Name.ShouldBeNull();
        model.CustomerHolder.Customer.Total.ShouldBe(0m);
    }
}

public class ForPathWithIgnoreShouldNotSetValue : AutoMapperSpecBase
{
    public partial class TimesheetModel
    {
        public int ID { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Contact { get; set; }
        public ContactModel ContactNavigation { get; set; }
    }

    public class TimesheetViewModel
    {
        public int? Contact { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class ContactModel
    {
        public int Id { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<TimesheetModel, TimesheetViewModel>()
            .ForMember(d => d.Contact, o => o.MapFrom(s => s.ContactNavigation.Id))
            .ReverseMap()
            .ForPath(s => s.ContactNavigation.Id, opt => opt.Ignore());
    });

    [Fact]
    public void Should_not_set_value()
    {
        var source = new TimesheetModel
        {
            Contact = 6,
            ContactNavigation = new ContactModel
            {
                Id = 5
            }
        };
        var dest = new TimesheetViewModel
        {
            Contact = 10
        };
        Mapper.Map(dest, source);

        source.ContactNavigation.Id.ShouldBe(5);
    }
}

public class ForPathWithNullExpressionShouldFail
{
    public class DestinationModel
    {
        public string Name { get; set; }
    }

    public class SourceModel
    {
        public string Name { get; set; }
    }
    
    [Fact]
    public void Should_throw_exception()
    {
        Assert.Throws<AutoMapperMappingException>(() =>
        {
            var cfg = new MapperConfiguration(config =>
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    config.CreateMap<SourceModel, DestinationModel>()
                        .ForPath(sourceModel => sourceModel.Name, opts => opts.MapFrom<string>(null));
                });
            });
        });
    }
}

public class ForPathWithPrivateSetters : AutoMapperSpecBase
{
    public class Order
    {
        public CustomerHolder CustomerHolder { get; private set; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; private set; }
    }

    public class Customer
    {
        public string Name { get; private set; }
        public decimal Total { get; private set; }
    }

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
            .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
    });

    [Fact]
    public void Should_unflatten()
    {
        var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
        var model = Mapper.Map<Order>(dto);
        model.CustomerHolder.Customer.Name.ShouldBe("George Costanza");
        model.CustomerHolder.Customer.Total.ShouldBe(74.85m);
    }
}

public class ForPathWithValueTypesAndFields : AutoMapperSpecBase
{
    public struct Order
    {
        public CustomerHolder CustomerHolder;
    }

    public struct CustomerHolder
    {
        public Customer Customer;
    }

    public struct Customer
    {
        public string Name;
        public decimal Total;
    }

    public struct OrderDto
    {
        public string CustomerName;
        public decimal Total;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
            .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
    });

    [Fact]
    public void Should_unflatten()
    {
        var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
        var model = Mapper.Map<Order>(dto);
        model.CustomerHolder.Customer.Name.ShouldBe("George Costanza");
        model.CustomerHolder.Customer.Total.ShouldBe(74.85m);
    }
}

public class ForPathWithConditions : AutoMapperSpecBase
{
    public class Order
    {
        public CustomerHolder CustomerHolder { get; set; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
        public int Value { get; set; }
    }

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<OrderDto, Order>()
            .ForPath(o => o.CustomerHolder.Customer.Name, o =>
            {
                o.Condition(c => !c.SourceMember.StartsWith("George"));
                o.MapFrom(s => s.CustomerName);
            })
            .ForPath(o => o.CustomerHolder.Customer.Total, o =>
            {
                o.Condition(c => c.Source.Total < 50);
                o.MapFrom(s => s.Total);
            })
            .ForPath(o => o.CustomerHolder.Customer.Value, o =>
            {
                o.Condition(c => c.Destination.CustomerHolder.Customer.Value == 0);
                o.MapFrom(s => s.Value);
            });
    });

    [Fact]
    public void Should_unflatten()
    {
        var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m, Value = 100 };
        var model = Mapper.Map<Order>(dto);
        model.CustomerHolder.Customer.Name.ShouldBeNull();
        model.CustomerHolder.Customer.Total.ShouldBe(0);
        model.CustomerHolder.Customer.Value.ShouldBe(100);
    }
}
