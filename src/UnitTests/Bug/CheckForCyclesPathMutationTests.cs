namespace AutoMapper.UnitTests.Bug;

public class CheckForCyclesPathMutationTests : AutoMapperSpecBase
{
    public class Order
    {
        public LineWithParentFirst Line { get; set; }
    }

    public class LineWithParentFirst
    {
        public LineWithParentFirst Parent { get; set; }
        public Service Services { get; set; }
    }

    public class OrderWithServicesFirst
    {
        public LineWithServicesFirst Line { get; set; }
    }

    public class LineWithServicesFirst
    {
        public Service Services { get; set; }
        public LineWithServicesFirst Parent { get; set; }
    }

    public class Service
    {
        public string Name { get; set; }
    }

    public class OrderDto
    {
        public LineDto Line { get; set; }
    }

    public class LineDto
    {
        public LineDto Parent { get; set; }
        public ServiceDto Services { get; set; }
    }

    public class ServiceDto
    {
        public string Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Order, OrderDto>();
        cfg.CreateMap<LineWithParentFirst, LineDto>();

        cfg.CreateMap<OrderWithServicesFirst, OrderDto>();
        cfg.CreateMap<LineWithServicesFirst, LineDto>();

        cfg.CreateMap<Service, ServiceDto>();
    });

    [Fact]
    public void Inlining_Should_Be_Deterministic_Regardless_Of_Member_Order()
    {
        var lineWithParentFirstMap = Configuration.FindTypeMapFor<LineWithParentFirst, LineDto>();
        var lineWithServicesFirstMap = Configuration.FindTypeMapFor<LineWithServicesFirst, LineDto>();

        var servicesMemberParentFirst = lineWithParentFirstMap.MemberMaps.Single(m => m.DestinationName == nameof(LineDto.Services));
        var servicesMemberServicesFirst = lineWithServicesFirstMap.MemberMaps.Single(m => m.DestinationName == nameof(LineDto.Services));

        // Both should have identical inlining behavior regardless of declaration order
        servicesMemberParentFirst.Inline.ShouldBe(servicesMemberServicesFirst.Inline);
    }
}
