namespace AutoMapper.IntegrationTests.ExplicitExpansion;

public class ExpandCollectionsWithStrings(DatabaseFixture databaseFixture) : IntegrationTest<ExpandCollectionsWithStrings.DatabaseInitializer>(databaseFixture)
{
    TrainingCourseDto _course;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Category, CategoryDto>();
        cfg.CreateProjection<TrainingCourse, TrainingCourseDto>();
        cfg.CreateProjection<TrainingContent, TrainingContentDto>().ForMember(c => c.Category, o => o.ExplicitExpansion());
    });

    [Fact]
    public void Should_expand_collections_items_with_strings()
    {
        using (var context = Fixture.CreateContext())
        {
            _course = ProjectTo<TrainingCourseDto>(context.TrainingCourses, null, "Content.Category").FirstOrDefault(n => n.CourseName == "Course 1");
        }
        _course.Content[0].Category.CategoryName.ShouldBe("Category 1");
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