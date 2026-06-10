namespace AutoMapper.IntegrationTests;

public class NullSubstitute(DatabaseFixture databaseFixture) : IntegrationTest<NullSubstitute.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        public double? Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CustomerViewModel
    {
        public double Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
            });

            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute(5));
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBe(5);
        }
    }
}
public class NullSubstituteWithStrings(DatabaseFixture databaseFixture) : IntegrationTest<NullSubstituteWithStrings.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class CustomerViewModel
    {
        public string Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith" });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute("5")));
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBe("5");
        }
    }
}
public class NullSubstituteWithEntity(DatabaseFixture databaseFixture) : IntegrationTest<NullSubstituteWithEntity.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public Value Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Value
    {
        public int Id { get; set; }
    }
    public class CustomerViewModel
    {
        public ValueViewModel Value { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class ValueViewModel
    {
        public int Id { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith" });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute(new Value()));
        cfg.CreateProjection<Value, ValueViewModel>();
    });
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBeNull();
        }
    }
}