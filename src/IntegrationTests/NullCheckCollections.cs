namespace AutoMapper.IntegrationTests;
public class NullCheckCollectionsFirstOrDefault(DatabaseFixture databaseFixture) : IntegrationTest<NullCheckCollectionsFirstOrDefault.DatabaseInitializer>(databaseFixture)
{
    public class SourceType
    {
        public int Id { get; set; }
        public ICollection<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
    public class Parameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
    public class DestinationType
    {
        public int? Index { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext context) => context.SourceTypes.Add(new SourceType { Parameters = { new Parameter { Name = "Index", Value = 101 } } });
    }
    public class TestContext : LocalDbContext
    {
        public DbSet<SourceType> SourceTypes { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateProjection<SourceType, DestinationType>().ForMember(d => d.Index, o => o.MapFrom(source => source.Parameters.FirstOrDefault(p => p.Name == "Index").Value)));
    [Fact]
    public void Should_project_ok()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<DestinationType>(context.SourceTypes).Single().Index.ShouldBe(101);
        }
    }
}
public class NullChildItemTest(DatabaseFixture databaseFixture) : IntegrationTest<NullChildItemTest.DatabaseInitializer>(databaseFixture)
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Parent, ParentDto>());
    public class TestContext : LocalDbContext
    {
        public DbSet<Parent> Parents { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Parents.Add(new Parent { Value = 5 });
            base.Seed(testContext);
        }
    }
    [Fact]
    public void Should_project_null_value()
    {
        using (var context = Fixture.CreateContext())
        {
            var query = ProjectTo<ParentDto>(context.Parents);
            var projected = query.Single();
            projected.Value.ShouldBe(5);
            projected.ChildValue.ShouldBeNull();
            projected.ChildGrandChildValue.ShouldBeNull();
            projected.Nephews.ShouldBeEmpty();
        }
    }
    public class ParentDto
    {
        public int? Value { get; set; }
        public int? ChildValue { get; set; }
        public int? ChildGrandChildValue { get; set; }
        public List<Child> Nephews { get; set; }
    }
    public class Parent
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public Child Child { get; set; }
        public List<Child> Nephews { get; set; }
    }
    public class Child
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public GrandChild GrandChild { get; set; }
    }
    public class GrandChild
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }
}
public class NullCheckCollections(DatabaseFixture databaseFixture) : IntegrationTest<NullCheckCollections.DatabaseInitializer>(databaseFixture)
{
    public class Student
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ScoreRecord> ScoreRecords { get; set; }
    }
    public class ScoreRecord
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public string Subject { get; set; }
        public int Score { get; set; }
    }
    public class ScoreModel
    {
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
    }
    public class StudentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ScoreModel Score { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Student> Students { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Students.Add(new Student{ Name = "Bob" });
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Student, StudentViewModel>().ForMember(d => d.Score, opts => opts.MapFrom(m => m.ScoreRecords));
        cfg.CreateProjection<ICollection<ScoreRecord>, ScoreModel>()
            .ForMember(d => d.MinScore, opts => opts.MapFrom(m => m.Min(s => s.Score)))
            .ForMember(d => d.MaxScore, opts => opts.MapFrom(m => m.Max(s => s.Score)));
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = Fixture.CreateContext())
        {
            ProjectTo<StudentViewModel>(context.Students).Single().Name.ShouldBe("Bob");
        }
    }
}