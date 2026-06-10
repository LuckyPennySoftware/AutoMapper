# Setup

```c#
var config = new MapperConfiguration(cfg => {
    cfg.AddProfile<AppProfile>();
    cfg.CreateMap<Source, Dest>();
}, loggerFactory);

var mapper = config.CreateMapper();
// or
IMapper mapper = new Mapper(config);
var dest = mapper.Map<Source, Dest>(new Source());
```
Starting with 9.0, the static API is no longer available.

## Gathering configuration before initialization

AutoMapper also lets you gather configuration before initialization:

```c#
var cfg = new MapperConfigurationExpression();
cfg.CreateMap<Source, Dest>();
cfg.AddProfile<MyProfile>();
MyBootstrapper.InitAutoMapper(cfg);

var mapperConfig = new MapperConfiguration(cfg, loggerFactory);
IMapper mapper = new Mapper(mapperConfig);

```

## LINQ projections

For the instance API, you can use IMapper.ProjectTo. If you prefer to keep using the IQueryable extension methods, you have to pass in the MapperConfiguration instance:

```c#
public class ProductsController : Controller {
    public ProductsController(MapperConfiguration config) {
        this.config = config;
    }
    private MapperConfiguration config;

    public ActionResult Index(int id) {
        var dto = dbContext.Products
                               .Where(p => p.Id == id)
                               .ProjectTo<ProductDto>(config)
                               .SingleOrDefault();

        return View(dto);
    }    
}
```

## Unsupported operations

One "feature" of AutoMapper allowed you to modify configuration at runtime. That caused many problems, so the new API does not allow you to do this. You'll need to move all your `Mapper.CreateMap` calls into a profile.

Dynamic mapping, such as `Mapper.DynamicMap`, is no longer possible.