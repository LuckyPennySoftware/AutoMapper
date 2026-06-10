using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class ProjectAndAllowNullCollections(DatabaseFixture databaseFixture) : IntegrationTest<ProjectAndAllowNullCollections.DatabaseInitializer>(databaseFixture)
{
    public class Foo
    {
        public virtual int ID { get; set; }

        public virtual ISet<Bar> Bars { get; } = new HashSet<Bar>();

        public virtual ISet<Baz> Bazs { get; } = new HashSet<Baz>();
    }

    public class Bar
    {
        public virtual int ID { get; set; }

        public virtual string Value { get; set; }

        public virtual ISet<Foo> Foos { get; } = new HashSet<Foo>();
    }

    public class Baz
    {
        public virtual int ID { get; set; }

        public virtual string Value { get; set; }
        public virtual ISet<Foo> Foos { get; } = new HashSet<Foo>();
    }

    public class FooDto
    {
        public int ID { get; set; }

        public List<BarDto> Bars { get; set; }

        public List<BazDto> Bazs { get; set; }
    }

    public class BarDto
    {
        public int ID { get; set; }

        public string Value { get; set; }
    }

    public class BazDto
    {
        public int ID { get; set; }

        public string Value { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<MyContext>
    {
        protected override void Seed(MyContext context)
        {
            var foo1 = new Foo();
            var foo2 = new Foo(); // { Bars = new List<Bar>() };
            var foo3 = new Foo(); // { Bars = new List<Bar>() };

            context.Foos.Add(foo1);
            context.Foos.Add(foo2);
            context.Foos.Add(foo3);

            context.SaveChanges();

            var bar1 = new Bar { Value = "bar1" };
            var bar2 = new Bar { Value = "bar2" };

            foo2.Bars.Add(bar1);

            foo3.Bars.Add(bar1);
            foo3.Bars.Add(bar2);

            context.Bars.Add(bar1);
            context.Bars.Add(bar2);

            context.SaveChanges();
        }
    }

    public class MyContext : LocalDbContext
    {
        public DbSet<Foo> Foos { get; set; }

        public DbSet<Bar> Bars { get; set; }

        public DbSet<Baz> Bazs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyContext).Assembly);
        }
    }

    public class FooConfiguration : IEntityTypeConfiguration<Foo>
    {
        public void Configure(EntityTypeBuilder<Foo> builder)
        {
            builder.HasMany(m => m.Bars).WithMany(b => b.Foos);
            builder.HasMany(m => m.Bazs).WithMany(b => b.Foos);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.AllowNullCollections = true;

        c.CreateProjection<Foo, FooDto>()
            .ForMember(d => d.Bars, o => o.ExplicitExpansion())
            .ForMember(d => d.Bazs, o => o.ExplicitExpansion());

        c.CreateProjection<Bar, BarDto>();
        c.CreateProjection<Baz, BazDto>();
    });

    [Fact]
    public void Should_work()
    {
        using(var context = Fixture.CreateContext())
        {
            var foos = ProjectTo<FooDto>(context.Foos.AsNoTracking(), null, m => m.Bars).ToList();

            foos[0].Bars.ShouldNotBeNull();
            foos[0].Bazs.ShouldBeNull();
        }
    }
}