using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    public class ScopeTests
    {
        [Fact]
        public void Can_depend_on_scoped_services_as_transient_default()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, typeof(Source).Assembly);
            services.AddScoped<ISomeService, MutableService>();

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                var mutableService = (MutableService)scope.ServiceProvider.GetService<ISomeService>();
                mutableService.Value = 10;

                var mapper = scope.ServiceProvider.GetService<IMapper>();

                var dest = mapper.Map<Dest2>(new Source2 {ConvertedValue = 5});

                dest.ConvertedValue.ShouldBe(15);
            }
        }

        [Fact]
        public void Can_depend_on_scoped_services_as_scoped()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ =>
            {
            }, [typeof(Source).Assembly], ServiceLifetime.Scoped);
            services.AddScoped<ISomeService, MutableService>();

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                var mutableService = (MutableService)scope.ServiceProvider.GetService<ISomeService>();
                mutableService.Value = 10;

                var mapper = scope.ServiceProvider.GetService<IMapper>();

                var dest = mapper.Map<Dest2>(new Source2 {ConvertedValue = 5});

                dest.ConvertedValue.ShouldBe(15);
            }
        }

        [Fact]
        public void Cannot_correctly_resolve_scoped_services_as_singleton()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, [typeof(Source).Assembly], ServiceLifetime.Singleton);
            services.AddScoped<ISomeService, MutableService>();

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                var mutableService = (MutableService)scope.ServiceProvider.GetService<ISomeService>();
                mutableService.Value = 10;

                var mapper = scope.ServiceProvider.GetService<IMapper>();

                var dest = mapper.Map<Dest2>(new Source2 {ConvertedValue = 5});

                dest.ConvertedValue.ShouldBe(5);
            }
        }
    }
}