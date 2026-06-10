using System.Text.RegularExpressions;

namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public static class Ext
{
    public static void SqlShouldSelectColumn   (this string sqlSelect, string columnName)=> sqlSelect.ShouldContain($".[{columnName}]");
    public static void SqlShouldNotSelectColumn(this string sqlSelect, string columnName)=> sqlSelect.ShouldNotContain($"[{columnName}]");
    public static void SqlFromShouldStartWith  (this string sqlSelect, string tableName)
    {
        Regex regex = new Regex($@"FROM(\s+)\[{tableName}\](\s+)AS");
        regex.Match(sqlSelect).Success.ShouldBeTrue();
        // sqlSelect.ShouldContain($"FROM [dbo].[{tableName}] AS");
    }
}

// Example of Value Type mapped to appropriate Nullable

public class ProjectionWithExplicitExpansion(DatabaseFixture databaseFixture) : IntegrationTest<ProjectionWithExplicitExpansion.DatabaseInitializer>(databaseFixture)
{
    public class SourceDeepInner
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 Dide { get; set; }
        public Int32 Did1 { get; set; }
    }
    public class SourceInner
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 Ides { get; set; }
        public Int32 Ide1 { get; set; }
        public SourceDeepInner Deep { get; set; }
    }
    public class Source
    {   [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 Desc { get; set; }
        public String Name { get; set; }
        public SourceInner Inner { get; set; }
    }
    public class Dto
    {
        public String Name { get; set; }
        public Nullable<int> Desc { get; set; }
        public Nullable<int> InnerDescFlattened   { get; set; }
        public Nullable<int> InnerFlattenedNonKey { get; set; }
        public Nullable<int> DeepFlattened       { get; set; }
    }

    public class Context : LocalDbContext
    {
        public List<string> Log = new List<string>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(s => Log.Add(s));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Source> Sources { get; set; }
        public DbSet<SourceInner> SourceInners { get; set; }
        public DbSet<SourceDeepInner> SourceDeepInners { get; set; }

        public string GetLastSelectSqlLogEntry() => Log.LastOrDefault(_ => _.Contains("SELECT"));
    }

    private static readonly IQueryable<Source> _iq = new List<Source> {
        new Source() { Name = "Name1", Desc = -12, Inner = new SourceInner {
            Ides = -25, Ide1 = -7,
            Deep = new SourceDeepInner() { Dide = 28, Did1 = 38,} } },
    } .AsQueryable();

    private static readonly Source _iqf = _iq.First();

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Sources.Add(_iqf);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dto>()
            .ForMember(dto => dto.Desc, conf => conf.ExplicitExpansion())
            .ForMember(dto => dto.Name, conf => conf.ExplicitExpansion())
            .ForMember(dto => dto.InnerDescFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Ides); })
            .ForMember(dto => dto.InnerFlattenedNonKey, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Ide1); })
            .ForMember(dto => dto.DeepFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Deep.Dide); })
            ;});

    [Fact]
    public void NoExplicitExpansion()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources).ToList().First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void ProjectReferenceType()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Name).First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

            dto.Name.ShouldBe(_iqf.Name); sqlSelect.SqlShouldSelectColumn   (nameof(_iqf.Name)); 
            dto.Desc.ShouldBeNull()        ; sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));  
        }
    }
    [Fact]
    public void ProjectValueType()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Desc).First();

            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

            dto.Desc.ShouldBe(_iqf.Desc); sqlSelect.ShouldContain   (nameof(_iqf.Desc));
            dto.Name.ShouldBeNull()        ; sqlSelect.ShouldNotContain(nameof(_iqf.Name)); 

        }
    }
    [Fact]
    public void ProjectBoth()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Name, _ => _.Desc).First();

            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldNotContain("JOIN");
            sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

            dto.Name.ShouldBe(_iqf.Name);
            dto.Desc.ShouldBe(_iqf.Desc);
        }
    }

    [Fact]
    public void ProjectInner()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.InnerDescFlattened).ToList().First();

            dto.InnerDescFlattened.ShouldBe(_iqf.Inner.Ides);
            dto.InnerFlattenedNonKey.ShouldBeNull();
            dto.DeepFlattened.ShouldBeNull();

            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void ProjectInnerNonKey()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.InnerFlattenedNonKey).ToList().First();

            dto.InnerFlattenedNonKey.ShouldBe(_iqf.Inner.Ide1);
            dto.InnerDescFlattened.ShouldBeNull();
            dto.DeepFlattened.ShouldBeNull();

            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
            sqlSelect.ShouldContain("JOIN");
            sqlSelect.ShouldContain(nameof(ctx.SourceInners));
            sqlSelect.ShouldNotContain(nameof(ctx.SourceDeepInners));
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
        }
    }

    [Fact]
    public void ProjectDeepInner()
    {
        using (var ctx = Fixture.CreateContext())
        {
            var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.DeepFlattened).ToList().First();
            var sqlSelect = ctx.GetLastSelectSqlLogEntry();
            sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));

            dto.DeepFlattened.ShouldBe(_iqf.Inner.Deep.Dide);
            dto.InnerDescFlattened.ShouldBeNull();

            sqlSelect.ShouldContain("JOIN");
            sqlSelect.ShouldContain(nameof(ctx.SourceInners));
            sqlSelect.ShouldContain("JOIN"); // ???
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
            sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
        }
    }
}
public class ConstructorExplicitExpansion(DatabaseFixture databaseFixture) : IntegrationTest<ConstructorExplicitExpansion.DatabaseInitializer>(databaseFixture)
{
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    record Dto(string Name){}
    public class Context : LocalDbContext
    {
        public DbSet<Entity> Entities { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Entities.Add(new(){ Name = "Name" });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c => c.CreateProjection<Entity, Dto>().ForCtorParam("Name", o=>o.ExplicitExpansion()));
    [Fact]
    public void Should_work()
    {
        using var context = Fixture.CreateContext();
        var dto = ProjectTo<Dto>(context.Entities).Single();
        dto.Name.ShouldBeNull();
        dto = ProjectTo<Dto>(context.Entities, null, d=>d.Name).Single();
        dto.Name.ShouldBe("Name");
    }
}