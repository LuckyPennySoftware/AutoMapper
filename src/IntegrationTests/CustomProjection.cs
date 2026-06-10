namespace AutoMapper.IntegrationTests;

public class CustomProjectionStringToString(DatabaseFixture databaseFixture) : IntegrationTest<CustomProjectionStringToString.DatabaseInitializer>(databaseFixture)
{
    public class TestContext : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    const string _badGreeting = "GRRRRR";
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Sources.Add(new Source { Greeting = _badGreeting });
            base.Seed(testContext);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(x => {
        x.CreateProjection<string, string>().ConvertUsing(s => _niceGreeting);
        x.CreateProjection<Source, Target>();
        x.CreateProjection<SourceChild, TargetChild>();
    });
    const string _niceGreeting = "Hello";
    [Fact]
    public void Direct_assignability_shouldnt_trump_custom_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<Target>(context.Sources).Single().Greeting.ShouldBe(_niceGreeting);
        }
    }
    public class Source
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
        public int Number { get; set; }
        public SourceChild Child { get; set; }
    }
    public class SourceChild
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
    }
    class Target
    {
        public string Greeting { get; set; }
        public int? Number { get; set; }
        public TargetChild Child { get; set; }
    }
    class TargetChild
    {
        public string Greeting { get; set; }
    }
}
public class CustomProjectionCustomClasses(DatabaseFixture databaseFixture) : IntegrationTest<CustomProjectionCustomClasses.DatabaseInitializer>(databaseFixture)
{
    public class TestContext : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Sources.Add(new Source());
            base.Seed(testContext);
        }
    }
    const string _niceGreeting = "Hello";
    protected override MapperConfiguration CreateConfiguration() => new(x =>
    {
        x.CreateProjection<Source, Target>().ConvertUsing(s => new Target { Greeting = _niceGreeting });
        x.CreateProjection<SourceChild, TargetChild>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<Target>(context.Sources).Single().Greeting.ShouldBe(_niceGreeting);
        }
    }
    public class Source
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
        public int Number { get; set; }
        public SourceChild Child { get; set; }
    }
    public class SourceChild
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
    }
    class Target
    {
        public string Greeting { get; set; }
        public int? Number { get; set; }
        public TargetChild Child { get; set; }
    }
    class TargetChild
    {
        public string Greeting { get; set; }
    }
}
public class CustomProjectionChildClasses(DatabaseFixture databaseFixture) : IntegrationTest<CustomProjectionChildClasses.DatabaseInitializer>(databaseFixture)
{
    public class TestContext : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Sources.Add(new Source { Child = new SourceChild { } });
            base.Seed(testContext);
        }
    }
    const string _niceGreeting = "Hello";
    protected override MapperConfiguration CreateConfiguration() => new(x =>
    {
        x.CreateProjection<Source, Target>();
        x.CreateProjection<SourceChild, TargetChild>().ConvertUsing(s => new TargetChild { Greeting = _niceGreeting });
    });
    [Fact]
    public void Should_work()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<Target>(context.Sources).Single().Child.Greeting.ShouldBe(_niceGreeting);
        }
    }
    public class Source
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
        public int Number { get; set; }
        public SourceChild Child { get; set; }
    }
    public class SourceChild
    {
        public int Id { get; set; }
        public string Greeting { get; set; }
    }
    class Target
    {
        public string Greeting { get; set; }
        public int? Number { get; set; }
        public TargetChild Child { get; set; }
    }
    class TargetChild
    {
        public string Greeting { get; set; }
    }
}