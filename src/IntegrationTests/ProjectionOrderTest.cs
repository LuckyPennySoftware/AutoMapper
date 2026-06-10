namespace AutoMapper.IntegrationTests;

public class ProjectionOrderTest(DatabaseFixture databaseFixture) : IntegrationTest<ProjectionOrderTest.DatabaseInitializer>(databaseFixture)
{
    public class Destination
    {
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class ChildDestination
    {
        public string String { get; set; }
    }

    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }


    public class Source1 : BaseEntity
    {
        public DateTime Date { get; set; }
        public virtual List<ChildSource> Items { get; set; }
    }

    public class Source2 : BaseEntity
    {
        public virtual List<ChildSource> Items { get; set; }
    }

    public class ChildSource : BaseEntity
    {
        public string String { get; set; }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Source1> Source1 { get; set; }

        public DbSet<Source2> Source2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source1, Destination>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Items.Count()))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));

        cfg.CreateProjection<Source2, Destination>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Items.Count()))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateTime.MinValue));
    });

    [Fact]
    public void Should_Not_Throw_NotSupportedException_On_Union()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<Destination>(context.Source1).Union(ProjectTo<Destination>(context.Source2)).ToString();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
    }
}