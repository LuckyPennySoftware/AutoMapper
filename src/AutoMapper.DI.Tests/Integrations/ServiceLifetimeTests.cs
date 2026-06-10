using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests.Integrations
{
	public class ServiceLifetimeTests
	{
		internal interface ISingletonService
		{
			Bar DoTheThing(Foo theObj);
		}

		internal class TestSingletonService : ISingletonService
		{
			private readonly IMapper _mapper;

			public TestSingletonService(IMapper mapper)
			{
				_mapper = mapper;
			}

			public Bar DoTheThing(Foo theObj)
			{
				var bar = _mapper.Map<Bar>(theObj);
				return bar;
			}
		}

		internal class Foo
		{
			public int TheValue { get; set; }
		}

		internal class Bar
		{
			public int TheValue { get; set; }
		}


		[Fact]
		public void CanUseDefaultInjectedIMapperInSingletonService()
		{
			//arrange
			var services = new ServiceCollection();
			services.TryAddSingleton<ISingletonService, TestSingletonService>();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(opt =>
            {
                opt.CreateMap<Foo, Bar>().ReverseMap();
            }, GetType().Assembly);
			var sp = services.BuildServiceProvider();
			Bar actual;

			//act
			using (var scope = sp.CreateScope())
			{
				var service = scope.ServiceProvider.GetService<ISingletonService>();
				actual = service.DoTheThing(new Foo{TheValue = 1});
			}

			//assert
			actual.ShouldNotBeNull();
			actual.TheValue.ShouldBe(1);
		}
	}
}