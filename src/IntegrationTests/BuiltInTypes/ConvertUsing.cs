namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class ConvertUsingWithNullables(DatabaseFixture databaseFixture) : IntegrationTest<ConvertUsingWithNullables.DatabaseInitializer>(databaseFixture)
{
    public class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateProjection<MyTable, MyTableModel>();
            CreateProjection<int, MyEnum>().ConvertUsing(x => (MyEnum)x);
            CreateProjection<int?, MyEnum>().ConvertUsing(x => x.HasValue ? (MyEnum)x.Value : MyEnum.Value1);
        }
    }

    public enum MyEnum
    {
        Value1 = 0,
        Value2 = 1
    }

    public class MyTable
    {
        public int Id { get; set; }
        public int EnumValue { get; set; }
        public int? EnumValueNullable { get; set; }
    }

    public class MyTableModel
    {
        public int Id { get; set; }
        public MyEnum EnumValue { get; set; }
        public MyEnum EnumValueNullable { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext context)
        {
            context.MyTable.AddRange(new[]{
                new MyTable { EnumValue = (int)MyEnum.Value2 },
                new MyTable { EnumValueNullable = (int?)MyEnum.Value1 },
            });
            base.Seed(context);
        }
    }

    public class TestContext : LocalDbContext
    {
        public DbSet<MyTable> MyTable { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AddProfile<MyProfile>());

    [Fact]
    public void Should_project_ok()
    {
        using(var context = Fixture.CreateContext())
        {
            var results = ProjectTo<MyTableModel>(context.MyTable).ToList();
            results[0].Id.ShouldBe(1);
            results[0].EnumValue.ShouldBe(MyEnum.Value2);
            results[0].EnumValueNullable.ShouldBe(MyEnum.Value1);
            results[1].Id.ShouldBe(2);
            results[1].EnumValue.ShouldBe(MyEnum.Value1);
            results[1].EnumValueNullable.ShouldBe(MyEnum.Value1);
        }
    }
}

public class ConvertUsingBug(DatabaseFixture databaseFixture) : IntegrationTest<ConvertUsingBug.DatabaseInitializer>(databaseFixture)
{
    public class Parent
    {
        [Key]
        public int ID { get; set; }
        public string ParentTitle { get; set; }

        public ICollection<Children> Children { get; set; }
    }

    public class Children
    {
        public int ID { get; set; }
        public string ChildTitle { get; set; }
    }

    public class ParentVM
    {
        [Key]
        public int ID { get; set; }
        public string ParentTitle { get; set; }
        public List<int> Children { get; set; }
    }

    public partial class ApplicationDBContext : LocalDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .HasMany(x => x.Children);
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Children> Children { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Parent, ParentVM>();
        cfg.CreateProjection<Children, int>()
            .ConvertUsing(c => c.ID);
    });

    [Fact]
    public void can_map_with_projection()
    {
        using (var db = Fixture.CreateContext())
        {
            var result = ProjectTo<ParentVM>(db.Parents);
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<ApplicationDBContext> { }
}
public class StringTypeConverter(DatabaseFixture databaseFixture) : IntegrationTest<StringTypeConverter.DatabaseInitializer>(databaseFixture)
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<string, string>().ConvertUsing(s => s ?? string.Empty);
        cfg.CreateMap<Planning, Appointment>().ForMember(a => a.Subject, o => o.MapFrom(p => p.Libelle ?? p.Service.Libelle));
    });
    [Fact]
    public void Should_work()
    {
        using var context = Fixture.CreateContext();
        var query = ProjectTo<Appointment>(context.Planning);
        query.Single().Subject.ShouldBe("Test");
    }
    public class Appointment
    {
        public int Id { get; set; }
        public string Subject { get; set; }
    }
    public class Planning
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public Service Service { get; set; }
    }
    public partial class Service
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
    }
    public partial class ApplicationDBContext : LocalDbContext
    {
        public DbSet<Planning> Planning { get; set; }
        public DbSet<Service> Service { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<ApplicationDBContext>
    {
        protected override void Seed(ApplicationDBContext context) => context.Planning.Add(new() { Libelle = "Test" });
    }
}