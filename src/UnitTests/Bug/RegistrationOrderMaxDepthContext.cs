namespace AutoMapper.UnitTests.Bug;

public class RegistrationOrderMaxDepthContext
{
    public class InitiativeEntity { public List<EpicEntity> Epics { get; set; } = []; }
    public class InitiativeModel { public List<EpicModel> Epics { get; set; } = []; }
    public class EpicEntity
    {
        public Guid EpicId { get; set; }
        public string Name { get; set; }
        public List<StoryEntity> Stories { get; set; } = [];
        public List<WorkItemEntity> WorkItems { get; set; } = [];
    }
    public class StoryEntity
    {
        public Guid StoryId { get; set; }
        public EpicEntity Epic { get; set; }
        public Guid EpicId { get; set; }
        public List<WorkItemEntity> WorkItems { get; set; }
    }
    public class WorkItemEntity
    {
        public Guid WorkItemId { get; set; }
        public EpicEntity Epic { get; set; }
        public Guid EpicId { get; set; }
        public List<EstimateEntity> Estimates { get; set; }
        public List<StoryEntity> Stories { get; set; }
    }
    public class EstimateEntity
    {
        public Guid EstimateId { get; set; }
        public WorkItemEntity WorkItem { get; set; }
        public Guid WorkItemId { get; set; }
    }
    public class EpicModel
    {
        public Guid EpicId { get; set; }
        public string Name { get; set; }
        public List<WorkItemModel> WorkItems { get; set; }
        public List<StoryModel> Stories { get; set; }
    }
    public class StoryModel { public Guid StoryId { get; set; } public Guid EpicId { get; set; } }
    public class WorkItemModel
    {
        public Guid WorkItemId { get; set; }
        public Guid EpicId { get; set; }
        public List<EstimateModel> Estimates { get; set; }
        public List<StoryModel> Stories { get; set; }
    }
    public class EstimateModel { public Guid EstimateId { get; set; } public WorkItemModel WorkItem { get; set; } }

    static EpicEntity NewEpic()
    {
        var epic = new EpicEntity { EpicId = Guid.NewGuid(), Name = "Epic" };
        var story = new StoryEntity { StoryId = Guid.NewGuid(), Epic = epic, EpicId = epic.EpicId };
        var workItem = new WorkItemEntity { WorkItemId = Guid.NewGuid(), Epic = epic, EpicId = epic.EpicId };
        var estimate = new EstimateEntity { WorkItem = workItem, WorkItemId = workItem.WorkItemId };
        epic.Stories.Add(story);
        epic.WorkItems.Add(workItem);
        story.WorkItems = [workItem];
        workItem.Stories = [story];
        workItem.Estimates = [estimate];
        return epic;
    }

    static void AssertMapped(EpicModel model)
    {
        model.Stories.Count.ShouldBe(1);
        model.WorkItems.Count.ShouldBe(1);
        model.WorkItems[0].Estimates.Count.ShouldBe(1);
        model.WorkItems[0].Estimates[0].WorkItem.ShouldBeSameAs(model.WorkItems[0]);
    }

    [Fact]
    public void Should_map_with_initiative_registered_first()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<InitiativeEntity, InitiativeModel>();
            cfg.CreateMap<EpicEntity, EpicModel>();
            cfg.CreateMap<WorkItemEntity, WorkItemModel>();
            cfg.CreateMap<EstimateEntity, EstimateModel>();
            cfg.CreateMap<StoryEntity, StoryModel>();
        }).CreateMapper();
        AssertMapped(mapper.Map<EpicModel>(NewEpic()));
    }

    [Fact]
    public void Should_map_with_epic_registered_first()
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EpicEntity, EpicModel>();
            cfg.CreateMap<InitiativeEntity, InitiativeModel>();
            cfg.CreateMap<WorkItemEntity, WorkItemModel>();
            cfg.CreateMap<EstimateEntity, EstimateModel>();
            cfg.CreateMap<StoryEntity, StoryModel>();
        }).CreateMapper();
        AssertMapped(mapper.Map<EpicModel>(NewEpic()));
    }
}
