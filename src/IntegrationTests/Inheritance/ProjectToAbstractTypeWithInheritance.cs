namespace AutoMapper.IntegrationTests.Inheritance;

public class ProjectToAbstractTypeWithInheritance(DatabaseFixture databaseFixture) : IntegrationTest<ProjectToAbstractTypeWithInheritance.DatabaseInitializer>(databaseFixture)
{
    public class StepGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Step> Steps { get; set; } = new();
    }
    public abstract class Step
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StepGroupId { get; set; }
        public virtual StepGroup StepGroup { get; set; }
        public virtual ICollection<StepInput> StepInputs { get; set; } = new HashSet<StepInput>();
    }
    public class CheckingStep : Step { }
    public class InstructionStep : Step { }
    public abstract class AbstractStep : Step { }
    public class StepInput
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public string Input { get; set; }
        public virtual Step Step { get; set; }
    }
    public class StepGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<StepModel> Steps { get; set; } = new();
    }
    public abstract class StepModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<StepInputModel> StepInputs { get; set; } = new HashSet<StepInputModel>();
    }
    public class CheckingStepModel : StepModel { }
    public class InstructionStepModel : StepModel { }
    public abstract class AbstractStepModel : StepModel { }
    public class StepInputModel
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public string Input { get; set; }
        public StepModel Step { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<StepGroup> StepGroups { get; set; }

        public DbSet<Step> Steps { get; set; }

        public DbSet<StepInput> StepInputs { get; set; }

        public DbSet<CheckingStep> CheckingSteps { get; set; }

        public DbSet<InstructionStep> InstructionSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Step>(entity =>
            {
                entity.HasOne(d => d.StepGroup).WithMany(p => p.Steps)
                    .HasForeignKey(d => d.StepGroupId);
            });

            modelBuilder.Entity<StepInput>(entity =>
            {
                entity.HasOne(d => d.Step).WithMany(p => p.StepInputs)
                    .HasForeignKey(d => d.StepId);
            });
        }
    }

    protected override MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<StepGroup, StepGroupModel>();
            cfg.CreateMap<Step, StepModel>();
            cfg.CreateMap<CheckingStep, CheckingStepModel>()
                .IncludeBase<Step, StepModel>();
            cfg.CreateMap<InstructionStep, InstructionStepModel>()
                .IncludeBase<Step, StepModel>();
            cfg.CreateMap<AbstractStep, AbstractStepModel>()
                .IncludeBase<Step, StepModel>();
            cfg.CreateMap<StepInput, StepInputModel>();
        });
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.StepGroups.Add(new StepGroup
            {
                Name = "StepGroup",
                Steps = new List<Step>
                {
                    new InstructionStep
                    {
                        Name = "InstructionStep",
                        StepInputs = new List<StepInput>
                        {
                            new StepInput
                            {
                                Input = "Input"
                            }
                        }
                    },
                    new CheckingStep
                    {
                        Name = "CheckingStep"
                    }
                }
            });

            base.Seed(context);
        }
    }

    [Fact]
    public void ProjectCollectionWithElementInheritingAbstractClass()
    {
        using var context = Fixture.CreateContext();
        var steps = ProjectTo<StepGroupModel>(context.StepGroups).Single().Steps;
        steps[0].ShouldBeOfType<CheckingStepModel>().Name.ShouldBe("CheckingStep");
        steps[1].ShouldBeOfType<InstructionStepModel>().Name.ShouldBe("InstructionStep");
    }

    [Fact]
    public void ProjectIncludingPolymorphicElement()
    {
        using var context = Fixture.CreateContext();
        var stepInput = ProjectTo<StepInputModel>(context.StepInputs).Single();
        stepInput.Step.ShouldBeOfType<InstructionStepModel>().Name.ShouldBe("InstructionStep");
    }
}