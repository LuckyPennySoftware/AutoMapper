using System;

namespace AutoMapper.Extensions.Microsoft.DependencyInjection.Tests
{
    public class Source
    {
        
    }

    public class Dest
    {
        
    }

    public class Source2
    {
        public int ConvertedValue { get; set; }
    }

    public class Dest2
    {
        public int ResolvedValue { get; set; }
        public int ConvertedValue { get; set; }
    }

    public class Source3
    {
        public int Value { get; set; }
    }

    [AutoMap(typeof(Source3))]
    public class Dest3
    {
        public int Value { get; set; }
    }

    public class Profile1 : Profile
    {
        public Profile1()
        {
            CreateMap<Source, Dest>();
        }
    }

    public abstract class AbstractProfile : Profile { }

    internal class Profile2 : Profile
    {
        public Profile2()
        {
            CreateMap<Source2, Dest2>()
                .ForMember(d => d.ResolvedValue, opt => opt.MapFrom<DependencyResolver>())
                .ForMember(d => d.ConvertedValue, opt => opt.ConvertUsing<DependencyValueConverter, int>());
            CreateMap(typeof(Enum), typeof(EnumDescriptor<>)).ConvertUsing(typeof(EnumDescriptorTypeConverter<>));
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

    public class MutableService : ISomeService
    {
        public int Value { get; set; }

        public int Modify(int value) => value + Value;
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

    internal class FooMappingAction : IMappingAction<object, object>
    {
        public void Process(object source, object destination, ResolutionContext context) { }
    }

    internal class FooValueResolver: IValueResolver<object, object, object>
    {
        public object Resolve(object source, object destination, object destMember, ResolutionContext context)
        {
            return null;
        }
    }

    internal class FooMemberValueResolver : IMemberValueResolver<object, object, object, object>
    {
        public object Resolve(object source, object destination, object sourceMember, object destMember, ResolutionContext context)
        {
            return null;
        }
    }

    internal class FooTypeConverter : ITypeConverter<object, object>
    {
        public object Convert(object source, object destination, ResolutionContext context)
        {
            return null;
        }
    }

    public class EnumDescriptor<TSource> where TSource : Enum
    {
        public int Value { get; set; }
    }

    public class EnumDescriptorTypeConverter<TSource> : ITypeConverter<Enum, EnumDescriptor<TSource>>
        where TSource : Enum
    {
        public EnumDescriptor<TSource> Convert(Enum source, EnumDescriptor<TSource> destination, ResolutionContext context) => 
            new EnumDescriptor<TSource>{ Value = int.MaxValue };
    }

    internal class FooValueConverter : IValueConverter<int, int>
    {
        public int Convert(int sourceMember, ResolutionContext context)
            => sourceMember + 1;
    }

    internal class DependencyValueConverter : IValueConverter<int, int>
    {
        private readonly ISomeService _service;

        public DependencyValueConverter(ISomeService service) => _service = service;

        public int Convert(int sourceMember, ResolutionContext context)
            => _service.Modify(sourceMember);
    }

    public class ConditionSource
    {
        public int Value { get; set; }
    }

    public class ConditionDest
    {
        public int Value { get; set; }
    }

    public class DependencyCondition : ICondition<ConditionSource, ConditionDest, int>
    {
        private readonly ISomeService _service;

        public DependencyCondition(ISomeService service) => _service = service;

        public bool Evaluate(ConditionSource source, ConditionDest destination, int sourceMember, int destMember, ResolutionContext context)
            => _service.Modify(sourceMember) > 0;
    }

    internal class ConditionProfile : Profile
    {
        public ConditionProfile()
        {
            CreateMap<ConditionSource, ConditionDest>()
                .ForMember(d => d.Value, o =>
                {
                    o.Condition<DependencyCondition>();
                    o.MapFrom(s => s.Value);
                });
        }
    }
}