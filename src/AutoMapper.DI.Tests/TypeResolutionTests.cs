using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    using System;
    using AutoMapper.Internal;
    using Shouldly;
    using Xunit;

    public class TypeResolutionTests
    {
        private readonly IServiceProvider _provider;

        public TypeResolutionTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(_ =>
            {
            }, typeof(Source));

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveConfiguration()
        {
            _provider.GetService<IConfigurationProvider>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldConfigureProfiles()
        {
            _provider.GetService<IConfigurationProvider>().Internal().GetAllTypeMaps().Count.ShouldBe(6);
        }

        [Fact]
        public void ShouldResolveMapper()
        {
            _provider.GetService<IMapper>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveMappingAction()
        {
            _provider.GetService<FooMappingAction>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveValueResolver()
        {
            _provider.GetService<FooValueResolver>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveMemberValueResolver()
        {
            _provider.GetService<FooMemberValueResolver>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveTypeConverter()
        {
            _provider.GetService<FooTypeConverter>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveGenericTypeConverter()
        {
            _provider.GetService<EnumDescriptorTypeConverter<ConsoleColor>>().ShouldNotBeNull();
            _provider.GetService<IMapper>().Map<EnumDescriptor<ConsoleColor>>(ConsoleColor.Green).Value.ShouldBe(int.MaxValue);
        }

        [Fact]
        public void ShouldResolveValueConverter()
        {
            _provider.GetService<FooValueConverter>().ShouldNotBeNull();
        }
    }
}