namespace AutoMapper.IntegrationTests;

public class IncludeMembers(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembers.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s=>s.InnerSource, s=>s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using(var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}
public class IncludeMembersExplicitExpansion(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersExplicitExpansion.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.ExplicitExpansion());
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.ExplicitExpansion());
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources, null, d=>d.Title);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBeNull();
            result.Title.ShouldBe("title");
        }
    }
}

public class IncludeMembersFirstOrDefault(DatabaseFixture databaseFixture)
    : IntegrationTest<IncludeMembersFirstOrDefault.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}

public class IncludeMembersFirstOrDefaultWithMapFromExpression(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultWithMapFromExpression.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { Description1 = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title1 = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom(s => s.Description1));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom(s => s.Title1));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultWithSubqueryMapFrom(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultWithSubqueryMapFrom.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetails.FirstOrDefault()));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.FirstOrDefault()));
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 4);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersSelectFirstOrDefaultWithSubqueryMapFrom(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersSelectFirstOrDefaultWithSubqueryMapFrom.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } }
                },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault(), s => s.OtherInnerSources.Select(s=>s).FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault()));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 4);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersFirstOrDefault(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersFirstOrDefault.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectFirstOrDefault(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectFirstOrDefault.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.Select(s => s).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefault(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefault.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source
            {
                Name = "name",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } }
                },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()));
        cfg.CreateProjection<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault());
        cfg.CreateProjection<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateProjection<InnerSourceDetails, DestinationDetails>();
        cfg.CreateProjection<OtherInnerSourceDetails, OtherDestinationDetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 6);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersWithMapFromExpression(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersWithMapFromExpression.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.MapFrom(s=>s.Description1));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.MapFrom(s=>s.Title1));
    });
    [Fact]
    public void Should_flatten_with_MapFrom()
    {
        using(var context = Fixture.CreateContext())
        {
            var result = ProjectTo<Destination>(context.Sources).Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}

public class IncludeMembersWithNullSubstitute(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersWithNullSubstitute.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
    });
    [Fact]
    public void Should_flatten()
    {
        using(var context = Fixture.CreateContext())
        {
            var result = ProjectTo<Destination>(context.Sources).Single();
            result.Name.ShouldBe("name");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class IncludeMembersMembersFirstOrDefaultWithNullSubstitute(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersMembersFirstOrDefaultWithNullSubstitute.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { Name = "name" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault());
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            FirstOrDefaultCounter.Assert(projectTo, 2);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class CascadedIncludeMembers(DatabaseFixture databaseFixture) : IntegrationTest<CascadedIncludeMembers.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id{ get; set; }
        public Level1 FieldLevel1{ get; set; }
    }
    public class Level1
    {
        public int Id{ get; set; }
        public Level2 FieldLevel2{ get; set; }
    }
    public class Level2
    {
        public int Id{ get; set; }
        public long TheField{ get; set; }
    }
    public class Destination
    {
        public int Id{ get; set; }
        public long TheField{ get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().IncludeMembers(s => s.FieldLevel1);
        cfg.CreateProjection<Level1, Destination>(MemberList.None).IncludeMembers(s => s.FieldLevel2);
        cfg.CreateProjection<Level2, Destination>(MemberList.None);
    });
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var source = new Source { FieldLevel1 = new Level1 { FieldLevel2 = new Level2 { TheField = 2 } } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources);
            var result = projectTo.Single();
            result.Id.ShouldBe(1);
            result.TheField.ShouldBe(2);
        }
    }
}
public class IncludeMembersWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class SourceA : Source
    {
        public InnerSourceA InnerSourceA { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", InnerSourceA = new InnerSourceA() { A = "a" }, InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceB);
            var sourceC = new Source { Name = "name3", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceC);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourceA);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None);
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.Title.ShouldBe("title");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.B.ShouldBe("b");

            var resultC = list[2].ShouldBeOfType<Destination>();
            resultC.Name.ShouldBe("name3");
            resultC.Description.ShouldBe("description");
            resultC.Title.ShouldBe("title");
        }
    }
}
public class IncludeMembersExplicitExpansionWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersExplicitExpansionWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class SourceA : Source
    {
        public InnerSourceA InnerSourceA { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", InnerSourceA = new InnerSourceA { A = "a" }, InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceB);
            var source = new Source { Name = "name3", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourceA);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.ExplicitExpansion());
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.ExplicitExpansion());
        cfg.CreateProjection<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.A, o => o.ExplicitExpansion());
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name), null, d => d.Title, d => d.GetType() == typeof(DestinationA) ? ((DestinationA)d).A : null);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBeNull();
            resultA.Title.ShouldBe("title");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBeNull();
            resultB.Title.ShouldBe("title");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBeNull();
            result.Title.ShouldBe("title");
        }
    }
}
public class IncludeMembersFirstOrDefaultWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { A = "a" } },
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None);
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 13);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.Title.ShouldBe("title");
            resultA.Author.ShouldBe("author");
            resultA.Publisher.ShouldBe("publisher");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.Author.ShouldBe("author");
            resultB.Publisher.ShouldBe("publisher");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultMixedPolymorhism(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultMixedPolymorhism.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { A = "a" } },
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<Source, DestinationB>()
            .Include<SourceA, DestinationA>()
            .Include<SourceA, Destination>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, Destination>();
        cfg.CreateMap<Source, DestinationB>().ForMember(s => s.B, o => o.MapFrom(s => "b"));
        cfg.CreateMap<InnerSource, Destination>(MemberList.None);
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.Title.ShouldBe("title");
            resultA.Author.ShouldBe("author");
            resultA.Publisher.ShouldBe("publisher");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.Author.ShouldBe("author");
            resultB.Publisher.ShouldBe("publisher");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultNoPolymorhism(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultNoPolymorhism.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { A = "a" } },
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { Description = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<Source, DestinationB>()
            .Include<SourceA, Destination>();
        cfg.CreateMap<SourceA, Destination>().ForMember(d=>d.Description, o=>o.MapFrom(s=>"descriptionA"));
        cfg.CreateMap<Source, DestinationB>().ForMember(s => s.B, o => o.MapFrom(s => "b"));
        cfg.CreateMap<InnerSource, Destination>(MemberList.None);
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, Destination>(MemberList.None);
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<Destination>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("descriptionA");
            resultA.Title.ShouldBe("title");
            resultA.Author.ShouldBe("author");
            resultA.Publisher.ShouldBe("publisher");

            var resultB = list[1].ShouldBeOfType<Destination>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.Author.ShouldBe("author");
            resultB.Publisher.ShouldBe("publisher");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultWithMapFromExpressionWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultWithMapFromExpressionWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
        public string Publisher { get; set; }
    }

    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { A = "a" } },
                InnerSources = { new InnerSource { Description1 = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title1 = "title", Author = "author" } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { Description1 = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title1 = "title", Author = "author" } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { Description1 = "description", Publisher = "publisher" } },
                OtherInnerSources = { new OtherInnerSource { Title1 = "title", Author = "author" } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom(s => s.Description1));
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom(s => s.Title1));
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.A, o => o.MapFrom(s => s.A));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 13);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.Title.ShouldBe("title");
            resultA.Author.ShouldBe("author");
            resultA.Publisher.ShouldBe("publisher");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.Author.ShouldBe("author");
            resultB.Publisher.ShouldBe("publisher");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
            result.Author.ShouldBe("author");
            result.Publisher.ShouldBe("publisher");
        }
    }
}
public class IncludeMembersFirstOrDefaultWithSubqueryMapFromWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersFirstOrDefaultWithSubqueryMapFromWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public List<InnerSourceDetailsA> InnerSourceDetails { get; } = new List<InnerSourceDetailsA>();
    }

    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceDetailsA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationA : Destination
    {
        public DestinationADetails DetailsA { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationADetails
    {
        public string A { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { InnerSourceDetails = { new InnerSourceDetailsA { A = "a" } } } },
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetails.FirstOrDefault()));
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.FirstOrDefault()));
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.DetailsA, o => o.MapFrom(s => s.InnerSourceDetails.FirstOrDefault()));
        cfg.CreateMap<InnerSourceDetails, DestinationDetails>();
        cfg.CreateMap<OtherInnerSourceDetails, OtherDestinationDetails>();
        cfg.CreateMap<InnerSourceDetailsA, DestinationADetails>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 40);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Details.Description.ShouldBe("description");
            resultA.Details.Publisher.ShouldBe("publisher");
            resultA.OtherDetails.Title.ShouldBe("title");
            resultA.OtherDetails.Author.ShouldBe("author");
            resultA.DetailsA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Details.Description.ShouldBe("description");
            resultB.Details.Publisher.ShouldBe("publisher");
            resultB.OtherDetails.Title.ShouldBe("title");
            resultB.OtherDetails.Author.ShouldBe("author");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersSelectFirstOrDefaultWithSubqueryMapFromWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersSelectFirstOrDefaultWithSubqueryMapFromWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceWrapperA> InnerSourceWrappersA { get; set; } = new List<InnerSourceWrapperA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSourceWrapperA
    {
        public int Id { get; set; }
        public InnerSourceA InnerSource { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapperA> InnerSourceDetailsWrapperA { get; } = new List<InnerSourceDetailsWrapperA>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetailsWrapperA
    {
        public int Id { get; set; }
        public InnerSourceDetailsA InnerSourceDetailsA { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceDetailsA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationA : Destination
    {
        public DestinationDetailsA DetailsA { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationDetailsA
    {
        public string A { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourceWrappersA = { new InnerSourceWrapperA { InnerSource = new InnerSourceA { InnerSourceDetailsWrapperA = { new InnerSourceDetailsWrapperA { InnerSourceDetailsA = new InnerSourceDetailsA { A = "a" } } } } } },
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault(), s => s.OtherInnerSources.Select(s => s).FirstOrDefault())
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourceWrappersA.Select(s => s.InnerSource).FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault()));
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault()));
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.DetailsA, o => o.MapFrom(s => s.InnerSourceDetailsWrapperA.Select(s => s.InnerSourceDetailsA).FirstOrDefault()));
        cfg.CreateMap<InnerSourceDetails, DestinationDetails>();
        cfg.CreateMap<OtherInnerSourceDetails, OtherDestinationDetails>();
        cfg.CreateMap<InnerSourceDetailsA, DestinationDetailsA>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 40);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Details.Description.ShouldBe("description");
            resultA.Details.Publisher.ShouldBe("publisher");
            resultA.OtherDetails.Title.ShouldBe("title");
            resultA.OtherDetails.Author.ShouldBe("author");
            resultA.DetailsA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Details.Description.ShouldBe("description");
            resultB.Details.Publisher.ShouldBe("publisher");
            resultB.OtherDetails.Title.ShouldBe("title");
            resultB.OtherDetails.Author.ShouldBe("author");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersFirstOrDefaultWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersFirstOrDefaultWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsA> InnerSourceDetailsA { get; } = new List<InnerSourceDetailsA>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceDetailsA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationA : Destination
    {
        public DestinationDetailsA DetailsA { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationDetailsA
    {
        public string A { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { InnerSourceDetailsA = { new InnerSourceDetailsA { A = "a" } } } },
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.FirstOrDefault()))
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>()
            .ForMember(d => d.DetailsA, o => o.MapFrom(s => s.InnerSourcesA.FirstOrDefault()));
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.FirstOrDefault());
        cfg.CreateMap<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.FirstOrDefault());
        cfg.CreateMap<InnerSourceA, DestinationDetailsA>().IncludeMembers(s => s.InnerSourceDetailsA.FirstOrDefault());
        cfg.CreateMap<InnerSourceDetails, DestinationDetails>();
        cfg.CreateMap<OtherInnerSourceDetails, OtherDestinationDetails>();
        cfg.CreateMap<InnerSourceDetailsA, DestinationDetailsA>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 33);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Details.Description.ShouldBe("description");
            resultA.Details.Publisher.ShouldBe("publisher");
            resultA.OtherDetails.Title.ShouldBe("title");
            resultA.OtherDetails.Author.ShouldBe("author");
            resultA.DetailsA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Details.Description.ShouldBe("description");
            resultB.Details.Publisher.ShouldBe("publisher");
            resultB.OtherDetails.Title.ShouldBe("title");
            resultB.OtherDetails.Author.ShouldBe("author");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectFirstOrDefaultWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectFirstOrDefaultWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetails> InnerSourceDetails { get; } = new List<InnerSourceDetails>();
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsA> InnerSourceDetailsA { get; } = new List<InnerSourceDetailsA>();
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceDetailsA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationA : Destination
    {
        public DestinationDetailsA DetailsA { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationDetailsA
    {
        public string A { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourcesA = { new InnerSourceA { InnerSourceDetailsA = { new InnerSourceDetailsA { A = "a" } } } },
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSources = { new InnerSource { InnerSourceDetails = { new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSources.Select(s => s).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()))
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>()
            .ForMember(d => d.DetailsA, o => o.MapFrom(s => s.InnerSourcesA.Select(s => s).FirstOrDefault()));
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateMap<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateMap<InnerSourceA, DestinationDetailsA>().IncludeMembers(s => s.InnerSourceDetailsA.Select(s => s).FirstOrDefault());
        cfg.CreateMap<InnerSourceDetails, DestinationDetails>();
        cfg.CreateMap<OtherInnerSourceDetails, OtherDestinationDetails>();
        cfg.CreateMap<InnerSourceDetailsA, DestinationDetailsA>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 33);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Details.Description.ShouldBe("description");
            resultA.Details.Publisher.ShouldBe("publisher");
            resultA.OtherDetails.Title.ShouldBe("title");
            resultA.OtherDetails.Author.ShouldBe("author");
            resultA.DetailsA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Details.Description.ShouldBe("description");
            resultB.Details.Publisher.ShouldBe("publisher");
            resultB.OtherDetails.Title.ShouldBe("title");
            resultB.OtherDetails.Author.ShouldBe("author");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefaultWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<SubqueryMapFromWithIncludeMembersSelectMemberFirstOrDefaultWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceWrapper> InnerSourceWrappers { get; set; } = new List<InnerSourceWrapper>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceWrapperA> InnerSourceWrappersA { get; set; } = new List<InnerSourceWrapperA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSourceWrapper
    {
        public int Id { get; set; }
        public InnerSource InnerSource { get; set; }
    }
    public class InnerSourceWrapperA
    {
        public int Id { get; set; }
        public InnerSourceA InnerSourceA { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapper> InnerSourceDetailsWrapper { get; } = new List<InnerSourceDetailsWrapper>();
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSourceDetailsWrapperA> InnerSourceDetailsWrapperA { get; } = new List<InnerSourceDetailsWrapperA>();
    }
    public class InnerSourceDetailsWrapper
    {
        public int Id { get; set; }
        public InnerSourceDetails InnerSourceDetails { get; set; }
    }
    public class InnerSourceDetailsWrapperA
    {
        public int Id { get; set; }
        public InnerSourceDetailsA InnerSourceDetailsA { get; set; }
    }
    public class InnerSourceDetails
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class InnerSourceDetailsA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<OtherInnerSourceDetails> OtherInnerSourceDetails { get; } = new List<OtherInnerSourceDetails>();
    }
    public class OtherInnerSourceDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DestinationDetails Details { get; set; }
        public OtherDestinationDetails OtherDetails { get; set; }
    }
    public class DestinationA : Destination
    {
        public DestinationDetailsA DetailsA { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class DestinationDetails
    {
        public string Description { get; set; }
        public string Publisher { get; set; }
    }
    public class DestinationDetailsA
    {
        public string A { get; set; }
    }
    public class OtherDestinationDetails
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA
            {
                Name = "name1",
                InnerSourceWrappersA = { new InnerSourceWrapperA { InnerSourceA = new InnerSourceA { InnerSourceDetailsWrapperA = { new InnerSourceDetailsWrapperA { InnerSourceDetailsA = new InnerSourceDetailsA { A = "a" } } } } } },
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB
            {
                Name = "name2",
                B = "b",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(sourceB);
            var source = new Source
            {
                Name = "name3",
                InnerSourceWrappers = { new InnerSourceWrapper { InnerSource = new InnerSource { InnerSourceDetailsWrapper = { new InnerSourceDetailsWrapper { InnerSourceDetails = new InnerSourceDetails { Description = "description", Publisher = "publisher" } } } } } },
                OtherInnerSources = { new OtherInnerSource { OtherInnerSourceDetails = { new OtherInnerSourceDetails { Title = "title", Author = "author" } } } }
            };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.InnerSourceWrappers.Select(s => s.InnerSource).FirstOrDefault()))
            .ForMember(d => d.OtherDetails, o => o.MapFrom(s => s.OtherInnerSources.Select(s => s).FirstOrDefault()))
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>()
            .ForMember(d => d.DetailsA, o => o.MapFrom(s => s.InnerSourceWrappersA.Select(s => s.InnerSourceA).FirstOrDefault()));
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, DestinationDetails>().IncludeMembers(s => s.InnerSourceDetailsWrapper.Select(s => s.InnerSourceDetails).FirstOrDefault());
        cfg.CreateMap<OtherInnerSource, OtherDestinationDetails>().IncludeMembers(s => s.OtherInnerSourceDetails.Select(s => s).FirstOrDefault());
        cfg.CreateMap<InnerSourceA, DestinationDetailsA>().IncludeMembers(s => s.InnerSourceDetailsWrapperA.Select(s => s.InnerSourceDetailsA).FirstOrDefault());
        cfg.CreateMap<InnerSourceDetails, DestinationDetails>();
        cfg.CreateMap<OtherInnerSourceDetails, OtherDestinationDetails>();
        cfg.CreateMap<InnerSourceDetailsA, DestinationDetailsA>();
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 33);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Details.Description.ShouldBe("description");
            resultA.Details.Publisher.ShouldBe("publisher");
            resultA.OtherDetails.Title.ShouldBe("title");
            resultA.OtherDetails.Author.ShouldBe("author");
            resultA.DetailsA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Details.Description.ShouldBe("description");
            resultB.Details.Publisher.ShouldBe("publisher");
            resultB.OtherDetails.Title.ShouldBe("title");
            resultB.OtherDetails.Author.ShouldBe("author");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Details.Description.ShouldBe("description");
            result.Details.Publisher.ShouldBe("publisher");
            result.OtherDetails.Title.ShouldBe("title");
            result.OtherDetails.Author.ShouldBe("author");
        }
    }
}
public class IncludeMembersWithMapFromExpressionWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersWithMapFromExpressionWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class SourceA : Source
    {
        public InnerSourceA InnerSourceA { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description1 { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title1 { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", InnerSourceA = new InnerSourceA { A = "a" }, InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            context.Sources.Add(sourceB);
            var source = new Source { Name = "name3", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourceA);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Description, o => o.MapFrom(s => s.Description1));
        cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.Title, o => o.MapFrom(s => s.Title1));
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.A, o => o.MapFrom(s => s.A));
    });
    [Fact]
    public void Should_flatten_with_MapFrom()
    {
        using (var context = Fixture.CreateContext())
        {
            var list = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name)).ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.Title.ShouldBe("title");
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.Title.ShouldBe("title");
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}
public class IncludeMembersWithNullSubstituteWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersWithNullSubstituteWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }
    public class SourceA : Source
    {
        public InnerSourceA InnerSourceA { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1" };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", };
            context.Sources.Add(sourceB);
            var source = new Source { Name = "name3" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourceA);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
        cfg.CreateProjection<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.A, o => o.NullSubstitute("a"));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var list = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name)).ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Code.ShouldBe(5);
            resultA.OtherCode.ShouldBe(7);
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Code.ShouldBe(5);
            resultB.OtherCode.ShouldBe(7);
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class IncludeMembersMembersFirstOrDefaultWithNullSubstituteWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMembersMembersFirstOrDefaultWithNullSubstituteWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InnerSource> InnerSources { get; set; } = new List<InnerSource>();
        public List<OtherInnerSource> OtherInnerSources { get; set; } = new List<OtherInnerSource>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
    }
    public class InnerSourceA
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? OtherCode { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public int OtherCode { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1" };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", };
            context.Sources.Add(sourceB);
            var source = new Source { Name = "name3" };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSources.FirstOrDefault(), s => s.OtherInnerSources.FirstOrDefault())
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
        cfg.CreateProjection<InnerSourceA, DestinationA>(MemberList.None).ForMember(d => d.A, o => o.NullSubstitute("a"));
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            FirstOrDefaultCounter.Assert(projectTo, 7);
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Code.ShouldBe(5);
            resultA.OtherCode.ShouldBe(7);
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Code.ShouldBe(5);
            resultB.OtherCode.ShouldBe(7);
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.Code.ShouldBe(5);
            result.OtherCode.ShouldBe(7);
        }
    }
}
public class CascadedIncludeMembersWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<CascadedIncludeMembersWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Level1 FieldLevel1 { get; set; }
    }
    public class SourceA : Source
    {
        public Level1A FieldLevel1A { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
    }
    public class Level1
    {
        public int Id { get; set; }
        public Level2 FieldLevel2 { get; set; }
    }
    public class Level2
    {
        public int Id { get; set; }
        public long TheField { get; set; }
    }

    public class Level1A
    {
        public int Id { get; set; }
        public Level2A FieldLevel2A { get; set; }
    }
    public class Level2A
    {
        public int Id { get; set; }
        public string A { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long TheField { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.FieldLevel1)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.FieldLevel1A);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateMap<Level1, Destination>(MemberList.None).IncludeMembers(s => s.FieldLevel2);
        cfg.CreateMap<Level2, Destination>(MemberList.None);
        cfg.CreateMap<Level1A, DestinationA>(MemberList.None).IncludeMembers(s => s.FieldLevel2A);
        cfg.CreateMap<Level2A, DestinationA>(MemberList.None);
    });
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", FieldLevel1A = new Level1A { FieldLevel2A = new Level2A { A = "a" } }, FieldLevel1 = new Level1 { FieldLevel2 = new Level2 { TheField = 2 } } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", FieldLevel1 = new Level1 { FieldLevel2 = new Level2 { TheField = 2 } } };
            context.Sources.Add(sourceB);
            var source = new Source { Name = "name3", FieldLevel1 = new Level1 { FieldLevel2 = new Level2 { TheField = 2 } } };
            context.Sources.Add(source);
            base.Seed(context);
        }
    }
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var list = projectTo.ToList();

            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.TheField.ShouldBe(2);
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.TheField.ShouldBe(2);
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<Destination>();
            result.Name.ShouldBe("name3");
            result.TheField.ShouldBe(2);
        }
    }
}
public class IncludeOnlySelectedMembersWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeOnlySelectedMembersWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
        public int ImNotIncluded { get; set; }
    }
    public class SourceA : Source
    {
        public string A { get; set; }
        public int ImNotIncludedA { get; set; }
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }

    public class InnerSourceA
    {
        public int Id { get; set; }
        public int ImNotIncludedA { get; set; }
        public string IAmIncluded { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
        public int ImNotIncludedB { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class DestinationA : Destination
    {
        public string A { get; set; }
        public string IAmIncluded { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", A = "a", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" }, InnerSourcesA = { new InnerSourceA { IAmIncluded = "a" } } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceB);
            var sourceC = new Source { Name = "name3", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceC);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault());
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var query = projectTo.ToQueryString();
            query.ShouldNotContain(nameof(Source.ImNotIncluded));

            var list = projectTo.ToList();
            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.A.ShouldBe("a");
            resultA.IAmIncluded.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.B.ShouldBe("b");

            var resultC = list[2].ShouldBeOfType<Destination>();
            resultC.Name.ShouldBe("name3");
            resultC.Description.ShouldBe("description");
        }
    }
}

public class IncludeMultipleExpressionsWithIheritance(DatabaseFixture databaseFixture) : IntegrationTest<IncludeMultipleExpressionsWithIheritance.DatabaseInitializer>(databaseFixture)
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
        public int ImNotIncluded { get; set; }
        public List<InnerSourceA> InnerSourcesA { get; set; } = new List<InnerSourceA>();
    }
    public class SourceA : Source
    {
        public List<InnerSourceA> InnerSourcesAFallback { get; set; } = new List<InnerSourceA>();
    }

    public class InnerSourceA
    {
        public int Id { get; set; }
        public int ImNotIncludedA { get; set; }
        public string IAmIncluded { get; set; }
    }
    public class SourceB : Source
    {
        public string B { get; set; }
        public int ImNotIncludedB { get; set; }
    }
    public class InnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class OtherInnerSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class DestinationA : Destination
    {
        public string IAmIncluded { get; set; }
    }
    public class DestinationB : Destination
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Source> Sources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SourceA>();
            modelBuilder.Entity<SourceB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var sourceA = new SourceA { Name = "name1", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" }, InnerSourcesAFallback = { new InnerSourceA { IAmIncluded = "fallback" } } };
            context.Sources.Add(sourceA);
            var sourceB = new SourceB { Name = "name2", B = "b", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceB);
            var sourceC = new Source { Name = "name3", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
            context.Sources.Add(sourceC);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource)
            .Include<SourceA, DestinationA>()
            .Include<SourceB, DestinationB>();
        cfg.CreateMap<SourceA, DestinationA>().IncludeMembers(s => s.InnerSourcesA.FirstOrDefault() ?? s.InnerSourcesAFallback.FirstOrDefault());
        cfg.CreateMap<InnerSourceA, DestinationA>(MemberList.None);
        cfg.CreateMap<SourceB, DestinationB>();
        cfg.CreateProjection<InnerSource, Destination>(MemberList.None);
        cfg.CreateProjection<OtherInnerSource, Destination>(MemberList.None);
    });
    [Fact]
    public void Should_flatten()
    {
        using (var context = Fixture.CreateContext())
        {
            var projectTo = ProjectTo<Destination>(context.Sources.OrderBy(p => p.Name));
            var list = projectTo.ToList();
            var resultA = list[0].ShouldBeOfType<DestinationA>();
            resultA.Name.ShouldBe("name1");
            resultA.Description.ShouldBe("description");
            resultA.IAmIncluded.ShouldBe("fallback");

            var resultB = list[1].ShouldBeOfType<DestinationB>();
            resultB.Name.ShouldBe("name2");
            resultB.Description.ShouldBe("description");
            resultB.B.ShouldBe("b");

            var resultC = list[2].ShouldBeOfType<Destination>();
            resultC.Name.ShouldBe("name3");
            resultC.Description.ShouldBe("description");
        }
    }
}