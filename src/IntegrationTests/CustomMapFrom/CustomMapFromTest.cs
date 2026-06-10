namespace AutoMapper.IntegrationTests.CustomMapFrom;

public class CustomMapFromTest(DatabaseFixture databaseFixture) : IntegrationTest<CustomMapFromTest.DatabaseInitializer>(databaseFixture)
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>()
            .ForMember(x => x.FullAddress, o => o.MapFrom(c => c.Address.Street + ", " + c.Address.City + " " + c.Address.State)));
    [Fact]
    public void can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            var customerVms = context.Customers.Select(c => new CustomerViewModel
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                FullAddress = c.Address.Street + ", " + c.Address.City + " " + c.Address.State
            }).ToList();

            customerVms.ForEach(x =>
            {
                x.FullAddress.ShouldNotBeNull();
                x.FullAddress.ShouldNotBeEmpty();
            });

            customerVms = ProjectTo<CustomerViewModel>(context.Customers).ToList();
            customerVms.ForEach(x =>
            {
                x.FullAddress.ShouldNotBeNull();
                x.FullAddress.ShouldNotBeEmpty();
            });
        }
    }
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Address Address { get; set; }
    }

    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullAddress { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }

    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Address = new Address
                {
                    Street = "123 Anywhere",
                    City = "Austin",
                    State = "TX"
                }
            });

            base.Seed(context);
        }
    }
}