using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using System.Reflection;
    using AutoMapper.Internal;
    using Shouldly;
    using Xunit;

    public class AssemblyResolutionTests
    {
        private static readonly IServiceProvider _provider;

        static AssemblyResolutionTests()
        {
            _provider = BuildServiceProvider();    
        }

        private static ServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ => { }, typeof(Source).GetTypeInfo().Assembly);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        [Fact]
        public void ShouldResolveConfiguration()
        {
            _provider.GetService<IConfigurationProvider>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldConfigureProfiles()
        {
            _provider.GetService<IConfigurationProvider>().Internal().GetAllTypeMaps().Count.ShouldBe(5);
        }

        [Fact]
        public void ShouldResolveMapper()
        {
            _provider.GetService<IMapper>().ShouldNotBeNull();
        }

        [Fact]
        public void CanRegisterTwiceWithoutProblems()
        {
            new Action(() => BuildServiceProvider()).ShouldNotThrow();
        }
    }
}