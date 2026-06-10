namespace AutoMapper.IntegrationTests.Inheritance;

public class OverrideDestinationMappingsTest(DatabaseFixture databaseFixture) : IntegrationTest<OverrideDestinationMappingsTest.DatabaseInitializer>(databaseFixture)
{
    public class Context : LocalDbContext
    {
        public DbSet<Entity> Entity { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Entity.AddRange(new[]
            {
                new Entity { Child = new ChildEntity { SomeValue = "Alain Brito"} },
                new Entity { Child = new ChildEntity { SomeValue = "Jimmy Bogard"} },
                new Entity { Child = new ChildEntity { SomeValue = "Bill Gates"} }
            });
            base.Seed(context);
        }
    }

    [Fact]
    public void Map_WhenOverrideDestinationTypeAndSourceIsDerived_MustCreateOverriddenDestinationType()
    {
        Entity entity = LoadEntity();

        var model = Mapper.Map<Model>(entity);

        model.Child.ShouldBeOfType<ChildModel>();
    }

    private Entity LoadEntity()
    {
        using(var context = Fixture.CreateContext())
        {
            return context.Entity.Include(e => e.Child).First();
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Entity, Model>();

        cfg.CreateMap<ChildEntity, ChildModelBase>()
            .Include<ChildEntity, ChildModel>()
            .ForMember(x => x.SomeValue, x => x.Ignore())
            .As<ChildModel>();

        cfg.CreateMap<ChildEntity, ChildModel>();
    });

    public class Entity
    {
        public int Id { get; set; }
        public ChildEntity Child { get; set; }
    }

    public class ChildEntity
    {
        public int Id { get; set; }
        public string SomeValue { get; set; }
    }

    public class Model
    {
        public ChildModelBase Child { get; set; }
    }

    public abstract class ChildModelBase
    {
        public string SomeValue { get; set; }
    }

    public class ChildModel : ChildModelBase
    {
    }
}