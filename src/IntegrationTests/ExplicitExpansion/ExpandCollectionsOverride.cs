namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class ExpandCollectionsOverride(DatabaseFixture databaseFixture) : IntegrationTest<ExpandCollectionsOverride.DatabaseInitializer>(databaseFixture)
{
    TrainingCourseDto _course;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Category, CategoryDto>();
        cfg.CreateMap<TrainingCourse, TrainingCourseDto>()
            .ForMember(p => p.CourseName, opt => opt.ExplicitExpansion());
        cfg.CreateMap<TrainingContent, TrainingContentDto>().ForMember(c => c.Category, o => o.ExplicitExpansion());
        cfg.CreateMap<TrainingCourse, TrainingCourseDetailDto>()
            .IncludeBase<TrainingCourse, TrainingCourseDto>()
            .ForMember(c => c.CourseName, opt => opt.ExplicitExpansion(false));
    });

    [Fact]
    public void Should_notexpand_courseName() {
        using (var context = Fixture.CreateContext()) {
            _course = ProjectTo<TrainingCourseDto>(context.TrainingCourses).FirstOrDefault();
        }
        _course.CourseName.ShouldBeNull();
    }

    [Fact]
    public void Should_expand_courseName() {
        using (var context = Fixture.CreateContext()) {
            _course = ProjectTo<TrainingCourseDetailDto>(context.TrainingCourses).FirstOrDefault();
        }
        _course.CourseName.ShouldBe("Course 1");
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var category = new Category { CategoryName = "Category 1" };
            var course = new TrainingCourse { CourseName = "Course 1" };
            context.TrainingCourses.Add(course);
            var content = new TrainingContent { ContentName = "Content 1", Category = category };
            context.TrainingContents.Add(content);
            course.Content.Add(content);
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<TrainingCourse> TrainingCourses { get; set; }
        public DbSet<TrainingContent> TrainingContents { get; set; }
    }

    public class TrainingCourse
    {
        [Key]
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public virtual IList<TrainingContent> Content { get; set; } = new List<TrainingContent>();
    }

    public class TrainingContent
    {
        [Key]
        public int ContentId { get; set; }

        public string ContentName { get; set; }
        public string CaptionName { get; set; }

        public Category Category { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }


    public class TrainingCourseDto
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public virtual IList<TrainingContentDto> Content { get; set; }
    }

    public class TrainingCourseDetailDto : TrainingCourseDto 
    {
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class TrainingContentDto
    {
        public int ContentId { get; set; }

        public string ContentName { get; set; }

        public CategoryDto Category { get; set; }
    }

}