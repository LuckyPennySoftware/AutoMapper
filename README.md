![AutoMapper](https://s3.amazonaws.com/automapper/logo.png)

[![CI](https://github.com/luckypennysoftware/automapper/workflows/CI/badge.svg)](https://github.com/luckypennysoftware/AutoMapper/actions?query=workflow%3ACI)
[![NuGet](http://img.shields.io/nuget/vpre/AutoMapper.svg?label=NuGet)](https://www.nuget.org/packages/AutoMapper/)
[![Documentation Status](https://readthedocs.org/projects/automapper/badge/?version=stable)](https://docs.automapper.io/en/stable/?badge=stable)


### What is AutoMapper?

AutoMapper is a simple little library built to solve a deceptively complex problem - getting rid of code that mapped one object to another. This type of code is rather dreary and boring to write, so why not invent a tool to do it for us?

This is the main repository for AutoMapper, but there's more:

* [Collection Extensions](https://github.com/AutoMapper/AutoMapper.Collection)
* [Expression Mapping](https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping)
* [EF6 Extensions](https://github.com/AutoMapper/AutoMapper.EF6)
* [IDataReader/Record Extensions](https://github.com/AutoMapper/AutoMapper.Data)
* [Enum Extensions](https://github.com/AutoMapper/AutoMapper.Extensions.EnumMapping)

### How do I get started?

First, configure AutoMapper to know what types you want to map, in the startup of your application:

```csharp
var configuration = new MapperConfiguration(cfg => 
{
    cfg.CreateMap<Foo, FooDto>();
    cfg.CreateMap<Bar, BarDto>();
}, loggerFactory);

// or more typically, using IServiceCollection with automatic service registration
services.AddAutoMapper(cfg => 
{
    cfg.CreateMap<Foo, FooDto>();
    cfg.CreateMap<Bar, BarDto>();
});
// AutoMapper will automatically register any implementations of:
// - IValueResolver<,>
// - IMemberValueResolver<,,>
// - ITypeConverter<,>
// - IValueConverter<,>
// - ICondition<,,>
// - IPreCondition<,>
// - IMappingAction<,>
// from the provided assemblies

// only during development, validate your mappings; remove it before release
#if DEBUG
configuration.AssertConfigurationIsValid();
#endif
// use DI (http://docs.automapper.io/en/latest/Dependency-injection.html) or create the mapper yourself
var mapper = configuration.CreateMapper();
```
Then in your application code, execute the mappings:

```csharp
var fooDto = mapper.Map<FooDto>(foo);
var barDto = mapper.Map<BarDto>(bar);
```

Check out the [getting started guide](https://automapper.readthedocs.io/en/latest/Getting-started.html). When you're done there, the [wiki](https://automapper.readthedocs.io/en/latest/) goes in to the nitty-gritty details. If you have questions, you can post them to [Stack Overflow](https://stackoverflow.com/questions/tagged/automapper).

### Where can I get it?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [AutoMapper](https://www.nuget.org/packages/AutoMapper/) from the package manager console:

```
PM> Install-Package AutoMapper
```
Or from the .NET CLI as:
```
dotnet add package AutoMapper
```

### Do you have an issue?

You might want to know exactly what [your mapping does](https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html) at runtime.

If you're still running into problems, file an issue above.

If you are a paying customer, you can contact support via your account.

### How do I set the license key?

You can set the license key when registering AutoMapper:

```csharp
services.AddAutoMapper(cfg => 
{
    cfg.LicenseKey = "<license key here>";
})
```

You can register for your license key at [AutoMapper.io](https://automapper.io)