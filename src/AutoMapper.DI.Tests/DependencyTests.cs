using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using global::Microsoft.Extensions.DependencyInjection;
    using Shouldly;
    using Xunit;

    public class DependencyTests
    {
        private readonly IServiceProvider _provider;

        public DependencyTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ISomeService>(sp => new FooService(5));
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, typeof(Source), typeof(Profile));
            _provider = services.BuildServiceProvider();

            _provider.GetService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public void ShouldResolveWithDependency()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<Source2, Dest2>(new Source2());

            dest.ResolvedValue.ShouldBe(5);
        }

        [Fact]
        public void ShouldConvertWithDependency()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<Source2, Dest2>(new Source2 { ConvertedValue = 5});

            dest.ConvertedValue.ShouldBe(10);
        }
    }

    public class ConditionDependencyTests
    {
        private readonly IServiceProvider _provider;

        public ConditionDependencyTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ISomeService>(sp => new FooService(5));
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, typeof(ConditionSource));
            _provider = services.BuildServiceProvider();

            _provider.GetService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public void ShouldApplyConditionWithDependency_WhenConditionPasses()
        {
            var mapper = _provider.GetService<IMapper>();
            // FooService.Modify(10) = 10 + 5 = 15 > 0, condition passes
            var dest = mapper.Map<ConditionSource, ConditionDest>(new ConditionSource { Value = 10 });

            dest.Value.ShouldBe(10);
        }

        [Fact]
        public void ShouldSkipMappingWithDependency_WhenConditionFails()
        {
            var mapper = _provider.GetService<IMapper>();
            // FooService.Modify(-10) = -10 + 5 = -5, which is not > 0, condition fails
            var dest = mapper.Map<ConditionSource, ConditionDest>(new ConditionSource { Value = -10 });

            dest.Value.ShouldBe(0);
        }
    }

    public class DestinationFactoryDependencyTests
    {
        private readonly IServiceProvider _provider;

        public DestinationFactoryDependencyTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ISomeService>(sp => new FooService(5));
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, typeof(FactorySource));
            _provider = services.BuildServiceProvider();

            _provider.GetService<IConfigurationProvider>().AssertConfigurationIsValid();
        }

        [Fact]
        public void ShouldConstructWithDependency()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<FactorySource, FactoryDest>(new FactorySource { Value = 10 });

            // FooService.Modify(10) = 10 + 5 = 15
            dest.InitialValue.ShouldBe(15);
            dest.Value.ShouldBe(10);
        }

        [Fact]
        public void ShouldConstructWithDependency_DifferentValue()
        {
            var mapper = _provider.GetService<IMapper>();
            var dest = mapper.Map<FactorySource, FactoryDest>(new FactorySource { Value = 20 });

            // FooService.Modify(20) = 20 + 5 = 25
            dest.InitialValue.ShouldBe(25);
            dest.Value.ShouldBe(20);
        }
    }
}
