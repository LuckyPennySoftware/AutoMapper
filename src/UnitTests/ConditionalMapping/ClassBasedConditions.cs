namespace AutoMapper.UnitTests.ConditionalMapping;

public class ClassBasedConditionTests
{
    // Test classes
    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class PersonDto
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    // Condition implementation - single interface with all parameters
    public class AgeCondition : ICondition<Person, PersonDto, int>
    {
        public bool Evaluate(Person source, PersonDto destination, int sourceMember, int destMember, ResolutionContext context)
        {
            return sourceMember >= 18;
        }
    }

    public class NameCondition : ICondition<Person, PersonDto, string>
    {
        public bool Evaluate(Person source, PersonDto destination, string sourceMember, string destMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(sourceMember);
        }
    }

    // PreCondition implementation - single interface with all parameters
    public class SourcePreCondition : IPreCondition<Person, PersonDto>
    {
        public bool Evaluate(Person source, PersonDto destination, ResolutionContext context)
        {
            return source != null;
        }
    }

    [Fact]
    public void When_using_member_condition_class()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Age, o =>
                {
                    o.Condition<AgeCondition>();
                    o.MapFrom(s => s.Age);
                });
        });

        var mapper = config.CreateMapper();

        // Test with valid age
        var person1 = new Person { Age = 25, Name = "John" };
        var result1 = mapper.Map<PersonDto>(person1);
        result1.Age.ShouldBe(25);

        // Test with invalid age
        var person2 = new Person { Age = 10, Name = "Jane" };
        var result2 = mapper.Map<PersonDto>(person2);
        result2.Age.ShouldBe(0); // Should not be mapped
    }

    [Fact]
    public void When_using_name_condition_class()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Name, o =>
                {
                    o.Condition<NameCondition>();
                    o.MapFrom(s => s.Name);
                });
        });

        var mapper = config.CreateMapper();

        // Test with valid name
        var person1 = new Person { Name = "John", Age = 25 };
        var result1 = mapper.Map<PersonDto>(person1);
        result1.Name.ShouldBe("John");

        // Test with empty name
        var person2 = new Person { Name = "", Age = 30 };
        var result2 = mapper.Map<PersonDto>(person2);
        result2.Name.ShouldBeNull();
    }

    [Fact]
    public void When_using_precondition_class()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Name, o =>
                {
                    o.PreCondition<SourcePreCondition>();
                    o.MapFrom(s => s.Name);
                });
        });

        var mapper = config.CreateMapper();

        var person = new Person { Name = "John", Age = 25 };
        var result = mapper.Map<PersonDto>(person);
        result.Name.ShouldBe("John");
    }

    [Fact]
    public void When_using_type_based_condition()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Person), typeof(PersonDto))
                .ForMember(nameof(PersonDto.Age), o =>
                {
                    o.Condition(typeof(AgeCondition));
                    o.MapFrom(nameof(Person.Age));
                });
        });

        var mapper = config.CreateMapper();

        var person1 = new Person { Age = 25, Name = "John" };
        var result1 = (PersonDto)mapper.Map(person1, typeof(Person), typeof(PersonDto));
        result1.Age.ShouldBe(25);

        var person2 = new Person { Age = 10, Name = "Jane" };
        var result2 = (PersonDto)mapper.Map(person2, typeof(Person), typeof(PersonDto));
        result2.Age.ShouldBe(0); // Should not be mapped
    }

    [Fact]
    public void When_using_type_based_precondition()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Person), typeof(PersonDto))
                .ForMember(nameof(PersonDto.Name), o =>
                {
                    o.PreCondition(typeof(SourcePreCondition));
                    o.MapFrom(nameof(Person.Name));
                });
        });

        var mapper = config.CreateMapper();

        var person = new Person { Name = "John", Age = 25 };
        var result = (PersonDto)mapper.Map(person, typeof(Person), typeof(PersonDto));
        result.Name.ShouldBe("John");
    }

    [Fact]
    public void When_mixing_lambda_and_class_conditions()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Person, PersonDto>()
                .ForMember(d => d.Age, o =>
                {
                    o.Condition<AgeCondition>();
                    o.MapFrom(s => s.Age);
                })
                .ForMember(d => d.Name, o =>
                {
                    o.Condition(s => !string.IsNullOrEmpty(s.Name));
                    o.MapFrom(s => s.Name);
                });
        });

        var mapper = config.CreateMapper();

        var person1 = new Person { Age = 25, Name = "John" };
        var result1 = mapper.Map<PersonDto>(person1);
        result1.Age.ShouldBe(25);
        result1.Name.ShouldBe("John");

        var person2 = new Person { Age = 10, Name = "Jane" };
        var result2 = mapper.Map<PersonDto>(person2);
        result2.Age.ShouldBe(0); // Class condition fails
        result2.Name.ShouldBe("Jane"); // Lambda condition passes
    }
}

