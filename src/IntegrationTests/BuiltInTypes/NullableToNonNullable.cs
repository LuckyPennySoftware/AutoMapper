namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class NullableLongToLong(DatabaseFixture databaseFixture) : IntegrationTest<NullableLongToLong.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public long? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CustomerViewModel
    {
        public long Id { get; set; }
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
        cfg.CreateProjection<Customer, CustomerViewModel>();
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            var model = ProjectTo<CustomerViewModel>(context.Customers).Single();
            model.Id.ShouldBe(1);
            model.FirstName.ShouldBe("Bob");
            model.LastName.ShouldBe("Smith");
        }
    }
}

public class NullableIntToLong(DatabaseFixture databaseFixture) : IntegrationTest<NullableIntToLong.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CustomerViewModel
    {
        public long Id { get; set; }
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
        cfg.CreateProjection<Customer, CustomerViewModel>();
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using(var context = Fixture.CreateContext())
        {
            var model = ProjectTo<CustomerViewModel>(context.Customers).Single();
            model.Id.ShouldBe(1);
            model.FirstName.ShouldBe("Bob");
            model.LastName.ShouldBe("Smith");
        }
    }
}