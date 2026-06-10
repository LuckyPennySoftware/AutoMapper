namespace AutoMapper.IntegrationTests.MaxDepth;

public class NestedDtos(DatabaseFixture databaseFixture) : IntegrationTest<NestedDtos.DatabaseInitializer>(databaseFixture)
{
    ArtDto _destination;

    public class Sem
    {
        [Key]
        public int Key { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Art> Arts { get; set; }
    }

    public class Art
    {
        [Key]
        public int Key { get; set; }
        public string AName { get; set; }

        [ForeignKey("Sem")]
        public int? SemKey { get; set; }
        public virtual Sem Sem { get; set; }
    }

    public class SemDto
    {
        public int Key { get; set; }
        public string Name { get; set; }
    }

    public class ArtDto
    {
        public int Key { get; set; }
        public string AName { get; set; }

        public SemDto Sem { get; set; }
    }

    public class TestContext : LocalDbContext
    {
        public DbSet<Art> Arts { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext context)
        {
            context.Arts.AddRange(new[] {
                new Art { AName = "art1", Sem = new Sem { Name = "sem1" } },
                new Art { AName = "art2", Sem = new Sem { Name = "sem2" } },
                new Art { AName = "art3", Sem = new Sem { Name = "sem3" } },
            });

            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Sem, SemDto>().MaxDepth(1).ConstructUsing(s => new SemDto());
        cfg.CreateProjection<Art, ArtDto>().MaxDepth(1).ConstructUsing(s => new ArtDto());
    });

    [Fact]
    public void Should_project_nested_dto()
    {
        using (var context = Fixture.CreateContext())
        {
            _destination = ProjectTo<ArtDto>(context.Arts).FirstOrDefault();
        }
        _destination.AName.ShouldBe("art1");
        _destination.Sem.Name.ShouldBe("sem1");
    }
}