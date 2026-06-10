namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class NestedExplicitExpand(DatabaseFixture databaseFixture) : IntegrationTest<NestedExplicitExpand.DatabaseInitializer>(databaseFixture)
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        var mappingClass1 = cfg.CreateProjection<Class1, Class1DTO>();
        mappingClass1.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
        mappingClass1.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
        mappingClass1.ForMember(dest => dest.Class2DTO, opt =>
        {
            opt.MapFrom(src => src.Class2);
            opt.ExplicitExpansion();
        });

        var mappingClass2 = cfg.CreateProjection<Class2, Class2DTO>();
        mappingClass2.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
        mappingClass2.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));
        mappingClass2.ForMember(dest => dest.Class3DTO, opt =>
        {
            opt.MapFrom(src => src.Class3);
            opt.ExplicitExpansion();
        });

        var mappingClass3 = cfg.CreateProjection<Class3, Class3DTO>();
        mappingClass3.ForMember(dest => dest.IdDTO, opt => opt.MapFrom(src => src.Id));
        mappingClass3.ForMember(dest => dest.NameDTO, opt => opt.MapFrom(src => src.Name));

        //This is the trouble mapping
        mappingClass3.ForMember(dest => dest.Class2DTO, opt =>
        {
            opt.MapFrom(src => src.Class2);
            opt.ExplicitExpansion();
        });
    });

    [Fact]
    public void Should_handle_nested_explicit_expand_with_expressions()
    {
        Class1DTO[] dtos;
        using(TestContext context = Fixture.CreateContext())
        {
            dtos = ProjectTo<Class1DTO>(context.Class1Set, null, r => r.Class2DTO, r => r.Class2DTO.Class3DTO).ToArray();                
        }
        Check(dtos);
    }

    [Fact]
    public void Should_handle_nested_explicit_expand_with_strings()
    {
        Class1DTO[] dtos;
        using(TestContext context = Fixture.CreateContext())
        {
            dtos = ProjectTo<Class1DTO>(context.Class1Set, null, "Class2DTO", "Class2DTO.Class3DTO").ToArray();
        }
        Check(dtos);
    }

    private void Check(Class1DTO[] dtos)
    {
        dtos.Length.ShouldBe(3);
        dtos.Select(d => d.IdDTO).ShouldBe(new[] { 1, 2, 3 });
        dtos.Select(d => d.Class2DTO.IdDTO).ShouldBe(new[] { 1, 2, 3 });
        dtos.Select(d => d.Class2DTO.Class3DTO.IdDTO).ShouldBe(new[] { 1, 2, 3 });
        dtos.Select(d => d.Class2DTO.Class3DTO.Class2DTO).ShouldBe(new Class2DTO[] { null, null, null });
    }

    public class TestContext : LocalDbContext
    {
        public DbSet<Class1> Class1Set { get; set; }
        public DbSet<Class2> Class2Set { get; set; }
        public DbSet<Class3> Class3Set { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
    {
        protected override void Seed(TestContext context)
        {
            context.Class1Set.AddRange(new[]
            {
                new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "SomeValue" }}, Name = "Alain Brito"},
                new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "OtherValue" }}, Name = "Jimmy Bogard"},
                new Class1 { Class2 = new Class2 { Class3 = new Class3 { Name = "SomeValue" }}, Name = "Bill Gates"}
            });
            base.Seed(context);
        }
    }

    public class Class1DTO
    {
        public int IdDTO { get; set; }
        public string NameDTO { get; set; }

        public Class2DTO Class2DTO { get; set; }
    }

    public class Class2DTO
    {
        public int IdDTO { get; set; }
        public string NameDTO { get; set; }

        public Class3DTO Class3DTO { get; set; }
    }

    public class Class3DTO
    {
        public int IdDTO { get; set; }
        public string NameDTO { get; set; }

        public Class2DTO Class2DTO { get; set; }
    }

    public class Class1
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public Class2 Class2 { get; set; }
    }

    public class Class2
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public Class3 Class3 { get; set; }
    }

    public class Class3
    {
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute("Class2")]
        public int Id { get; set; }
        public string Name { get; set; }

        public Class2 Class2 { get; set; }
    }
}