namespace AutoMapper.UnitTests.Projection;

public class NonNullableToNullable : AutoMapperSpecBase
{
    class Source
    {
        public int Id { get; set; }
    }
    class Destination
    {
        public int? Id { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c=>c.CreateProjection<Source, Destination>());
    [Fact]
    public void Should_project() => ProjectTo<Destination>(new[] { new Source() }.AsQueryable()).First().Id.ShouldBe(0);
}
public class ProjectionAndMappingCombined : NonValidatingSpecBase
{
    class Source
    {
        public int Id { get; set; }
    }
    class Destination
    {
        public int Id { get; set; }
    }
    [Fact]
    public void Should_project()
    {
        var mappingConfiguration = new MapperConfiguration(cfg => cfg.CreateProjection<Source, Destination>());
        
        new[] { new Source() }.AsQueryable().ProjectTo<Destination>(mappingConfiguration).First().Id.ShouldBe(0);
    }

    [Fact]
    public void Should_not_map()
    {
        var mappingConfiguration = new MapperConfiguration(cfg => cfg.CreateProjection<Source, Destination>());
        var mapper = mappingConfiguration.CreateMapper();
        
        typeof(AutoMapperConfigurationException).ShouldBeThrownBy(() =>
            mapper.Map<Destination[]>(new[] { new Source() }.AsQueryable()));
    }
    
    [Fact]
    public void Should_map_and_project()
    {
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<Source, Destination>();
            cfg.CreateMap<Source, Destination>();
        });
        
        var mapper = mappingConfiguration.CreateMapper();
        
        typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() =>
            mapper.Map<Destination[]>(new[] { new Source() }.AsQueryable()));
        new[] { new Source() }.AsQueryable().ProjectTo<Destination>(mappingConfiguration).First().Id.ShouldBe(0);
        mapper.Map<Destination[]>(new[] { new Source() }.AsQueryable()).First().Id.ShouldBe(0);
    }
}
public class InMemoryMapObjectPropertyFromSubQuery : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        var products = new[] { new Product { Id = 1, ECommercePublished = true, Articles = new[] { new Article { Id = 1, IsDefault = true, NationId = 1, ProductId = 1 } } } }.AsQueryable();
        var projection = products.ProjectTo<ProductModel>(Configuration);
        var productModel = projection.First();
        productModel.Price.RegionId.ShouldBe((short)1);
        productModel.Price.IsDefault.ShouldBeTrue();
        productModel.Price.Id.ShouldBe(1);
        productModel.Id.ShouldBe(1);
    }

    public partial class Article
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public bool IsDefault { get; set; }
        public short NationId { get; set; }
        public virtual Product Product { get; set; }
    }

    public partial class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ECommercePublished { get; set; }
        public virtual ICollection<Article> Articles { get; set; }
        public int Value { get; }
        public int NotMappedValue { get; set; }
        public virtual List<Article> OtherArticles { get; }
    }

    public class PriceModel
    {
        public int Id { get; set; }
        public short RegionId { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ProductModel
    {
        public int Id { get; set; }
        public PriceModel Price { get; set; }
    }
}