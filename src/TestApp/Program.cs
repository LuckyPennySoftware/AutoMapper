using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();
services.AddTransient<ISomeService>(sp => new FooService(5));
services.AddAutoMapper(opt => opt.AddMaps(typeof(Source)));
var provider = services.BuildServiceProvider();
using (var scope = provider.CreateScope())
{
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

    foreach (var typeMap in mapper.ConfigurationProvider.Internal().GetAllTypeMaps())
    {
        Console.WriteLine($"{typeMap.SourceType.Name} -> {typeMap.DestinationType.Name}");
    }

    foreach (var service in services)
    {
        Console.WriteLine(service.ServiceType + " - " + service.ImplementationType);
    }

    var dest = mapper.Map<Dest2>(new Source2());
    Console.WriteLine(dest!.ResolvedValue);
}

Console.ReadKey();

public class Source
{
}

public class Dest
{
}

public class Source2
{
}

public class Dest2
{
    public int ResolvedValue { get; set; }
}

public class Profile1 : Profile
{
    public Profile1()
    {
        CreateMap<Source, Dest>();
    }
}

public class Profile2 : Profile
{
    public Profile2()
    {
        CreateMap<Source2, Dest2>()
            .ForMember(d => d.ResolvedValue, opt => opt.MapFrom<DependencyResolver>());
    }
}

public class DependencyResolver : IValueResolver<object, object, int>
{
    private readonly ISomeService _service;

    public DependencyResolver(ISomeService service)
    {
        _service = service;
    }

    public int Resolve(object source, object destination, int destMember, ResolutionContext context)
    {
        return _service.Modify(destMember);
    }
}

public interface ISomeService
{
    int Modify(int value);
}

public class FooService : ISomeService
{
    private readonly int _value;

    public FooService(int value)
    {
        _value = value;
    }

    public int Modify(int value) => value + _value;
}

