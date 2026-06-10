namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class ByteArrayColumns(DatabaseFixture databaseFixture) : IntegrationTest<ByteArrayColumns.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public byte[] RowVersion { get; set; }
    }

    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] RowVersion { get; set; }
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
                RowVersion = new byte[] { 1, 2, 3 }
            });

            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>();
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            var customerVms = ProjectTo<CustomerViewModel>(context.Customers).ToList();
            customerVms.ForEach(x =>
            {
                x.RowVersion.SequenceEqual(new byte[] { 1, 2, 3 }).ShouldBeTrue();
            });
        }
    }
}