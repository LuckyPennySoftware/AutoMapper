namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class ProjectEnumerableOfIntToList(DatabaseFixture databaseFixture) : IntegrationTest<ProjectEnumerableOfIntToList.DatabaseInitializer>(databaseFixture)
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }

    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> ItemsIds { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Items = new List<Item>(new[] { new Item(), new Item(), new Item()})
            });

            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d=>d.ItemsIds, o=>o.MapFrom(s=>s.Items.Select(i=>i.Id)));
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using(var context = Fixture.CreateContext())
        {
            var customer = ProjectTo<CustomerViewModel>(context.Customers).Single();
            customer.ItemsIds.SequenceEqual(new int[] { 1, 2, 3 }).ShouldBeTrue();
        }
    }
}