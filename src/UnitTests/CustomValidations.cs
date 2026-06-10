using AutoMapper.Configuration;
using AutoMapper.Internal.Mappers;

namespace AutoMapper.UnitTests;

public class CustomValidations
{
    public class Source
    {
    }

    public class Destination
    {
    }

    public class When_using_custom_validation
    {
        bool _calledForRoot = false;
        bool _calledForValues = false;
        bool _calledForInt = false;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Dest
        {
            public int[] Values { get; set; }
        }

        [Fact]
        public void Should_call_the_validator()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.Internal().Validator(Validator);
                cfg.CreateMap<Source, Dest>();
            });

            new Action(config.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>().Message.ShouldBe(nameof(When_using_custom_validation));

            _calledForRoot.ShouldBeTrue();
            _calledForValues.ShouldBeTrue();
            _calledForInt.ShouldBeTrue();
        }

        private void Validator(ValidationContext context)
        {
            if (context.TypeMap != null)
            {
                _calledForRoot = true;
                context.TypeMap.Types.ShouldBe(context.Types);
                context.Types.SourceType.ShouldBe(typeof(Source));
                context.Types.DestinationType.ShouldBe(typeof(Dest));
                context.ObjectMapper.ShouldBeNull();
                context.MemberMap.ShouldBeNull();
                context.Exceptions.Add(new AutoMapperConfigurationException(nameof(When_using_custom_validation)));
            }
            else
            {
                context.MemberMap.SourceMember.Name.ShouldBe("Values");
                context.MemberMap.DestinationName.ShouldBe("Values");
                if (context.Types.Equals(new TypePair(typeof(int), typeof(int))))
                {
                    _calledForInt = true;
                    context.ObjectMapper.ShouldBeOfType<AssignableMapper>();
                }
                else
                {
                    _calledForValues = true;
                    context.ObjectMapper.ShouldBeOfType<CollectionMapper>();
                    context.Types.SourceType.ShouldBe(typeof(int[]));
                    context.Types.DestinationType.ShouldBe(typeof(int[]));
                }
            }
        }
    }

    public class When_using_custom_validation_for_convertusing_with_mappingfunction : NonValidatingSpecBase
    {
        // Nullable so can see a false state
        private static bool? _validated;

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            Func<Source, Destination, Destination> mappingFunction = (source, destination) => new Destination();
            cfg.CreateMap<Source, Destination>().ConvertUsing(mappingFunction);
            cfg.Internal().Validator(SetValidated);
        });

        private static void SetValidated(ValidationContext context)
        {
            if (context.TypeMap.SourceType == typeof(Source) &&
                context.TypeMap.DestinationType == typeof(Destination))
            {
                _validated = true;
            }
        }

        [Fact]
        public void Validator_should_be_called_by_AssertConfigurationIsValid()
        {
            _validated.ShouldBeNull();

            AssertConfigurationIsValid();

            _validated.ShouldBe(true);
        }
    }

    public class When_using_custom_validation_for_convertusing_with_typeconvertertype : NonValidatingSpecBase
    {
        // Nullable so can see a false state
        private static bool? _validated;

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ConvertUsing<CustomTypeConverter>();
            cfg.Internal().Validator(SetValidated);
        });

        private static void SetValidated(ValidationContext context)
        {
            if (context.TypeMap.SourceType == typeof(Source) &&
                context.TypeMap.DestinationType == typeof(Destination))
            {
                _validated = true;
            }
        }

        [Fact]
        public void Validator_should_be_called_by_AssertConfigurationIsValid()
        {
            _validated.ShouldBeNull();

            AssertConfigurationIsValid();

            _validated.ShouldBe(true);
        }

        internal class CustomTypeConverter : ITypeConverter<Source, Destination>
        {
            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination();
            }
        }
    }

    public class When_using_custom_validation_for_convertusing_with_typeconverter_instance : NonValidatingSpecBase
    {
        // Nullable so can see a false state
        private static bool? _validated;

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ConvertUsing(new CustomTypeConverter());
            cfg.Internal().Validator(SetValidated);
        });

        private static void SetValidated(ValidationContext context)
        {
            if (context.TypeMap.SourceType == typeof(Source) &&
                context.TypeMap.DestinationType == typeof(Destination))
            {
                _validated = true;
            }
        }

        [Fact]
        public void Validator_should_be_called_by_AssertConfigurationIsValid()
        {
            _validated.ShouldBeNull();

            AssertConfigurationIsValid();

            _validated.ShouldBe(true);
        }

        internal class CustomTypeConverter : ITypeConverter<Source, Destination>
        {
            public Destination Convert(Source source, Destination destination, ResolutionContext context)
            {
                return new Destination();
            }
        }
    }

    public class When_using_custom_validation_for_convertusing_with_mappingexpression : NonValidatingSpecBase
    {
        // Nullable so can see a false state
        private static bool? _validated;

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            Expression<Func<Source, Destination>> mappingExpression = source => new Destination();
            cfg.CreateMap<Source, Destination>().ConvertUsing(mappingExpression);
            cfg.Internal().Validator(SetValidated);
        });

        private static void SetValidated(ValidationContext context)
        {
            if (context.TypeMap.SourceType == typeof(Source) &&
                context.TypeMap.DestinationType == typeof(Destination))
            {
                _validated = true;
            }
        }

        [Fact]
        public void Validator_should_be_called_by_AssertConfigurationIsValid()
        {
            _validated.ShouldBeNull();
            AssertConfigurationIsValid();
            _validated.ShouldBe(true);
        }
    }
}