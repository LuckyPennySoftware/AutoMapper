using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
	public class ServiceLifetimeTests
	{
		//Implicitly Transient
		[Fact]
		public void AddAutoMapperExtensionDefaultWithAssemblySingleDelegateArgCollection()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
            serviceCollection.AddAutoMapper(_ =>
            {
            });
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
		}

		[Fact]
		public void AddAutoMapperExtensionDefaultWithServiceLifetime()
		{
			//arrange
			var serviceCollection = new ServiceCollection();

			//act
            serviceCollection.AddAutoMapper(_ =>
            {
            }, new List<Assembly>(), ServiceLifetime.Singleton);
			var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IMapper));

			//assert
			serviceDescriptor.ShouldNotBeNull();
			serviceDescriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
		}
	}
}