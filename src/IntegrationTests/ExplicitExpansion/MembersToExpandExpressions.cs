namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class MembersToExpandExpressions  : AutoMapperSpecBase, IAsyncLifetime
{
    public class SourceDeepInner
    {
        public int Desc { get; set; }
        public int Id { get; set; }
    }
    public class SourceInner
    {
        public int Id { get; set; }
        public int Desc { get; set; }
        public SourceDeepInner Deep { get; set; }
    }
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Desc { get; set; }
        public SourceInner Inner { get; set; }
    }
    public class Dto
    {
        public string Name { get; set; }
        public int? Desc { get; set; }
        public int? InnerDescFlattened { get; set; }
        public int? DeepFlattened { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        cfg.CreateProjection<Source, Dto>()
            .ForMember(dto => dto.InnerDescFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner     .Desc); })
            .ForMember(dto => dto.     DeepFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Deep.Desc); }));
    public class TestContext : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    static Source _source = new Source() { Name = "Name1", Desc = -12, Inner = new SourceInner { Desc = -25, Deep = new SourceDeepInner() { Desc = 28 } } };
    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext testContext)
        {
            testContext.Sources.Add(_source);
            base.Seed(testContext);
        }
    }
    [Fact]
    public void Should_project_ok()
    {
        using (var context = new TestContext())
        {
            ProjectTo<Dto>(context.Sources, null, _ => _.Name).First().Name.ShouldBe(_source.Name);
            ProjectTo<Dto>(context.Sources, null, _ => _.Desc).First().Desc.ShouldBe(_source.Desc);
            ProjectTo<Dto>(context.Sources, null, _ => _.Name, _ => _.Desc);
            ProjectTo<Dto>(context.Sources, null, _ => _.InnerDescFlattened).First().InnerDescFlattened.ShouldBe(_source.Inner.Desc);
            ProjectTo<Dto>(context.Sources, null, _ => _.DeepFlattened).First().DeepFlattened.ShouldBe(_source.Inner.Deep.Desc);
        }
    }
    public async ValueTask InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}