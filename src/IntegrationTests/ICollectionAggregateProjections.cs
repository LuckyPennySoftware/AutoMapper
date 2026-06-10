namespace AutoMapper.IntegrationTests;
public class ICollectionAggregateProjections(DatabaseFixture databaseFixture) : IntegrationTest<ICollectionAggregateProjections.DatabaseInitializer>(databaseFixture)
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

    public class CustomerViewModel
    {
        public int ItemCodesCount { get; set; }
        public int ItemCodesMin { get; set; }
        public int ItemCodesMax { get; set; }
        public int ItemCodesSum { get; set; }
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
        public List<int> ItemCodes { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<CustomerItemCodes, CustomerViewModel>());

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            var result = ProjectTo<CustomerViewModel>(context.Customers.Select(customer => new CustomerItemCodes
            {
                ItemCodes = customer.Items.Select(item => item.Code).ToList()
            })).Single();

            result.ItemCodesCount.ShouldBe(3);
            result.ItemCodesMin.ShouldBe(1);
            result.ItemCodesMax.ShouldBe(5);
            result.ItemCodesSum.ShouldBe(9);
        }
    }
}