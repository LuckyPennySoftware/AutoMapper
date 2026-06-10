namespace AutoMapper.IntegrationTests;
public class IEnumerableMemberProjections(DatabaseFixture databaseFixture) : IntegrationTest<IEnumerableMemberProjections.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }

    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }

    public class CustomerViewModel
    {
        public IEnumerable<ItemModel> Items { get; set; }
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
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });

            base.Seed(context);
        }
    }

    public class CustomerItemCodes
    {
        public IEnumerable<int> ItemCodes { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>();
        cfg.CreateProjection<Item, ItemModel>();
    });

    [Fact]
    public void Can_map_to_ienumerable()
    {
        using (var context = Fixture.CreateContext())
        {
            var result = ProjectTo<CustomerViewModel>(context.Customers).Single();

            result.Items.Count().ShouldBe(3);
        }
    }
}