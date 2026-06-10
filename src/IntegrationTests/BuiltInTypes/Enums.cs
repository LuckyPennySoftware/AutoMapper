namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class EnumToUnderlyingType(DatabaseFixture databaseFixture) : IntegrationTest<EnumToUnderlyingType.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = ConsoleColor.Yellow });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe((int)ConsoleColor.Yellow);
        }
    }
}
public class UnderlyingTypeToEnum(DatabaseFixture databaseFixture) : IntegrationTest<UnderlyingTypeToEnum.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = (int)ConsoleColor.Yellow });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.Yellow);
        }
    }
}
public class EnumToEnum(DatabaseFixture databaseFixture) : IntegrationTest<EnumToEnum.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DayOfWeek ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = DayOfWeek.Saturday });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.DarkYellow);
        }
    }
}