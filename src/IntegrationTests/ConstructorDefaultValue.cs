namespace AutoMapper.IntegrationTests;
public class ConstructorDefaultValue(DatabaseFixture databaseFixture) : IntegrationTest<ConstructorDefaultValue.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
    }
    public class CustomerViewModel
    {
        public CustomerViewModel(int value = 5) => Value = value;
        public int Value { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer());
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using var context = Fixture.CreateContext();
        ProjectTo<CustomerViewModel>(context.Customers).Single().Value.ShouldBe(5);
    }
}
public class StructConstructorMapping(DatabaseFixture databaseFixture) : IntegrationTest<StructConstructorMapping.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
    }
    public class CustomerViewModel
    {
        public DateOnly Date { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { Date = new(1984, 5, 23) });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>();
        cfg.CreateProjection<DateTime, DateOnly>();
    });
    [Fact]
    public void Can_map_with_projection()
    {
        using var context = Fixture.CreateContext();
        ProjectTo<CustomerViewModel>(context.Customers).Single().Date.ShouldBe(new(1984, 5, 23));
    }
}