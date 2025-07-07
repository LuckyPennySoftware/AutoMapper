namespace AutoMapper.IntegrationTests.CustomMapFrom;
public class MultipleLevelsSubquery : IntegrationTest<MultipleLevelsSubquery.DatabaseInitializer>
{
    [Fact]
    public void Should_work()
    {
        using var context = new Context();
        var resultQuery = ProjectTo<FooModel>(context.Foos);
        resultQuery.Single().MyBar.MyBaz.FirstWidget.Id.ShouldBe(1);
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Foo, FooModel>().ForMember(f => f.MyBar, opts => opts.MapFrom(src => src.Bar));
        c.CreateMap<Bar, BarModel>().ForMember(f => f.MyBaz, opts => opts.MapFrom(src => src.Baz));
        c.CreateMap<Baz, BazModel>().ForMember(f => f.FirstWidget, opts => opts.MapFrom(src => src.Widgets.FirstOrDefault()));
        c.CreateMap<Widget, WidgetModel>();
    });
    public class Context : LocalDbContext
    {
        public virtual DbSet<Foo> Foos { get; set; }
        public virtual DbSet<Baz> Bazs { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var testBaz = new Baz();
            testBaz.Widgets.Add(new Widget());
            testBaz.Widgets.Add(new Widget());
            var testBar = new Bar();
            testBar.Foos.Add(new Foo());
            testBaz.Bars.Add(testBar);
            context.Bazs.Add(testBaz);
        }
    }
    public class Foo
    {
        public int Id { get; set; }
        public int BarId { get; set; }
        public virtual Bar Bar { get; set; }
    }
    public class Bar
    {
        public Bar() => Foos = new HashSet<Foo>();
        public int Id { get; set; }
        public int BazId { get; set; }
        public virtual Baz Baz { get; set; }
        public virtual ICollection<Foo> Foos { get; set; }
    }
    public class Baz
    {
        public Baz()
        {
            Bars = new HashSet<Bar>();
            Widgets = new HashSet<Widget>();
        }
        public int Id { get; set; }
        public virtual ICollection<Bar> Bars { get; set; }
        public virtual ICollection<Widget> Widgets { get; set; }
    }
    public partial class Widget
    {
        public int Id { get; set; }
        public int BazId { get; set; }
        public virtual Baz Baz { get; set; }
    }
    public class FooModel
    {
        public int Id { get; set; }
        public int BarId { get; set; }
        public BarModel MyBar { get; set; }
    }
    public class BarModel
    {
        public int Id { get; set; }
        public int BazId { get; set; }
        public BazModel MyBaz { get; set; }
    }
    public class BazModel
    {
        public int Id { get; set; }
        public WidgetModel FirstWidget { get; set; }
    }
    public class WidgetModel
    {
        public int Id { get; set; }
        public int BazId { get; set; }
    }
}
public class MemberWithSubQueryProjections : IntegrationTest<MemberWithSubQueryProjections.DatabaseInitializer>
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class CustomerViewModel
    {
        public CustomerNameModel Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class CustomerNameModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()));
        cfg.CreateProjection<Customer, CustomerNameModel>();
        cfg.CreateProjection<Item, ItemModel>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers);
            var result = resultQuery.Single();
            result.Name.FirstName.ShouldBe("Bob");
            result.Name.LastName.ShouldBe("Smith");
            result.FirstItem.Id.ShouldBe(1);
            result.FirstItem.Code.ShouldBe(1);
        }
    }
}
public class MemberWithSubQueryProjectionsNoMap : IntegrationTest<MemberWithSubQueryProjectionsNoMap.DatabaseInitializer>
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class CustomerViewModel
    {
        public string Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src.LastName : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()));
        cfg.CreateProjection<Item, ItemModel>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers);
            var result = resultQuery.Single();
            result.Name.ShouldBe("Smith");
            result.FirstItem.Id.ShouldBe(1);
            result.FirstItem.Code.ShouldBe(1);
        }
    }
}
public class MapObjectPropertyFromSubQueryTypeNameMax : IntegrationTest<MapObjectPropertyFromSubQueryTypeNameMax.DatabaseInitializer>
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
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(1);
            var productModel = projection.First();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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
        [NotMapped]
        public int NotMappedValue { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
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
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
    }
}

public class MapObjectPropertyFromSubQueryExplicitExpansion : IntegrationTest<MapObjectPropertyFromSubQueryExplicitExpansion.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o =>
            {
                o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault());
                o.ExplicitExpansion();
            });
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_map_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(0);
            var productModel = projection.First();
            productModel.Price.ShouldBeNull();
            productModel.Id.ShouldBe(1);
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Article> Articles { get; set; }
    }
}

public class MapObjectPropertyFromSubQuery : IntegrationTest<MapObjectPropertyFromSubQuery.DatabaseInitializer>
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
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(1);
            var productModel = projection.First();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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
        [NotMapped]
        public int NotMappedValue { get; set; }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
    }
}

public class MapObjectPropertyFromSubQueryWithInnerObject : IntegrationTest<MapObjectPropertyFromSubQueryWithInnerObject.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(2);
            var productArticleModel = projection.First();
            var productModel = productArticleModel.Product;
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
            var otherProductModel = productArticleModel.OtherProduct;
            otherProductModel.Price.RegionId.ShouldBe((short)1);
            otherProductModel.Price.IsDefault.ShouldBeTrue();
            otherProductModel.Price.Id.ShouldBe(2);
            otherProductModel.Id.ShouldBe(2);
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public Product OtherProduct { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public ProductModel Product { get; set; }
        public ProductModel OtherProduct { get; set; }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product1 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            var product2 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 2 } } });
            context.ProductArticles.Add(new ProductArticle { Product = product1.Entity, OtherProduct = product2.Entity });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }
}

public class MapObjectPropertyFromSubQueryWithCollection : IntegrationTest<MapObjectPropertyFromSubQueryWithCollection.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(1);
            var productModel = projection.First().Products.First();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public ICollection<ProductModel> Products { get; set; }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Products = new[] { product.Entity } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }
}

public class MapObjectPropertyFromSubQueryWithCollectionSameName : NonValidatingSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<ProductArticle, ProductArticleModel>();
        cfg.CreateProjection<Product, ProductModel>()
            .ForMember(d => d.ArticlesModel, o => o.MapFrom(s => s))
            .ForMember(d => d.Articles, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateProjection<Product, ArticlesModel>();
        cfg.CreateProjection<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles);
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(1);
            var productModel = projection.First().Products.First();
            Check(productModel.Articles);
            productModel.Id.ShouldBe(1);
            productModel.ArticlesCount.ShouldBe(1);
            productModel.ArticlesModel.Articles.Count.ShouldBe(1);
            Check(productModel.ArticlesModel.Articles.Single());
        }
    }

    private static void Check(PriceModel priceModel)
    {
        priceModel.RegionId.ShouldBe((short)1);
        priceModel.IsDefault.ShouldBeTrue();
        priceModel.Id.ShouldBe(1);
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public ICollection<ProductModel> Products { get; set; }
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
        public PriceModel Articles { get; set; }
        public int ArticlesCount { get; set; }
        public ArticlesModel ArticlesModel { get; set; }
    }

    public class ArticlesModel
    {
        public ICollection<PriceModel> Articles { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Products = new[] { product.Entity } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }
    }
    public async ValueTask InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class SubQueryWithMapFromNullable : IntegrationTest<SubQueryWithMapFromNullable.DatabaseInitializer>
{
    // Source Types
    public class Cable
    {
        public int CableId { get; set; }
        public ICollection<CableEnd> Ends { get; set; } = new List<CableEnd>();
    }

    public class CableEnd
    {
        [ForeignKey(nameof(CrossConnectId))]
        public virtual Cable CrossConnect { get; set; }
        [Column(Order = 0), Key]
        public int CrossConnectId { get; set; }
        [Column(Order = 1), Key]
        public string Name { get; set; }
        [ForeignKey(nameof(RackId))]
        public virtual Rack Rack { get; set; }
        public int? RackId { get; set; }
    }

    public class DataHall
    {
        public int DataHallId { get; set; }
        public int DataCentreId { get; set; }
        public ICollection<Rack> Racks { get; set; } = new List<Rack>();
    }

    public class Rack
    {
        public int RackId { get; set; }
        [ForeignKey(nameof(DataHallId))]
        public virtual DataHall DataHall { get; set; }
        public int DataHallId { get; set; }
    }

    // Dest Types
    public class CableListModel
    {
        public int CableId { get; set; }
        public CableEndModel AEnd { get; set; }
        public CableEndModel AnotherEnd { get; set; }
    }

    public class CableEndModel
    {
        public string Name { get; set; }
        public int? DataHallId { get; set; }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Cable> Cables { get; set; }
        public DbSet<CableEnd> CableEnds { get; set; }
        public DbSet<DataHall> DataHalls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CableEnd>().HasKey(c => new { c.CrossConnectId, c.Name });
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var rack = new Rack();
            var dh = new DataHall { DataCentreId = 10, Racks = { rack } };
            context.DataHalls.Add(dh);
            var cable = new Cable
            {
                Ends = new List<CableEnd>()
                    {
                        new CableEnd { Name = "A", Rack = rack},
                        new CableEnd { Name = "B" },
                    }
            };
            context.Cables.Add(cable);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<CableEnd, CableEndModel>().ForMember(dest => dest.DataHallId, opt => opt.MapFrom(src => src.Rack.DataHall.DataCentreId));
        cfg.CreateProjection<Cable, CableListModel>()
            .ForMember(dest => dest.AEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "A")))
            .ForMember(dest => dest.AnotherEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "B")));
    });

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<CableListModel>(context.Cables);
            var result = projection.Single();
            result.AEnd.DataHallId.ShouldBe(10);
            result.AnotherEnd.DataHallId.ShouldBeNull();
        }
    }
}

public class MapObjectPropertyFromSubQueryCustomSource : IntegrationTest<MapObjectPropertyFromSubQueryCustomSource.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Owner, OwnerDto>();
        cfg.CreateProjection<Brand, BrandDto>()
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owners.FirstOrDefault()));
        cfg.CreateProjection<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Product.Brand));
    });

    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Brand
    {
        public int Id { get; set; }
        public List<Owner> Owners { get; set; } = new List<Owner>();
    }
    public class Product
    {
        public int Id { get; set; }
        public Brand Brand { get; set; }
    }
    public class ProductReview
    {
        public int Id { get; set; }
        public Product Product { get; set; }
    }
    /* Destination types */
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public BrandDto Brand { get; set; }
    }
    public class BrandDto
    {
        public int Id { get; set; }
        public OwnerDto Owner { get; set; }
    }
    public class OwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.ProductReviews.Add(new ProductReview
            { Product = new Product { Brand = new Brand { Owners = { new Owner { Name = "Owner" } } } } });
            context.ProductReviews.Add(new ProductReview
            { Product = new Product { Brand = new Brand { Owners = { new Owner() } } } });
            context.ProductReviews.Add(new ProductReview { Product = new Product() });
        }
    }

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductReviewDto>(context.ProductReviews);
            var results = projection.ToArray();
            results.Any(result => result?.Brand?.Owner?.Name == "Owner").ShouldBeTrue();
            results.Any(result => result?.Brand?.Owner == null).ShouldBeTrue();
            results.Any(result => result?.Brand == null).ShouldBeTrue();
        }
    }
}

public class MemberWithSubQueryIdentity : IntegrationTest<MemberWithSubQueryIdentity.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new MapperConfiguration(cfg =>
    {
        cfg.CreateProjection<AEntity, Dto>()
            .ForMember(dst => dst.DtoSubWrapper, opt => opt.MapFrom(src => src));
        cfg.CreateProjection<AEntity, DtoSubWrapper>()
            .ForMember(dst => dst.DtoSub, opt => opt.MapFrom(src => src.BEntity.CEntities.FirstOrDefault(x => x.Id == src.CEntityId)));
        cfg.CreateProjection<CEntity, DtoSub>();
    });
    [Fact]
    public void Should_work()
    {
        var query = ProjectTo<Dto>(new ClientContext().AEntities);
        var result = query.Single();
        result.DtoSubWrapper.DtoSub.ShouldNotBeNull();
        result.DtoSubWrapper.DtoSub.SubString.ShouldBe("Test");
    }
    public class Dto
    {
        public int Id { get; set; }
        public DtoSubWrapper DtoSubWrapper { get; set; }
    }
    public class DtoSubWrapper
    {
        public DtoSub DtoSub { get; set; }
    }
    public class DtoSub
    {
        public int Id { get; set; }
        public string SubString { get; set; }
    }
    public class AEntity
    {
        public int Id { get; set; }
        public int BEntityId { get; set; }
        public int CEntityId { get; set; }
        public BEntity BEntity { get; set; }
    }
    public class BEntity
    {
        public int Id { get; set; }
        public ICollection<CEntity> CEntities { get; set; }
    }
    public class CEntity
    {
        public int Id { get; set; }
        public int BEntityId { get; set; }
        public string SubString { get; set; }
        public BEntity BEntity { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.AEntities.Add(new AEntity
            {
                CEntityId = 6,
                BEntity = new BEntity
                {
                    CEntities = new List<CEntity>
                        {
                            new CEntity
                            {
                                Id = 6,
                                BEntityId = 1,
                                SubString = "Test"
                            }
                        }
                },
            });
        }
    }
    public class ClientContext : LocalDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AEntity>()
                .HasOne(x => x.BEntity)
                .WithMany()
                .HasForeignKey(x => x.BEntityId);

            modelBuilder.Entity<BEntity>()
                .HasMany(x => x.CEntities)
                .WithOne(x => x.BEntity)
                .HasForeignKey(x => x.BEntityId);

            modelBuilder.Entity<CEntity>()
                .Property(x => x.Id)
                .ValueGeneratedNever();
        }
        public DbSet<AEntity> AEntities { get; set; }
    }
}

public class MultipleLevelsSubqueryWithInheritance : IntegrationTest<MultipleLevelsSubqueryWithInheritance.DatabaseInitializer>
{
    [Fact]
    public void Should_work()
    {
        using var context = new Context();
        var resultQuery = ProjectTo<FooModel>(context.Foos);
        resultQuery.Single().MyBar.MyBaz.FirstWidget.Id.ShouldBe(1);
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Foo, FooModel>().ForMember(f => f.MyBar, opts => opts.MapFrom(src => src.Bar));
        c.CreateMap<Bar, BarModel>().ForMember(f => f.MyBaz, opts => opts.MapFrom(src => src.Baz));
        c.CreateMap<Baz, BazModel>().ForMember(f => f.FirstWidget, opts => opts.MapFrom(src => src.Widgets.FirstOrDefault()));
        c.CreateMap<Widget, WidgetModel>();
    });
    public class Context : LocalDbContext
    {
        public virtual DbSet<Foo> Foos { get; set; }
        public virtual DbSet<Baz> Bazs { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var testBaz = new Baz();
            testBaz.Widgets.Add(new Widget());
            testBaz.Widgets.Add(new Widget());
            var testBar = new Bar();
            testBar.Foos.Add(new Foo());
            testBaz.Bars.Add(testBar);
            context.Bazs.Add(testBaz);
        }
    }
    public class Foo
    {
        public int Id { get; set; }
        public int BarId { get; set; }
        public virtual Bar Bar { get; set; }
    }
    public class Bar
    {
        public Bar() => Foos = new HashSet<Foo>();
        public int Id { get; set; }
        public int BazId { get; set; }
        public virtual Baz Baz { get; set; }
        public virtual ICollection<Foo> Foos { get; set; }
    }
    public class Baz
    {
        public Baz()
        {
            Bars = new HashSet<Bar>();
            Widgets = new HashSet<Widget>();
        }
        public int Id { get; set; }
        public virtual ICollection<Bar> Bars { get; set; }
        public virtual ICollection<Widget> Widgets { get; set; }
    }
    public partial class Widget
    {
        public int Id { get; set; }
        public int BazId { get; set; }
        public virtual Baz Baz { get; set; }
    }
    public class FooModel
    {
        public int Id { get; set; }
        public int BarId { get; set; }
        public BarModel MyBar { get; set; }
    }
    public class BarModel
    {
        public int Id { get; set; }
        public int BazId { get; set; }
        public BazModel MyBaz { get; set; }
    }
    public class BazModel
    {
        public int Id { get; set; }
        public WidgetModel FirstWidget { get; set; }
    }
    public class WidgetModel
    {
        public int Id { get; set; }
        public int BazId { get; set; }
    }
}
public class MemberWithSubQueryProjectionsWithInheritance : IntegrationTest<MemberWithSubQueryProjectionsWithInheritance.DatabaseInitializer>
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class CustomerA : Customer
    {
    }
    public class CustomerB : Customer
    {
        public string B { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemA : Item
    {
        public string A { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModelA : ItemModel
    {
        public string A { get; set; }
    }
    public class CustomerViewModel
    {
        public CustomerNameModel Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class CustomerAViewModel : CustomerViewModel
    {
        public ItemModelA FirstItemA { get; set; }
    }
    public class CustomerBViewModel : CustomerViewModel
    {
        public string B { get; set; }
    }
    public class CustomerNameModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CustomerA>();
            modelBuilder.Entity<CustomerB>();
            modelBuilder.Entity<ItemA>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new CustomerA
            {
                FirstName = "Alice",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new ItemA { Code = 3, A = "a", }, new Item { Code = 5 } }
            });
            context.Customers.Add(new CustomerB
            {
                FirstName = "Bob",
                LastName = "Smith",
                B = "b",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            }); context.Customers.Add(new Customer
            {
                FirstName = "Jim",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()))
            .Include<CustomerA, CustomerAViewModel>()
            .Include<CustomerB, CustomerBViewModel>();


        cfg.CreateMap<CustomerA, CustomerAViewModel>()
            .ForMember(dst => dst.FirstItemA, opt => opt.MapFrom(src => src.Items.OfType<ItemA>().FirstOrDefault()));
        cfg.CreateMap<CustomerB, CustomerBViewModel>();
        cfg.CreateMap<Customer, CustomerNameModel>();
        cfg.CreateMap<Item, ItemModel>()
            .Include<ItemA, ItemModelA>();
        cfg.CreateMap<ItemA, ItemModelA>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers.OrderBy(p => p.FirstName));
            var list = resultQuery.ToList();

            var resultA = list[0].ShouldBeOfType<CustomerAViewModel>();
            resultA.Name.FirstName.ShouldBe("Alice");
            resultA.Name.LastName.ShouldBe("Smith");
            resultA.FirstItem.Code.ShouldBe(1);
            resultA.FirstItemA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<CustomerBViewModel>();
            resultB.Name.FirstName.ShouldBe("Bob");
            resultB.Name.LastName.ShouldBe("Smith");
            resultB.FirstItem.Code.ShouldBe(1);
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<CustomerViewModel>();
            result.Name.FirstName.ShouldBe("Jim");
            result.Name.LastName.ShouldBe("Smith");
            result.FirstItem.Code.ShouldBe(1);
        }
    }
}
public class MemberWithSubQueryProjectionsNoMapWithInheritance : IntegrationTest<MemberWithSubQueryProjectionsNoMapWithInheritance.DatabaseInitializer>
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Item> Items { get; set; }
    }
    public class CustomerA : Customer
    {
        public string A { get; set; }
    }
    public class CustomerB : Customer
    {
        public string B { get; set; }
    }
    public class Item
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }
    public class CustomerViewModel
    {
        public string Name { get; set; }
        public ItemModel FirstItem { get; set; }
    }
    public class CustomerAViewModel : CustomerViewModel
    {
        public string A { get; set; }
    }
    public class CustomerBViewModel : CustomerViewModel
    {
        public string B { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CustomerA>();
            modelBuilder.Entity<CustomerB>();
        }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new CustomerA
            {
                FirstName = "Alice",
                LastName = "Smith",
                A = "a",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            context.Customers.Add(new CustomerB
            {
                FirstName = "Bob",
                LastName = "Smith",
                B = "b",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            }); context.Customers.Add(new Customer
            {
                FirstName = "Jim",
                LastName = "Smith",
                Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
            });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Customer, CustomerViewModel>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src.LastName : null))
            .ForMember(dst => dst.FirstItem, opt => opt.MapFrom(src => src.Items.FirstOrDefault()))
            .Include<CustomerA, CustomerAViewModel>()
            .Include<CustomerB, CustomerBViewModel>();

        cfg.CreateMap<CustomerA, CustomerAViewModel>();
        cfg.CreateMap<CustomerB, CustomerBViewModel>();
        cfg.CreateMap<Item, ItemModel>();
    });
    [Fact]
    public void Should_work()
    {
        using (var context = new Context())
        {
            var resultQuery = ProjectTo<CustomerViewModel>(context.Customers.OrderBy(p => p.FirstName));
            var list = resultQuery.ToList();

            var resultA = list[0].ShouldBeOfType<CustomerAViewModel>();
            resultA.Name.ShouldBe("Smith");
            resultA.FirstItem.Code.ShouldBe(1);
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<CustomerBViewModel>();
            resultB.Name.ShouldBe("Smith");
            resultB.FirstItem.Code.ShouldBe(1);
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<CustomerViewModel>();
            result.Name.ShouldBe("Smith");
            result.FirstItem.Code.ShouldBe(1);
        }
    }
}
public class MapObjectPropertyFromSubQueryTypeNameMaxWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryTypeNameMaxWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()))
            .Include<ProductA, ProductAModel>()
            .Include<ProductB, ProductBModel>();

        cfg.CreateMap<ProductA, ProductAModel>();
        cfg.CreateMap<ProductB, ProductBModel>();
        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(12);
            var list = projection.ToList();

            var productAModel = list[0].ShouldBeOfType<ProductAModel>();
            productAModel.Price.RegionId.ShouldBe((short)1);
            productAModel.Price.IsDefault.ShouldBeTrue();
            productAModel.A.ShouldBe("a");

            var productBModel = list[1].ShouldBeOfType<ProductBModel>();
            productBModel.Price.RegionId.ShouldBe((short)1);
            productBModel.Price.IsDefault.ShouldBeTrue();
            productBModel.B.ShouldBe("b");

            var productModel = list[2].ShouldBeOfType<ProductModel>();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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
        [NotMapped]
        public int NotMappedValue { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
    }

    public class ProductA : Product
    {
        public string A { get; set; }
    }
    public class ProductB : Product
    {
        public string B { get; set; }
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
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName1 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName2 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName3 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName4 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName5 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName6 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName7 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName8 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName9 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName10 { get; set; }
        public int VeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnNameVeryLongColumnName11 { get; set; }
    }

    public class ProductAModel : ProductModel
    {
        public string A { get; set; }
    }
    public class ProductBModel : ProductModel
    {
        public string B { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new ProductA { Name = "P1", A = "a", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new ProductB { Name = "P2", B = "b", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new Product { Name = "P3", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductA>();
            modelBuilder.Entity<ProductB>();
        }
    }
}
public class MapObjectPropertyFromSubQueryExplicitExpansionWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryExplicitExpansionWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.Price, o =>
            {
                o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault());
                o.ExplicitExpansion();
            })
            .Include<ProductA, ProductAModel>()
            .Include<ProductB, ProductBModel>();

        cfg.CreateMap<ProductA, ProductAModel>();
        cfg.CreateMap<ProductB, ProductBModel>();

        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_map_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(0);
            var list = projection.ToList();

            var productAModel = list[0].ShouldBeOfType<ProductAModel>();
            productAModel.Price.ShouldBeNull();
            productAModel.Name.ShouldBe("P1");
            productAModel.A.ShouldBe("a");

            var productBModel = list[1].ShouldBeOfType<ProductBModel>();
            productBModel.Price.ShouldBeNull();
            productBModel.Name.ShouldBe("P2");
            productBModel.B.ShouldBe("b");

            var productModel = list[2].ShouldBeOfType<ProductModel>();
            productModel.Price.ShouldBeNull();
            productModel.Name.ShouldBe("P3");
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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
    }

    public class ProductA : Product
    {
        public string A { get; set; }
    }
    public class ProductB : Product
    {
        public string B { get; set; }
    }

    public class PriceModel
    {
        public int Id { get; set; }
        public short RegionId { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ProductModel
    {
        public string Name { get; set; }
        public PriceModel Price { get; set; }
    }

    public class ProductAModel : ProductModel
    {
        public string A { get; set; }
    }
    public class ProductBModel : ProductModel
    {
        public string B { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new ProductA { ECommercePublished = true, Name = "P1", A = "a", Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new ProductB { ECommercePublished = true, Name = "P2", B = "b", Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new Product { ECommercePublished = true, Name = "P3", Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductA>();
            modelBuilder.Entity<ProductB>();
        }
    }
}
public class MapObjectPropertyFromSubQueryWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()))
            .Include<ProductA, ProductAModel>()
            .Include<ProductB, ProductBModel>();
        cfg.CreateMap<ProductA, ProductAModel>();
        cfg.CreateMap<ProductB, ProductBModel>();
        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductModel>(context.Products.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(12);
            var list = projection.ToList();

            var productAModel = list[0].ShouldBeOfType<ProductAModel>();
            productAModel.Price.RegionId.ShouldBe((short)1);
            productAModel.Price.IsDefault.ShouldBeTrue();
            productAModel.Name.ShouldBe("P1");
            productAModel.A.ShouldBe("a");

            var productBModel = list[1].ShouldBeOfType<ProductBModel>();
            productBModel.Price.RegionId.ShouldBe((short)1);
            productBModel.Price.IsDefault.ShouldBeTrue();
            productBModel.Name.ShouldBe("P2");
            productBModel.B.ShouldBe("b");

            var productModel = list[2].ShouldBeOfType<ProductModel>();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Name.ShouldBe("P3");
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
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
        [NotMapped]
        public int NotMappedValue { get; set; }
    }
    public partial class ProductA : Product
    {
        public string A { get; set; }
    }
    public partial class ProductB : Product
    {
        public string B { get; set; }
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
        public string Name { get; set; }
        public PriceModel Price { get; set; }
    }
    public class ProductAModel : ProductModel
    {
        public string A { get; set; }
    }
    public class ProductBModel : ProductModel
    {
        public string B { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.Products.Add(new ProductA { Name = "P1", A = "a", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new ProductB { Name = "P2", B = "b", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.Products.Add(new Product { Name = "P3", ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductA>();
            modelBuilder.Entity<ProductB>();
        }
    }
}
public class MapObjectPropertyFromSubQueryWithInnerObjectWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryWithInnerObjectWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ProductArticle, ProductArticleModel>()
            .Include<ProductArticleA, ProductArticleAModel>()
            .Include<ProductArticleB, ProductArticleBModel>();

        cfg.CreateMap<ProductArticleA, ProductArticleAModel>();
        cfg.CreateMap<ProductArticleB, ProductArticleBModel>();
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(24);
            var list = projection.ToList();

            var productArticleAModel = list[0].ShouldBeOfType<ProductArticleAModel>();
            productArticleAModel.Name.ShouldBe("P1");
            productArticleAModel.A.ShouldBe("a");
            var productModel = productArticleAModel.Product;
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            var otherProductModel = productArticleAModel.OtherProduct;
            otherProductModel.Price.RegionId.ShouldBe((short)2);
            otherProductModel.Price.IsDefault.ShouldBeTrue();

            var productArticleBModel = list[1].ShouldBeOfType<ProductArticleBModel>();
            productArticleBModel.Name.ShouldBe("P2");
            productArticleBModel.B.ShouldBe("b");
            productModel = productArticleBModel.Product;
            productModel.Price.RegionId.ShouldBe((short)3);
            productModel.Price.IsDefault.ShouldBeTrue();
            otherProductModel = productArticleBModel.OtherProduct;
            otherProductModel.Price.RegionId.ShouldBe((short)4);
            otherProductModel.Price.IsDefault.ShouldBeTrue();

            var productArticleModel = list[2].ShouldBeOfType<ProductArticleModel>();
            productArticleModel.Name.ShouldBe("P3");
            productModel = productArticleModel.Product;
            productModel.Price.RegionId.ShouldBe((short)5);
            productModel.Price.IsDefault.ShouldBeTrue();
            otherProductModel = productArticleModel.OtherProduct;
            otherProductModel.Price.RegionId.ShouldBe((short)6);
            otherProductModel.Price.IsDefault.ShouldBeTrue();
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public Product Product { get; set; }
        public Product OtherProduct { get; set; }
    }
    public class ProductArticleA : ProductArticle
    {
        public string A { get; set; }
    }
    public class ProductArticleB : ProductArticle
    {
        public string B { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductModel Product { get; set; }
        public ProductModel OtherProduct { get; set; }
    }
    public class ProductArticleAModel : ProductArticleModel
    {
        public string A { get; set; }
    }
    public class ProductArticleBModel : ProductArticleModel
    {
        public string B { get; set; }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product1 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            var product2 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 2, ProductId = 2 } } });
            context.ProductArticles.Add(new ProductArticleA { A = "a", Name = "P1", Product = product1.Entity, OtherProduct = product2.Entity });

            var product3 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 3, ProductId = 1 } } });
            var product4 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 4, ProductId = 2 } } });
            context.ProductArticles.Add(new ProductArticleB { B = "b", Name = "P2", Product = product3.Entity, OtherProduct = product4.Entity });

            var product5 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 5, ProductId = 1 } } });
            var product6 = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 6, ProductId = 2 } } });
            context.ProductArticles.Add(new ProductArticle { Name = "P3", Product = product5.Entity, OtherProduct = product6.Entity });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductArticleA>();
            modelBuilder.Entity<ProductArticleB>();
        }
    }
}
public class MapObjectPropertyFromSubQueryWithCollectionWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryWithCollectionWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ProductArticle, ProductArticleModel>()
            .Include<ProductArticleA, ProductArticleAModel>()
            .Include<ProductArticleB, ProductArticleBModel>();
        cfg.CreateMap<ProductArticleA, ProductArticleAModel>();
        cfg.CreateMap<ProductArticleB, ProductArticleBModel>();
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.Price, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()));
        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(12);
            var productModel = projection.First().Products.First();
            productModel.Price.RegionId.ShouldBe((short)1);
            productModel.Price.IsDefault.ShouldBeTrue();
            productModel.Price.Id.ShouldBe(1);
            productModel.Id.ShouldBe(1);
        }
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class ProductArticleA : ProductArticle
    {
        public string A { get; set; }
    }

    public class ProductArticleB : ProductArticle
    {
        public string B { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductModel> Products { get; set; }
    }

    public class ProductArticleAModel : ProductArticleModel
    {
        public string A { get; set; }
    }
    public class ProductArticleBModel : ProductArticleModel
    {
        public string B { get; set; }
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

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticleA { Name = "P1", A = "a", Products = new[] { product.Entity } });

            product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticleB { Name = "P2", B = "b", Products = new[] { product.Entity } });

            product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Name = "P3", Products = new[] { product.Entity } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductArticleA>();
            modelBuilder.Entity<ProductArticleB>();
        }
    }
}
public class MapObjectPropertyFromSubQueryWithCollectionSameNameWithInheritance : NonValidatingSpecBase, IAsyncLifetime
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<ProductArticle, ProductArticleModel>()
            .Include<ProductArticleA, ECommerceProductArticleModel>()
            .Include<ProductArticleB, ProductArticleBModel>();
        cfg.CreateMap<ProductArticleA, ECommerceProductArticleModel>()
            .ForMember(d => d.ECommerceProducts, o => o.MapFrom(source => source.Products.Where(p => p.ECommercePublished)));
        cfg.CreateMap<ProductArticleB, ProductArticleBModel>();
        cfg.CreateMap<Product, ProductModel>()
            .ForMember(d => d.ArticlesModel, o => o.MapFrom(s => s))
            .ForMember(d => d.Articles, o => o.MapFrom(source => source.Articles.Where(x => x.IsDefault && x.NationId == 1 && source.ECommercePublished).FirstOrDefault()))
            .Include<Product, ECommerceProductModel>();
        cfg.CreateMap<Product, ArticlesModel>();
        cfg.CreateMap<Article, PriceModel>()
            .ForMember(d => d.RegionId, o => o.MapFrom(s => s.NationId));

        cfg.CreateMap<Product, ECommerceProductModel>();
    });

    [Fact]
    public void Should_cache_the_subquery()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductArticleModel>(context.ProductArticles.OrderBy(p => p.Name));
            var counter = new FirstOrDefaultCounter();
            counter.Visit(projection.Expression);
            counter.Count.ShouldBe(16);
            var ecommerce = projection.ToList().OfType<ECommerceProductArticleModel>().First();
            ecommerce.ECommerceProducts.Count.ShouldBe(1);
            ecommerce.Products.Count.ShouldBe(2);

            var productModel = projection.First().Products.First();
            Check(productModel.Articles);
            productModel.Id.ShouldBe(1);
            productModel.ArticlesCount.ShouldBe(1);
            productModel.ArticlesModel.Articles.Count.ShouldBe(1);
            Check(productModel.ArticlesModel.Articles.Single());
        }
    }

    private static void Check(PriceModel priceModel)
    {
        priceModel.RegionId.ShouldBe((short)1);
        priceModel.IsDefault.ShouldBeTrue();
        priceModel.Id.ShouldBe(1);
    }

    class FirstOrDefaultCounter : ExpressionVisitor
    {
        public int Count;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "FirstOrDefault")
            {
                Count++;
            }
            return base.VisitMethodCall(node);
        }
    }

    public class ProductArticle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class ProductArticleA : ProductArticle
    {
        public string A { get; set; }
    }

    public class ProductArticleB : ProductArticle
    {
        public string B { get; set; }
    }

    public class ProductArticleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductModel> Products { get; set; }
    }

    public class ECommerceProductArticleModel : ProductArticleModel
    {
        public ICollection<ECommerceProductModel> ECommerceProducts { get; set; }
    }

    public class ProductArticleBModel : ProductArticleModel
    {
        public string B { get; set; }
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
        public PriceModel Articles { get; set; }
        public int ArticlesCount { get; set; }
        public ArticlesModel ArticlesModel { get; set; }
    }

    public class ECommerceProductModel : ProductModel
    {
    }

    public class ArticlesModel
    {
        public ICollection<PriceModel> Articles { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            var product2 = context.Products.Add(new Product { ECommercePublished = false, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticleA { Name = "P1", A = "a", Products = new[] { product.Entity, product2.Entity } });
            product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticleB { Name = "P2", B = "b", Products = new[] { product.Entity } });
            product = context.Products.Add(new Product { ECommercePublished = true, Articles = new[] { new Article { IsDefault = true, NationId = 1, ProductId = 1 } } });
            context.ProductArticles.Add(new ProductArticle { Name = "P3", Products = new[] { product.Entity } });
        }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductArticle> ProductArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductArticleA>();
            modelBuilder.Entity<ProductArticleB>();
        }
    }
    public async ValueTask InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
public class SubQueryWithMapFromNullableWithInheritance : IntegrationTest<SubQueryWithMapFromNullableWithInheritance.DatabaseInitializer>
{
    // Source Types
    public class Cable
    {
        public int CableId { get; set; }
        public string Name { get; set; }
        public ICollection<CableEnd> Ends { get; set; } = new List<CableEnd>();
    }
    public class CableA : Cable
    {
        public string A { get; set; }
    }
    public class CableB : Cable
    {
        public string B { get; set; }
    }

    public class CableEnd
    {
        [ForeignKey(nameof(CrossConnectId))]
        public virtual Cable CrossConnect { get; set; }
        [Column(Order = 0), Key]
        public int CrossConnectId { get; set; }
        [Column(Order = 1), Key]
        public string Name { get; set; }
        [ForeignKey(nameof(RackId))]
        public virtual Rack Rack { get; set; }
        public int? RackId { get; set; }
    }

    public class DataHall
    {
        public int DataHallId { get; set; }
        public int DataCentreId { get; set; }
        public ICollection<Rack> Racks { get; set; } = new List<Rack>();
    }

    public class Rack
    {
        public int RackId { get; set; }
        [ForeignKey(nameof(DataHallId))]
        public virtual DataHall DataHall { get; set; }
        public int DataHallId { get; set; }
    }

    // Dest Types
    public class CableListModel
    {
        public int CableId { get; set; }
        public CableEndModel AEnd { get; set; }
        public CableEndModel AnotherEnd { get; set; }
    }
    public class CableListModelA : CableListModel
    {
        public string A { get; set; }
    }
    public class CableListModelB : CableListModel
    {
        public string B { get; set; }
    }

    public class CableEndModel
    {
        public string Name { get; set; }
        public int? DataHallId { get; set; }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Cable> Cables { get; set; }
        public DbSet<CableEnd> CableEnds { get; set; }
        public DbSet<DataHall> DataHalls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CableEnd>().HasKey(c => new { c.CrossConnectId, c.Name });
            modelBuilder.Entity<CableA>();
            modelBuilder.Entity<CableB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            var rack = new Rack();
            var dh = new DataHall { DataCentreId = 10, Racks = { rack } };
            context.DataHalls.Add(dh);

            context.Cables.Add(new CableA
            {
                Name = "C1",
                A = "a",
                Ends = new List<CableEnd>()
                {
                    new CableEnd { Name = "A", Rack = rack},
                    new CableEnd { Name = "B" },
                }
            });
            context.Cables.Add(new CableB
            {
                Name = "C2",
                B = "b",
                Ends = new List<CableEnd>()
                {
                    new CableEnd { Name = "A", Rack = rack},
                    new CableEnd { Name = "B" },
                }
            });
            context.Cables.Add(new Cable
            {
                Name = "C3",
                Ends = new List<CableEnd>()
                {
                    new CableEnd { Name = "A", Rack = rack},
                    new CableEnd { Name = "B" },
                }
            });
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<CableEnd, CableEndModel>().ForMember(dest => dest.DataHallId, opt => opt.MapFrom(src => src.Rack.DataHall.DataCentreId));
        cfg.CreateMap<Cable, CableListModel>()
            .ForMember(dest => dest.AEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "A")))
            .ForMember(dest => dest.AnotherEnd, opt => opt.MapFrom(src => src.Ends.FirstOrDefault(x => x.Name == "B")))
            .Include<CableA, CableListModelA>()
            .Include<CableB, CableListModelB>();
        cfg.CreateMap<CableA, CableListModelA>();
        cfg.CreateMap<CableB, CableListModelB>();
    });

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<CableListModel>(context.Cables.OrderBy(c => c.Name));
            var list = projection.ToList();

            var resultA = list[0].ShouldBeOfType<CableListModelA>();
            resultA.AEnd.DataHallId.ShouldBe(10);
            resultA.AnotherEnd.DataHallId.ShouldBeNull();
            resultA.A.ShouldBe("a");

            var resultB = list[1].ShouldBeOfType<CableListModelB>();
            resultB.AEnd.DataHallId.ShouldBe(10);
            resultB.AnotherEnd.DataHallId.ShouldBeNull();
            resultB.B.ShouldBe("b");

            var result = list[2].ShouldBeOfType<CableListModel>();
            result.AEnd.DataHallId.ShouldBe(10);
            result.AnotherEnd.DataHallId.ShouldBeNull();
        }
    }
}
public class MapObjectPropertyFromSubQueryCustomSourceWithInheritance : IntegrationTest<MapObjectPropertyFromSubQueryCustomSourceWithInheritance.DatabaseInitializer>
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Owner, OwnerDto>();
        cfg.CreateMap<Brand, BrandDto>()
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owners.FirstOrDefault()));
        cfg.CreateMap<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Product.Brand))
            .Include<ProductReviewA, ProductReviewADto>()
            .Include<ProductReviewB, ProductReviewBDto>();
        cfg.CreateMap<ProductReviewA, ProductReviewADto>();
        cfg.CreateMap<ProductReviewB, ProductReviewBDto>();
    });

    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Brand
    {
        public int Id { get; set; }
        public List<Owner> Owners { get; set; } = new List<Owner>();
    }
    public class Product
    {
        public int Id { get; set; }
        public Brand Brand { get; set; }
    }
    public class ProductReview
    {
        public int Id { get; set; }
        public Product Product { get; set; }
    }
    public class ProductReviewA : ProductReview
    {
        public string A { get; set; }
    }
    public class ProductReviewB : ProductReview
    {
        public string B { get; set; }
    }
    /* Destination types */
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public BrandDto Brand { get; set; }
    }
    public class ProductReviewADto : ProductReviewDto
    {
        public string A { get; set; }
    }
    public class ProductReviewBDto : ProductReviewDto
    {
        public string B { get; set; }
    }
    public class BrandDto
    {
        public int Id { get; set; }
        public OwnerDto Owner { get; set; }
    }
    public class OwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ClientContext : LocalDbContext
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductReviewA>();
            modelBuilder.Entity<ProductReviewB>();
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<ClientContext>
    {
        protected override void Seed(ClientContext context)
        {
            context.ProductReviews.Add(new ProductReviewA
            { A = "a", Product = new Product { Brand = new Brand { Owners = { new Owner { Name = "Owner" } } } } });
            context.ProductReviews.Add(new ProductReviewB
            { B = "b", Product = new Product { Brand = new Brand { Owners = { new Owner() } } } });
            context.ProductReviews.Add(new ProductReview { Product = new Product() });
        }
    }

    [Fact]
    public void Should_project_ok()
    {
        using (var context = new ClientContext())
        {
            var projection = ProjectTo<ProductReviewDto>(context.ProductReviews);
            var results = projection.ToArray();
            results.Any(result => result?.Brand?.Owner?.Name == "Owner").ShouldBeTrue();
            results.Any(result => result?.Brand?.Owner == null).ShouldBeTrue();
            results.Any(result => result?.Brand == null).ShouldBeTrue();
            results.OfType<ProductReviewADto>().Any(result => result.A == "a").ShouldBeTrue();
            results.OfType<ProductReviewBDto>().Any(result => result.B == "b").ShouldBeTrue();
        }
    }
}