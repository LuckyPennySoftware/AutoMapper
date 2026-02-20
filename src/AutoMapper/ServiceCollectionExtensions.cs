using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
/// Extensions to scan for AutoMapper classes and register the configuration, mapping, and extensions with the service collection:
/// <list type="bullet">
/// <item> Finds <see cref="Profile"/> classes and initializes a new <see cref="MapperConfiguration" />,</item> 
/// <item> Scans for <see cref="ITypeConverter{TSource,TDestination}"/>, <see cref="IValueResolver{TSource,TDestination,TDestMember}"/>, <see cref="IMemberValueResolver{TSource,TDestination,TSourceMember,TDestMember}" />, <see cref="ICondition{TSource,TDestination,TMember}"/>, <see cref="IPreCondition{TSource,TDestination}"/>, <see cref="IValueConverter{TSourceMember,TDestinationMember}"/> and <see cref="IMappingAction{TSource,TDestination}"/> implementations and registers them as <see cref="ServiceLifetime.Transient"/>, </item>
/// <item> Registers <see cref="IConfigurationProvider"/> as <see cref="ServiceLifetime.Singleton"/>, and</item>
/// <item> Registers <see cref="IMapper"/> as a configurable <see cref="ServiceLifetime"/> (default is <see cref="ServiceLifetime.Transient"/>)</item>
/// </list>
/// After calling AddAutoMapper you can resolve an <see cref="IMapper" /> instance from a scoped service provider, or as a dependency
/// To use <see cref="AutoMapper.QueryableExtensions.Extensions.ProjectTo{TDestination}(IQueryable,IConfigurationProvider, System.Linq.Expressions.Expression{System.Func{TDestination, object}}[])" /> you can resolve the <see cref="IConfigurationProvider"/> instance directly for from an <see cref="IMapper" /> instance.
/// </summary>
public static class ServiceCollectionExtensions
{
    static readonly Type[] AmTypes = [typeof(IValueResolver<,,>), typeof(IMemberValueResolver<,,,>), typeof(ITypeConverter<,>), typeof(IValueConverter<,>), typeof(ICondition<,,>), typeof(IPreCondition<,>), typeof(IMappingAction<,>)];
    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction)
        => AddAutoMapperClasses(services, (sp, cfg) => configAction?.Invoke(cfg), null);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction, params Assembly[] assemblies)
        => AddAutoMapperClasses(services, (sp, cfg) => configAction?.Invoke(cfg), assemblies);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configAction, params Assembly[] assemblies)
        => AddAutoMapperClasses(services, configAction, assemblies);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddAutoMapperClasses(services, (sp, cfg) => configAction?.Invoke(cfg), assemblies, serviceLifetime);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configAction, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddAutoMapperClasses(services, configAction, assemblies, serviceLifetime);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction, params Type[] profileAssemblyMarkerTypes)
        => AddAutoMapperClasses(services, (sp, cfg) => configAction?.Invoke(cfg), profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configAction, params Type[] profileAssemblyMarkerTypes)
        => AddAutoMapperClasses(services, configAction, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configAction,
        IEnumerable<Type> profileAssemblyMarkerTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddAutoMapperClasses(services, (sp, cfg) => configAction?.Invoke(cfg), profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), serviceLifetime);

    public static IServiceCollection AddAutoMapper(this IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configAction,
        IEnumerable<Type> profileAssemblyMarkerTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        => AddAutoMapperClasses(services, configAction, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), serviceLifetime);

    private static IServiceCollection AddAutoMapperClasses(IServiceCollection services, Action<IServiceProvider, IMapperConfigurationExpression> configAction,
        IEnumerable<Assembly> assembliesToScan, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (configAction != null)
        {
            services.AddOptions<MapperConfigurationExpression>().Configure<IServiceProvider>((options, sp) => configAction(sp, options));
        }
        if (assembliesToScan != null)
        {
            assembliesToScan = assembliesToScan.Where(a => !a.IsDynamic && a != typeof(Mapper).Assembly).Distinct();
            services.Configure<MapperConfigurationExpression>(options => options.AddMaps(assembliesToScan));
            foreach (var type in assembliesToScan.SelectMany(a => a.GetTypes().Where(type => type.IsClass && !type.IsAbstract && IsAmType(type))))
            {
                // use try add to avoid double-registration
                services.TryAddTransient(type);
            }
        }
        // Just return if we've already added AutoMapper to avoid double-registration
        if (services.Any(sd => sd.ServiceType == typeof(IMapper)))
        {
            return services;
        }
        services.AddSingleton<IConfigurationProvider>(sp =>
        {
            // A mapper configuration is required
            var options = sp.GetRequiredService<IOptions<MapperConfigurationExpression>>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new MapperConfiguration(options.Value, loggerFactory);
        });
        services.Add(new(typeof(IMapper), sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService), serviceLifetime));
        return services;
        bool IsAmType(Type type) => Array.Exists(AmTypes, openType => type.GetGenericInterface(openType) != null);
    }
}